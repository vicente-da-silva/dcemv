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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DCEMV.DemoServer.Persistence.Api.Entities
{
   
    public class POSTransactionPM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int POSTransactionId { get; set; }
        
        [Required]
        public List<POSTransactionItemPM> POSTransactionItems { get; set; }

        [Required]
        public DateTime TransactionDateTime { get; set; }

        /***************************/
        //Foreign Keys
        //[Required]
        public string AccountNumberIdToRef { get; set; }
        //[ForeignKey("AccountNumberId")]
        public AccountPM AccountTo { get; set; }

        //[Required]
        public string AccountNumberIdFromRef { get; set; }
        //[ForeignKey("AccountNumberId")]
        public AccountPM AccountFrom { get; set; }

        //[Required]
        //public string TransactionId { get; set; }
        //[ForeignKey("TransactionId")]
        public TransactionPM Transaction { get; set; }
        public int TransactionIdRef { get; set; }
        /***************************/

        public POSTransactionPM()
        {
            POSTransactionItems = new List<POSTransactionItemPM>();
        }
    }
}
