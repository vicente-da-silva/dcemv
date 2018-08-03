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
using IdentityModel.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DCEMV.ServerShared;

namespace DCEMV.DemoApp
{
    public static class SecureStore
    {
        private const string APP_NAME_KEY = "DCEMVApp";
        private const string USERNAME_FOR_SS = "DCEMV";
        private const string ACCESS_TOKEN_KEY = "AccessToken";
        private const string REFRESH_TOKEN_KEY = "RefreshToken";

        private static Xamarin.Auth.Account Account;
        private static Xamarin.Auth.AccountStore AccountStore;

        private static void Init()
        {
            if (Account == null)
            {
                if(AccountStore == null)
                    AccountStore = Xamarin.Auth.AccountStore.Create();

                if (AccountStore == null)
                    throw new Exception("Account Store not Supported");

                try
                {
                    Account = AccountStore.FindAccountsForService(APP_NAME_KEY).FirstOrDefault();
                }
                catch
                {
                    Account = null;
                }

                if (Account == null)
                {
                    Account = new Xamarin.Auth.Account
                    {
                        Username = USERNAME_FOR_SS
                    };

                    AccountStore.Save(Account, APP_NAME_KEY);
                }
            }
        }

        public static void DeleteAccount()
        {
            Init();
            AccountStore.Delete(Account, APP_NAME_KEY);
            Account = null;
        }
        public static void SaveAccount(string accessToken,string refreshToken)
        {
            Init();
            if (GetAccessToken() == null)
                Account.Properties.Add(ACCESS_TOKEN_KEY, accessToken);
            else
                Account.Properties[ACCESS_TOKEN_KEY] = accessToken;

            if (GetRefreshToken() == null)
                Account.Properties.Add(REFRESH_TOKEN_KEY, refreshToken);
            else
                Account.Properties[REFRESH_TOKEN_KEY] = refreshToken;

            AccountStore.Save(Account, APP_NAME_KEY);
        }
        public static string GetAccessToken()
        {
            Init();
            try
            {
                return Account.Properties[ACCESS_TOKEN_KEY];
            }
            catch
            {
                return null;
            }
        }
        public static string GetRefreshToken()
        {
            Init();
            try
            {
                return Account.Properties[REFRESH_TOKEN_KEY];
            }
            catch
            {
                return null;
            }
        }
    }
    public class SessionSingleton
    {
        private const string CONTACT_DEVICE_ID_KEY = "ContactDeviceId";
        private const string CONTACTLESS_DEVICE_ID_KEY = "ContactlessDeviceId";

