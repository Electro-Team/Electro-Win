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

        private IService service;

        private DNSController dNSController;
        private Brush setDnsTextColor = Brushes.White;

        ///Commands
        private RelayCommand configureDnsCommand;
        private RelayCommand setDnsCommand;

        public Action<bool> ServiceUpdated;
        public Action<bool> IsComboBoxEnabled;

        //Constructor
        public DNSViewModel()
        {
            serviceText = "● Not Connected";
            dNSController = DNSController.GetInstance();
            service = new OpenVPN(this);
        }

        #region Private Methods

        //Set DNS and start to connect to service.
        private async void configureDns(object obj)
        {
            if (IsTurnedOn == false)
            {
                IsGettingData = true;
                ServiceText = service.ServiceText;
                IsComboBoxEnabled?.Invoke(false);
                bool result = await service.Connect();
                isDnsChanged = true;
                
                if (!result)
                {
                    ElectroMessageBox.Show("Connection can not be established.");
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceText = "● Error";
                }

            }
            else
            {
                service.Dispose();
                IsTurnedOn = false;
                ServiceText = "● Not Connected";
                isDnsChanged = false;
            }
            SetDnsTextColor = isDnsChanged ? Brushes.Green : Brushes.White;
        }

        private async void SetDns(object obj)
        {
            isDnsChanged = !isDnsChanged;
            
            ChangeModel("DNS Changer");
            if (isDnsChanged)
            {
                IsGettingData = true;
                ServiceText = service.ServiceText;
                IsComboBoxEnabled?.Invoke(false);
                bool result = await service.Connect();
                isDnsChanged = true;
                if (!result)
                {
                    ElectroMessageBox.Show("Connection can not be established.");
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceText = "● Error";
                }
            }
            else
            {
                service.Dispose();
                IsTurnedOn = false;
                ServiceText = "● Not Connected";
                isDnsChanged = false;
            }
            if(IsOpenVpn)
                ChangeModel("OpenVpn");
            else
                ChangeModel("PPTP");
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
                    IsGettingData = false;
                    IsTurnedOn = true;
                    this.ServiceText = serviceText;
                    ServiceUpdated?.Invoke(true);
                }
                else
                {
                    IsGettingData = false;
                    IsTurnedOn = false;
                    ServiceUpdated?.Invoke(false);
                }
                IsComboBoxEnabled?.Invoke(true);
            }
            else
                this.ServiceText = serviceText;
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
                                             (configureDnsCommand = new RelayCommand(configureDns));

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
        #endregion
    }
}
