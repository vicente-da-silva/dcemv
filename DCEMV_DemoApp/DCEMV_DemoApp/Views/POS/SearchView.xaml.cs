using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XTerminal.Proxies;

namespace XTerminal
{
    public partial class SearchView : ModalPage
    {
        private List<InventoryItemDetailViewModel> inventoryItems;
        public bool IsCancelled { get; private set; }
        public InventoryItemDetailViewModel SelectedItem { get; private set; }

        public SearchView(List<InventoryItemDetailViewModel> items)
        {
            InitializeComponent();
            gridProgress.IsVisible = false;
            inventoryItems = items;
            lstInventoryItems.ItemsSource = inventoryItems;
        }

        public void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();

            if (search.Length < 3)
            {
                lstInventoryItems.ItemsSource = inventoryItems;
                return;
            }

            lstInventoryItems.ItemsSource = inventoryItems.Where(x => { return x.Name.ToLower().Contains(search) || x.Description.ToLower().Contains(search) || x.Barcode.Contains(search); }).ToList();
        }

        private void lstInventoryItems_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SelectedItem = (InventoryItemDetailViewModel)e.SelectedItem;
            IsCancelled = false;
            ClosePage();
        }

        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            IsCancelled = true;
            ClosePage();
        }
    }
}
