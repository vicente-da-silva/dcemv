using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XServerCommon.Models;
using XTerminal.Proxies;

namespace XTerminal
{
    public enum UpdateType
    {
        Add,
        Update
    }
    public partial class UpdateInventoryGroupView : ModalPage
    {

        public InventoryGroup InventoryGroup { get; private set; }
        private UpdateType updateType;
        public bool IsCancelled { get; private set; }

        public UpdateInventoryGroupView(InventoryGroup group)
        {
            InitializeComponent();
            updateType = UpdateType.Update;

            InventoryGroup = new InventoryGroup()
            {
                InventoryGroupId = group.InventoryGroupId,
                Name = group.Name,
                Description = group.Description,
            };
            gridMain.BindingContext = InventoryGroup;
            gridProgress.IsVisible = false;
        }

        public UpdateInventoryGroupView()
        {
            InitializeComponent();
            updateType = UpdateType.Add;
            InventoryGroup = new InventoryGroup();
            gridMain.BindingContext = InventoryGroup;
            gridProgress.IsVisible = false;
        }

        private async void cmdOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                InventoryGroup ig = new InventoryGroup()
                {
                    Name = InventoryGroup.Name,
                    Description = InventoryGroup.Description,
                };
                XServerApiClient client = SessionSingleton.GenXServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    switch (updateType)
                    {
                        case UpdateType.Add:
                           
                            int id = await client.StoreInventorygroupPostAsync(ig.ToJsonString());
                            InventoryGroup.InventoryGroupId = id;
                            break;

                        case UpdateType.Update:
                            ig.InventoryGroupId = InventoryGroup.InventoryGroupId;
                            await client.StoreInventorygroupPutAsync(ig.ToJsonString());
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
