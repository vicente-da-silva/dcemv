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

namespace DCEMV.PersoApp
{
    public enum ViewTypes
    {
        Settings,
        XMLPersoView,
        IntroView,
        AppsView,
        InstallCapView,
        InstallAppletView,
        InstallAllView,
        TestView,
        CardDataView,
    }
    public class MasterPageItem

    {
        public string Title { get; set; }
        public ImageSource IconSource { get; set; }
        public ViewTypes View { get; set; }

    }
    public partial class HomeMasterView : ContentPage
    {
        private List<MasterPageItem> otherPageItems;
        
        public ListView ListViewOthers { get { return listViewOthers; } }

        public HomeMasterView()
        {
            InitializeComponent();
            SetWait(false);

            otherPageItems = new List<MasterPageItem>();

            otherPageItems.Add(new MasterPageItem
            {
                Title = "Card Data",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.CardDataView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "View/Remove Apps",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.AppsView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Cap Load",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.InstallCapView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Install Applet",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.InstallAppletView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Remove/Cap/Install",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.InstallAllView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Perso App",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.XMLPersoView
            });
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Test App",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
                View = ViewTypes.TestView
            });
           
            
           
            otherPageItems.Add(new MasterPageItem
            {
                Title = "Settings",
                IconSource = ImageResourceExtension.ProvideImageSource("hamburger.png"),
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
            listViewOthers.HeightRequest = otherPageItems.Count * 40;
        }

        internal void ClearSelectedItems()
        {
            listViewOthers.SelectedItem = null;
        }
    }
}
