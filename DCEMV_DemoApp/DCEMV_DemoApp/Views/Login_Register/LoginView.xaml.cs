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

namespace DCEMV.DemoApp
{
    public partial class LoginView : ModalPage
    {
        public bool IsCancelled { get; set; }
        
        public LoginView()
        {
            InitializeComponent();
            gridProgress.IsVisible = false;

            txtEmail.Text = SessionSingleton.UserName;
            txtPassword.Text = SessionSingleton.Password;
        }

        private async void cmdOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    await SessionSingleton.LoginResourceOwner(txtEmail.Text, txtPassword.Text);
                    IsCancelled = false;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ClosePage();
                    });
                });
            }
            catch (Exception ex)
            {
                SessionSingleton.EndSession();
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            IsCancelled = true;
            ClosePage();
        }
       
        //private async void LoginHybrid()
        //{
        //    OidcClientOptions options = new OidcClientOptions
        //    {
        //        Authority = "https://localhost:44359",
        //        ClientId = "clientHybrid",
        //        ClientSecret = "secret",
        //        Scope = "openid profile readAccess writeAccess offline_access roles",
        //        RedirectUri = applicationCallbackUri,
        //        Browser = authbrowser,
        //        Flow = OidcClientOptions.AuthenticationFlow.Hybrid,
        //    };

        //    OidcClient client = new OidcClient(options);
        //    LoginResult result = await client.LoginAsync();

        //    if (!string.IsNullOrEmpty(result.Error))
        //    {
        //        System.Diagnostics.Debug.WriteLine(result.Error);
        //        return;
        //    }

        //    StringBuilder sb = new StringBuilder(128);

        //    foreach (Claim claim in result.User.Claims)
        //    {
        //        sb.AppendLine($"{claim.Type}: {claim.Value}");
        //    }

        //    sb.AppendLine($"refresh token: {result.RefreshToken}");
        //    sb.AppendLine($"access token: {result.AccessToken}");

        //    System.Diagnostics.Debug.WriteLine(sb.ToString());

        //    _client = new HttpClient(result.RefreshTokenHandler);
        //    _client.BaseAddress = new Uri("https://localhost:44354/");

        //    HttpResponseMessage httpresponseMessage = await _client.GetAsync("products");
        //    if (httpresponseMessage.IsSuccessStatusCode)
        //    {
        //        string response = await httpresponseMessage.Content.ReadAsStringAsync();
        //        System.Diagnostics.Debug.WriteLine(JArray.Parse(response).ToString());
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.WriteLine(httpresponseMessage.ReasonPhrase);
        //    }

        //    await ClosePage();
        //}
    }
}
