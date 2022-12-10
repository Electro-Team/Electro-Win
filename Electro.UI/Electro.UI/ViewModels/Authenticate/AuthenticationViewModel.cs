using Electro.UI.Views;
using Electro.UI.Views.Authenticate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electro.UI.ViewModels.Authenticate
{
    public class AuthenticationViewModel
    {
        private static AuthenticationViewModel instatnce;
        private  AuthenticationView view;

        public AuthenticationViewModel()
        {
            Instatnce = this;
            view = new AuthenticationView();
            view.DataContext = this;


            view.ShowDialog();
        }

        public void ShotDown()
        {
            view.Close();
        }

        public static AuthenticationViewModel Instatnce
        {
            get => instatnce;
            set => instatnce = value;
        }
    }
}
