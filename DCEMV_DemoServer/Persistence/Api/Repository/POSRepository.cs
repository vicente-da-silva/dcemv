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
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using DCEMV.ServerShared;
using DCEMV.DemoServer.Persistence.Api.Entities;

namespace DCEMV.DemoServer.Persistence.Api.Repository
{
    public interface IStoreRepository
    {
        List<InventoryItemPM> GetInventoryItems(string credentialsId);
        InventoryItemPM GetInventoryItem(int id, string credentialsId);
        int AddInventoryItem(InventoryItemPM item, string credentialsId);
        void UpdateInventoryItem(InventoryItemPM item, string credentialsId);
        void DeleteInventoryItem(int id, string credentialsId);

        List<InventoryGroupPM> GetInventoryGroups(string credentialsId);
        InventoryGroupPM GetInventoryGroup(int id, string credentialsId);
        int AddInventoryGroup(InventoryGroupPM group, string credentialsId);
        void UpdateInventoryGroup(InventoryGroupPM group, string credentialsId);
        void DeleteInventoryGroup(int id, string credentialsId);

        POSTransactionPM GetPOSTransaction(int id, string credentialsId);
        void AddPOSTransaction(TransactionPM tx, POSTransactionPM txPos, string credentialsId);

    }


    public class StoreRepository : IStoreRepository
    {
        private readonly ApiDbContext _context = null;
        private IAccountsRepository _accountRepository;
        private ITransactionsRepository _transactionRepository;
        private ICardsRepository _cardsRepository;

        public StoreRepository(ApiDbContext context, IAccountsRepository accountRepository, ITransactionsRepository transactionRepository, ICardsRepository cardsRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _cardsRepository = cardsRepository;
        }

        public InventoryItemPM GetInventoryItem(int id, string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            return (from p in _context.InventoryItems
                    where p.AccountNumberIdRef == accountNumberIdRef && p.InventoryItemId == id
                    orderby p.Name
                    select p).First();
        }

        public List<InventoryItemPM> GetInventoryItems(string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            return (from p in _context.InventoryItems
                    where p.AccountNumberIdRef == accountNumberIdRef
                    orderby p.Name
                    select p).Take(50).ToList();
        }

        public int AddInventoryItem(InventoryItemPM item, string credentialsId)
        {
            item.AccountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            _context.InventoryItems.Add(item);
            _context.SaveChanges();
            return item.InventoryItemId;
        }

        public void UpdateInventoryItem(InventoryItemPM item, string credentialsId)
        {
            //will only get accounts for this user
            InventoryItemPM itemOld = GetInventoryItem(item.InventoryItemId, credentialsId);
            if (itemOld == null)
                throw new ValidationException("no item found");

            //InventoryItemPM item = new InventoryItemPM() { Name = name, Description = description, Barcode = barcode, Price = price, InventoryGroupIdRef = inventoryGroupId };
            itemOld.Update(item);
            _context.SaveChanges();
        }

        public void DeleteInventoryItem(int id, string credentialsId)
        {
            InventoryItemPM i = GetInventoryItem(id, credentialsId);
            if (i == null)
                throw new ValidationException("no item found");
            _context.InventoryItems.Remove(i);
            _context.SaveChanges();
        }

        public InventoryGroupPM GetInventoryGroup(int id, string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            return (from p in _context.InventoryGroups
                    where p.AccountNumberIdRef == accountNumberIdRef && p.InventoryGroupId == id
                    orderby p.Name
                    select p).First();
        }

        public List<InventoryGroupPM> GetInventoryGroups(string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            return (from p in _context.InventoryGroups
                    where p.AccountNumberIdRef == accountNumberIdRef
                    orderby p.Name
                    select p).Take(50).ToList();
        }

        public int AddInventoryGroup(InventoryGroupPM group, string credentialsId)
        {
            group.AccountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            _context.InventoryGroups.Add(group);
            _context.SaveChanges();
            return group.InventoryGroupId;
        }

        public void UpdateInventoryGroup(InventoryGroupPM group, string credentialsId)
        {
            InventoryGroupPM groupOld = GetInventoryGroup(group.InventoryGroupId, credentialsId);
            if (groupOld == null)
                throw new ValidationException("no group found");

            //InventoryGroupPM group = new InventoryGroupPM() { Name = name, Description = description };
            groupOld.Update(group);
            _context.SaveChanges();
        }

        public void DeleteInventoryGroup(int id, string credentialsId)
        {
            InventoryGroupPM g = GetInventoryGroup(id, credentialsId);
            if (g == null)
                throw new ValidationException("no group found");

            _context.InventoryGroups.Remove(g);
            _context.SaveChanges();
        }

        public POSTransactionPM GetPOSTransaction(int id, string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            return (from p in _context.POSTransactions
                    where p.AccountNumberIdToRef == accountNumberIdRef && p.POSTransactionId == id
                    select p).First();
        }

        public void AddPOSTransaction(TransactionPM tx, POSTransactionPM txPos, string credentialsId)
        {
            string accountNumberIdRef = _accountRepository.GetAccountForUser(credentialsId).AccountNumberId;

            if (tx.AccountNumberIdToRef != accountNumberIdRef)
                throw new ValidationException("Incorrect account to ref");

            tx.AccountNumberIdFromRef = _cardsRepository.GetCard(tx.CardSerialNumberIdFrom).AccountNumberIdRef;

            txPos.TransactionDateTime = DateTime.Now;
            tx.TransactionDateTime = txPos.TransactionDateTime;
            txPos.AccountNumberIdToRef = tx.AccountNumberIdToRef;
            txPos.AccountNumberIdFromRef = tx.AccountNumberIdFromRef;

            using (IDbContextTransaction dbTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    TransactionPM tpm = new TransactionPM()
                    {
                        Amount = tx.Amount,
                        TransactionType = TransactionType.SendMoneyFromCardToApp,//hardcoded
                        AccountNumberIdFromRef = tx.AccountNumberIdFromRef,
                        AccountNumberIdToRef = tx.AccountNumberIdToRef,
                        CardSerialNumberIdFrom = tx.CardSerialNumberIdFrom,
                        CardSerialNumberIdTo = tx.CardSerialNumberIdTo,
                        CardFromEMVData = tx.CardFromEMVData,

                    };
                    int id = _transactionRepository.AddTransaction(tpm, credentialsId, false);
                    txPos.TransactionIdRef = id;
                    _context.POSTransactions.Add(txPos);
                    _context.SaveChanges();
                    dbTransaction.Commit();
                }
                catch(Exception ex)
                {
                    dbTransaction.Rollback();
                    throw new TechnicalException("Error Occured during transaction db operation. Transaction Rolled Back:" + ex.Message);
                }
            }
        }

    }
}
