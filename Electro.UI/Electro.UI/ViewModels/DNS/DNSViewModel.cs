using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel
    {
        private static string[] dnses;
        private static string[] before_electro_dns;
        HashSet<string> before_electro_dns_temp = new HashSet<string>();
        private static string version = "0.0.0.1";
        private static string PrimaryDNS = "185.231.182.126";
        private static string SecondaryDNS = "37.152.182.112";
        private static string[] dns = {PrimaryDNS, SecondaryDNS};
        private string name;
        private string IP;
        private string DNS;
        private bool configObtained;
        private bool isGettingData;
        private bool isTurnedOn;
        private RelayCommand configureDnsCommand;
        private MainViewModel _mainViewModel;
        private Timer timer = new Timer(3000);
        private HttpClient client = new HttpClient();

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
        public RelayCommand ConfigureDnsCommand => configureDnsCommand ??
                                             (configureDnsCommand = new RelayCommand(configureDns));

        public DNSViewModel(MainViewModel mainViewModel)
        {
            
            this._mainViewModel = mainViewModel;
            networkInfos(out IP, out DNS, out name);
            ConfigObtained = true;
        }

        private void networkInfos(out string ip, out string dns, out string nic)  // To get current wifi config
        {

            ip = "";
            dns = "";
            nic = "";
            var s = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    { 
                        nic = ni.Name;
                    }
                }
            }

        }
       /*
        private void runArgumentInCMD(string arg)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.Verb = "runas";
                psi.Arguments = arg;
                Process.Start(psi);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }*/

        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {                
                try
                {
                    IsGettingData = true;
                    dnses = GetDnsAdresses(true, false).Select(ip => ip.ToString()).ToArray();
                    int tmp_counter = 0;
                    if (dnses.Length > 0)
                    {
                        foreach (string dns in dnses)
                        {
                            if (tmp_counter > 1)
                            {
                                ElectroMessageBox.Show(dns);
                                before_electro_dns_temp.Add(dns);
                            }
                            tmp_counter += 1;
                        }
                    }
                    before_electro_dns = before_electro_dns_temp.ToArray();
                    await SetDNS(dns);
                    IsTurnedOn = true;
                }
                catch (Exception e)
                {
                    ElectroMessageBox.Show(e.ToString());
                }
            }
            else
            {
                if (before_electro_dns.Length != 0)
                {
                    await SetDNS(before_electro_dns);
                }
                else
                {
                    await UnsetDNS();
                }
                   IsTurnedOn = false;
            }
        }
        /*private async Task setDNS()
        {
            ConfigObtained = false;
            var config = await getDnsConfig();
            if (config != null)
            {
                runArgumentInCMD($"/c netsh interface ipv4 add dnsserver {name} {(config as JasonDatas.Root)?.dns.electro[0].DNS1} index=1 & netsh interface ipv4 add dnsserver {name} {(config as JasonDatas.Root)?.dns.electro[1].DNS2} index=2");
                _mainViewModel.IsServiceOn = true;
            }
            ConfigObtained = true;
        }
        private void unsetDNS()
        {
            runArgumentInCMD($"/c netsh interface ipv4 set dnsservers {name} dhcp");
            _mainViewModel.IsServiceOn = false;
        }*/

        private async Task<object> getDnsConfig()
        {
            //string content = await client.GetStringAsync(
            //    $\"https://elcdn.ir/app/pc/win/etc/settings.json");
            //return JsonConvert.DeserializeObject<JasonDatas.Root>(content);
            return 1;
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
        public static IPAddress[] GetDnsAdresses(bool ip4Wanted, bool ip6Wanted)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            HashSet<IPAddress> dnsAddresses = new HashSet<IPAddress>();

            foreach (NetworkInterface networkInterface in interfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                    foreach (IPAddress forAddress in ipProperties.DnsAddresses)
                    {
                        if ((ip4Wanted && forAddress.AddressFamily == AddressFamily.InterNetwork) || (ip6Wanted && forAddress.AddressFamily == AddressFamily.InterNetworkV6))
                        {
                            dnsAddresses.Add(forAddress);
                        }
                    }
                }
            }

            return dnsAddresses.ToArray();
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
    }
}