        public static string ApiServerURL { get; set; }
        //public static string CredentialsId { get; set; }
        public static HttpClient HttpClient { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static string ClientName { get; set; }
        public static string ClientSecret { get; set; }
        public static string[] ClientScopes { get; set; }
        public static Account Account { get; set; }
        
        public static void EndSession()
        {
            SecureStore.DeleteAccount();
            Account = null;
        }

        public static void SetTokens(string accessToken, string refreshToken)
        {
            SecureStore.SaveAccount(accessToken,refreshToken);
        }
        public static string AccessToken
        {
            get
            {
                return SecureStore.GetAccessToken();
            }
        }

        public static string RefreshToken
        {
            get
            {
                return SecureStore.GetRefreshToken();
            }
        }

        public static string ContactDeviceId {
            get
            {
                string deviceid = null;
                if (App.Current.Properties.Keys.ToList().Exists(x=>x == CONTACT_DEVICE_ID_KEY))
                    deviceid = (string)App.Current.Properties[CONTACT_DEVICE_ID_KEY];
                return deviceid;
            }
            set
            {
                App.Current.Properties[CONTACT_DEVICE_ID_KEY] = value;
                App.Current.SavePropertiesAsync();
            }
        }
        public static string ContactlessDeviceId
        {
            get
            {
                string deviceid = null;
                if (App.Current.Properties.Keys.ToList().Exists(x => x == CONTACTLESS_DEVICE_ID_KEY))
                    deviceid = (string)App.Current.Properties[CONTACTLESS_DEVICE_ID_KEY];
                return deviceid;
            }
            set
            {
                App.Current.Properties[CONTACTLESS_DEVICE_ID_KEY] = value;
                App.Current.SavePropertiesAsync();
            }
        }

        private static readonly SessionSingleton instance = new SessionSingleton();

        private SessionSingleton()
        {
            ApiServerURL = "http://192.168.0.100:44359"; 
            //ApiServerURL = "http://localhost:44359";
            UserName = "testuser@domain.com";
            Password = "Password1!";

            ClientName = "clientROP";
            ClientSecret = "secret";
            ClientScopes = new string[] { "openid", "profile", "readAccess", "writeAccess", "offline_access", "roles" };
        }

        public static SessionSingleton Instance
        {
            get
            {
                return instance;
            }
        }

        public static Proxies.DCEMVDemoServerClient GenDCEMVServerApiClient()
        {
            if (HttpClient != null)
                HttpClient.Dispose();
            HttpClient = new HttpClient();
            HttpClient.SetBearerToken(AccessToken);
            Proxies.DCEMVDemoServerClient xserverApi = new Proxies.DCEMVDemoServerClient(ApiServerURL, HttpClient);
            return xserverApi;
        }

        public static async Task LoginResourceOwner(string username, string userpassword)
        {
            DiscoveryClient discoClient = new DiscoveryClient(ApiServerURL);
            discoClient.Policy.RequireHttps = false;
            DiscoveryResponse disco = await discoClient.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            TokenClient client = new TokenClient(disco.TokenEndpoint, ClientName, ClientSecret);
            TokenResponse result = await client.RequestResourceOwnerPasswordAsync(username, userpassword, string.Join(" ", ClientScopes));
            if (result.IsError)
                throw new Exception("Could not log in, username or password is incorrect, or you have not yet confirmed your email.");

            StringBuilder sb = new StringBuilder(128);
            sb.AppendLine($"refresh token: {result.RefreshToken}");
            sb.AppendLine($"access token: {result.AccessToken}");
            System.Diagnostics.Debug.WriteLine(sb.ToString());

            SessionSingleton.SetTokens(result.AccessToken, result.RefreshToken); 
        }
        public static async Task LoadAccount()
        {
            try
            {
                await LoadAccountEx();
            }
            catch(Proxies.SwaggerException ex)
            {
                if (ex.StatusCode == 401)
                {
                    await RequestRefreshToken();
                    await LoadAccountEx();
                }
                else
                    throw ex;
            }
        }

        private static async Task LoadAccountEx()
        {
            Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
            using (SessionSingleton.HttpClient)
            {
                Proxies.Account accountJson = await client.AccountGetAsync();
                Account account = Account.FromJsonString(accountJson.ToJson());
                SessionSingleton.Account = account;
            }
        }

        public static async Task RequestRefreshToken()
        {
            DiscoveryClient discoClient = new DiscoveryClient(ApiServerURL);
            DiscoveryResponse disco = await discoClient.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            TokenClient client = new TokenClient(disco.TokenEndpoint, ClientName, ClientSecret);
            TokenResponse result = await client.RequestRefreshTokenAsync(SessionSingleton.RefreshToken);
            if (result.IsError)
                throw new Exception("Could not refresh token, try logoff and login again.");
            SessionSingleton.SetTokens(result.AccessToken, result.RefreshToken);
        }
        private static async Task<string> GetUSerInfo(string userInfoEndpoint)
        {
            UserInfoClient clientUserInfo = new UserInfoClient(userInfoEndpoint);
            UserInfoResponse response = await clientUserInfo.GetAsync(SessionSingleton.AccessToken);
            if (response.IsError)
            {
                throw new Exception("Could not log in");
            }
            System.Diagnostics.Debug.WriteLine("User claims:");
            string credentialsId = "";
            foreach (Claim claim in response.Claims)
            {
                System.Diagnostics.Debug.WriteLine("{0}\n {1}", claim.Type, claim.Value);
                if (claim.Type == "sub")
                    credentialsId = claim.Value;
            }
            if (string.IsNullOrEmpty(credentialsId))
                throw new Exception("Could not log in");

            return credentialsId;
        }
    }
}
