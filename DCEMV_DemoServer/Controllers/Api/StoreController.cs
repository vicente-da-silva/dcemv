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
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using DCEMV.FormattingUtils;
using System;
using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;
using DCEMV.TLVProtocol;
using DCEMV.EMVProtocol.Kernels;
using Newtonsoft.Json;

namespace DCEMV.DemoServer.Controllers.Api
{
    //[Authorize]
    [AllowAnonymous]
    public class StoreController : ControllerBase
    {
        private IStoreRepository _posRepository;

        public StoreController(IStoreRepository posRepository)
        {
            _posRepository = posRepository;
        }

        [HttpGet]
        [Route("store/inventoryitems")]
        public List<string> GetInventoryItems()
        {
            List<InventoryItemPM> iis = _posRepository.GetInventoryItems(GetCurrentUserId());
            List<string> iids = new List<string>();

            iis.ForEach(x =>
            {
                InventoryItem iid = new InventoryItem()
                {
                    Barcode = x.Barcode,
                    Description = x.Description,
                    InventoryGroupIdRef = x.InventoryGroupIdRef,
                    InventoryItemId = x.InventoryItemId,
                    Name = x.Name,
                    Price = x.Price,
                };

                iids.Add(iid.ToJsonString());
            });

            return iids;
        }

        [HttpGet]
        [Route("store/inventoryitem")]
        public string GetInventoryItem(int id)
        {
            InventoryItemPM x = _posRepository.GetInventoryItem(id, GetCurrentUserId());
            return JsonConvert.ToString(new InventoryItem()
            {
                Barcode = x.Barcode,
                Description = x.Description,
                InventoryGroupIdRef = x.InventoryGroupIdRef,
                InventoryItemId = x.InventoryItemId,
                Name = x.Name,
                Price = x.Price,
            }.ToJsonString());
        }

        [HttpPost]
        [Route("store/inventoryitem")]
        public int AddInventoryItem(string json)
        {
            InventoryItem ii = InventoryItem.FromJsonString(json);

            if (!Validate.NameValidation(ii.Name))
                throw new ValidationException("Invalid name");

            if (!Validate.NameValidation(ii.Description))
                throw new ValidationException("Invalid description");

            if (!Validate.NumberValidation(ii.Barcode))
                throw new ValidationException("Invalid barcode");

            InventoryItemPM item = new InventoryItemPM()
            {
                Name = ii.Name,
                Description = ii.Description,
                Barcode = ii.Barcode,
                Price = ii.Price,
                InventoryGroupIdRef = ii.InventoryGroupIdRef,
            };

            return _posRepository.AddInventoryItem(item, GetCurrentUserId());
        }

        [HttpPut]
        [Route("store/inventoryitem")]
        public void UpdateInventoryItem(string json)
        {
            InventoryItem ii = InventoryItem.FromJsonString(json);

            if (!Validate.NameValidation(ii.Name))
                throw new ValidationException("Invalid name");

            if (!Validate.NameValidation(ii.Description))
                throw new ValidationException("Invalid description");

            if (!Validate.NumberValidation(ii.Barcode))
                throw new ValidationException("Invalid barcode");

            InventoryItemPM pm = new InventoryItemPM()
            {
                InventoryItemId = ii.InventoryItemId,
                Name = ii.Name,
                Description = ii.Description,
                Barcode = ii.Barcode,
                Price = ii.Price,
                InventoryGroupIdRef = ii.InventoryGroupIdRef,
            };

            _posRepository.UpdateInventoryItem(pm, GetCurrentUserId());
        }

        [HttpDelete]
        [Route("store/inventoryitem")]
        public void DeleteInventoryItem(int id)
        {
            _posRepository.DeleteInventoryItem(id, GetCurrentUserId());
        }


        [HttpGet]
        [Route("store/inventorygroups")]
        public List<string> GetInventoryGroups()
        {
            List<InventoryGroupPM> iis = _posRepository.GetInventoryGroups(GetCurrentUserId());
            List<string> iids = new List<string>();

            iis.ForEach(x =>
            {
                InventoryGroup iid = new InventoryGroup()
                {
                    Description = x.Description,
                    InventoryGroupId = x.InventoryGroupId,
                    Name = x.Name,
                };

                iids.Add(iid.ToJsonString());
            });

            return iids;
        }

        [HttpGet]
        [Route("store/inventorygroup")]
        public string GetInventoryGroup(int id)
        {
            InventoryGroupPM x = _posRepository.GetInventoryGroup(id, GetCurrentUserId());
            return JsonConvert.ToString(new InventoryGroup()
            {
                Description = x.Description,
                InventoryGroupId = x.InventoryGroupId,
                Name = x.Name,
            }.ToJsonString());
        }

