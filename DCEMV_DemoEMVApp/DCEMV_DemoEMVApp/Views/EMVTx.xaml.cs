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
        StepTxCtl,
        StepSummary
    }
    public partial class EMVTx : ModalPage
    {
        public static Logger Logger = new Logger(typeof(EMVTx));

        private IOnlineApprover onlineApprover;

        public EMVTx(ICardInterfaceManger contactCardInterfaceManger,
                     ICardInterfaceManger contactlessCardInterfaceManger,
                     IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();
            gridProgress.IsVisible = false;
           
            this.onlineApprover = onlineApprover;

            emvTxCtl.Init(contactCardInterfaceManger, SessionSingleton.ContactDeviceId,
                contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId, 
                QRCodeMode.ScanAndProcess, "", 
                configProvider, onlineApprover, tcpClientStream);

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;
            UpdateView(ViewState.StepTxCtl);
        }
        
        #region UI Code
        private void UpdateView(ViewState viewState)
        {
            switch (viewState)
            {
                case ViewState.StepTxCtl:
                    emvTxCtl.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.StepSummary:
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;
            }
        }
        #endregion

        #region App Start, Stop and Events
        protected override void OnDisappearing()
        {
            emvTxCtl.Stop();//should already be stopped

            UpdateView(ViewState.StepTxCtl);
            
            base.OnDisappearing();
        }
        private void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            if ((e as TxCompletedEventArgs).TxResult == TxResult.Cancelled)
                UpdateView(ViewState.StepTxCtl);

            if ((e as TxCompletedEventArgs).EMV_Data.IsPresent())
            {
                //the contact app would have already gone online, and the TxResult would be approved or declined
                //the reason going online for contactless is handled differently to going online for
                //contact is that with contact we have to go online and then back to the card before we can
                //process the transaction on the backend, for contacless we can choose to go online and process
                //the transaction in one call
                if ((e as TxCompletedEventArgs).TxResult == TxResult.ContactlessOnline)
                {
                    EMVApproverRequest auth = new EMVApproverRequest() { EMV_Data = (e as TxCompletedEventArgs).EMV_Data.Get() };
                    try
                    {
                        EMVApproverResponse response = (EMVApproverResponse)onlineApprover.DoAuth(auth);
                        emvTxCtl.SetTxFinalResultLabel("CTLS Online:" + response.ResponseMessage);
                    }
                    catch (Exception ex)
                    {
                        emvTxCtl.SetTxFinalResultLabel("CTLS Online:" + ex.Message);
                    }
                }
            }
            if ((e as TxCompletedEventArgs).QR_Data.IsPresent())
            {
                if ((e as TxCompletedEventArgs).TxResult == TxResult.QRCodeScanned)
                {
                    //send qr code auth request
                    QRCodeApproverRequest auth = new QRCodeApproverRequest() { QRData = (e as TxCompletedEventArgs).QR_Data.Get() };
                    try
                    {
                        QRCodeApproverResponse response = (QRCodeApproverResponse)onlineApprover.DoAuth(auth);
                        emvTxCtl.SetTxFinalResultLabel("QRCode Online:" + response.ResponseMessage);
                    }
                    catch (Exception ex)
                    {
                        emvTxCtl.SetTxFinalResultLabel("QRCode Online:" + ex.Message);
                    }
                }
            }
        }
        #endregion
    }
}
