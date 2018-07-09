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

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCEMV.TerminalCommon
{
    /**
     * Confirm code happens when:
     * anonomously : reset password, code needed to change password, code sent via email with forgot password screen, use confirm email screen
     * anonomously : on registration, code sent to confirm email, use confirm email screen
     * signed in   : add or change phone number, code sent via sms to confirm phone number, confirm screen comes up automatically
     **/
    public enum CodeType
    {
        PhoneNumber,
        EmailAddress
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmCodeCtl : Grid
    {
        public event EventHandler OKClicked;
        public event EventHandler CancelClicked;

        public CodeType CodeType { get; set; }
        private string codeValue;
        public string PhoneNumber { get { return txtPhoneNumber.Text; } set { txtPhoneNumber.Text = value; } }
        public string EmailAddress { get { return txtEmail.Text; } set { txtEmail.Text = value; } }
        public string OTP { get { return txtOTP.Text; } set { txtOTP.Text = value; } }

        public ConfirmCodeCtl()
        {
            InitializeComponent();
            cmdCancel.IsVisible = false;
        }
        public void Init(CodeType codeType, bool showCancel = false)
        {
            if (showCancel)
                cmdCancel.IsVisible = true;

            CodeType = codeType;

            switch (codeType)
            {
                case CodeType.EmailAddress:
                    txtEmail.Text = "testuser@domain.com";
                    txtOTP.Placeholder = "Email Token";
                    txtPhoneNumber.IsVisible = false;
                    lblPhoneNumber.IsVisible = false;
                    imgPhoneNumber.IsVisible = false;
                    break;

                case CodeType.PhoneNumber:
                    txtPhoneNumber.Text = codeValue;
                    txtPhoneNumber.IsEnabled = false;
                    txtOTP.Placeholder = "OTP";
                    txtEmail.IsVisible = false;
                    lblEmail.IsVisible = false;
                    imgEmail.IsVisible = false;
                    break;
            }
            gridProgress.IsVisible = false;
        }
        private void OnOKClicked()
        {
            OKClicked?.Invoke(this, new EventArgs());
        }
        private void OnCancelClicked()
        {
            CancelClicked?.Invoke(this, new EventArgs());
        }
        
        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            OnCancelClicked();
        }
        private void cmdOk_Clicked(object sender, EventArgs e)
        {
            OnOKClicked();
        }
    }
}