        [HttpPost]
        [Route("store/inventorygroup")]
        public int AddInventoryGroup(string json)
        {
            InventoryGroup ig = InventoryGroup.FromJsonString(json);

            if (!Validate.NameValidation(ig.Name))
                throw new ValidationException("Invalid name");

            if (!Validate.NameValidation(ig.Description))
                throw new ValidationException("Invalid description");

            InventoryGroupPM pm = new InventoryGroupPM()
            {
                Name = ig.Name,
                Description = ig.Description,
            };

            return _posRepository.AddInventoryGroup(pm, GetCurrentUserId());
        }

        [HttpPut]
        [Route("store/inventorygroup")]
        public void UpdateInventoryGroup(string json)
        {
            InventoryGroup ig = InventoryGroup.FromJsonString(json);

            if (!Validate.NameValidation(ig.Name))
                throw new ValidationException("Invalid name");

            if (!Validate.NameValidation(ig.Description))
                throw new ValidationException("Invalid description");

            InventoryGroupPM pm = new InventoryGroupPM()
            {
                InventoryGroupId = ig.InventoryGroupId,
                Name = ig.Name,
                Description = ig.Description,
            };

            _posRepository.UpdateInventoryGroup(pm, GetCurrentUserId());
        }

        [HttpDelete]
        [Route("store/inventorygroup")]
        public void DeleteInventoryGroup(int id)
        {
            _posRepository.DeleteInventoryGroup(id, GetCurrentUserId());
        }

        //[HttpGet]
        //[Route("store/sale")]
        //public POSTransaction GetPOSTransaction(int id)
        //{
        //    return _posRepository.GetPOSTransaction(id, GetCurrentUserId());
        //}

        [HttpPost]
        [Route("store/sale")]
        public void AddPOSTransaction(string jsonTx, string jsonPosTx)
        {
            TransferTransaction transaction = TransferTransaction.FromJsonString(jsonTx);
            POSTransaction posDetail = POSTransaction.FromJsonString(jsonPosTx);

            if (transaction.Amount == 0)
                throw new ValidationException("Invalid Amount");

            //TODO: make sure data in EMV matches duplicate data fields in transaction
            TLV tlv = TLVasJSON.FromJSON(transaction.CardFromEMVData);
            TLV _9F02 = tlv.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag);
            long emvAmount = FormattingUtils.Formatting.BcdToLong(_9F02.Value);
            if (transaction.Amount != emvAmount)
                throw new ValidationException("Invalid Amount: Card does not match Cryptogram");

            if (TransactionController.VerifyCardSignature(tlv) == null)
                throw new ValidationException("Invalid Cryptogram");

            transaction.TransactionType = TransactionType.SendMoneyFromCardToApp;

            switch (transaction.TransactionType)
            {
                case TransactionType.SendMoneyFromCardToApp:
                    if (!String.IsNullOrEmpty(transaction.AccountFrom))
                        throw new ValidationException("Invalid AccountNumberFrom");
                    if (!String.IsNullOrEmpty(transaction.CardSerialTo))
                        throw new ValidationException("Invalid CardSerialNumberTo");

                    if (!Validate.GuidValidation(transaction.AccountTo))
                        throw new ValidationException("Invalid AccountNumberTo");
                    if (String.IsNullOrEmpty(transaction.CardSerialFrom))
                        throw new ValidationException("Invalid CardSerialNumberFrom");
                    break;

                default:
                    throw new ValidationException("Invalid transaction type: " + transaction.TransactionType);
            }

            if (posDetail.InvItems == null || posDetail.InvItems.Count == 0)
                throw new ValidationException("Invalid items");

            TransactionPM txpm = new TransactionPM()
            {
                TransactionType = transaction.TransactionType,
                AccountNumberIdFromRef = transaction.AccountFrom,
                AccountNumberIdToRef = transaction.AccountTo,
                CardSerialNumberIdFrom = transaction.CardSerialFrom,
                CardSerialNumberIdTo = transaction.CardSerialTo,
                Amount = transaction.Amount,
                CardFromEMVData = transaction.CardFromEMVData,
            };

            List<POSTransactionItemPM> items = new List<POSTransactionItemPM>();
            posDetail.InvItems.ForEach(x =>
            {
                POSTransactionItemPM tipm = new POSTransactionItemPM()
                {
                    Amount = x.Amount,
                    Name = x.Name,
                    Quantity = x.Quantity,
                    InventoryItemId = x.InventoryItemId,
                };
                items.Add(tipm);
            });

            POSTransactionPM posTxpm = new POSTransactionPM()
            {
                POSTransactionItems = items,
            };

            _posRepository.AddPOSTransaction(txpm, posTxpm, GetCurrentUserId());
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("sub").Value;
        }
    }
}
