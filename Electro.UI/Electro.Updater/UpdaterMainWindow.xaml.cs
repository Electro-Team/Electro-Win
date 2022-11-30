using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Electro.UI.Windows;
using Newtonsoft.Json;
using NLog;
using Version = Electro.UI.Tools.Version;

namespace Electro.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UpdaterMainWindow : Window
    {
        private WebClient webClient = new WebClient();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public UpdaterMainWindow()
        {
            InitializeComponent();
            Loaded += UpdaterMainWindow_Loaded;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
        }

        private async void UpdaterMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/ver/versionnew.json");
                var objects = JsonConvert.DeserializeObject<Version>(data);
                if (objects != null)
                {
                    if (!Directory.Exists("temp"))
                    {
                        Directory.CreateDirectory("temp");
                    }
                    File.Move("Electro.exe", @"temp\Electro.exe");
                    UriBuilder uriBuilder = new UriBuilder(objects.downloadPath);
                    await webClient.DownloadFileTaskAsync(uriBuilder.Uri, "Electro.exe");
                }
            }
            catch (Exception exception)
            {
                ElectroMessageBox.Show($"Operation failed.");
                logger.Error(exception);
                throw;
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    File.Delete("Electro.exe");
                    File.Move(@"temp\Electro.exe", "Electro.exe");
                    if (Directory.Exists("temp"))
                    {
                        Directory.Delete("temp", true);
                    }
                    ElectroMessageBox.Show("Updating Electro Failed!", "Error Updating");
                    Application.Current.Shutdown();
                }
                if (Directory.Exists("temp"))
                {
                    Directory.Delete("temp", true);
                }
                ElectroMessageBox.Show("Update Succeeded.", "Electro Updater");
                System.Diagnostics.Process.Start(@"Electro.exe");
                Application.Current.Shutdown();
            }
            catch (Exception exception)
            {
                logger.Error(exception);
                ElectroMessageBox.Show($"Download Operation Failed.");
                Application.Current.Shutdown();
            }
        }
    }
}
