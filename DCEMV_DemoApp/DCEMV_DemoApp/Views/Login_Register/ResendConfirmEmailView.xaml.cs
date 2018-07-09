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
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{
    public partial class ResendConfirmEmailView : ModalPage
    {
        public ResendConfirmEmailView()
        {
            InitializeComponent();
            gridProgress.IsVisible = false;
            txtEmail.Text = SessionSingleton.UserName;
        }

        private async void cmdOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.ProfileResendconfirmemailGetAsync(txtEmail.Text);
                    }
                    ClosePage();
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

        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            ClosePage();
        }
    }
}
