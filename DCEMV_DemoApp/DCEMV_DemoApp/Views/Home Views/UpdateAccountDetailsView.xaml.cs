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
using System.Collections.Generic;
using System.Threading.Tasks;
using DCEMV.ServerShared;

namespace DCEMV.DemoApp
{
    public partial class UpdateAccountDetailsView : ModalPage
    {
        private Account account;
        private const string Bus = "Business";
        private const string Ind = "Individual";
        public UpdateAccountDetailsView()
        {
            InitializeComponent();
            List<string> customerTypes = new List<string>();
            customerTypes.Add(Bus);
            customerTypes.Add(Ind);
            pickAccountType.ItemsSource = customerTypes;
            gridProgress.IsVisible = false;

            account = new Account
            {
                CustomerType = SessionSingleton.Account.CustomerType,
                AccountNumberId = SessionSingleton.Account.AccountNumberId
            };

            switch (SessionSingleton.Account.CustomerType)
            {
                case CustomerType.None:
                    pickAccountType.IsEnabled = true;
                    gridInd.IsVisible = false;
                    gridBus.IsVisible = false;
                    break;

                case CustomerType.Individual:
                    pickAccountType.IsEnabled = false;
                    pickAccountType.SelectedItem = MapPickerValue(account.CustomerType);
                    account.FirstName = SessionSingleton.Account.FirstName;
                    account.LastName = SessionSingleton.Account.LastName;
                    break;

                case CustomerType.Business:
                    pickAccountType.IsEnabled = false;
                    pickAccountType.SelectedItem = MapPickerValue(account.CustomerType);
                    account.BusinessName = SessionSingleton.Account.BusinessName;
                    account.CompanyRegNumber = SessionSingleton.Account.CompanyRegNumber;
                    account.TaxNumber = SessionSingleton.Account.TaxNumber;
                    break;

                }

            gridMain.BindingContext = account;
        }

        private async void cmdOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        account.CustomerType = MapPickerValue();
                        switch (account.CustomerType)
                        {
                            case CustomerType.Individual:
                                await client.AccountUpdateindividualaccountdetailsPostAsync(account.ToJsonString());

                                SessionSingleton.Account.AccountState = 0;
                                SessionSingleton.Account.CustomerType = account.CustomerType;
                                SessionSingleton.Account.FirstName = account.FirstName;
                                SessionSingleton.Account.LastName = account.LastName;
                                break;

                            case CustomerType.Business:
                                await client.AccountUpdatebusinessaccountdetailsPostAsync(account.ToJsonString());

                                SessionSingleton.Account.AccountState = 0;
                                SessionSingleton.Account.CustomerType = account.CustomerType;
                                SessionSingleton.Account.BusinessName = account.BusinessName;
                                SessionSingleton.Account.CompanyRegNumber = account.CompanyRegNumber;
                                SessionSingleton.Account.TaxNumber = account.TaxNumber;
                                break;
                        }
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

        private void pickAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (MapPickerValue())
            {
                case CustomerType.None:
                    gridInd.IsVisible = false;
                    gridBus.IsVisible = false;
                    break;

                case CustomerType.Individual:
                    gridInd.IsVisible = true;
                    gridBus.IsVisible = false;

                    txtBusinessName.Text = "";
                    txtCompanyRegNumber.Text = "";
                    txtTaxNumber.Text = "";
                    break;

                case CustomerType.Business:
                    gridInd.IsVisible = false;
                    gridBus.IsVisible = true;

                    txtFirstName.Text = "";
                    txtLastName.Text = "";
                    break;
            }
        }

        private CustomerType MapPickerValue()
        {
            switch (pickAccountType.SelectedItem)
            {
                case Ind:
                    return CustomerType.Individual;

                case Bus:
                    return CustomerType.Business;

                default:
                    throw new Exception("Invalid customer type");
            }
        }

        private string MapPickerValue(CustomerType type)
        {
            switch (type)
            {
                case CustomerType.Individual:
                    return Ind;

                case CustomerType.Business:
                    return Bus;

                default:
                    throw new Exception("Invalid customer type");
            }
        }

        
    }
}
