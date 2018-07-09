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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.TerminalCommon;

namespace DCEMV.DemoApp
{

    public class TransactionViewModel
    {
        public TxFlow TxFlow { get; set; }
        public DateTime DateTime {get;set;}
        public long Amount { get; set; }
        public bool HasPosData { get; set; }
        public bool IsTopUp { get; set; }
        public int TransactionId { get; set; }
        public int POSTransactionId { get; set; }

    }
    public partial class AccountTransactionsView : ModalPage
    {
        public AccountTransactionsView()
        {
            InitializeComponent();
            gridProgress.IsVisible = true;

            Task.Run(async () =>
            {
                Account account = await CallGetTransactionsWebService();
                DisplayTransactions(account);
            });
        }
        private void DisplayTransactions(Account account)
        {
            List<TransactionViewModel> consolidatedList = new List<TransactionViewModel>();

            account.TransferFromTransactions.ForEach(x=> 
            {
                consolidatedList.Add(new TransactionViewModel()
                {
                    Amount = -x.Amount,
                    DateTime = x.DateTime,
                    TxFlow = TxFlow.Out,
                    TransactionId = x.TransactionId,
                });
            });
            account.TransferToTransactions.ForEach(x =>
            {
                consolidatedList.Add(new TransactionViewModel()
                {
                    Amount = x.Amount,
                    DateTime = x.DateTime,
                    TxFlow = TxFlow.In,
                    TransactionId = x.TransactionId,
                });
            });
            account.TopUpTransactions.ForEach(x =>
            {
                consolidatedList.Add(new TransactionViewModel()
                {
                    Amount = x.Amount,
                    DateTime = x.DateTime,
                    TxFlow = TxFlow.In,
                    IsTopUp = true,
                });
            });

            account.POSTransactions.ForEach(x =>
            {
                TransactionViewModel tvm = consolidatedList.Find(y=> y.TransactionId == x.TransactionId);
                if (tvm != null)
                {
                    tvm.HasPosData = true;
                    tvm.POSTransactionId = x.POSTransactionId;
                }
            });
            
            Device.BeginInvokeOnMainThread(() =>
            {
                gridProgress.IsVisible = false;
                lblBalance.BindingContext = account;
                lstTransactions.ItemsSource = consolidatedList.OrderByDescending(x=>x.DateTime);
            });
        }

        private async Task<Account> CallGetTransactionsWebService()
        {
            try
            {
                Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    Proxies.Account ret = await client.TransactionsGetAsync();
                    return Account.FromJsonString(ret.ToJson());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
