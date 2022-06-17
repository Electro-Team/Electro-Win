using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel
    {
        private static string PrimaryDNS = "185.231.182.126";
        private static string SecondaryDNS = "37.152.182.112";
        private static string[] dns = {PrimaryDNS, SecondaryDNS};
        private string serviceText;
        private bool configObtained;
        private bool isGettingData;
        private bool isTurnedOn;
        private RelayCommand configureDnsCommand;
        private HttpClient client = new HttpClient();
        private static string[] currentDns;


        public DNSViewModel()
        {
            serviceText = "Service Off";
            load();
        }

        public Action<bool> ServiceUpdated;
        public bool ConfigObtained
        {
            get => configObtained;
            set
            {
                configObtained = value;
                OnPropertyChanged();
            }
        }

        public bool IsGettingData
        {
            get => isGettingData;
            set
            {
                isGettingData = value;
                OnPropertyChanged();
            }
        }

        public bool IsTurnedOn
        {
            get => isTurnedOn;
            set
            {
                isTurnedOn = value;
                OnPropertyChanged();
            }
        }

        public string ServiceText
        {
            get
            {
                return serviceText;
            }
            set
            {
                serviceText = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand ConfigureDnsCommand => configureDnsCommand ??
                                             (configureDnsCommand = new RelayCommand(configureDns));

        private async void load()
        {
            try
            {
                var data = await client.GetStringAsync("https://elcdn.ir/app/pc/win/etc/settings.json");
                if (data==null)
                 {
                   data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json"); 
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
            catch (Exception e)
            {
                //TODO: MainWindow LoadedEvent and this method runs together which shows 2 messageBoxes at the same time if there is no connection. a better way must be provided to solve this issue.
                //ElectroMessageBox.Show("Could not connect to server!", "Error");
                //Application.Current.Shutdown();
                try
                {
                    var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json");
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
                catch (Exception)
                {
                }
            }
        }

        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {
                try
                {
                    IsGettingData = true;
                    var data = await client.GetStringAsync("https://elcdn.ir/app/pc/win/etc/settings.json");
                    if (data==null)
                     {
                         data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json"); 
                     }
                    var objects = JsonConvert.DeserializeObject<Rootobject>(data);
                    await SetDNS1(objects?.dns.electro);
                    IsTurnedOn = true;
                    ServiceText = "Service On";
                }
                catch (Exception e)
                {
                    try
                    {
                        var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json");
                        var objects = JsonConvert.DeserializeObject<Rootobject>(data);
                        await SetDNS1(objects?.dns.electro);
                        IsTurnedOn = true;
                        ServiceText = "Service On";
                    }
                    catch (Exception exception)
                    {
                        ElectroMessageBox.Show("Error Getting Server Data!");
                    }
                }
                finally
                {
                    IsGettingData = false;
                }
            }
            else
            {
                await UnsetDNS1();
                IsTurnedOn = false;
                ServiceText = "Service Off";
            }
            ServiceUpdated?.Invoke(IsTurnedOn);
        }
        public static async Task<NetworkInterface> GetActiveEthernetOrWifiNetworkInterface()
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
        public async Task UnsetDNS()
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
                            objdns["DNSServerSearchOrder"] = currentDns?.ToArray();
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
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
        }
    }
}
