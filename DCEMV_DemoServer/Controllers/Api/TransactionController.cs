﻿/*
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DCEMV.FormattingUtils;
using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;
using DCEMV.TLVProtocol;
using DCEMV.SPDHProtocol;
using DCEMV.Shared;
using Newtonsoft.Json;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.SimulatedPaymentProvider;
using DCEMV.EMVSecurity;

namespace DCEMV.DemoServer.Controllers.Api
{
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private static Logger Logger = new Logger(typeof(TransactionController));

        private readonly ITransactionsRepository _transactionRepository;
        private readonly IAccountsRepository _accountsRepository;

        private const string authIp = "192.168.0.100";
        private const int authPort = 6010;

        private static byte[] arcApproved = FormattingUtils.Formatting.HexStringToByteArray("3030"); //approved code
        private static byte[] arcDeclined = FormattingUtils.Formatting.HexStringToByteArray("3035"); //declined code

        public TransactionController(
            ITransactionsRepository transactionRepository,
            IAccountsRepository accountsRepository)
        {
            _transactionRepository = transactionRepository;
            _accountsRepository = accountsRepository;

        }

        [HttpGet]
        [Route("transaction/gettransactionstate")]
        public bool GetTransactionState(string trackingId)
        {
            try
            {
                if(_transactionRepository.GetTransactionState(trackingId) == 0)
                    return false;
                else
                    return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        //[NoCache]
        [HttpPost]
        [Route("transaction/qrcodetransfer")]
        public void QRCodeTransfer(string json)
        {
            QRCodeTransferTransaction tx = QRCodeTransferTransaction.FromJsonString(json);

            if (tx.Amount == 0)
                throw new ValidationException("Invalid Amount");

            if (!Validate.GuidValidation(tx.AccountFrom))
                throw new ValidationException("Invalid AccountNumberFrom");
            if (!Validate.GuidValidation(tx.AccountTo))
                throw new ValidationException("Invalid AccountNumberTo");
            if (!Validate.GuidValidation(tx.TrackingId))
                throw new ValidationException("Invalid TrackingId");

            TransactionPM tpm = new TransactionPM()
            {
                Amount = tx.Amount,
                TransactionType = TransactionType.SendMoneyFromAppToCard,
                AccountNumberIdFromRef = tx.AccountFrom,
                AccountNumberIdToRef = tx.AccountTo,
                TrackingId = tx.TrackingId,
            };
            _transactionRepository.AddQRCodeBasedTransaction(tpm, GetCurrentUserId());
        }

        //[NoCache]
        [HttpPost]
        [Route("transaction/cardtransfer")]
        public void CardTransfer(string json)
        {
            CardTransferTransaction tx = CardTransferTransaction.FromJsonString(json);

            if (tx.Amount == 0)
                throw new ValidationException("Invalid Amount");

            TLV tlv = TLVasJSON.FromJSON(tx.CardFromEMVData);

            byte[] arpc = VerifyCardSignature(tlv);
            if (arpc == null)
                throw new ValidationException("ARQC failure");

            //TODO: only accept transactions from DC EMV cards, not EMV cards

            switch (tx.TransactionType)
            {
                case TransactionType.SendMoneyFromAppToCard:

                    if (!Validate.GuidValidation(tx.AccountFrom))
                        throw new ValidationException("Invalid AccountNumberFrom");
                    if (String.IsNullOrEmpty(tx.CardSerialTo))
                        throw new ValidationException("Invalid CardSerialNumberTo");

                    if (!String.IsNullOrEmpty(tx.AccountTo))
                        throw new ValidationException("Invalid AccountNumberTo");
                    if (!String.IsNullOrEmpty(tx.CardSerialFrom))
                        throw new ValidationException("Invalid CardSerialNumberFrom");
                    break;

                case TransactionType.SendMoneyFromCardToApp:
                    if (!String.IsNullOrEmpty(tx.AccountFrom))
                        throw new ValidationException("Invalid AccountNumberFrom");
                    if (!String.IsNullOrEmpty(tx.CardSerialTo))
                        throw new ValidationException("Invalid CardSerialNumberTo");

                    if (!Validate.GuidValidation(tx.AccountTo))
                        throw new ValidationException("Invalid AccountNumberTo");
                    if (String.IsNullOrEmpty(tx.CardSerialFrom))
                        throw new ValidationException("Invalid CardSerialNumberFrom");
                    break;

                default:
                    throw new ValidationException("Invalid transaction type: " + tx.TransactionType);
            }

            TransactionPM tpm = new TransactionPM()
            {
                Amount = tx.Amount,
                TransactionType = tx.TransactionType,
                AccountNumberIdFromRef = tx.AccountFrom,
                AccountNumberIdToRef = tx.AccountTo,
                CardSerialNumberIdFrom = tx.CardSerialFrom,
                CardSerialNumberIdTo = tx.CardSerialTo,
                CardFromEMVData = tx.CardFromEMVData
            };

            _transactionRepository.AddCardBasedTransaction(tpm, GetCurrentUserId());
        }

        //[NoCache]
        [HttpPost]
        [Route("transaction/topup")]
        public void TopUp(string json)
        {
            CCTopUpTransaction tx = CCTopUpTransaction.FromJsonString(json);
            if (tx.Amount == 0)
                throw new ValidationException("Invalid Amount");

            if (string.IsNullOrEmpty(tx.CVV))
                throw new ValidationException("Invalid CVV");

            CCTopUpTransactionPM tpm = new CCTopUpTransactionPM()
            {
                Amount = tx.Amount,
                CVV = tx.CVV,
                EMV_Data = tx.EMV_Data
            };

            TLV EMV_Data = TLVasJSON.FromJSON(tpm.EMV_Data);

            //TODO: only accept transactions from EMV cards, not DC EMV cards

            //TODO: reject contact transactions, with ARQC, contact would have already been online via
            //AuthTransactionToIssuer

            //contactless online
            //if (((EMV_Data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.ARQC)
            //{
            //    try
            //    {
            //        ApproverResponse onlineResponse = GoOnline(
            //                                new ApproverRequest()
            //                                {
            //                                    EMV_Data = EMV_Data,
            //                                });

            //        if (!onlineResponse.IsApproved)
            //        {
            //            throw new ValidationException("Contactless Online Auth Declined");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new ValidationException("Contactless Online Auth Declined, Unable to go online:" + ex.Message);
            //    }
            //}

            bool isAccepted = AdviseTransactionToIssuer();
            if (!isAccepted)
            {
                throw new ValidationException("Advice Message not accepted");
            }

            _transactionRepository.AddTopUpTransaction(tpm, GetCurrentUserId());
        }

        [HttpPost]
        [Route("transaction/authtransactiontoissuer")]
        public string AuthTransactionToIssuer(string json)
        {
            ContactCardOnlineAuthRequest request = ContactCardOnlineAuthRequest.FromJsonString(json);
            ContactCardOnlineAuthResponse response = new ContactCardOnlineAuthResponse();
            try
            {
                EMVApproverResponse onlineResponse = GoOnline(
                                        new EMVApproverRequest()
                                        {
                                            EMV_Data = TLVasJSON.Convert(request.EMV_Data),
                                        });

                response.AuthCode_8A = TLVasJSON.Convert(onlineResponse.AuthCode_8A);
                response.IssuerAuthData_91 = TLVasJSON.Convert(onlineResponse.IssuerAuthData_91);
                response.ResponseMessage = onlineResponse.ResponseMessage;
                if (onlineResponse.IsApproved)
                    response.Response = ContactCardOnlineAuthResponseEnum.Approved;
                else
                    response.Response = ContactCardOnlineAuthResponseEnum.Declined;

                return JsonConvert.ToString(response.ToJsonString());
            }
            catch (Exception ex)
            {
                response.Response = ContactCardOnlineAuthResponseEnum.UnableToGoOnline;
                Logger.Log("Unable to go online:" + ex.Message);
                return JsonConvert.ToString(response.ToJsonString());
            }
        }

        public static byte[] VerifyCardSignature(TLV tlv)
        {
            CryptoMetaData cryptoMetaData = EMVDESSecurity.BuildCryptoMeta(tlv);

            //fire up HSM
            if (jcesecmod == null)
                jcesecmod = new EMVDESSecurity(lmkFilePath);

            TLV _8A;
            bool isApproved = true;
            if (isApproved)
                _8A = TLV.Create(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag, arcApproved);
            else
                _8A = TLV.Create(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag, arcDeclined);

            byte[] arpc = jcesecmod.VerifyCryptogramGenARPC(tlv, cryptoMetaData, _8A.Value, mkACEncrypted, mkACEncryptedCV);

            return arpc;
        }

        private EMVApproverResponse GoOnline(EMVApproverRequest request)
        {
            IOnlineApprover onlineApprover = new SPDHApprover(authIp, authPort);
            //IOnlineApprover onlineApprover = new ContactlessDummyOnlineApprover(authIp, authPort);
            //IOnlineApprover onlineApprover = new SimulatedApprover();
            EMVApproverResponse onlineResponse;
            using (TCPClientStream tcpClientStream = new TCPClientStreamNetCore())
            {
                request.TCPClientStream = tcpClientStream;
                onlineResponse = (EMVApproverResponse)onlineApprover.DoAuth(request);
            }
            return onlineResponse;
        }

        private bool AdviseTransactionToIssuer()
        {
            return true;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("sub").Value;
        }
    }
}
