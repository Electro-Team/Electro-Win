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
        protected const string hostport = "443";

        private const int timeout = 10000;

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
            try
            {
                //Create Account.
                bool createAcountResult = GeneralCommand("AccountCreate " + accountName, " /SERVER:" + hostName + " /HUB:" + hubName + " /USERNAME:" + username);

                //Set Password.
                bool setPassResult = GeneralCommand("AccountPasswordSet " + accountName, " /PASSWORD:" + password + " /TYPE:" + "radius");
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
            }
        }

        private void DeleteElectroAccount()
        {
            try
            {
                //Delete Account.
                bool createAcountResult = GeneralCommand("AccountDelete " + accountName, "");
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
            }
        }


        private void SoftEtherConnctVPNCMD()
        {
            try
            {
                //Account Connect.
                bool createAcountResult = GeneralCommand("AccountConnect " + accountName, "");

                //Check Connection.
                bool setPassResult = GeneralCommand("AccountStatusGet " + accountName, "");
            }
            catch (Exception ex)
            {
                ElectroMessageBox.Show(ex.Message);
            }
        }


        private bool GeneralCommand(string cmd, string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = AppContext.BaseDirectory + "/vpncmd/vpncmd_x64";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            processStartInfo.CreateNoWindow = true;

            Process app = new Process();
            app.StartInfo = processStartInfo;
            app.StartInfo.Arguments = "vpncmd" + " /CLIENT localhost " + " /CMD " + cmd + arguments;
            app.Start();

            Timer t = new Timer(f => { app.Kill(); }, null, timeout, Timeout.Infinite);

            string output = app.StandardOutput.ReadToEnd();
            var words = output.Split(' ', '\n', '\r', '\t');
            var matchquery = from string word in words where word.ToLowerInvariant() == "Error".ToLowerInvariant() select word;

            if (matchquery.Count() >= 1)
                return false;

            app.WaitForExit(timeout);


            return true;
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
            IstallationVPNClientCMDCode(false);
            IstallationVPNClientCMDCode(true);
            SetNicAdapter();
            await GetUserSoftEtherInfoFromServer();
            RunInitialVpnCMDCommands();
            DeleteElectroAccount();
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
            //Account DisConnection.
            bool setPassResult = GeneralCommand("AccountDisconnect " + accountName, "");
        }

        public string UniqueId
        {
            get => uniqueId;
            set => uniqueId = value;
        }
    }
}
