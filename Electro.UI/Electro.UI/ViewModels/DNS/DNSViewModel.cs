using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel
    {
        private static string PrimaryDNS = "185.231.182.126";
        private static string SecondaryDNS = "37.152.182.112";
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
        }

        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {
                IsTurnedOn = true;
                
                //try
                //{
                //    //IsGettingData = true;
                //    //await setDNS();
                //    IsTurnedOn = true;
                //}
                //catch (Exception e)
                //{
                //    ElectroMessageBox.Show("امکان برقراری با سرور وجود ندارد");
                //}
            }
            else
            {
                unsetDNS();
                IsTurnedOn = false;
            }
        }
        private async Task setDNS()
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
        }

        private async Task<object> getDnsConfig()
        {
            //string content = await client.GetStringAsync(
            //    $"https://elcdn.ir/app/pc/win/etc/settings.json");
            //return JsonConvert.DeserializeObject<JasonDatas.Root>(content);
            return 1;
        }

        //public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
        //{
        //    var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
        //        a => a.OperationalStatus == OperationalStatus.Up &&
        //             (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
        //             a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

        //    return Nic;
        //}
        //public void SetDNS(string[] DnsString)
        //{
        //    var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
        //    if (CurrentInterface == null) return;

        //    ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //    ManagementObjectCollection objMOC = objMC.GetInstances();

        //    foreach (ManagementObject objMO in objMOC)
        //    {
        //        if ((bool)objMO["IPEnabled"])
        //        {
        //            if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
        //            {
        //                ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
        //                if (objdns != null)
        //                {
        //                    objdns["DNSServerSearchOrder"] = DnsString;
        //                    ManagementBaseObject setDNS = objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
        //                }
        //            }
        //        }
        //    }
        //}
        //public void UnsetDNS()
        //{
        //    var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
        //    if (CurrentInterface == null) return;

        //    ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
        //    ManagementObjectCollection objMOC = objMC.GetInstances();
        //    foreach (ManagementObject objMO in objMOC)
        //    {
        //        if ((bool)objMO["IPEnabled"])
        //        {
        //            if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
        //            {
        //                ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
        //                if (objdns != null)
        //                {
        //                    objdns["DNSServerSearchOrder"] = null;
        //                    objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
