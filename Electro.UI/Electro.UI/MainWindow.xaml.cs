using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Electro.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string WifiIP;
        string WifiDns;
        string WifiName;
        public MainWindow()
        {
            InitializeComponent();
            WifiInf(out WifiIP, out WifiDns, out WifiName);
        }
        static IList<IPAddress> GetCurrentDnsServersForActiveInterfaces()
        {
            List<IPAddress> returnAddresses = new List<IPAddress>();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var nic in nics)
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties ipProperties = nic.GetIPProperties();
                    IPAddressCollection addresses = ipProperties.DnsAddresses;

                    foreach (IPAddress address in addresses)
                    {
                        returnAddresses.Add(address);
                    }
                }
            }

            return returnAddresses;
        }
        private static void WifiInf(out string ip, out string dns, out string nic)  // To get current wifi config
        {
            ip = "";
            dns = "";
            nic = "";
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
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


        private void SetIP(string arg)
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


        private void btn_setIP_Click(object sender, EventArgs e)
        {
            

            SetIP($"netsh interface ipv4 add dnsserver {WifiName} 8.8.8.8 index=1 & netsh interface ipv4 add dnsserver {WifiName} 8.8.4.4 index=2");
        }

    }
}
