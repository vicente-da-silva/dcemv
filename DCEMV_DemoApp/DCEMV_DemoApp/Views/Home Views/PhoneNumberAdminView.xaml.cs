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

using Xamarin.Forms;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{
    public partial class PhoneNumberAdminView : ModalPage
    {
        private Profile profile;
        public string PhoneNumber { get; set; }
        public PhoneNumberAdminView()
        {
            InitializeComponent();
            gridProgress.IsVisible = true;
            gridAddPhone.IsVisible = false;
            gridPhoneAdmin.IsVisible = true;
            gridConfirmCode.IsVisible = false;
            ctlConfirmCode.OKClicked += CtlConfirmCode_OKClicked;
            try
            {
                Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        profile = await client.ProfileGetprofiledetailsGetAsync();
                        UpdataView();
                    }
                });
            }
            catch (Exception ex)
            {
                Task.Run(async ()=> { await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK"); });
            }
        }

        

        private void UpdataView()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridProgress.IsVisible = false;
                
                if (profile.PhoneNumber != null)
                {
                    cmdAddPhoneNumber.IsVisible = false;
                    cmdChangePhoneNumber.IsVisible = true;
                    cmdRemovePhoneNumber.IsVisible = true;
                    lblPhoneNumber.Text = profile.PhoneNumber;
                }
                else
                {
                    cmdAddPhoneNumber.IsVisible = true;
                    cmdChangePhoneNumber.IsVisible = false;
                    cmdRemovePhoneNumber.IsVisible = false;
                    lblPhoneNumber.Text = "None";
                }
            });
        }

        private async void cmdRemovePhoneNumber_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.ProfileRemovephonenumberPostAsync();
                        profile.PhoneNumber = null;
                        UpdataView();
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private void cmdBack_Clicked(object sender, EventArgs e)
        {
            ClosePage();
        }

        #region AddPhone
        private async void cmdAddPhoneNumber_Clicked(object sender, EventArgs e)
        {
            try
            {
                //AddPhoneNumberView spv = new AddPhoneNumberView(profile.PhoneNumber);
                //spv.PageClosing += (sender2, e2) =>
                //{
                //    if (!spv.IsCancelled)
                //        profile.PhoneNumber = spv.PhoneNumber;
                //    UpdataView();
                //};
                //OpenPage(spv);

                gridAddPhone.IsVisible = true;
                gridPhoneAdmin.IsVisible = false;
                gridConfirmCode.IsVisible = false;
                txtPhoneNumber.Text = profile.PhoneNumber;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private async void cmdChangePhoneNumber_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridAddPhone.IsVisible = true;
                gridPhoneAdmin.IsVisible = false;
                gridConfirmCode.IsVisible = false;
                txtPhoneNumber.Text = profile.PhoneNumber;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }
        private async void cmdPhoneAddOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.ProfileAddphonenumberPostAsync(txtPhoneNumber.Text);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            gridProgress.IsVisible = false;

                            gridConfirmCode.IsVisible = true;
                            ctlConfirmCode.Init(TerminalCommon.CodeType.PhoneNumber);
                            ctlConfirmCode.PhoneNumber = txtPhoneNumber.Text;
                            ctlConfirmCode.OTP = "";
                            gridAddPhone.IsVisible = false;
                            gridPhoneAdmin.IsVisible = false;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }
        #endregion

        #region ConfirmCode
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
                            case TerminalCommon.CodeType.EmailAddress:
                                await client.ProfileConfirmemailPostAsync(ctlConfirmCode.EmailAddress, ctlConfirmCode.OTP);
                                break;

                            case TerminalCommon.CodeType.PhoneNumber:
                                await client.ProfileVerifyphonenumberPostAsync(ctlConfirmCode.PhoneNumber, ctlConfirmCode.OTP);
                                break;
                        }
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            profile.PhoneNumber = txtPhoneNumber.Text;
                            gridConfirmCode.IsVisible = false;
                            gridAddPhone.IsVisible = false;
                            gridPhoneAdmin.IsVisible = true;
                            UpdataView();
                        });

                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        #endregion
    }
}
