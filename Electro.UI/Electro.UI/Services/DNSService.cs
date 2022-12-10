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
using Electro.UI.Interfaces;

namespace Electro.UI.Services
{
    public class DNSService : IDNSService
    {
        //Fields
        private static readonly string PrimaryDNS = "78.157.42.100";
        private static readonly string SecondaryDNS = "78.157.42.101";
        private static string[] dns = { PrimaryDNS, SecondaryDNS };
        private static string[] currentDns;

        #region Properties(Getter, Setter)

        #endregion

        //Constructor
        public DNSService() => Load();
        #region Private Methods
        private async void Load()
        {
            try
            {
                await CheckDNS();
            }
            catch (Exception e)
            {
                MyLogger.GetInstance().Logger.Error(e);
            }
        }
        private async Task CheckDNS()
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
            if (networkInterface == null)
            {
                ElectroMessageBox.Show("Turn Off other VPN");
            }
            else
            {
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
        private async Task SetDNS(string[] Dns)
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
        #endregion

        #region public Methods
        public async Task GetDataFromServerAndSetDNS()
        {
            var data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            if (data == null)
            {
                data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson);
            }
            var objects = JsonConvert.DeserializeObject<Rootobject>(data);
            dns = objects?.dns.electro;
            await SetDNS(objects?.dns.electro);
        }

        public async Task UnsetDNS()
        {
            if (currentDns == null)
                currentDns = dns;
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
                        methodParameters["DNSServerSearchOrder"] = dns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
                }
            }
        }

        public static void UnsetDnsEvent()
        {
            if (currentDns == null)
                currentDns = dns;
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
                        methodParameters["DNSServerSearchOrder"] = dns;
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

                ElectroMessageBox.Show("Electro DNS set.");
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
            await UnsetDNS();
            ElectroMessageBox.Show("Electro DNS unset.");
        }
        #endregion
    }
}
