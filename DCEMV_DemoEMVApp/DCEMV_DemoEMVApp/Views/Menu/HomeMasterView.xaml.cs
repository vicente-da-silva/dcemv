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

namespace DCEMV.DemoEMVApp
{
    public enum ViewTypes
    {
        Transact,
        QRCode,
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
        private List<MasterPageItem> otherPageItems;
        
        public ListView ListViewTransact { get { return listViewTransact; } }
        public ListView ListViewOthers { get { return listViewOthers; } }

        public HomeMasterView()
        {
            InitializeComponent();
            
            /*****************************************/
            transactPageItems = new List<MasterPageItem>();
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Transact",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.Transact,
            });
            transactPageItems.Add(new MasterPageItem
            {
                Title = "Present QR Code",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.QRCode,
            });
            listViewTransact.ItemsSource = transactPageItems;
            /*****************************************/
            otherPageItems = new List<MasterPageItem>();
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Settings",
                IconSource =ImageResourceExtension.ProvideImageSource("hamburger.png") ,
                View = ViewTypes.Settings
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
            listViewOthers.HeightRequest = otherPageItems.Count * 40;
        }

        internal void ClearSelectedItems()
        {
            listViewTransact.SelectedItem = null;
            listViewOthers.SelectedItem = null;
        }
    }
}
