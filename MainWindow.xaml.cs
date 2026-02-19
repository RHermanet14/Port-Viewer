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

namespace Port_Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> data = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            data = await LoadPortDataAsync();
            ParseData();
            //PrintData(); // Debug
            PopulateGrid();
        }

        private void PrintData()
        {
            foreach(string hi in data)
            {
                Debug.WriteLine(hi);
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
                string line = data[i];
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
            data = result;
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
                    Text = data[i],
                    Margin = Margin = new Thickness(20),
                    MinHeight = 25,
                    HorizontalAlignment=HorizontalAlignment.Center,
                    ToolTip = data[i],
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

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            ClearGrid();
            PopulateGrid();
        }

        private void Sort_By_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Search_For_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}