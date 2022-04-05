using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.Windows;
using Newtonsoft.Json.Linq;
namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string version = "0.0.0.1";
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
                string[] files = Directory.GetFiles(@"C:\File", "*.txt");
                foreach (var file in files)
                {
                    if (file.Contains("update_"))
                    {
                        Process.Start(file + " update");
                        Application.Current.Shutdown();
                    }
                }
                try
                {
                    WebRequest webRequest = WebRequest.Create("https://elcdn.ir/app/pc/win/etc/settings.json");
                    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                    if (response.StatusDescription == "OK")
                    {
                        Stream dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
                        string responseFromServer = reader.ReadToEnd();
                        var data = JObject.Parse(responseFromServer);
                        if (data["lastVersion"].ToString() != version)
                        {
                            if (!File.Exists(@"update" + @"_" + data["lastVersion"].ToString() + @".exe"))
                            {
                                WebClient webClient = new WebClient();
                                UriBuilder uriBuilder = new UriBuilder(data["downloadPath"].ToString());
                                webClient.DownloadFileAsync(uriBuilder.Uri, @"update" + @"_" + data["lastVersion"].ToString() + @".exe");
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    // pass
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
