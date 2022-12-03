using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.ViewModels.DNS;
using Electro.UI.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Threading;
using Version = Electro.UI.Tools.Version;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            NLog.LogManager.Configuration = config;

            var runningInstances = System.Diagnostics.Process.GetProcessesByName(
                    System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (runningInstances.Length > 1)
            {
                Environment.Exit(0);
            }
          
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Creating thread
            // Using thread class
            DNSViewModel dnV=new DNSViewModel();
            Thread thr = new Thread(new ThreadStart(DNSViewModel.UnsetDnsEvent));
            thr.Start();
         // DNSViewModel.UnsetDnsEvent();
        }
    }
}
