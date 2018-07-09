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
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Persistence.Api.Entities
{
    
   
    public class TransactionPM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public string CardSerialNumberIdFrom { get; set; }
        public string CardSerialNumberIdTo { get; set; }
        [Required]
        public long Amount { get; set; }
        [Required]
        public DateTime TransactionDateTime { get; set; }
        [Required]
        public TransactionType TransactionType { get; set; }
        [Required]
        [FromForm]
        public string CardFromEMVData { get; set; }

        /***************************/
        //Foreign Keys
        //[Required]
        public string AccountNumberIdFromRef { get; set; }
        //[ForeignKey("AccountNumberIdFrom")]
        public AccountPM AccountFrom { get; set; }

        //[Required]
        public string AccountNumberIdToRef { get; set; }
        //[ForeignKey("AccountNumberIdTo")]
        public AccountPM AccountTo { get; set; }

        public POSTransactionPM POSTransaction { get; set; }
        /***************************/
    }
}
