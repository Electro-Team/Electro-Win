using DeviceId;
using Electro.UI.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electro.UI.Interfaces;

namespace Electro.UI.Services
{
    public class OpenVPN : IService
    {
        //Fields
        private IDNSService dnsService;
        IConnectionObserver connectionObserver;
        public string ServiceText => "● Connecting to OVPN..."; 

        //Constructor
        public OpenVPN(IConnectionObserver connectionObserver,
            IDNSService dnsService)
        {
            this.connectionObserver = connectionObserver;
            this.dnsService = dnsService;
        }

        #region Private Methods
        private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                if (!connectionObserver.IsGettingData)
                {
                    await StopProcess();
                    return;
                }
                string output = outLine.Data;
                MyLogger.GetInstance().Logger.Info(output);

                if (output != null)
                {
                    for (int i = 0; i < OpenVPNConnectionDescription.Description.Count; i++)
                    {
                        var descript = OpenVPNConnectionDescription.Description.ElementAt(i);
                        if (output.Contains(descript.Key))
                        {
                            connectionObserver.ConnectionObserver(null, descript.Value + 
                                                                        $" {i}/{OpenVPNConnectionDescription.Description.Count}");
                        }
                    }
                    if (output.Contains("Initialization Sequence Completed"))
                    {
                        // this may lead to error duo to connection error and does not show the user that the vpn connection succeed.
                        await Task.Factory.StartNew(async() =>
                        {
                            //Create Batch File.
                            string pathToRouteBatch = await CreateBatchAndGetPath();

                            //Run CMD.
                            await RunCMDCode(pathToRouteBatch);
                        });
                        connectionObserver?.ConnectionObserver(true, "● Connected to OVPN");
                    }
                    else if (output.Contains("fatal error"))
                    {
                        await StopProcess();
                        connectionObserver?.ConnectionObserver(false, "● Fatal Error!");
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

        private async Task<ProcessStartInfo> GetOvpnInfo()
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

        private async Task StopProcess()
        {
            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();

            await dnsService?.GetDataFromServerAndSetDNS();
        }


        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                ///Change DNS
                await dnsService?.GetDataFromServerAndSetDNS();

                ///Start OpenVPN EXE.
                ProcessStartInfo openVpnStartInfo = await GetOvpnInfo();
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
        }

        public async void Dispose()
        {
            await dnsService.UnsetDNS();
            await StopProcess();
            connectionObserver?.ConnectionObserver(false, "");
        }
        #endregion
    }
}
