using System.Diagnostics;
using System.Text;
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
            // it parses the data
        }
    }
}