using Electro.UI.Interfaces;
using Electro.UI.Models;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Electro.UI.Services
{
    public class Softether : IService
    {
        private string uniqueId;

        private string accountName;
        private string username;
        private string password;
        private string hubName;
        private string hostName;


        public string ServiceText => "● Connecting to Softether...";

        private void IstallationVPNClientCMDCode(bool isforInstall)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.RedirectStandardOutput = true;


                psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpnclient_x64.exe";
                psi.Arguments = isforInstall ? "/install" : "/uninstall";

                Process proc = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
            }
        }

        private void SetNicAdapter()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.RedirectStandardOutput = true;


                psi.FileName = AppContext.BaseDirectory + "/vpncmd/nic64.exe";
                psi.Arguments = "instvlan vpn";

                Process proc = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };
                proc.Start();
                Thread.Sleep(30);
                proc.WaitForExit();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    // do something with line
                }
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
                throw;
            }
        }

        private async Task GetUserSoftEtherInfoFromServer()
        {
            try
            {
                string response = await HttpRequestHandler.SoftEtherConfigRequestAndGetResponse(/*UniqueId*/);
                dynamic response_dynamic = JsonConvert.DeserializeObject(response);

                hostName = response_dynamic.domainandport;
                hubName = response_dynamic.hubname;
                accountName = response_dynamic.accountname;

                User user = User.GetUser();
                //username = user.UniqueId;
                //password = user.Password;
                username = "vpn";
                password = "electame";
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show("error occurred.");
                ElectroMessageBox.Show(ex.Message);
            }

        }

        private void RunInitialVpnCMDCommands()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;
            
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            //Thread.Sleep(30);
            //proc.WaitForExit();

            proc.Start();
            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("2");
                    sw.WriteLine("");

                    sw.WriteLine("accountcreate " + accountName);

                    //hostname and port:
                    sw.WriteLine("" + hostName);

                    //destination virtual hub name :
                    sw.WriteLine("" + hubName);

                    //connecting user name :
                    sw.WriteLine("" + username);
                    //NAT
                    sw.WriteLine("vpn");

                    sw.WriteLine("accountpasswordset");

                    sw.WriteLine("" + accountName);

                    //pass
                    sw.WriteLine(password);
                    sw.WriteLine(password);

                    sw.WriteLine("radius");
                }
            }
        }

        private void DeleteElectroAccount()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;

                psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";
                psi.Arguments = "accountdelete electro";

                Process proc = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };

                proc.Start();
                Thread.Sleep(30);
                proc.WaitForExit();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    // do something with line
                }
            }
            catch (Exception)
            {
                ElectroMessageBox.Show("error occurred.");
                throw;
            }
        }

        //private void SoftEtherConnctVPNCMD()
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo();
        //    psi.CreateNoWindow = true;
        //    psi.UseShellExecute = false;
        //    psi.CreateNoWindow = true;
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.Verb = "runas";
        //    psi.RedirectStandardOutput = true;
        //    psi.RedirectStandardInput = true;

        //    psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";
        //    psi.Arguments = "accountconnect " + accountName;

        //    Process proc = new Process
        //    {
        //        StartInfo = psi,
        //        EnableRaisingEvents = true
        //    };

        //    proc.Start();
        //    Thread.Sleep(30);
        //    proc.WaitForExit();

        //    psi.Arguments = "accountstatusget " + accountName;

        //    proc.Start();
        //    Thread.Sleep(30);
        //    proc.WaitForExit();
        //}

        private void SoftEtherConnctVPNCMD()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;

            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            //Thread.Sleep(30);
            //proc.WaitForExit();

            proc.Start();
            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("2");
                    sw.WriteLine("");

                    sw.WriteLine("accountconnect " + accountName);
                   
                }
            }
            proc.WaitForExit();


            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.CreateNoWindow = true;
            //psi.UseShellExecute = false;
            //psi.CreateNoWindow = true;
            //psi.WindowStyle = ProcessWindowStyle.Hidden;
            //psi.Verb = "runas";
            //psi.RedirectStandardOutput = true;
            //psi.RedirectStandardInput = true;

            //psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";
            //psi.Arguments = "accountconnect " + accountName;

            //Process proc = new Process
            //{
            //    StartInfo = psi,
            //    EnableRaisingEvents = true
            //};

            //proc.Start();
            //Thread.Sleep(30);
            //proc.WaitForExit();

            //psi.Arguments = "accountstatusget " + accountName;

            //proc.Start();
            //Thread.Sleep(30);
            //proc.WaitForExit();
        }

        private void SaveSoftEtherConfigs()
            => Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node", "softether", "configured");

        public bool IsFirstTime()
        {
            bool result = true;
            try
            {
                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Electro\", "softether", null) == null)
                {
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node", "softether", "configured");
                    //code if key Not Exist
                }
                else
                {
                    string SoftEthervalue = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Electro\",
                             "softether", "NULL").ToString();
                    if (SoftEthervalue == "configured")
                        result = false;
                }
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
            }
            return result;
        }

        private async Task FirstTimeConnection()
        {
            //IstallationVPNClientCMDCode(false);
            //IstallationVPNClientCMDCode(true);
            //SetNicAdapter();
            await GetUserSoftEtherInfoFromServer();
            RunInitialVpnCMDCommands();
            //DeleteElectroAccount();
            SaveSoftEtherConfigs();
        }


        public async Task<bool> Connect()
        {
            if (true || IsFirstTime())
                await FirstTimeConnection();

            SoftEtherConnctVPNCMD();
            return true;
        }

        public void Dispose()
        {
            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.CreateNoWindow = true;
            //psi.UseShellExecute = false;
            //psi.CreateNoWindow = true;
            //psi.WindowStyle = ProcessWindowStyle.Hidden;
            //psi.Verb = "runas";
            //psi.RedirectStandardOutput = true;
            //psi.RedirectStandardInput = true;

            //psi.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64.exe";
            //psi.Arguments = "accountdisconnect " + accountName;

            //Process proc = new Process
            //{
            //    StartInfo = psi,
            //    EnableRaisingEvents = true
            //};

            //proc.Start();
            ////Thread.Sleep(30);
            //proc.WaitForExit();
        }

        public string UniqueId
        {
            get => uniqueId;
            set => uniqueId = value;
        }
    }
}
