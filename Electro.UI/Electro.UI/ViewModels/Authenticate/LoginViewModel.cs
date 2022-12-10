using Electro.UI.Tools;
using Electro.UI.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.ViewModels.Authenticate
{
    public class LoginViewModel : BaseModel
    {
        //Fields
        private string uniqueId = "Your Email/Phone Number";
        private string password = "Your Password";

        ///Commands
        private RelayCommand loginCommand;

        public RelayCommand LoginCommand => loginCommand ??
                                                   (loginCommand = new RelayCommand(Login));


        private async void Login(object obj)
        {
            try
            {
                
                string response = await HttpRequestHandler.SendCheckUserRegistryRequestAndGetResponse(uniqueId, password);
                dynamic response_dynamic = JsonConvert.DeserializeObject(response);
                string status = response_dynamic.status;


                if (status == "Ok")
                {
                    AuthenticationViewModel.Instatnce.ShotDown();
                }

                else if (status == "NotOk")
                {
                    ElectroMessageBox.Show("error occurred.");
                    App.Current.Shutdown();
                }
                else if (status == "AlreadyRegistered")
                {
                    ElectroMessageBox.Show("error occurred.");
                    App.Current.Shutdown();
                }
                else
                {
                    ElectroMessageBox.Show("error occurred.");
                    App.Current.Shutdown();
                }
            }
            catch (Exception)
            {
                ElectroMessageBox.Show("error occurred.");
                App.Current.Shutdown();
            }
        }

        public string UniqueId
        {
            get => uniqueId;
            set
            {
                uniqueId = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }
    }
}
