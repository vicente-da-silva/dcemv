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
using System;
using DCEMV.TLVProtocol;
using DCEMV.EMVProtocol.EMVQRCode;
using System.Threading.Tasks;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using DCEMV_QRDEProtocol;

namespace DCEMV.EMVProtocol
{

    public class EMVTerminalQRCodeScanApplication
    {
        public static Logger Logger = new Logger(typeof(EMVTerminalQRCodeScanApplication));

        protected TerminalConfigurationData terminalConfigurationData;
        public event EventHandler ProcessCompleted;
        public event EventHandler ExceptionOccured;

        //private readonly TransactionRequest tr;

        public EMVTerminalQRCodeScanApplication()
        {
            terminalConfigurationData = new TerminalConfigurationData();
        }

        public TLV GetDefaultTLV(string tag)
        {
            return terminalConfigurationData.TerminalConfigurationDataObjects.Get(tag);
        }

        public void StartTransactionRequest(ref TransactionRequest tr, string barcodeValue)
        {
            //unpack barcode
            QRDEList listOut = new QRDEList();
            listOut.Deserialize(barcodeValue);
            int depth = 0;
            Logger.Log("Barcode Scanned:");
            Logger.Log(listOut.ToPrintString(ref depth));

            long amount = Convert.ToInt64(listOut.Get(EMVQRTagsEnum.TRANSACTION_AMOUNT_54.Tag).Value);
            long amountOther = 0;
            tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);

            EMVTerminalProcessingOutcome processingOutcome = new EMVTerminalProcessingOutcome()
            {
                NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                UIRequestOnOutcomePresent = false,
                UIRequestOnRestartPresent = false,
                QRData = listOut,
            };

            OnProcessCompleted(processingOutcome);

        }
        public void CancelTransactionRequest()
        {
        }
        public void StopTerminalApplication()
        {
        }

        protected void OnExceptionOccured(Exception e)
        {
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }

        protected void OnProcessCompleted(EMVTerminalProcessingOutcome po)
        {
            try
            {
                TerminalProcessingOutcomeEventArgs tpo = new TerminalProcessingOutcomeEventArgs() { TerminalProcessingOutcome = po };
                ProcessCompleted?.Invoke(this, tpo);
            }
            catch (Exception ex)
            {
                Logger.Log("Error in OnProcessCompleted:" + ex.Message);
                return;
            }
        }
    }
}
