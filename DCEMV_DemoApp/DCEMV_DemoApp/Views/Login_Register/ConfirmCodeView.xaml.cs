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
using System.Threading.Tasks;
using DCEMV.TerminalCommon;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{
    /**
     * Confirm code happens when:
     * anonomously : reset password, code needed to change password, code sent via email with forgot password screen, use confirm email screen
     * anonomously : on registration, code sent to confirm email, use confirm email screen
     * signed in   : add or change phone number, code sent via sms to confirm phone number, confirm screen comes up automatically
     **/

    public partial class ConfirmCodeView : ModalPage
    {
        public ConfirmCodeCtl ConfirmCodeCtl { get { return ctlConfirmCode; } }
        public ConfirmCodeView()
        {
            InitializeComponent();
            gridProgress.IsVisible = false;
            ctlConfirmCode.OKClicked += CtlConfirmCode_OKClicked;
            ctlConfirmCode.CancelClicked += CtlConfirmCode_CancelClicked;
        }
        
        private async void CtlConfirmCode_OKClicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        switch (ctlConfirmCode.CodeType)
                        {
                            case CodeType.EmailAddress:
                                await client.ProfileConfirmemailPostAsync(ctlConfirmCode.EmailAddress, ctlConfirmCode.OTP);
                                break;

                            case CodeType.PhoneNumber:
                                await client.ProfileVerifyphonenumberPostAsync(ctlConfirmCode.PhoneNumber, ctlConfirmCode.OTP);
                                break;
                        }
                        
                    }
                });
            }
            catch(Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private void CtlConfirmCode_CancelClicked(object sender, EventArgs e)
        {
            ClosePage();
        }
    }
}
