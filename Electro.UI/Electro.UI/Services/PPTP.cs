using DeviceId;
using DotRas;
using Electro.UI.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Services
{
    internal class PPTP : IService
    {
        //Fields
        private DNSController dNSController = DNSController.GetInstance();
        IConnectionObserver connectionObserver;
        public string ServiceText { get => "● Connecting to PPTP..."; }

        //Constructor
        internal PPTP(IConnectionObserver connectionObserver)
            => this.connectionObserver = connectionObserver;

        #region Private Methods
        private async void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (e.TimedOut || e.Error != null)
            {
                connectionObserver.ConnectionObserver(false, "● Error");
                MyLogger.GetInstance().Logger.Error(e.Error);
            }
            else if (e.Connected)
            {
                var routeBatch = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.Route12Bat);
                await DirectoryHelperFunctions.WriteAsync(routeBatch, AppContext.BaseDirectory + "//route12.bat");
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.FileName = @"cmd.exe";
                psi.Verb = "runas";

                psi.Arguments = "/C \"" + AppContext.BaseDirectory + "//route12.bat";

                Process proc = new Process();
                proc.StartInfo = psi;
                proc.Start();
                string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.HardwareID + deviceId);
                connectionObserver.ConnectionObserver(true, "● Connected to PPTP");
            }
        }

        private void StopProcess()
        {
            if (dNSController.Dialer.IsBusy)
            {
                dNSController.Dialer.DialAsyncCancel();
            }
            else
            {
                if (dNSController.Handle != null)
                {
                    RasConnection Connection = RasConnection.GetActiveConnectionByHandle(dNSController.Handle);
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
        }
        #endregion

        #region Public Methods
        public async Task<bool> Connect()
        {
            try
            {
                await dNSController?.GetDataFromServerAndSetDNS();

                var ServerIp = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.PptpIp);
                using (RasPhoneBook PhoneBook = new RasPhoneBook())
                {
                    PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));
                    RasEntry Entry;

                    if (PhoneBook.Entries.Contains(MyConnection._ConnectionName))
                    {
                        PhoneBook.Entries.Remove(MyConnection._ConnectionName);
                    }

                    Entry = RasEntry.CreateVpnEntry(MyConnection._ConnectionName, ServerIp, RasVpnStrategy.PptpOnly, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn));

                    Entry.Options.PreviewDomain = false;
                    Entry.Options.ShowDialingProgress = false;
                    Entry.Options.PromoteAlternates = false;
                    Entry.Options.DoNotNegotiateMultilink = false;
                    Entry.Options.RequirePap = true;
                    Entry.Options.RequireChap = true;
                    Entry.Options.RequireMSChap2 = true;
                    Entry.Options.RequireEncryptedPassword = false;
                    PhoneBook.Entries.Add(Entry);



                    dNSController.Dialer.EntryName = MyConnection._ConnectionName;
                    dNSController.Dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
                    dNSController.Dialer.Credentials = new NetworkCredential(MyConnection._username, MyConnection._password);
                    dNSController.Dialer.DialCompleted -= Dialer_DialCompleted;
                    dNSController.Dialer.DialCompleted += Dialer_DialCompleted;
                    dNSController.Handle = dNSController.Dialer.DialAsync();
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
            await dNSController.UnsetDNS1();
            StopProcess();
            connectionObserver?.ConnectionObserver(false, "");
        }
        #endregion
    }
}
