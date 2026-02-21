using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;

namespace Port_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private struct DataLine(string port, string pid)
        {
            public string Port { get; set; } = port;
            public string Pid { get; set; } = pid;
        }
        private List<DataLine> data = [];
        //private List<DataPoint> sub_data = [];
        private List<string> lines = [];
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lines = await LoadPortDataAsync();
            data = ParseData();
            //PrintData(); // Debug
            PopulateGrid();
        }

        private void PrintData()
        {
            foreach(DataLine hi in data)
            {
                Debug.WriteLine($"{hi.Port}\t{hi.Pid}");
            }
        }

        private static async Task<List<string>> LoadPortDataAsync()
        {
            var lines = new List<string>();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netstat -ano",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            while (true)
            {
                string? line = await process.StandardOutput.ReadLineAsync();
                if (line is null)
                    break;
                lines.Add(line);
            }
            await process.WaitForExitAsync();
            return lines;
        }

        private List<DataLine> ParseData()
        {
            List<DataLine> result = [];
            for (int i = 4; i < lines.Count; i++) // skip first group
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    break;
                string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 5)
                    break;
                bool stop = false;
                string second = "";
                string fifth = "";

                for (int j = 0; j < 5; j++)
                {
                    if (parts[j].Equals("UDP"))
                    {
                        stop = true;
                        break;
                    }

                    if (j == 1) second = parts[j]; // Port
                    if (j == 4) fifth = parts[j]; // PID
                }
                if (stop)
                    break;
                result.Add(new(second, fifth));
            }
            return result;
        }

        private void ClearGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
        }
    
        private void PopulateGrid()
        {
            int num_data = data.Count;
            for (int i = 0; i < num_data; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for(int i = 0; i < num_data; i++)
            {
                TextBlock port_textblock = new()
                {
                    Text = data[i].Port,
                    Margin = Margin = new Thickness(20),
                    MinHeight = 25,
                    HorizontalAlignment=HorizontalAlignment.Center,
                    ToolTip = data[i].Port,
                };

                TextBlock pid_textblock = new()
                {
                    Text = data[i].Pid,
                    Margin = Margin = new Thickness(20),
                    MinHeight = 25,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ToolTip = data[i].Pid,
                };
                port_textblock.MouseLeftButtonDown += Copy_Text; pid_textblock.MouseLeftButtonDown += Copy_Text;

                Grid.SetRow(port_textblock, i);
                Grid.SetRow(pid_textblock, i);
                Grid.SetColumn(pid_textblock, 1);
                grid.Children.Add(port_textblock); grid.Children.Add(pid_textblock);
            }
        }

        private void Copy_Text(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textblock)
                Clipboard.SetText(textblock.Text);
            else
                MessageBox.Show("Error: port or pid was not successfully copied to clipboard");
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e) // Call the function again
        {
            ClearGrid();
            Window_Loaded(sender, e);
        }

        private void Refresh() // Refresh without calling function again, used by ordering and search
        {
            ClearGrid();
            PopulateGrid();
        }

        private void Sort_By_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            switch(item.Name)
            {
                case "IPort": // Order by
                    var sortedFruits = data.OrderBy(s => s);
                    Refresh();
                    break;
                case "DPort":
                    Refresh();
                    break;
                case "IPID":
                    Refresh();
                    break;
                case "DPID":
                    Refresh();
                    break;
                default: // Do nothing
                    break;
            }
            // use linq
            Refresh_Button_Click(sender, e);
        }

        private void Search_For_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            switch(item.Name)
            {
                case "SearchPort": // Match / contains
                    break;
                case "SearchPID":
                    break;
                default: // Do nothing
                    break;
            }
            // Use LINQ
            Refresh_Button_Click(sender, e); // Add second string list, check if null before LoadPortDataAsync();
        }
    }
}