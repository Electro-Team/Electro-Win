using Electro.UI.Interfaces;
using Electro.UI.Tools;
using Electro.UI.Windows;
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

        private async void GetUserSoftEtherInfoFromServer()
        {
            string response = await HttpRequestHandler.SoftEtherConfigRequestAndGetResponse(UniqueId);
            dynamic response_dynamic = JsonConvert.DeserializeObject(response);
            string status = response_dynamic.status;
            if (status == "Ok")
            {
                string[] stringSeparators = new string[] { "*[]*" };
                string[] result = response.Split(stringSeparators, StringSplitOptions.None);
                hostName = result[0];
                hubName = result[1];
                username = result[2];
                password = result[3];
                accountName = result[4];
            }
            else
                ElectroMessageBox.Show("error occurred.");

        }

        private void RunInitialVpnCMDCommands()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/nic64.exe";

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            proc.Start();
            Thread.Sleep(30);
            proc.WaitForExit();

            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("accountcreate " + accountName);

                    //hostname and port:
                    sw.WriteLine("" + hostName);

                    //destination virtual hub name :
                    sw.WriteLine("" + hostName);

                    //connecting user name :
                    sw.WriteLine("" + username);

                    //pass
                    sw.WriteLine(password);
                    sw.WriteLine(password);

                    sw.WriteLine("radius");
                }
            }
        }

        private void DeleteElectroAccount()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/nic64.exe";
            psi.Arguments = "accountdelete electro";

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            proc.Start();
            Thread.Sleep(30);
            proc.WaitForExit();
        }

        private void SoftEtherConnctVPNCMD()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/nic64.exe";
            psi.Arguments = "accountconnect " + accountName;

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            proc.Start();
            Thread.Sleep(30);
            proc.WaitForExit();

            psi.Arguments = "accountstatusget " + accountName;

            proc.Start();
            Thread.Sleep(30);
            proc.WaitForExit();
        }

        private bool CheckFirstTimeConnection()
        {
            return true;
        }

        private void FirstTimeConnection()
        {
            IstallationVPNClientCMDCode(false);
            IstallationVPNClientCMDCode(true);
            SetNicAdapter();
            GetUserSoftEtherInfoFromServer();
            RunInitialVpnCMDCommands();
            DeleteElectroAccount();
        }


        public async Task<bool> Connect()
        {
            if (CheckFirstTimeConnection())
                FirstTimeConnection();

            SoftEtherConnctVPNCMD();
            return true;
        }

        public void Dispose()
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            psi.FileName = AppContext.BaseDirectory + "/vpncmd/nic64.exe";
            psi.Arguments = "accountdisconnect " + accountName;

            Process proc = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            proc.Start();
            Thread.Sleep(30);
            proc.WaitForExit();
        }

        public string UniqueId
        {
            get => uniqueId;
            set => uniqueId = value;
        }
    }
}
