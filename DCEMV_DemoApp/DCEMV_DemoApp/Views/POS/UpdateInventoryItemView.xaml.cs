using FormattingUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XServerCommon.Models;
using XTerminal.Proxies;

namespace XTerminal
{
    public partial class UpdateInventoryItemView : ModalPage
    {
        public InventoryItemDetailViewModel InventoryItem { get; private set; }
        private UpdateType updateType;
        public bool IsCancelled { get; private set; }
        public UpdateInventoryItemView(InventoryItem item, ObservableCollection<InventoryGroup> groups)
        {
            InitializeComponent();
            updateType = UpdateType.Update;

            InventoryItem = new InventoryItemDetailViewModel()
            {
                InventoryItemId = item.InventoryItemId,
                Name = item.Name,
                Description = item.Description,
                Barcode = item.Barcode,
                Price = item.Price,
                InventoryGroupIdRef = item.InventoryGroupIdRef,
            };
            gridMain.BindingContext = InventoryItem;
            pickGroup.ItemsSource = groups;
            DetermineSelectedIndex(InventoryItem, pickGroup);
            gridProgress.IsVisible = false;
        }

        public UpdateInventoryItemView(ObservableCollection<InventoryGroup> groups)
        {
            InitializeComponent();
            updateType = UpdateType.Add;
            InventoryItem = new InventoryItemDetailViewModel();
            gridMain.BindingContext = InventoryItem;
            pickGroup.ItemsSource = groups;
            gridProgress.IsVisible = false;
        }


        private void DetermineSelectedIndex(InventoryItem item, Picker picker)
        {
            int counter = 0;
            foreach (InventoryGroup group in picker.ItemsSource)
            {
                if(group.InventoryGroupId == item.InventoryGroupIdRef)
                {
                    picker.SelectedIndex = counter;
                }
                counter++;
            }
        }
        
        private async void cmdOk_Clicked(object sender, EventArgs e)
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
                    Price = Validate.AmountToCents(InventoryItem.Price)
                };
                XServerApiClient client = SessionSingleton.GenXServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    switch (updateType)
                    {
                        case UpdateType.Add:
                            int id = await client.StoreInventoryitemPostAsync(ii.ToJsonString());
                            InventoryItem.InventoryItemId = id;
                            break;

                        case UpdateType.Update:
                            InventoryItem.InventoryGroupIdRef = group.InventoryGroupId;
                            ii.InventoryItemId = InventoryItem.InventoryItemId;
                            await client.StoreInventoryitemPutAsync(ii.ToJsonString());
                            break;
                    }
                }
                IsCancelled = false;
                ClosePage();
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
            IsCancelled = true;
            ClosePage();
        }
    }
}
