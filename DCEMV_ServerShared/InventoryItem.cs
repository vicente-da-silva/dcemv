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

using Newtonsoft.Json;

namespace DCEMV.ServerShared
{
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public long Price { get; set; }

        public int InventoryGroupIdRef { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static InventoryItem FromJsonString(string jsonVal)
        {
            return JsonConvert.DeserializeObject<InventoryItem>(jsonVal);
        }
    }
}
