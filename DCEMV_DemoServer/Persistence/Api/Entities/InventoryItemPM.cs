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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCEMV.DemoServer.Persistence.Api.Entities
{

    public class InventoryItemPM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryItemId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Barcode { get; set; }
        [Required]
        public long Price { get; set; }

        /***************************/
        //Foreign Keys
        //[Required]
        public int InventoryGroupIdRef { get; set; }
        //[ForeignKey("InventoryGroupId")]
        public InventoryGroupPM Group { get; set; }

        //[Required]
        public string AccountNumberIdRef { get; set; }
        //[ForeignKey("AccountNumberId")]
        public AccountPM Account { get; set; }
        /***************************/


        internal void Update(InventoryItemPM inventoryItem)
        {
            Name = inventoryItem.Name;
            Description = inventoryItem.Description;
            Barcode = inventoryItem.Barcode;
            InventoryGroupIdRef = inventoryItem.InventoryGroupIdRef;
            Group = null;
            Price = inventoryItem.Price;
        }
    }
}
