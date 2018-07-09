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
using System.Collections.Generic;
using Xamarin.Forms;

namespace DCEMV.DemoApp
{
    public enum ViewTypes
    {
        TransactSendMoneyView,
        TransactReceiveMoneyView,
        TopUpView,
        CredentialsPhoneNumberAdminView,
        CredentialsChangePasswordView,
        DeviceAdminView,
        AccountAdminView,
        AccountCardAdminView,
        AccountTransactions,
        LogoffView,
        SellView,
        InventoryGroupView,
        InventoryItemView,
        Settings
    }
    public class MasterPageItem

    {
        public string Title { get; set; }
        public ImageSource IconSource { get; set; }
        public ViewTypes View { get; set; }

    }
    public partial class HomeMasterView : ContentPage
    {
        private List<MasterPageItem> transactPageItems;
        private List<MasterPageItem> credentialsPageItems;
        private List<MasterPageItem> accountPageItems;
        private List<MasterPageItem> posPageItems;
        private List<MasterPageItem> otherPageItems;

        public ListView ListViewAccount { get { return listViewAccount; } }
        public ListView ListViewCredentials { get { return listViewCredentials; } }
        public ListView ListViewTransact { get { return listViewTransact; } }
        public ListView ListViewPOS { get { return listViewPOS; } }
        public ListView ListViewOthers { get { return listViewOthers; } }

        public HomeMasterView()
        {
            InitializeComponent();
            
            /*****************************************/
            transactPageItems = new List<MasterPageItem>();
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Send Money",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.TransactSendMoneyView
            });
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Receive Money",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.TransactReceiveMoneyView
            });
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Top-Up",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.TopUpView
            });
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Sell Stuff",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.SellView
            });
            listViewTransact.ItemsSource = transactPageItems;
            /*****************************************/
            credentialsPageItems = new List<MasterPageItem>();
            credentialsPageItems.Add(new MasterPageItem
            {
                Title = "Phone Number Admin",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.CredentialsPhoneNumberAdminView
            });
            credentialsPageItems.Add(new MasterPageItem
            {
                Title = "Change Password",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.CredentialsChangePasswordView
            });
            listViewCredentials.ItemsSource = credentialsPageItems;
            /*****************************************/
            accountPageItems = new List<MasterPageItem>();
            accountPageItems.Add(new MasterPageItem
            {
                Title = "Details Admin",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.AccountAdminView
            });
            accountPageItems.Add(new MasterPageItem
            {
                Title = "Card Admin",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.AccountCardAdminView
            });
            accountPageItems.Add(new MasterPageItem
            {
                Title = "Account Transactions",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.AccountTransactions
            });
            
            listViewAccount.ItemsSource = accountPageItems;
            /*****************************************/
            //masterPageItems.Add(new MasterPageItem
            //{
            //    Title = "Device Settings",
            //    IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
            //    View = ViewTypes.DeviceAdminView
            //});
            posPageItems = new List<MasterPageItem>();
            posPageItems.Add(new MasterPageItem
            {
                Title = "Manage Groups",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.InventoryGroupView,
            });
            posPageItems.Add(new MasterPageItem
            {
                Title = "Manage Items",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.InventoryItemView
            });
            listViewPOS.ItemsSource = posPageItems;
            /*****************************************/
            otherPageItems = new List<MasterPageItem>();
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Settings",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.Settings
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Logoff",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png")  ,
                View = ViewTypes.LogoffView
            });
            listViewOthers.ItemsSource = otherPageItems;

        }

        public void SetWait(bool on)
        {
            gridProgress.IsVisible = on;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            listViewTransact.HeightRequest = transactPageItems.Count * 40;
            listViewAccount.HeightRequest = accountPageItems.Count * 40;
            listViewCredentials.HeightRequest = credentialsPageItems.Count * 40;
            listViewPOS.HeightRequest = posPageItems.Count * 40;
            listViewOthers.HeightRequest = otherPageItems.Count * 40;
        }

        internal void ClearSelectedItems()
        {
            listViewAccount.SelectedItem = null;
            listViewCredentials.SelectedItem = null;
            listViewTransact.SelectedItem = null;
            listViewPOS.SelectedItem = null;
            listViewOthers.SelectedItem = null;
        }
    }
}
