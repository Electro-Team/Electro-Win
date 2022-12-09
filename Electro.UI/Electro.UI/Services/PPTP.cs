using DeviceId;
using DotRas;
using Electro.UI.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Electro.UI.Interfaces;

namespace Electro.UI.Services
{
    public class PPTP : IService
    {
        //Fields
        private IDNSService dnsService;
        private IConnectionObserver connectionObserver;
        private RasDialer dialer = new RasDialer();
        private RasHandle handle;
        public string ServiceText { get => "● Connecting to PPTP..."; }

        //Constructor
        public PPTP(IConnectionObserver connectionObserver,
            IDNSService dnsService)
        {
            this.connectionObserver = connectionObserver;
            this.dnsService = dnsService;
        }

        #region Private Methods
        private async void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (!connectionObserver.IsGettingData)
            {
                Dispose();
                return;
            }

            if (e.TimedOut || e.Error != null)
            {
                connectionObserver.ConnectionObserver(false, "● Error");
                MyLogger.GetInstance().Logger.Error(e.Error);
            }

            //"The VPN connection between your computer and the VPN server could not be completed. The most common cause for this failure is that at least one Internet device (for example, a firewall or a router) between your computer and the VPN server is not configured to allow Generic Routing Encapsulation (GRE) protocol packets. If the problem persists, contact your network administrator or Internet Service Provider."

            else if (e.Connected)
            {
                Task.Factory.StartNew(async () =>
                {
                    var tempDirectory = DirectoryHelperFunctions.GetTemporaryDirectory();
                    var routeBatch = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.Route12Bat);
                    await DirectoryHelperFunctions.WriteAsync(routeBatch, tempDirectory + "//route12.bat");
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.FileName = @"cmd.exe";
                    psi.Verb = "runas";

                    psi.Arguments = "/C \"" + tempDirectory + "//route12.bat";

                    Process proc = new Process();
                    proc.StartInfo = psi;
                    proc.Start();
                    string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                    await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.HardwareID + deviceId);
                });
                connectionObserver.ConnectionObserver(true, "● Connected to PPTP");
            }
        }

        private async Task StopProcess()
        {
            if (dialer.IsBusy)
            {
                dialer.DialAsyncCancel();
            }
            else
            {
                if (handle != null)
                {
                    RasConnection Connection = RasConnection.GetActiveConnectionByHandle(handle);
                    if (Connection != null)
                    {
                        Connection.HangUp();
                    }
                }
            }

            using (RasPhoneBook PhoneBook = new RasPhoneBook())
            {
                PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));

                if (PhoneBook.Entries.Contains(MyConnection._ConnectionName))
                {
                    PhoneBook.Entries.Remove(MyConnection._ConnectionName);
                }
            }

            var tempDirectory = DirectoryHelperFunctions.GetTemporaryDirectory();
            string pathToRouteBatch = tempDirectory + "//routedel.bat";
            var routeBatch = await MyHttpClient.GetInstance().Client.GetStringAsync("http://elcdn.ir/app/vpn/routes/routedel.bat");
            await DirectoryHelperFunctions.WriteAsync(routeBatch, pathToRouteBatch);
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.FileName = @"cmd.exe";
            psi.Verb = "runas";

            psi.Arguments = "/C \"" + pathToRouteBatch;

            Process proc = new Process();
            proc.StartInfo = psi;
            proc.Start();
            await dnsService.GetDataFromServerAndSetDNS();
        }
        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                await dnsService?.GetDataFromServerAndSetDNS();
                //make sure that there is no connection enabled.
                await StopProcess();
                var ServerIp = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.PptpIp);
                using (RasPhoneBook PhoneBook = new RasPhoneBook())
                {
                    PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));
                    RasEntry Entry;

                    if (PhoneBook.Entries.Contains(MyConnection._ConnectionName))
                    {
                        PhoneBook.Entries.Remove(MyConnection._ConnectionName);
                    }

                    Entry = RasEntry.CreateVpnEntry(MyConnection._ConnectionName, 
                        ServerIp, 
                        RasVpnStrategy.PptpOnly, 
                        RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));

                    Entry.Options.PreviewDomain = false;
                    Entry.Options.ShowDialingProgress = false;
                    Entry.Options.PromoteAlternates = false;
                    Entry.Options.DoNotNegotiateMultilink = false;
                    Entry.Options.RequirePap = true;
                    Entry.Options.RequireChap = true;
                    Entry.Options.RequireMSChap2 = true;
                    Entry.Options.RequireEncryptedPassword = false;
                    Entry.Options.RequireDataEncryption = false;
                    Entry.EncryptionType = RasEncryptionType.None;
                    Entry.IPv4InterfaceMetric = 1;
                    Entry.DnsAddress = IPAddress.Parse("10.8.0.1");
                    Entry.Options.RemoteDefaultGateway = false;
                    PhoneBook.Entries.Add(Entry);

                    dialer.EntryName = MyConnection._ConnectionName;
                    dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
                    dialer.Credentials = new NetworkCredential(MyConnection._username, MyConnection._password);
                    dialer.DialCompleted -= Dialer_DialCompleted;
                    dialer.DialCompleted += Dialer_DialCompleted;
                    handle = dialer.DialAsync();
                }
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
            connectionObserver?.ConnectionObserver(false, "● Not Connected");
        }
        #endregion
    }
}
