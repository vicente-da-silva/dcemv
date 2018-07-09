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
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DCEMV.PersoApp
{
    public partial class SettingsView : ModalPage
    {
        private ICardInterfaceManger cardInterfaceManger;

        public SettingsView(ICardInterfaceManger cardInterfaceManger)
        {
            InitializeComponent();
            this.cardInterfaceManger = cardInterfaceManger;
            gridProgress.IsVisible = true;
            try
            {
                Task.Run(async () =>
                {
                    ObservableCollection<string> ids = await cardInterfaceManger.GetCardReaders();
                    UpdateView(ids);
                });
            }
            catch (Exception ex)
            {
                Task.Run(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private void UpdateView(ObservableCollection<string> ids)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                pickCardReaders.ItemsSource = ids;
                if (!String.IsNullOrEmpty(SessionSingleton.DeviceId))
                    pickCardReaders.SelectedItem = SessionSingleton.DeviceId;
                gridProgress.IsVisible = false;
            });
        }

        private void pickCardReaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            string id = pickCardReaders.SelectedItem as string;
            if (!String.IsNullOrEmpty(id))
            {
                SessionSingleton.DeviceId = id;
            }
        }

        private void cmdOk_Clicked(object sender, EventArgs e)
        {
            ClosePage();
        }
    }
}
