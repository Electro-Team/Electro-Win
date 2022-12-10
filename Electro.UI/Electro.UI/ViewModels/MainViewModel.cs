using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Electro.UI.Tools;
using Electro.UI.ViewModels.DNS;
using Newtonsoft.Json;

namespace Electro.UI.ViewModels
{
    public class MainViewModel : BaseModel
    {
        //Fields
        private readonly DNSViewModel dnsViewModel;
        private NotifyIconWrapper.NotifyRequestRecord _notifyRequest;
        private bool isServiceOn;
        private bool _showInTaskbar;
        private bool isStartup;
        private string sponsorImageUrl;
        private string sponsorLinkUrl;
        private readonly WindowState _windowState;
        private string selectedService = "OpenVpn";
        //private bool formEnable = true;

        ///Commands
        private RelayCommand notifyCommand;
        private RelayCommand elTeamSiteCommand;
        private RelayCommand discordCommand;
        private RelayCommand telegramCommand;
        private RelayCommand instagramCommand;
        private RelayCommand donateCommand;
        private RelayCommand sponsorCommand;
        #region Properties(Getter, Setter)

        #region Commands 
        public RelayCommand NotifyCommand => notifyCommand ??
                                             (notifyCommand = new RelayCommand(Notify));

        public RelayCommand ElTeamSiteCommand => elTeamSiteCommand ??
                                                 (elTeamSiteCommand = new RelayCommand(ElTeamSite));

        public RelayCommand DiscordCommand => discordCommand ??
                                              (discordCommand = new RelayCommand(Discord));

        public RelayCommand TelegramCommand => telegramCommand ??
                                               (telegramCommand = new RelayCommand(Telegram));

        public RelayCommand InstagramCommand => instagramCommand ??
                                                (instagramCommand = new RelayCommand(Instagram));

        public RelayCommand DonateCommand => donateCommand ??
                                             (donateCommand = new RelayCommand(Donate));

        public RelayCommand SponsorCommand => sponsorCommand ??
                                              (sponsorCommand = new RelayCommand(Sponsor));
        #endregion

        public DNSViewModel DnsViewModel => dnsViewModel;
        public bool IsServiceOn
        {
            get => isServiceOn;
            set
            {
                isServiceOn = value;
                OnPropertyChanged();
            }
        }
        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                ShowInTaskbar = true;
                OnPropertyChanged();
                ShowInTaskbar = value != WindowState.Minimized;
            }
        }

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set
            {
                _showInTaskbar = value;
                OnPropertyChanged();
            }
        }

        public string SponsorImageUrl
        {
            get => sponsorImageUrl;
            set
            {
                sponsorImageUrl = value;
                OnPropertyChanged();
            }
        }
        public bool IsStartup
        {
            get => isStartup;
            set
            {
                isStartup = value;
                //if (isStartup)
                //{
                //    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine
                //        .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                //    Assembly curAssembly = Assembly.GetExecutingAssembly();
                //    key?.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                //}
                //else
                //{
                //    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine
                //        .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                //    Assembly curAssembly = Assembly.GetExecutingAssembly();
                //    key?.DeleteValue(curAssembly.GetName().Name);
                //}

                OnPropertyChanged();
            }
        }
        public NotifyIconWrapper.NotifyRequestRecord NotifyRequest
        {
            get => _notifyRequest;
            set
            {
                _notifyRequest = value;
                OnPropertyChanged();
            }
        }

        public string SelectedService
        {
            get => selectedService;
            set
            {
                if (selectedService != value)
                {
                    dnsViewModel.ChangeModel(value);
                    selectedService = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<string> ServicesCombo
                => new string[] { "OpenVpn", "PPTP", "DNS Changer" };

        //public bool FormEnable
        //{
        //    get => formEnable;
        //    set
        //    {
        //        formEnable = value;
        //        OnPropertyChanged();
        //    }
        //}

        #endregion
        //Constructor
        public MainViewModel(DNSViewModel dnsViewModel)
        {
            this.dnsViewModel = dnsViewModel;
            dnsViewModel.ServiceUpdated += ServiceUpdated;
            //dnsViewModel.FreezeForm += FreezeForm;
            _ = GetSponsorInfo();
        }

        #region Private Methods

        #region Command Handler
        private void Notify(object obj)
        {
            if (DnsViewModel.IsTurnedOn)
            {
                NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
                {
                    Title = "Electro",
                    Text = "Electro is still running!",
                    Duration = 1000
                };
            }
        }
        private void ServiceUpdated(bool isTurnedOn)
        {
            string description;
            if (isTurnedOn)
            {
                description = "Electro service turned on.";
            }
            else
            {
                description = "Electro service turned off.";
            }
            NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
            {
                Title = "Electro",
                Text = description,
                Duration = 1000
            };
        }
        //private void FreezeForm(bool freezeForm)
        //    => this.FormEnable = !freezeForm;

        private void ElTeamSite(object obj) => Process.Start("http://www.Electrotm.org");
        private void Discord(object obj) => Process.Start("https://discord.io/elteam");
        private void Telegram(object obj) => Process.Start("https://t.me/elteam_IR");
        private void Instagram(object obj) => Process.Start("https://www.instagram.com/irelectro/");
        private void Donate(object obj) => Process.Start("https://donateon.ir/MaxisAmir");
        private void Sponsor(object obj) => Process.Start(sponsorLinkUrl);
        #endregion

        private async Task GetSponsorInfo()
        {
            try
            {
                var data = await MyHttpClient.GetInstance().Client.GetStringAsync(MyUrls.SettingsJson2);
                if (data != null)
                {
                    var sponsorJsonData = JsonConvert.DeserializeObject<SponsorJsonData>(data);
                    SponsorImageUrl = sponsorJsonData.adPictureUrl;
                    sponsorLinkUrl = sponsorJsonData.adLinkUrl;
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                //logger must add
            }
        }
        #endregion

        #region Public Methods

        #endregion
    }
}
