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

    public class POSTransactionItemPM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int POSTransactionItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public long Amount { get; set; }

        /***************************/
        //Foreign Keys
        //[Required]
        public int POSTransactionIdRef { get; set; }
        //[ForeignKey("POSTransactionId")]
        public POSTransactionPM POSTransaction { get; set; }
        /***************************/
    }
}
