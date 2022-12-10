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
    public class SignupViewModel : BaseModel
    {
        //Fields
        private string uniqueId = "Your Email/Phone Number";
        private string password = "Your Password";
        private string password2 = "Your Password Again";
        private string username = "Your Username";


        ///Commands
        private RelayCommand signupCommand;

        public RelayCommand SignupCommand => signupCommand ??
                                                   (signupCommand = new RelayCommand(Signup));


        private async void Signup(object obj)
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
        public string Password2
        {
            get => password2;
            set
            {
                password2 = value;
                OnPropertyChanged();
            }
        }
        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }
    }
}
