using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Electro.UI.Tools;
using Electro.UI.ViewModels.DNS;

namespace Electro.UI.ViewModels
{
    public class MainViewModel : BaseModel
    {
        private DNSViewModel dnsViewModel;
        private bool isServiceOn;
        private NotifyIconWrapper.NotifyRequestRecord _notifyRequest;
        private RelayCommand notifyCommand;
        private RelayCommand elTeamSiteCommand;
        private RelayCommand discordCommand;
        private RelayCommand telegramCommand;
        private RelayCommand instagramCommand;
        private RelayCommand donateCommand;
        private bool _showInTaskbar;
        private bool isStartup;
        private WindowState _windowState;
        public MainViewModel()
        {
            
            dnsViewModel = new DNSViewModel();
            dnsViewModel.ServiceUpdated += ServiceUpdated;
            //Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine
            //    .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //Assembly curAssembly = Assembly.GetExecutingAssembly();
            //var appValue = key?.GetValue(curAssembly.GetName().Name);
            //if (appValue != null)
            //{
            //    isStartup = true;
            //}
        }

        public DNSViewModel DnsViewModel => dnsViewModel;

        public RelayCommand NotifyCommand => notifyCommand ??
                                             (notifyCommand = new RelayCommand(Notify));

        public RelayCommand ElTeamSiteCommand => elTeamSiteCommand ??
                                                 (elTeamSiteCommand = new RelayCommand(elTeamSite));

        public RelayCommand DiscordCommand => discordCommand ??
                                              (discordCommand = new RelayCommand(discord));

        public RelayCommand TelegramCommand => telegramCommand ??
                                               (telegramCommand = new RelayCommand(telegram));

        public RelayCommand InstagramCommand => instagramCommand ??
                                                (instagramCommand = new RelayCommand(instagram));

        public RelayCommand DonateCommand => donateCommand ??
                                             (donateCommand = new RelayCommand(donate));
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

        public bool IsStartup
        {
            get => isStartup;
            set
            {
                isStartup = value;
                if (isStartup)
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine
                        .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    key?.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                }
                else
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine
                        .OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    key?.DeleteValue(curAssembly.GetName().Name);
                }

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

        private void Notify(object obj)
        {
            NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
            {
                Title = "Electro",
                Text = "Electro has been minimized!",
                Duration = 1000
            };
        }
        private void ServiceUpdated(bool isTurnedOn)
        {
            string description;
            if (isTurnedOn)
            {
                description = "DNS service turned on.";
            }
            else
            {
                description = "DNS service turned off.";
            }
            NotifyRequest = new NotifyIconWrapper.NotifyRequestRecord
            {
                Title = "Electro",
                Text = description,
                Duration = 1000
            };
        }
        private void elTeamSite(object obj) => Process.Start("http://www.elteam.ir");
        private void discord(object obj) => Process.Start("https://discord.io/elteam");
        private void telegram(object obj) => Process.Start("https://t.me/elteam_IR");
        private void instagram(object obj) => Process.Start("https://www.instagram.com/irelectro/");
        private void donate(object obj) => Process.Start("https://donateon.ir/MaxisAmir");
        
    }
}
