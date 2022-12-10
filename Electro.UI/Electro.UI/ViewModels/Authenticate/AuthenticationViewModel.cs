﻿using Electro.UI.Views;
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
        private AuthenticationView view;

        public AuthenticationViewModel()
        {
            this.view = new AuthenticationView();
            this.view.DataContext = this;


            this.view.ShowDialog();
        }

    }
}
