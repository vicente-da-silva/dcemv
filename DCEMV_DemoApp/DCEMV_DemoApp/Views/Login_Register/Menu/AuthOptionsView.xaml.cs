/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System;

namespace DCEMV.DemoApp
{
    public partial class AuthOptionsView : ModalPage
    {
        public AuthOptionsView()
        {
            InitializeComponent();
        }
        
        private void cmdLogin_Clicked(object sender, EventArgs e)
        {
            LoginView login = new LoginView();
            login.PageClosing += (sender2, e2) => 
            {
                if (!login.IsCancelled && !String.IsNullOrEmpty(SessionSingleton.AccessToken))
                {
                    ClosePage();
                }
            };
            OpenPage(login);
        }
        
        private void cmdRegister_Clicked(object sender, EventArgs e)
        {
            OpenPage(new RegisterView());
        }

        private void cmdForgotPassword_Clicked(object sender, EventArgs e)
        {
            OpenPage(new ForgotPasswordView());
        }

        private void cmdConfirmEmail_Clicked(object sender, EventArgs e)
        {
            ConfirmCodeView ccv = new ConfirmCodeView();
            ccv.ConfirmCodeCtl.Init(TerminalCommon.CodeType.EmailAddress,true);
            OpenPage(ccv);
        }

        private void cmdResendConfirmEmail_Clicked(object sender, EventArgs e)
        {
            OpenPage(new ResendConfirmEmailView());
        }

        private void cmdResetPassword_Clicked(object sender, EventArgs e)
        {
            OpenPage(new ResetPasswordView());
        }
    }
}
