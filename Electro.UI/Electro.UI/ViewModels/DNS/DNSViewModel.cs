using System;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Electro.UI.Services;
using System.Windows.Media;
using Electro.UI.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Electro.UI.ViewModels.DNS
{
    public class DNSViewModel : BaseModel, IConnectionObserver
    {
        //Fields
        private string serviceText;
        private bool configObtained;
        private bool isGettingData;
        private bool isTurnedOn;
        private bool isOpenVpn;
        private bool canChangeServiceType = true;

        private IService service = new Softether();
        private IDNSService dnsService;
        private IServiceProvider serviceProvider;

        ///Commands
        private RelayCommand configureDnsCommand;
        private RelayCommand setDnsCommand;

        public Action<bool> ServiceUpdated;
        //public Action<bool> FreezeForm;
        private bool isEnableToChangeService = true;

        #region Properties(Getter, Setter)

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
                                                   (configureDnsCommand = new RelayCommand(Connect));

        public RelayCommand SetDnsCommand => setDnsCommand ??
                                             (setDnsCommand = new RelayCommand(SetDns));

        public bool IsOpenVpn
        {
            get => isOpenVpn;
            set
            {
                if (value)
                    ChangeModel("OpenVpn");
                else
                    ChangeModel("PPTP");

                isOpenVpn = value;
                OnPropertyChanged();
            }
        }
        public bool IsEnableToChangeService
        {
            get => isEnableToChangeService;
            set
            {
                isEnableToChangeService = value;
                //FreezeForm?.Invoke(value);
                OnPropertyChanged();
            }

        }
        #endregion

        //Constructor
        public DNSViewModel(IDNSService dnsService,
            IServiceProvider serviceProvider)
        {
            serviceText = "● Not Connected";
            this.dnsService = dnsService;
            this.serviceProvider = serviceProvider;
        }

        #region Private Methods

        //Set DNS and start to connect to service.
        private async void Connect(object obj)
        {
            if (IsGettingData == true && IsTurnedOn == false)
            {
                service.Dispose();
                ServiceText = "● Not Connected";
                IsTurnedOn = false;
                IsGettingData = false;
                IsEnableToChangeService = true;
            }
            else if (IsTurnedOn == false)
            {
                //IsOpenVpn = IsOpenVpn;
                OnPropertyChanged("IsOpenVpn");
                IsGettingData = true;
                ServiceText = service.ServiceText;
                IsEnableToChangeService = false;
                bool result = await service.Connect();

                if (!result)
                {
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceText = "● Error";
                }

            }
            else
            {
                service.Dispose();
                ServiceText = "● Not Connected";
                IsTurnedOn = false;
                IsEnableToChangeService = true;
            }
        }

        private async void SetDns(object obj)
        {
            await dnsService.Connect();
        }
        #endregion

        #region Public Methods
        public static void UnsetDnsEvent()
           => DNSService.UnsetDnsEvent();

        public void ChangeModel(string service)
        {
            switch (service)
            {
                case "OpenVpn":
                    this.service = serviceProvider.GetRequiredService<OpenVPN>();
                    break;
                case "PPTP":
                    //this.service = serviceProvider.GetRequiredService<PPTP>();
                    this.service = serviceProvider.GetRequiredService<Softether>();
                    break;
                case "Softether":
                    this.service = serviceProvider.GetRequiredService<Softether>();
                    break;
                default:
                    this.service = serviceProvider.GetRequiredService<PPTP>();
                    break;
            }
        }

        public void ConnectionObserver(bool? SuccessfullyCoonected, string serviceText)
        {
            if (SuccessfullyCoonected != null)
            {
                if (SuccessfullyCoonected ?? false)
                {
                    IsGettingData = false;
                    IsTurnedOn = true;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(true);
                    IsEnableToChangeService = false;
                }
                else
                {
                    IsGettingData = false;
                    IsTurnedOn = false;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(false);
                    IsEnableToChangeService = true;
                }

            }
            else
            {
                this.ServiceText = serviceText;
            }
        }
        #endregion
    }
}
