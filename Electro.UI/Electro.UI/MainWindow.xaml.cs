using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
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
using Version = Electro.UI.Tools.Version;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string version = "1.0.0.0";
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.IsHitTestVisible = false;
                HttpClient client = new HttpClient();

                var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/ver/version.json");
                var objects = JsonConvert.DeserializeObject<Version>(data);
                if (objects != null)
                {
                    this.IsHitTestVisible = true;
                    if (!objects.lastVersion.Equals(Assembly.GetEntryAssembly()?.GetName().Version.ToString()))
                    {
                        try
                        {
                            var p = new Process();
                            p.StartInfo.FileName = "Electro.Updater.exe";
                            p.StartInfo.Arguments = "update";
                            p.Start();
                            Application.Current.Shutdown();
                        }
                        catch (Exception exception)
                        {
                            ElectroMessageBox.Show("Electro Updater file does not exist!", "Warning");
                            Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ElectroMessageBox.Show("Could not connect to server!", "Error");
                Application.Current.Shutdown();
            }
        }
    }
}
