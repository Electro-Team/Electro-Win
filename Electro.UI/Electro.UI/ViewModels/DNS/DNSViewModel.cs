using System;
using Electro.UI.Tools;
using Electro.UI.Windows;
using Electro.UI.Services;
using System.Windows.Media;

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
        private bool isDnsChanged = false;
        private bool canChangeServiceType = true;

        private IService service;

        private DNSController dNSController;
        // Anti-pattern must remove.
        private Brush setDnsTextColor = Brushes.White;

        ///Commands
        private RelayCommand configureDnsCommand;
        private RelayCommand setDnsCommand;

        public Action<bool> ServiceUpdated;
        //public Action<bool> FreezeForm;
        private bool isEnableToChangeService = true;

        //Constructor
        public DNSViewModel()
        {
            serviceText = "● Not Connected";
            dNSController = DNSController.GetInstance();
            service = new PPTP(this);
        }

        #region Private Methods

        //Set DNS and start to connect to service.
        private async void Connect(object obj)
        {
            if (IsTurnedOn == false)
            {
                IsOpenVpn = IsOpenVpn;
                IsGettingData = true;
                ServiceText = service.ServiceText;
                IsEnableToChangeService = false;
                bool result = await service.Connect();
                isDnsChanged = true;

                if (!result)
                {
                    ElectroMessageBox.Show("Connection can not be established.");
                    IsGettingData = false;
                    IsTurnedOn = false;
                    CanChangeServiceType = true;
                    ServiceText = "● Error";
                }

            }
            else
            {
                service.Dispose();
                ServiceText = "● Not Connected";
                IsTurnedOn = false;
                IsEnableToChangeService = true;
                isDnsChanged = false;
            }
            SetDnsTextColor = isDnsChanged ? Brushes.Green : Brushes.White;
        }

        private async void SetDns(object obj)
        {
            isDnsChanged = !isDnsChanged;
            if (isDnsChanged)
            {
                await dNSController.Connect();
                isDnsChanged = true;
                if (!result)
                {
                    ElectroMessageBox.Show("Connection can not be established.");
                    isDnsChanged = false;
                }
            }
            else
            {
                dNSController.Dispose();
                isDnsChanged = false;
            }
            SetDnsTextColor = isDnsChanged ? Brushes.Green : Brushes.White;
        }
        #endregion

        #region Public Methods
        public static void UnsetDnsEvent()
           => DNSController.UnsetDnsEvent();

        public void ChangeModel(string service)
        {
            switch (service)
            {
                case "OpenVpn":
                    this.service = new OpenVPN(this);
                    break;
                case "PPTP":
                    this.service = new PPTP(this);
                    break;
                case "DNS Changer":
                    this.service = DNSController.GetInstance(this);
                    break;
                default:
                    this.service = new OpenVPN(this);
                    break;
            }
        }

        public void ConnectionObserver(bool? SuccessfullyCoonected, string serviceText)
        {
            if (SuccessfullyCoonected != null)
            {
                if (SuccessfullyCoonected ?? false)
                {
                    CanChangeServiceType = false;
                    IsGettingData = false;
                    IsTurnedOn = true;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(true);
                    IsEnableToChangeService = false;
                }
                else
                {
                    CanChangeServiceType = true;
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceUpdated?.Invoke(false);
                    IsEnableToChangeService = true;
                }

            }
            else
            {
                this.ServiceText = serviceText;
                CanChangeServiceType = true;
            }
        }
        #endregion

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

        public Brush SetDnsTextColor
        {
            get => setDnsTextColor;
            set
            {
                setDnsTextColor = value;
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
    }
}
