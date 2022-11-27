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
using Electro.UI.ViewModels;
using Electro.UI.Windows;
using Newtonsoft.Json;
using NLog;
using Version = Electro.UI.Tools.Version;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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

                var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/ver/versionnew.json");
                var objects = JsonConvert.DeserializeObject<Version>(data);
                if (objects != null)
                {
                    if (!objects.lastVersion.Equals(Assembly.GetEntryAssembly()?.GetName().Version.ToString()))
                    {
                        try
                        {
                            var p = new Process();
                            p.StartInfo.FileName = "Electro.Updater.exe";
                            p.StartInfo.Arguments = "update";
                            p.StartInfo.UseShellExecute = true;
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

                //if (!Properties.Settings.Default.InitializeTAP)
                //{
                //    var installed = InstallTapAdapter();
                //}
                this.IsHitTestVisible = true;
            }
            catch (Exception exception)
            {
                ElectroMessageBox.Show("Could not connect to server!", "Error");
                Application.Current.Shutdown();
            }
        }
        public bool InstallTapAdapter()

        {

            bool installed = false;

            ProcessStartInfo processInfo = null;

            Process process = null;

            try

            {

                string command = "";



                command = "tapinstall.exe install \"OemWin2k.inf\" tap0901";



                processInfo = new ProcessStartInfo("cmd.exe", "/C " + command);

                processInfo.UseShellExecute = false;

                processInfo.RedirectStandardOutput = true;

                processInfo.RedirectStandardError = true;

                processInfo.CreateNoWindow = true;
                processInfo.WorkingDirectory = "C:\\Program Files\\TAP-Windows\\bin";


                process = new Process();

                process.StartInfo = processInfo;

                process.Start();



                string str = process.StandardOutput.ReadToEnd();

                string err = process.StandardError.ReadToEnd();

                int exitCode = process.ExitCode;



                if (err.Length > 0)

                    throw new Exception(err);


                // Write into logs
                logger.Info("COMPLETED Installing tap Exit code = " + exitCode);

                if (str.IndexOf("Drivers installed successfully") > -1)

                {

                    installed = true;
                    // Write into logs  
                    logger.Info("Tap Adapter Installed Successfully");

                }



                // Write into logs
                logger.Info("Finished TAP");

            }

            catch (Exception e)

            {
                // Write into logs
                logger.Error("Error Installing Tap Adapter : " + e.Message);

            }

            finally

            {
                processInfo = null;
                if (process != null)

                {
                    process.Close();
                    process = null;
                }
            }

            return installed;

        }
    }
}
