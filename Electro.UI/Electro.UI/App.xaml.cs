using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            bool startMinimized = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "update")
                {
                        while(true)
                        {
                        Process[] processes = Process.GetProcessesByName("Electro");
                        if (processes.Length <= 0)
                        {
                            File.Delete("Electro.exe");
                            string src = Process.GetCurrentProcess().MainModule.FileName;
                            File.Copy(src, "Electro.exe");
                            Process.Start("Electro.exe remove_" + src.ToString());
                            Application.Current.Shutdown();
                        }
                        }
                }
                if (e.Args[i].StartsWith("remove_"))
                {
                    string process_name = e.Args[i].Split('_')[1].Replace(".exe", "");
                    while (true)
                    {
                        Process[] processes = Process.GetProcessesByName(process_name);
                        if (processes.Length <= 0)
                        {
                            File.Delete(process_name + ".exe");
                        }
                    }
                }
            }

            MainWindow mainWindow = new MainWindow();
            if (startMinimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.Show();
        }
    }
}
