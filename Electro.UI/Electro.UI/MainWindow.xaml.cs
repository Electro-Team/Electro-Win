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
using AutoUpdaterDotNET;
using Electro.UI.ViewModels;
using Electro.UI.Windows;
using Newtonsoft.Json;
using AutoUpdaterDotNET;
using NLog;
using Version = Electro.UI.Tools.Version;
using System.IO;
using Electro.UI.Tools;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Fields
        private static string version = "1.0.0.0";

        //Constructor
        public MainWindow()
        {
            _ = new AuthenticationViewModel();

            //AutoUpdater.Start("http://elcdn.ir/dl/pc/update.xml");
            //AutoUpdater.ShowSkipButton = false;
            //AutoUpdater.Synchronous = true;
            //AutoUpdater.Mandatory = true;
            //AutoUpdater.UpdateMode = Mode.ForcedDownload;
            try
            {
                string path = AppContext.BaseDirectory + @"openVPN\Batch.txt";
                if (!File.Exists(path))
                {
                    InstallTapAdapter();
                    File.Create(path);
                }
            }
            catch (Exception ex)
            {
                MyLogger.GetInstance().Logger.Error(ex);
            }
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        #region Private Methods

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    this.IsHitTestVisible = false;
            //    HttpClient client = new HttpClient();

            //    var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/ver/versionnew.json");
            //    var objects = JsonConvert.DeserializeObject<Version>(data);
            //    if (objects != null)
            //    {
            //       // if (!objects.lastVersion.Equals(Assembly.GetEntryAssembly()?.GetName().Version.ToString()))
            //       // {
            //        //    try
            //         //   {
            //            //    var p = new Process();
            //             //   p.StartInfo.FileName = "Electro.Updater.exe";
            //            //    p.StartInfo.Arguments = "update";
            //              //  p.StartInfo.UseShellExecute = true;
            //               // p.Start();
            //             //   Application.Current.Shutdown();
            //          //  }
            //          //  catch (Exception exception)
            //          //  {
            //            //    ElectroMessageBox.Show("Electro Updater file does not exist!", "Warning");
            //             //   Application.Current.Shutdown();
            //           // }
            //       // }
            //    }

            //    //if (!Properties.Settings.Default.InitializeTAP)
            //    //{
            //    //    var installed = InstallTapAdapter();
            //    //}
            //    this.IsHitTestVisible = true;
            //}
            //catch (Exception exception)
            //{
            //    ElectroMessageBox.Show("Could not connect to server!", "Error");
            //    Application.Current.Shutdown();
            //}
        }
        #region Public Methods

        #endregion
        public bool InstallTapAdapter()
        {
            bool installed = false;
            ProcessStartInfo processInfo = null;
            Process proc = new System.Diagnostics.Process();
            try
            {
                /*
                proc.StartInfo.FileName = AppContext.BaseDirectory+"\\openVPN\\Driver\\addtap.bat";
                proc.StartInfo.Verb = "runas";
                proc.StartInfo.WorkingDirectory = AppContext.BaseDirectory + "\\openVPN\\Driver";
                proc.Start();
                */
                string str = proc.StandardOutput.ReadToEnd();
                string err = proc.StandardError.ReadToEnd();
                int exitCode = proc.ExitCode;

                if (err.Length > 0)
                    throw new Exception(err);

                // Write into logs
                MyLogger.GetInstance().Logger.Info("COMPLETED Installing tap Exit code = " + exitCode);

                if (str.IndexOf("Drivers installed successfully") > -1)

                {
                    installed = true;
                    // Write into logs  
                    MyLogger.GetInstance().Logger.Info("Tap Adapter Installed Successfully");
                }
                // Write into logs
                MyLogger.GetInstance().Logger.Info("Finished TAP");
            }
            catch (Exception e)
            {
                // Write into logs
                MyLogger.GetInstance().Logger.Error("Error Installing Tap Adapter : " + e.Message);
            }
            finally
            {
                processInfo = null;
                if (proc != null)
                {
                    proc.Close();
                    proc = null;
                }
            }
            return installed;
        } 
        #endregion
    }
}
