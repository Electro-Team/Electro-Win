using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private bool _showInTaskbar;
        private WindowState _windowState;
        public MainViewModel()
        {
            dnsViewModel = new DNSViewModel(this);
        }

        public DNSViewModel DnsViewModel => dnsViewModel;

        public RelayCommand NotifyCommand => notifyCommand ??
                                             (notifyCommand = new RelayCommand(Notify));
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
                Title = "Notify",
                Text = "App has been minimized!",
                Duration = 1000
            };
        }
    }
}
