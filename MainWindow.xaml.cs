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
        private enum data_type {PORT, PID};
        private struct DataPoint
        {
            public string line;
            public data_type type;
        }
        private List<DataPoint> data = [];
        // private List<DataPoint> sub_data = [];
        private List<string> lines = [];
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lines = await LoadPortDataAsync();
            ParseData();
            //PrintData(); // Debug
            PopulateGrid();
        }

        private void PrintData()
        {
            foreach(DataPoint hi in data)
            {
                Debug.WriteLine(hi.line);
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

        private void ParseData()
        {
            List<string> result = [];
            for (int i = 4; i < data.Count; i++) // skip first group
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

                    if (j == 1) second = parts[j];
                    if (j == 4) fifth = parts[j];
                }
                if (stop)
                    break;
                result.Add(second);
                result.Add(fifth);
            }
            data = result; // broken
        }

        private void ClearGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
        }
    
        private void PopulateGrid()
        {
            int num_data = data.Count;
            int num_rows = (int)Math.Ceiling((double)num_data / 2);
            for (int i = 0; i < num_rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for(int i = 0; i < num_data; i++)
            {
                TextBlock text = new()
                {
                    Text = data[i].line,
                    Margin = Margin = new Thickness(20),
                    MinHeight = 25,
                    HorizontalAlignment=HorizontalAlignment.Center,
                    ToolTip = data[i],
                    Tag = data[i].type, // possibly fix
                };
                text.MouseLeftButtonDown += Copy_Text;
                Grid.SetRow(text, (i / 2));
                if (i % 2 != 0)// PID
                    Grid.SetColumn(text, 1);
                grid.Children.Add(text);
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