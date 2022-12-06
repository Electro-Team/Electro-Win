using DeviceId;
using Electro.UI.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Services
{
    internal class OpenVPN : IService
    {
        //Fields
        private DNSController dNSController = DNSController.GetInstance();
        IConnectionObserver connectionObserver;
        public string ServiceText => "● Connecting to OVPN..."; 

        //Constructor
        internal OpenVPN(IConnectionObserver connectionObserver)
            => this.connectionObserver = connectionObserver;

        #region Private Methods
        private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                string output = outLine.Data;
                MyLogger.GetInstance().Logger.Info(output);

                if (output != null)
                {
                    //connectionObserver.ConnectionObserver(null, output);
                    if (output.Contains("Initialization Sequence Completed"))
                    {
                        //Create Batch File.
                        string pathToRouteBatch = await CreateBatchAndGetPath();

                        //Run CMD.
                        await RunCMDCode(pathToRouteBatch);
                        connectionObserver?.ConnectionObserver(true, "● Connected to OVPN");
                    }
                }
            }
            catch (Exception e)
            {
                MyLogger.GetInstance().Logger.Error(e);
            }
        }

        private async Task<string> CreateBatchAndGetPath()
        {
            string newTempDir = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToRouteBatch = newTempDir + "//route12.bat";
            var routeBatch = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.Route12Bat);
            await DirectoryHelperFunctions.WriteAsync(routeBatch, pathToRouteBatch);
            return pathToRouteBatch;
        }

        private async Task RunCMDCode(string pathToRouteBatch)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.FileName = @"cmd.exe";
            psi.Verb = "runas";

            psi.Arguments = "/C \"" + pathToRouteBatch;

            Process proc = new Process();
            proc.StartInfo = psi;

            proc.Start();
            string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
            await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.HardwareID + deviceId);
        }

        private async Task<ProcessStartInfo> GetOvnInfo()
        {
            string newTempDir = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToConfig = newTempDir + "//profile.ovpn";
            string pathToConfigcfg = "C://Windows" + "//user.cfg";
            var openVpnProfile = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.OpenVpn);
            var openVpnProfileconf = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.OpenVpnUser);
            await DirectoryHelperFunctions.WriteAsync(openVpnProfile, pathToConfig);
            await DirectoryHelperFunctions.WriteAsync(openVpnProfileconf, pathToConfigcfg);
            string arguments = "--config \"" + pathToConfig + "\" --block-outside-dns";
            ProcessStartInfo openVpnStartInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                FileName = AppContext.BaseDirectory + "/Openvpn/openvpn.exe",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return openVpnStartInfo;
        }

        private void StopProcess()
        {
            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();
        }


        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                ///Change DNS
                await dNSController?.GetDataFromServerAndSetDNS();

                ///Start OpenVPN EXE.
                ProcessStartInfo openVpnStartInfo = await GetOvnInfo();
                Process openVpn = new Process
                {
                    StartInfo = openVpnStartInfo,
                    EnableRaisingEvents = true
                };
                openVpn.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                openVpn.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                openVpn.Start();
                openVpn.BeginOutputReadLine();
                openVpn.BeginErrorReadLine();
                return true;
            }
            catch (Exception e)
            {
                MyLogger.GetInstance().Logger.Error(e);
                return false;
            }
            finally
            {
            }
        }

        public async void Dispose()
        {
            await dNSController.UnsetDNS1();
            StopProcess();
            connectionObserver?.ConnectionObserver(false, "");
        }
        #endregion
    }
}
