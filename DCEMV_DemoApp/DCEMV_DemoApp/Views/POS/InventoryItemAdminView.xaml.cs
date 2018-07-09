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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{
    public enum UpdateType
    {
        Add,
        Update
    }
    public partial class InventoryItemAdminView : ModalPage
    {
        private ObservableCollection<InventoryItemDetailViewModel> items;
        private ObservableCollection<InventoryGroup> groups;

        public InventoryItemDetailViewModel InventoryItem { get; private set; }
        private UpdateType updateType;
        public InventoryItemAdminView()
        {
            InitializeComponent();
            gridProgress.IsVisible = true;
            gridInventoryItemList.IsVisible = true;
            gridAddEdit.IsVisible = false;

            try
            {
                Task.Run(async () =>
                {
                    groups = await CacheProvider.Instance.GetInventoryGroups();
                    items = await CacheProvider.Instance.GetinventoryItems();
                    UpdataView();
                });
            }
            catch (Exception ex)
            {
                Task.Run(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                });
            }
        }

        private void UpdataView()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                items.ToList().ForEach(x =>
                {
                    x.Group = groups.ToList().Find(y => y.InventoryGroupId == x.InventoryGroupIdRef);
                });
                lstInventoryItems.ItemsSource = null;
                lstInventoryItems.ItemsSource = items;
                gridProgress.IsVisible = false;
            });
        }

       

        public void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search.Length < 3)
            {
                lstInventoryItems.ItemsSource = items;
                return;
            }

            lstInventoryItems.ItemsSource = items.Where(x => { return x.Name.ToLower().Contains(search) || x.Description.ToLower().Contains(search) || x.Barcode.Contains(search); }).ToList();
        }

        private async void cmdRemove_Clicked(object sender, EventArgs e)
        {
            if (lstInventoryItems.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.StoreInventoryitemDeleteAsync(((InventoryItem)lstInventoryItems.SelectedItem).InventoryItemId);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            items.Remove((InventoryItemDetailViewModel)lstInventoryItems.SelectedItem);
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

        private async void cmdEdit_Clicked(object sender, EventArgs e)
        {
            if (lstInventoryItems.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }
            
            updateType = UpdateType.Update;
            InventoryItem item = (InventoryItem)lstInventoryItems.SelectedItem;
            InventoryItem = new InventoryItemDetailViewModel()
            {
                InventoryItemId = item.InventoryItemId,
                Name = item.Name,
                Description = item.Description,
                Barcode = item.Barcode,
                Price = item.Price,
                InventoryGroupIdRef = item.InventoryGroupIdRef,
            };
            gridAddEdit.BindingContext = InventoryItem;
            pickGroup.ItemsSource = groups;
            DetermineSelectedIndex(InventoryItem, pickGroup);

            gridInventoryItemList.IsVisible = false;
            gridAddEdit.IsVisible = true;
        }

        private void cmdAdd_Clicked(object sender, EventArgs e)
        {
            updateType = UpdateType.Add;
            InventoryItem = new InventoryItemDetailViewModel();
            gridAddEdit.BindingContext = InventoryItem;
            pickGroup.ItemsSource = groups;

            gridInventoryItemList.IsVisible = false;
            gridAddEdit.IsVisible = true;
        }

        #region Add_Edit
        private void DetermineSelectedIndex(InventoryItem item, Picker picker)
        {
            int counter = 0;
            foreach (InventoryGroup group in picker.ItemsSource)
            {
                if (group.InventoryGroupId == item.InventoryGroupIdRef)
                {
                    picker.SelectedIndex = counter;
                }
                counter++;
            }
        }

        private async void cmdAddEditOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                InventoryGroup group = (pickGroup.SelectedItem as InventoryGroup);
                InventoryItem.InventoryGroupIdRef = group.InventoryGroupId;
                InventoryItem ii = new InventoryItem()
                {
                    Name = InventoryItem.Name,
                    Description = InventoryItem.Description,
                    Barcode = InventoryItem.Barcode,
                    InventoryGroupIdRef = InventoryItem.InventoryGroupIdRef,
                    Price = InventoryItem.Price
                };
                DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    switch (updateType)
                    {
                        case UpdateType.Add:
                            int id = await client.StoreInventoryitemPostAsync(ii.ToJsonString());
                            InventoryItem.InventoryItemId = id;

                            items.Add(InventoryItem);
                            UpdataView(); //TODO:needed because of bug? in xamarin forms, edited field does not display

                            break;

                        case UpdateType.Update:
                            InventoryItem.InventoryGroupIdRef = group.InventoryGroupId;
                            ii.InventoryItemId = InventoryItem.InventoryItemId;
                            await client.StoreInventoryitemPutAsync(ii.ToJsonString());

                            InventoryItem selectedItem = (InventoryItem)lstInventoryItems.SelectedItem;
                            selectedItem.Name = InventoryItem.Name;
                            selectedItem.Description = InventoryItem.Description;
                            selectedItem.Barcode = InventoryItem.Barcode;
                            selectedItem.Price = InventoryItem.Price;
                            selectedItem.InventoryGroupIdRef = InventoryItem.InventoryGroupIdRef;

                            UpdataView(); //TODO:needed because of bug? in xamarin forms, edited field does not display

                            break;
                    }
                }
                gridInventoryItemList.IsVisible = true;
                gridAddEdit.IsVisible = false;
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
            gridInventoryItemList.IsVisible = true;
            gridAddEdit.IsVisible = false;
        }
        #endregion
    }
}
