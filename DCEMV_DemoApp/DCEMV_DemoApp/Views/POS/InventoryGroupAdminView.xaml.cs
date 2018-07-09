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
using System.Threading.Tasks;

using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{

    public partial class InventoryGroupAdminView : ModalPage
    {
        private ObservableCollection<InventoryGroup> groups;
        public InventoryGroup InventoryGroup { get; private set; }
        private UpdateType updateType;
        public InventoryGroupAdminView()
        {
            InitializeComponent();
            gridProgress.IsVisible = true;
            gridAddEdit.IsVisible = false;
            gridInventoryItemList.IsVisible = true;

            try
            {
                Task.Run(async () =>
                {
                    groups = await CacheProvider.Instance.GetInventoryGroups();
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
                //if(lstInventoryGroups.ItemsSource == null)
                lstInventoryGroups.ItemsSource = null;
                lstInventoryGroups.ItemsSource = groups;
                gridProgress.IsVisible = false;
            });
        }
        
        private async void cmdRemove_Clicked(object sender, EventArgs e)
        {
            if (lstInventoryGroups.SelectedItem == null)
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
                        await client.StoreInventorygroupDeleteAsync(((InventoryGroup)lstInventoryGroups.SelectedItem).InventoryGroupId);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            groups.Remove((InventoryGroup)lstInventoryGroups.SelectedItem);
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
            if (lstInventoryGroups.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            //InventoryGroup selectedItem = (InventoryGroup)lstInventoryGroups.SelectedItem;
            //UpdateInventoryGroupView view = new UpdateInventoryGroupView(selectedItem);
            //view.PageClosing += (sender2, e2) =>
            //{
            //    if (!view.IsCancelled)
            //    {
            //        selectedItem.Name = view.InventoryGroup.Name;
            //        selectedItem.Description = view.InventoryGroup.Description;
            //        UpdataView(); //TODO:needed because of bug? in xamarin forms, edited field does not display
            //    }
            //};
            //OpenPage(view);

            updateType = UpdateType.Update;
            InventoryGroup = new InventoryGroup()
            {
                InventoryGroupId = ((InventoryGroup)lstInventoryGroups.SelectedItem).InventoryGroupId,
                Name = ((InventoryGroup)lstInventoryGroups.SelectedItem).Name,
                Description = ((InventoryGroup)lstInventoryGroups.SelectedItem).Description,
            };
            gridAddEdit.BindingContext = InventoryGroup;
            gridAddEdit.IsVisible = true;
            gridInventoryItemList.IsVisible = false;
        }

        private void cmdAdd_Clicked(object sender, EventArgs e)
        {
            //UpdateInventoryGroupView view = new UpdateInventoryGroupView();
            //view.PageClosing += (sender2, e2) =>
            //{
            //    if (!view.IsCancelled)
            //        groups.Add(view.InventoryGroup);
            //};
            //OpenPage(view);

            updateType = UpdateType.Add;
            InventoryGroup = new InventoryGroup();
            gridAddEdit.BindingContext = InventoryGroup;
            gridAddEdit.IsVisible = true;
            gridInventoryItemList.IsVisible = false;
        }

        #region Add_Edit
        
        private async void cmdAddEditOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                InventoryGroup ig = new InventoryGroup()
                {
                    Name = InventoryGroup.Name,
                    Description = InventoryGroup.Description,
                };
                DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    switch (updateType)
                    {
                        case UpdateType.Add:

                            int id = await client.StoreInventorygroupPostAsync(ig.ToJsonString());
                            InventoryGroup.InventoryGroupId = id;

                            groups.Add(InventoryGroup);
                            UpdataView(); //TODO:needed because of bug? in xamarin forms, edited field does not display

                            break;

                        case UpdateType.Update:
                            ig.InventoryGroupId = InventoryGroup.InventoryGroupId;
                            await client.StoreInventorygroupPutAsync(ig.ToJsonString());

                            InventoryGroup selectedItem = (InventoryGroup)lstInventoryGroups.SelectedItem;
                            selectedItem.Name = InventoryGroup.Name;
                            selectedItem.Description = InventoryGroup.Description;
                            UpdataView(); //TODO:needed because of bug? in xamarin forms, edited field does not display

                            break;
                    }
                }

                gridAddEdit.IsVisible = false;
                gridInventoryItemList.IsVisible = true;
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
