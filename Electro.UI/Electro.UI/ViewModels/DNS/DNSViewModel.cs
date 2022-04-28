﻿using System;
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

        public DNSViewModel()
        {
            serviceText = "Service Off";
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

        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {
                try
                {
                    IsGettingData = true;
                    var data = await client.GetStringAsync("https://elcdn.ir/app/pc/win/etc/settings.json");
                    var objects = JsonConvert.DeserializeObject<Rootobject>(data);
                    await SetDNS(objects?.dns.electro);
                    IsTurnedOn = true;
                    ServiceText = "Service On";
                }
                catch (Exception e)
                {
                    ElectroMessageBox.Show("Error Getting Server Data!");
                }
                finally
                {
                    IsGettingData = false;
                }
            }
            else
            {
                await UnsetDNS();
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
                            objdns["DNSServerSearchOrder"] = null;
                            objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                        }
                    }
                }
            }
        }
        private static List<string> GetDnsAdress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<string> addresses = new List<string>();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;

                    foreach (IPAddress dnsAdress in dnsAddresses)
                    {
                        addresses.Add(dnsAdress.ToString());
                    }
                }
            }

            return addresses;
        }
    }
}
