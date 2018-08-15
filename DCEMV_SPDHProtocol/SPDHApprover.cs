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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using DCEMV.TCPIPDriver;
using DCEMV.TLVProtocol;

namespace DCEMV.SPDHProtocol
{

    public class SPDHApprover : IOnlineApprover
    {
        public static Logger Logger = new Logger(typeof(SPDHApprover));
        private string host;
        private int port;
        public SPDHApprover(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public ApproverResponseBase DoReversal(ApproverRequestBase request, bool isOnline)
        {
            throw new NotImplementedException();
        }

        public ApproverResponseBase DoAdvice(ApproverRequestBase request, bool isOnline)
        {
            throw new NotImplementedException();
        }

        public ApproverResponseBase DoCheckAuthStatus(ApproverRequestBase request)
        {
            throw new NotImplementedException();
        }

        public ApproverResponseBase DoAuth(ApproverRequestBase requestIn)
        {
            bool isMagStripe;
            EMVApproverRequest request = ((EMVApproverRequest)requestIn);
            TLV cryptogram = request.EMV_Data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag);
            if (cryptogram != null)
                isMagStripe = false;
            else
                isMagStripe = true;

            TransactionTypeEnum tt = (TransactionTypeEnum)Formatting.GetEnum(typeof(TransactionTypeEnum), request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag).Value[0]);

            TransactionBase np;
            switch (tt)
            {
                case TransactionTypeEnum.PurchaseGoodsAndServices:
                    np = new F00_NormalPurchase();
                    break;

                case TransactionTypeEnum.PurchaseWithCashback:
                    np = new F08_PurchaseWithCashBack();
                    break;

                case TransactionTypeEnum.Refund:
                    np = new F12_MerchandiseReturnAdjustment();
                    break;

                default:
                    throw new Exception("Unimplemented TransactionTypeEnum:" + tt);
            }

            np.SetHeaderValues(SPDHTransactionCode.NormalPurchase, SPDHMessageType.FinancialTransaction, SPDHMessageSubType.Online, DateTime.Now, "300047", "");
            
            np.Fids.Add(new FID_B_Amount1(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag).Value)));

            FIDBase fb_6 = new FIDBase('6', ' ', new byte[0]);
            np.Fids.Add(fb_6);

            if (tt == TransactionTypeEnum.PurchaseWithCashback || tt == TransactionTypeEnum.Refund)
            {
                np.Fids.Add(new FID_C_Amount2(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag).Value)));
            }
            if (isMagStripe)
            {
                np.Fids.Add(new FID_q_Track2_Customer(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRACK_2_DATA_9F6B_KRN2.Tag).Value)));
                fb_6.Children.Add(new FID_6_I_TransactionCurrencyCode(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag).Value)));
                fb_6.Children.Add(new FID_6_E_POSEntryMode(Formatting.ConvertToHexAscii(new byte[] { 0x91 })));
            }
            else
            {
                np.Fids.Add(new FID_q_Track2_Customer(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag).Value)));
                fb_6.Children.Add(new FID_6_I_TransactionCurrencyCode(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag).Value)));
                //fb_6.Children.Add(new FID_6_E_POSEntryMode(Formatting.ConvertToHexAscii(new byte[] { 0x07 })));
                fb_6.Children.Add(new FID_6_E_POSEntryMode(Formatting.ASCIIStringToByteArray("051")));
            }
            
            if (!isMagStripe)
            {
                List<byte[]> fidBytes = new List<byte[]>();
                byte[] smartCardScheme = new byte[] { 0x30, 0x31 };

                fidBytes.Add(smartCardScheme);
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value));
                fidBytes.Add(Formatting.ASCIIStringToByteArray(Formatting.ByteArrayToHexString(request.EMV_Data.Children.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN.Tag).Value).Substring(1)));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag).Value));
                fidBytes.Add(Formatting.ASCIIStringToByteArray(Formatting.ByteArrayToHexString(request.EMV_Data.Children.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag).Value).Substring(1)));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag).Value));

                fb_6.Children.Add(new FID_6_O_EMVRequestData(fidBytes.SelectMany(x => x).ToArray()));

                fidBytes = new List<byte[]>();
                fidBytes.Add(smartCardScheme);
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.APPLICATION_VERSION_NUMBER_TERMINAL_9F09_KRN.Tag).Value));
                fidBytes.Add(Formatting.ConvertToHexAscii(request.EMV_Data.Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag).Value));

                fb_6.Children.Add(new FID_6_P_EMVAdditionalRequestData(fidBytes.SelectMany(x => x).ToArray()));
            }

            Logger.Log(np.ToPrintString());

            bool check = np.Validate();
            byte[] received;

            using (request.TCPClientStream)
            {
                request.TCPClientStream.Connect(host,port);
                received = TCPIPManager.SendTransaction(request.TCPClientStream, np.Serialize());
            }

            int pos = 0;
            TransactionBase fb;
            switch (tt)
            {
                case TransactionTypeEnum.PurchaseGoodsAndServices:
                    fb = new F00_NormalPurchase();
                    break;

                case TransactionTypeEnum.PurchaseWithCashback:
                    fb = new F08_PurchaseWithCashBack();
                    break;

                case TransactionTypeEnum.Refund:
                    fb = new F12_MerchandiseReturnAdjustment();
                    break;

                default:
                    throw new Exception("Unimplemented TransactionTypeEnum:" + tt);
            }
            pos = fb.Deserialize(received, pos);

            Logger.Log(fb.ToPrintString());

            FIDBase responseMessageFid = fb.FindFid(FidMetaList.ResponseDisplay).Get();
            string responseMessage = Formatting.ByteArrayToASCIIString(responseMessageFid.Value);
            bool responseCode;
            int responseCodeAsNumber = Convert.ToInt32(Formatting.ByteArrayToASCIIString(fb.Header.GetValue(HeaderEntryEnum.ResponseCode)));
            if (responseCodeAsNumber >= 0 && responseCodeAsNumber <= 10)
                responseCode = true;
            else
                responseCode = false;

            FIDBase responseMessage6QFid = fb.FindFid(FidMetaList.EMVResponseData).Get();
            string responseMessage6Q = Formatting.ByteArrayToASCIIString(responseMessage6QFid.Value);
            string smartCardSchemeResponse = responseMessage6Q.Substring(0,2);
            string authResponseCode = "";
            string issuerAuthData;
            if (smartCardSchemeResponse == "00") //schem 1
            {
                issuerAuthData = responseMessage6Q.Substring(2);
            }
            else //01 == schem 2
            {
                authResponseCode = responseMessage6Q.Substring(2, 2);
                issuerAuthData = responseMessage6Q.Substring(4);
            }

            TLV authcodeTLV = null;
            if (!String.IsNullOrEmpty(authResponseCode))
                authcodeTLV = TLV.Create(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag, Formatting.ASCIIStringToByteArray(authResponseCode));

            TLV issuerAuthDataTLV = TLV.Create(EMVTagsEnum.ISSUER_AUTHENTICATION_DATA_91_KRN.Tag, Formatting.HexStringToByteArray(issuerAuthData));

            return new EMVApproverResponse()
            {
                IsApproved = responseCode,
                ResponseMessage = responseMessage,
                AuthCode_8A = authcodeTLV,
                IssuerAuthData_91 = issuerAuthDataTLV
            };
        }
    }
}
