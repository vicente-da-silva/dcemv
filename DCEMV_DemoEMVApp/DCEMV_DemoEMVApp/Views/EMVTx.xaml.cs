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
using DCEMV.Shared;
using DCEMV.EMVProtocol;
using DCEMV.EMVProtocol.Kernels;
using System;
using DCEMV.TLVProtocol;
using System.ComponentModel;
using DCEMV.TerminalCommon;

namespace DCEMV.DemoEMVApp
{
    public class TotalAmountViewModel : INotifyPropertyChanged
    {
        private string total;
        public string Total
        {
            get { return total; }
            set
            {
                total = value;
                OnPropertyChanged("Total");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public enum ViewState
    {
        Step1TransactDetails,
        Step2TapCard,
        Step3Summary
    }
    public partial class EMVTx : ModalPage
    {
        public static Logger Logger = new Logger(typeof(EMVTx));

        private TransactionRequest tr;
        private TotalAmountViewModel totalAmount;

        //for EMVTxCtl
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;

        public EMVTx(ICardInterfaceManger contactCardInterfaceManger,
                     ICardInterfaceManger contactlessCardInterfaceManger,
                     IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();
            
            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;

            gridProgress.IsVisible = false;
            totalAmount = new TotalAmountViewModel();
            totalAmount.Total = "";
            txtAmount.BindingContext = totalAmount;

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            totalAmount.Total = "1000";
            
            //if(!String.IsNullOrEmpty(totalAmount.Total))
            //{
            //    CmdNextToPaymentApp_Clicked(null,null);
            //}
            //else
            //{
                lblStatusAskAmount.Text = "Enter the amount below";
                UpdateView(ViewState.Step1TransactDetails);
            //}
        }
        
        #region UI Code
        private void UpdateView(ViewState viewState)
        {
            switch (viewState)
            {
                case ViewState.Step1TransactDetails:
                    gridTransactDetails.IsVisible = true;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step2TapCard:
                    gridTransactDetails.IsVisible = false;
                    emvTxCtl.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step3Summary:
                    gridTransactDetails.IsVisible = false;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;
            }
        }
        #endregion

        #region App Start, Stop and Events
        private void CmdNextToPaymentApp_Clicked(object sender, EventArgs e)
        {
            long amount;
            if (!Int64.TryParse(totalAmount.Total, out amount))
            {
                lblStatusAskAmount.Text = "Enter the amount below without decimals";
                return;
            }

            UpdateView(ViewState.Step2TapCard);
            long amountOther = 0;
            tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);

            emvTxCtl.Start(tr, contactCardInterfaceManger, SessionSingleton.ContactDeviceId, 
                contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId,
                configProvider, onlineApprover, tcpClientStream);
        }
        protected override void OnDisappearing()
        {
            emvTxCtl.Stop();//should already be stopped

            UpdateView(ViewState.Step1TransactDetails);
            
            base.OnDisappearing();
        }
        private void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            //(e as TxCompletedEventArgs).InterFaceType;
            //(e as TxCompletedEventArgs).TxResult;

            if ((e as TxCompletedEventArgs).TxResult == TxResult.Cancelled)
                UpdateView(ViewState.Step1TransactDetails);

            if((e as TxCompletedEventArgs).EMV_Data.IsPresent())
            {
                //the contact app would have already gone online, and the TxResult would be approved or declined
                if ((e as TxCompletedEventArgs).TxResult == TxResult.ContactlessOnline)
                {
                    ApproverRequest auth = new ApproverRequest() { EMV_Data = (e as TxCompletedEventArgs).EMV_Data.Get() };
                    try
                    {
                        ApproverResponse response = onlineApprover.DoAuth(auth);
                        emvTxCtl.SetTxFinalResultLabel("CTLS Online:" + response.ResponseMessage);
                    }
                    catch (Exception ex)
                    {
                        emvTxCtl.SetTxFinalResultLabel("CTLS Online:" + ex.Message);
                    }
                }
            }
        }
        #endregion
    }
}
