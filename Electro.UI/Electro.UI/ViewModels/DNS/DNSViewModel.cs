using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.Tools;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel
    {
        private static string PrimaryDNS = "185.231.182.126";
        private static string SecondaryDNS = "37.152.182.112";
        private string Name;
        private string IP;
        private string DNS;
        private RelayCommand setDnsCommand;
        private RelayCommand unsetDnsCommand;
        private MainViewModel _mainViewModel;

        public RelayCommand SetDnsCommand => setDnsCommand ??
                                             (setDnsCommand = new RelayCommand(setDNS));

        public RelayCommand UnsetDnsCommand => unsetDnsCommand ??
                                               (unsetDnsCommand = new RelayCommand(unsetDNS));

        public DNSViewModel(MainViewModel mainViewModel)
        {
            this._mainViewModel = mainViewModel;
            networkInfos(out IP, out DNS, out Name);
        }

        private void networkInfos(out string ip, out string dns, out string nic)  // To get current wifi config
        {
            ip = "";
            dns = "";
            nic = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {

                    foreach (IPAddress dnsAdress in ni.GetIPProperties().DnsAddresses)
                    {
                        if (dnsAdress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            dns = dnsAdress.ToString();
                        }
                    }


                    foreach (UnicastIPAddressInformation ips in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ips.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ips.Address.ToString().StartsWith("169")) //to exclude automatic ips
                        {
                            ip = ips.Address.ToString();
                            nic = ni.Name;
                        }
                    }
                }
            }

        }


        private void runArgumentInCMD(string arg)
        {
            try
            {

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");
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

        private void setDNS(object obj)
        {
            runArgumentInCMD($"/c netsh interface ipv4 add dnsserver {Name} {PrimaryDNS} index=1 & netsh interface ipv4 add dnsserver {Name} {SecondaryDNS} index=2");
            _mainViewModel.IsServiceOn = true;
        }
        private void unsetDNS(object obj)
        {
            runArgumentInCMD($"/c netsh interface ipv4 set dnsservers {Name} dhcp");
            _mainViewModel.IsServiceOn = false;
        }
    }
}
