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
using ZXing.Net.Mobile.Forms;
using Xamarin.Forms;
using ZXing;
using DCEMV.FormattingUtils;
using DCEMV.EMVProtocol.EMVQRCode;
using DCEMV_QRDEProtocol;

namespace DCEMV.DemoEMVApp
{
    public partial class EMVQRCodeView : ModalPage
    {
        public static Logger Logger = new Logger(typeof(EMVTx));

        //for EMVTxCtl
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;

        public EMVQRCodeView(ICardInterfaceManger contactCardInterfaceManger,
                     ICardInterfaceManger contactlessCardInterfaceManger,
                     IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            emvTxCtl.Init(contactCardInterfaceManger, SessionSingleton.ContactDeviceId,
                contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId,
                QRCodeMode.PresentAndPoll, "5906374433f04eb5b67d25c3e50487dc",
                configProvider, onlineApprover, tcpClientStream);
        }

        private void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            if ((e as TxCompletedEventArgs).QR_Data.IsPresent())
            {
                if ((e as TxCompletedEventArgs).TxResult == TxResult.QRCodeToPoll)
                {
                    //query if tx was approved
                    QRCodeApproverRequest auth = new QRCodeApproverRequest() { QRData = (e as TxCompletedEventArgs).QR_Data.Get() };
                    try
                    {
                        QRCodeApproverResponse response = (QRCodeApproverResponse)onlineApprover.DoCheckAuthStatus(auth);
                        emvTxCtl.SetTxFinalResultLabel("QRCode Online Check:" + response.ResponseMessage);
                    }
                    catch (Exception ex)
                    {
                        emvTxCtl.SetTxFinalResultLabel("QRCode Online Check:" + ex.Message);
                    }
                }
            }
        }
    }
}
