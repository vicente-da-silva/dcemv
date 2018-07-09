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
using DCEMV.Shared;
using System;
using Xamarin.Forms;

namespace DCEMV.PersoApp
{

    public partial class HomeMDView : MasterDetailPage
    {
        private ICardInterfaceManger cardInterfaceManger;
        private ViewTypes currentView;
        
        public HomeMDView(ICardInterfaceManger cardInterfaceManger)
        {
            InitializeComponent();

            this.cardInterfaceManger = cardInterfaceManger;
           
            masterPage.ListViewOthers.ItemSelected += OnItemSelected;

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            
            ModalPage selected;
            
            if (item != null)
            {
                try
                {
                    if (String.IsNullOrEmpty(SessionSingleton.DeviceId) && (currentView != ViewTypes.Settings && currentView != ViewTypes.IntroView))
                    {
                        App.Current.MainPage.DisplayAlert("Error", "No card reader selected in settings", "OK");
                        Detail = new IntroView();
                        masterPage.ClearSelectedItems();
                        IsPresented = false;
                        currentView = ViewTypes.IntroView;
                        return;
                    }

                    switch (item.View)
                    {
                        case ViewTypes.Settings:
                            selected = new SettingsView(cardInterfaceManger);
                            break;

                        case ViewTypes.XMLPersoView:
                            selected = new XMLPersoView(cardInterfaceManger);
                            break;

                        case ViewTypes.AppsView:
                            selected = new AppsView(cardInterfaceManger);
                            break;

                        case ViewTypes.InstallCapView:
                            selected = new InstallCapView(cardInterfaceManger);
                            break;

                        case ViewTypes.InstallAppletView:
                            selected = new InstallAppView(cardInterfaceManger);
                            break;

                        case ViewTypes.InstallAllView:
                            selected = new InstallAllView(cardInterfaceManger);
                            break;

                        case ViewTypes.TestView:
                            selected = new TestView(cardInterfaceManger);
                            break;

                        case ViewTypes.CardDataView:
                            selected = new CardDataView(cardInterfaceManger);
                            break;

                        default:
                            throw new Exception("Unknown Form Type:" + item.View);
                    }
                    currentView = item.View;
                    selected.PageClosing += (sender2, e2) =>
                    {
                        Detail = new IntroView();
                        currentView = ViewTypes.IntroView;
                    };
                    Detail = selected;
                    masterPage.ClearSelectedItems();
                    IsPresented = false;
                    
                }
                catch (Exception ex)
                {
                    App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    Detail = new IntroView();
                    masterPage.ClearSelectedItems();
                    IsPresented = false;
                    currentView = ViewTypes.IntroView;
                    return;
                }
            }
        }

        
    }
}
