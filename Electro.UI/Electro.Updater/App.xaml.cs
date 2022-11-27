using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using Version = Electro.UI.Tools.Version;

namespace Electro.Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 1)
            {
                Current.Shutdown();
            }

            UpdaterMainWindow mainWindow = new UpdaterMainWindow();
            mainWindow.Show();
        }
    }
}
