using DotRas;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.Services
{
    internal sealed class DNSController : IService
    {
        //Fields
        private static DNSController _instance;
        private static IConnectionObserver connectionObserver;
        public string ServiceText => "● Changing DNS...";

        private static readonly string PrimaryDNS = "185.231.182.126";
        private static readonly string SecondaryDNS = "37.152.182.112";
        private static string[] dns = { PrimaryDNS, SecondaryDNS };
        private static string[] googleDns = { "8.8.8.8", "8.8.4.4" };
        private static string[] currentDns;

        private RasDialer _dialer = new RasDialer();
        private RasHandle _handle;

        //Constructor
        private DNSController() => Load();
        public static DNSController GetInstance(IConnectionObserver connectionObserver = null)
        {
            if (DNSController.connectionObserver == null && connectionObserver != null)
                DNSController.connectionObserver = connectionObserver;

            if (_instance == null)
                _instance = new DNSController();

            return _instance;
        }

        #region Private Methods
        private async void Load()
        {
            try
            {
                CheckDNS();
            }
            catch (Exception e)
            {
                //TODO: MainWindow LoadedEvent and this method runs together which shows 2 messageBoxes at the same time if there is no connection. a better way must be provided to solve this issue.
                //ElectroMessageBox.Show("Could not connect to server!", "Error");
                //Application.Current.Shutdown();
                try
                {
                    CheckDNS();
                }
                catch (Exception)
                {
                }
            }
        }
        private async void CheckDNS()
        {
            var data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            if (data == null)
            {
                data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            }
            var objects = JsonConvert.DeserializeObject<Rootobject>(data);
            var dnsAddress = await GetDnsAddress();

            if (dnsAddress != null)
            {
                List<string> electroList = objects.dns.electro.ToList();
                foreach (string dns in electroList)
                {
                    if (dnsAddress.Any(s => s == dns))
                    {
                        return;
                    }
                }
                currentDns = new string[dnsAddress.Count];
                for (int i = 0; i < dnsAddress.Count; i++)
                {
                    currentDns[i] = dnsAddress[i];
                }
            }
        }
        private async Task<List<string>> GetDnsAddress()
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            List<string> addresses = new List<string>();
            IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
            IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
            IPAddressCollection dhcpAddressCollection = ipProperties.DhcpServerAddresses;

            foreach (var dns in dnsAddresses)
            {
                if (dhcpAddressCollection.Any(s => Equals(s, dns)))
                {
                    return null;
                }
            }
            foreach (IPAddress dnsAddress in dnsAddresses)
            {
                addresses.Add(dnsAddress.ToString());
            }
            return addresses;
        }
        private static async Task<NetworkInterface> GetActiveEthernetOrWifiNetworkInterface()
        {
            return await Task.Run(() =>
            {
                var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                     (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                     a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
                return Nic;
            });
        }
        #endregion

        #region public Methods
        public async Task SetDNS(string[] DnsString)
        {
            var CurrentInterface = await GetActiveEthernetOrWifiNetworkInterface();
            if (CurrentInterface == null) return;

            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
                    {
                        ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                        if (objdns != null)
                        {
                            ElectroMessageBox.Show($"{objdns["DNSServerSearchOrder"]}");
                            objdns["DNSServerSearchOrder"] = DnsString;
                            ManagementBaseObject setDNS = objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }

        public async Task GetDataFromServerAndSetDNS()
        {
            var data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            if (data == null)
            {
                data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            }
            var objects = JsonConvert.DeserializeObject<Rootobject>(data);
            await SetDNS1(objects?.dns.electro);
        }

        public async Task SetDNS1(string[] Dns)
        {
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = (object)Dns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }
        }
        public async Task UnsetDNS1()
        {
            if (currentDns == null)
                currentDns = googleDns;
            NetworkInterface networkInterface = await GetActiveEthernetOrWifiNetworkInterface();
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = currentDns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }
        }

        public static void UnsetDnsEvent()
        {
            if (currentDns == null)
                currentDns = googleDns;
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                     (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                     a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
            if (networkInterface == null)
                return;
            foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
            {
                if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
                {
                    ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
                    if (methodParameters != null)
                    {
                        methodParameters["DNSServerSearchOrder"] = currentDns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }

            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
                process.Kill();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await GetDataFromServerAndSetDNS();

                //connectionObserver.ConnectionObserver(true, "● DNS Changed");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async void Dispose()
        {
            await UnsetDNS1();
            //connectionObserver.ConnectionObserver(false, "");
        }
        #endregion

        #region Properties(Getter, Setter)
        public RasDialer Dialer
        {
            get => _dialer;
            set => _dialer = value;
        }
        public RasHandle Handle
        {
            get => _handle;
            set => _handle = value;
        }
        #endregion
    }
}
