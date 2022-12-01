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
using DotRas;
using System.Runtime.InteropServices;
using DeviceId;
using NLog;
using System.Runtime.Remoting.Services;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static string PrimaryDNS = "78.157.42.100";
        private static string SecondaryDNS = "78.157.42.101";
        private static string[] dns = {PrimaryDNS, SecondaryDNS};
        private string serviceText;
        private bool configObtained;
        private bool isGettingData;
        private bool isTurnedOn;
        private RelayCommand configureDnsCommand;
        private HttpClient client = new HttpClient();
        private static string[] currentDns;
        private static bool _isOpenVpn = false;

        private RasDialer _dialer = new RasDialer();
        private RasHandle _handle;
        private string _ConnectionName = "ElectroService";
        private string _username = "Electro";
        private string _password = "ElectroPW";
        public DNSViewModel()
        {
            serviceText = "● Not Connected";
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
                var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json");
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
        private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            try
            {
                string output = outLine.Data;
                logger.Info(output);

                if (output != null)
                {
                    serviceText = output;

                    if (output.Contains("Initialization Sequence Completed"))
                    {
                        IsGettingData = false;

                        IsTurnedOn = true;
                        _isOpenVpn = true;
                        ServiceText = "● Connected";
                        string newTempDir = getTemporaryDirectory();
                      //  string pathToRouteBatch = newTempDir + "//route12.bat";
                     //   var routeBatch = await client.GetStringAsync("https://elcdn.ir/app/vpn/routes/route12.bat");
                     //   await WriteAsync(routeBatch, pathToRouteBatch);
                     //   ProcessStartInfo psi = new ProcessStartInfo();
                      //  psi.CreateNoWindow = true;
                      //  psi.UseShellExecute = false;
                      //  psi.FileName = @"cmd.exe";
                    //    psi.Verb = "runas";

                    //    psi.Arguments = "/C \"" + pathToRouteBatch;

                       // Process proc = new Process();
                       // proc.StartInfo = psi;

                      //  proc.Start();
                        string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                        await client.GetStringAsync("http://10.202.7.211:8080/newConnection/hardwareID/PC" + deviceId);
                        ServiceUpdated?.Invoke(IsTurnedOn);
                    }
                  
                    else if(output.ToLower().Contains("error") || output.Contains("Could not determine IPv4/IPv6 protocol"))
                    {

                        var ServerIp = await client.GetStringAsync("http://elcdn.ir/app/vpn/pptpip.txt");
                        if (ServerIp == null)
                        {
                            ServerIp = await client.GetStringAsync("http://elcdn.ir/app/vpn/pptpip.txt");
                        }

                        using (RasPhoneBook PhoneBook = new RasPhoneBook())
                        {

                            PhoneBook.Open(AppContext.BaseDirectory + @"\OpenVPN\Driver\Electro.pbk");
                            RasEntry Entry;
                            if (PhoneBook.Entries.Contains(_ConnectionName))
                            {
                                PhoneBook.Entries.Remove(_ConnectionName);
                            }
                            Entry = RasEntry.CreateVpnEntry(_ConnectionName, ServerIp, RasVpnStrategy.PptpOnly, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn)); ;

                            // Entry.Options.PreviewDomain = false;
                            //   Entry.Options.ShowDialingProgress = false;
                            //  Entry.Options.PromoteAlternates = false;
                            // / Entry.Options.DoNotNegotiateMultilink = false;
                            //  / Entry.Options.RequireChap = true;
                            //    Entry.Options.RequireMSChap2 = true;
                            //     Entry.Options.RequireEncryptedPassword = false;
                            //   Entry.Options.RequirePap = true;
                            //  Entry.Options.CustomEncryption = false;
                            //  PhoneBook.Entries.Add(Entry);
                            Entry.Options.RequireDataEncryption = false;
                            Entry.Options.RemoteDefaultGateway = false;
                            Entry.Options.RequireEncryptedPassword = false;
                            PhoneBook.Entries.Add(Entry);

                            _dialer.EntryName = _ConnectionName;
                            _dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers);
                            _dialer.Credentials = new NetworkCredential(_username, _password);


                            _dialer.PhoneBookPath = AppContext.BaseDirectory + @"\OpenVPN\Driver\Electro.pbk";
                            _dialer.DialCompleted -= Dialer_DialCompleted;

                            _dialer.DialCompleted += Dialer_DialCompleted;
                            _handle = _dialer.DialAsync();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }
        public async Task WriteAsync(string data, string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                await sw.WriteAsync(data);
            }
        }
        private async void Dialer_DialCompleted(object sender, DialCompletedEventArgs e)
        {
            if (e.TimedOut || e.Error != null)
            {
                logger.Error(e.Error);
                IsGettingData = false;
                IsTurnedOn = false;

                ServiceText = "● Error";
            }
            else if (e.Connected)
            {
                IsGettingData = false;
                IsTurnedOn = true;
                ServiceText = "● Connected";
                string newTempDir = getTemporaryDirectory();
                  string pathToRouteBatch = newTempDir + "//route12.bat";
                var routeBatch = await client.GetStringAsync("http://elcdn.ir/app/vpn/routes/route12.bat");
                await WriteAsync(routeBatch, pathToRouteBatch );
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.FileName = @"cmd.exe";
                psi.Verb = "runas";

                    psi.Arguments = "/C \"" + pathToRouteBatch ;
              
                Process proc = new Process();
                proc.StartInfo = psi;
                proc.Start();
                string deviceId = new DeviceIdBuilder().AddMacAddress().AddUserName().ToString();
                await client.GetStringAsync("http://10.202.7.211:8080/newConnection/hardwareID/PC" + deviceId);
                ServiceUpdated?.Invoke(IsTurnedOn);
            }

            _isOpenVpn = false;
        }
        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {
                try
                {
                    IsGettingData = true;
                    ServiceText = "● Connecting...";
                    var openVpnProcess = Process.GetProcesses().
                        Where(pr => pr.ProcessName == "openvpn");

                    foreach (var process in openVpnProcess)
                    {
                        process.Kill();
                    }

                    
                    IsGettingData = true;
                    ServiceText = "● Connecting...";
                    var data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json");
                    if (data == null)
                    {
                        data = await client.GetStringAsync("http://elcdn.ir/app/pc/win/etc/settings.json");
                    }
                    var objects = JsonConvert.DeserializeObject<Rootobject>(data);
                    await SetDNS1(objects?.dns.electro);
                    
                    string newTempDir = getTemporaryDirectory();
                    string pathToConfig = newTempDir + "//profile.ovpn";
                    string pathToConfigcfg = "C://Windows" + "//user.cfg";
                    var openVpnProfile = await client.GetStringAsync("http://elcdn.ir/app/vpn/profile.ovpn");
                    var openVpnProfileconf = await client.GetStringAsync("http://elcdn.ir/app/vpn/user.cfg");
                    await WriteAsync(openVpnProfile, pathToConfig);
                    await WriteAsync(openVpnProfileconf, pathToConfigcfg);
                    string arguments = "--config \"" + pathToConfig + "\" --block-outside-dns";
                    ProcessStartInfo openVpnStartInfo = new ProcessStartInfo
                    {
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        FileName = AppContext.BaseDirectory + "/Openvpn/openvpn.exe",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                    };
                    Process openVpn = new Process
                    {
                        StartInfo = openVpnStartInfo,
                        EnableRaisingEvents = true
                    };
                    
                    openVpn.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                    openVpn.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                    openVpn.Start();
                    openVpn.BeginOutputReadLine();
                    openVpn.BeginErrorReadLine();
                    
                    _isOpenVpn = false;

                }
                catch (Exception e)
                {
                    logger.Error(e);
                   // ElectroMessageBox.Show("Connection can not be established.");

                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceText = "● Error";
                }
                finally
                {
                }
            }
            else
            {
                await UnsetDNS1();
                UnsetDNS();
                IsTurnedOn = false;
                ServiceText = "● Not Connected";
            }
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
        public void UnsetDNS()
        {
            if (!_isOpenVpn)
            {
                if (_dialer.IsBusy)
                {
                    _dialer.DialAsyncCancel();
                }
                else
                {
                    if (_handle != null)
                    {
                        RasConnection Connection = RasConnection.GetActiveConnectionByHandle(_handle);
                        if (Connection != null)
                        {
                            Connection.HangUp();
                        }
                    }
                }

                using (RasPhoneBook PhoneBook = new RasPhoneBook())
                {
                    PhoneBook.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.AllUsers));

                    if (PhoneBook.Entries.Contains(_ConnectionName))
                    {
                        PhoneBook.Entries.Remove(_ConnectionName);
                    }
                }
            }
            else
            {
                var openVpnProcess = Process.GetProcesses().
                    Where(pr => pr.ProcessName == "openvpn");

                foreach (var process in openVpnProcess)
                {
                    process.Kill();
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
                        methodParameters["DNSServerSearchOrder"] = (object)dns;
                        instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
                    }
               }
            }
        }
        public  void UnsetDnsEvent()
        {
        //    var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
        //        a => a.OperationalStatus == OperationalStatus.Up &&
       //              (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
       //              a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));
      //      if (networkInterface == null)
       //         return;
      //      foreach (ManagementObject instance in new ManagementClass("Win32_NetworkAdapterConfiguration").GetInstances())
      //      {
       //         if ((bool)instance["IPEnabled"] && instance["Description"].ToString().Equals(networkInterface.Description))
     //           {
      //              ManagementBaseObject methodParameters = instance.GetMethodParameters("SetDNSServerSearchOrder");
     //               if (methodParameters != null)
     //               {
     //                   methodParameters["DNSServerSearchOrder"] = currentDns;
    //                    instance.InvokeMethod("SetDNSServerSearchOrder", methodParameters, (InvokeMethodOptions)null);
    //                }
   //             }
      //      }

            var openVpnProcess = Process.GetProcesses().
                Where(pr => pr.ProcessName == "openvpn");

            foreach (var process in openVpnProcess)
            {
                process.Kill();
            }
            
        }
        private string getTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
