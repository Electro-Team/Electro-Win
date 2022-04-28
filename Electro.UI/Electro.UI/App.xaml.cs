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
using Electro.UI.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            //for (int i = 0; i != e.Args.Length; ++i)
            //{
            //    if (e.Args[i] == "update")
            //    {
            //        while (true)
            //        {
            //            Process[] processes = Process.GetProcessesByName("Electro.UI.exe");
            //            ElectroMessageBox.Show(processes.Length.ToString());
            //            if (processes.Length <= 0)
            //            {
            //                try
            //                {
            //                    File.Delete("Electro.exe");
            //                }
            //                catch (UnauthorizedAccessException)
            //                {
            //                    FileAttributes attributes = File.GetAttributes("Electro.UI.exe");
            //                    if ((attributes & FileAttributes.Archive) == FileAttributes.Archive)
            //                    {
            //                        attributes &= ~FileAttributes.Archive;
            //                        File.SetAttributes("Electro.UI.exe", attributes);
            //                        File.Delete("Electro.UI.exe");
            //                    }
            //                    else
            //                    {
            //                        throw;
            //                    }
            //                }
            //                //File.Delete("Electro.UI.exe");
            //                string src = Process.GetCurrentProcess().MainModule.FileName;
            //                File.Copy(src, "Electro.UI.exe");
            //                var p = new System.Diagnostics.Process();
            //                p.StartInfo.FileName = "cmd.exe";
            //                p.StartInfo.Arguments = "/c " + "Electro.UI.exe remove_" + src.ToString();
            //                p.StartInfo.RedirectStandardOutput = true;
            //                p.StartInfo.UseShellExecute = false;
            //                p.StartInfo.CreateNoWindow = true;
            //                p.Start();
            //                Application.Current.Shutdown();
            //            }
            //        }
            //    }
            //    else if (e.Args[i].StartsWith("remove_"))
            //    {
            //        string process_name = (e.Args[i].Split('_')[1] + "_" + e.Args[i].Split('_')[2]);
            //        System.Threading.Thread.Sleep(5000);
            //        FileInfo f = new FileInfo(process_name);
            //        f.Delete();

            //    }
            //}
            //string[] files = Directory.GetFiles(@".", "*.exe");
            //foreach (var file in files)
            //{
            //    if (file.Contains("update_"))
            //    {
            //        var p = new System.Diagnostics.Process();
            //        p.StartInfo.FileName = "cmd.exe";
            //        p.StartInfo.Arguments = "/c " + file + " update";
            //        p.StartInfo.RedirectStandardOutput = true;
            //        p.StartInfo.UseShellExecute = false;
            //        p.StartInfo.CreateNoWindow = true;
            //        p.Start();
            //        Application.Current.Shutdown();
            //    }
            //}
            //try
            //{
            //    //http://eldl.ir/app/windows/update/Electro.exe
            //    WebRequest webRequest = WebRequest.Create("https://elcdn.ir/app/pc/win/etc/settings.json");
            //    HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            //    if (response.StatusDescription == "OK")
            //    {
            //        Stream dataStream = response.GetResponseStream();
            //        StreamReader reader = new StreamReader(dataStream);
            //        string responseFromServer = reader.ReadToEnd();
            //        var data = JObject.Parse(responseFromServer);
            //        if (data["lastVersion"].ToString() != version)
            //        {
            //            if (!File.Exists(@"update" + @"_" + data["lastVersion"].ToString() + @".exe"))
            //            {
            //                WebClient webClient = new WebClient();
            //                UriBuilder uriBuilder = new UriBuilder(data["downloadPath"].ToString());
            //                if (!data["downloadPath"].ToString().EndsWith(".exe"))
            //                {

            //                }
            //                else
            //                {
            //                    webClient.DownloadFileAsync(uriBuilder.Uri, @"update" + @"_" + data["lastVersion"].ToString() + @".exe");
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ElectroMessageBox.Show("Connection to update server failed !" + Environment.NewLine + ex.Message);
            //    Application.Current.Shutdown();
            //}


            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
