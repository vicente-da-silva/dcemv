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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DCEMV.ServerShared;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{
    public class CacheProvider
    {
        private static readonly CacheProvider instance = new CacheProvider();

        public static CacheProvider Instance
        {
            get
            {
                return instance;
            }
        }

        private ObservableCollection<InventoryGroup> groups;
        private ObservableCollection<InventoryItemDetailViewModel> items;

        private CacheProvider()
        {
            groups = new ObservableCollection<InventoryGroup>();
            items = new ObservableCollection<InventoryItemDetailViewModel>();
        }
        public async Task<ObservableCollection<InventoryGroup>> GetInventoryGroups()
        {
            if (groups.Count == 0)
            {

                DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    ObservableCollection<string> list = await client.StoreInventorygroupsGetAsync();
                    ObservableCollection<InventoryGroup> listRet = new ObservableCollection<InventoryGroup>();
                    list.ToList().ForEach(z =>
                    {
                        InventoryGroup igi = InventoryGroup.FromJsonString(z);
                        listRet.Add(igi);
                    });
                    if (list == null)
                        return groups;
                    else
                        groups = listRet;
                }
            }

            return groups;
        }

        public async Task<ObservableCollection<InventoryItemDetailViewModel>> GetinventoryItems()
        {
            if (items.Count == 0)
            {
                DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    ObservableCollection<string> list = await client.StoreInventoryitemsGetAsync();
                    ObservableCollection<InventoryItemDetailViewModel> listRet = new ObservableCollection<InventoryItemDetailViewModel>();
                    list.ToList().ForEach(z =>
                    {
                        InventoryItem x = InventoryItem.FromJsonString(z);
                        InventoryItemDetailViewModel ivm = new InventoryItemDetailViewModel()
                        {
                            Barcode = x.Barcode,
                            Description = x.Description,
                            InventoryGroupIdRef = x.InventoryGroupIdRef,
                            InventoryItemId = x.InventoryItemId,
                            Name = x.Name,
                            Price = x.Price,
                        };
                        listRet.Add(ivm);
                    });

                    if (list == null)
                        return items;
                    else
                        items = listRet;
                }
            }

            return items;
        }
    }
}
