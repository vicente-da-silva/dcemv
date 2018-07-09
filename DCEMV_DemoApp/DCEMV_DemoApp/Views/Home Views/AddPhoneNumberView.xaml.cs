using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XTerminal.Proxies;

namespace XTerminal
{
    public partial class AddPhoneNumberView : ModalPage
    {
        public bool IsCancelled { get; set; }
        public string PhoneNumber { get; set; }
        public AddPhoneNumberView(string phoneNumber)
        {
            InitializeComponent();
            IsCancelled = true;
            gridProgress.IsVisible = false;
            PhoneNumber = phoneNumber;
            if(phoneNumber!=null)
                txtPhoneNumber.Text = PhoneNumber;
        }
        
        private async void cmdOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    XServerApiClient client = SessionSingleton.GenXServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.ProfileAddphonenumberPostAsync(txtPhoneNumber.Text);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            gridProgress.IsVisible = false;
                            ConfirmCodeView vpn = new ConfirmCodeView(CodeType.PhoneNumber, txtPhoneNumber.Text);
                            vpn.Disappearing += (sender2, e2) =>
                            {
                                if (!vpn.IsCancelled)
                                {
                                    PhoneNumber = txtPhoneNumber.Text;
                                    IsCancelled = false;
                                    ClosePage();
                                }
                            };
                            OpenPage(vpn);
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
    }
}
