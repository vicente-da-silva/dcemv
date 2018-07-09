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
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum Kernel2OutcomeStatusEnum
    {
        APPROVED = 0x01,
        DECLINED = 0x02,
        ONLINE_REQUEST = 0x03,
        END_APPLICATION = 0x04,
        SELECT_NEXT = 0x05,
        TRY_ANOTHER_INTERFACE = 0x06,
        TRY_AGAIN = 0x07,
        N_A = 0x0F,
    }
    public enum Kernel2StartEnum
    {
        A = 0x00, //not implemented
        B = 0x01, //transmission error, restart kernel
        C = 0x02, //kernel wants to end and have preprocessing select another application if there is one
        D = 0x03, //not implemented
        N_A = 0x0F, //no error
    }
    public enum ACTypeEnum
    {
        AAC = 0x00,
        TC = 0x01,
        ARQC = 0x02,
    }
    public enum SignalsEnum
    {
        START,
        STOP,
        TEMINATE_ON_NEXT_RA,

        WAITING_FOR_EXCHANGE_RELAY_RESISTANCE_DATA_RESPONSE,
        WAITING_FOR_GPO_REPONSE,
        WAITING_FOR_PDOL_DATA,
        WAITING_FOR_GET_DATA_RESPONSE,
        WAITING_FOR_EMV_READ_RECORD_RESPONSE,
        WAITING_FOR_CVM_PROCESSING,
        WAITING_FOR_GET_CHALLENGE,
        WAITING_FOR_VERIFY,
        WAITING_FOR_PIN_RESPONSE,
        WAITING_FOR_GET_PIN_TRY_COUNTER,
        WAITING_FOR_TERMINAL_RISK_MANAGEMENT,
        WAITING_FOR_EMV_MODE_FIRST_WRITE_FLAG,
        WAITING_FOR_POST_GEN_AC_BALANCE,
        WAITING_FOR_PRE_GEN_AC_BALANCE,
        WAITING_FOR_RECOVER_AC,
        WAITING_FOR_GEN_AC_1,
        WAITING_FOR_GEN_AC_2,
        WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC,
        WAITING_FOR_PUT_DATA_RESPONSE_BEFORE_GEN_AC,
        WAITING_FOR_MAG_STRIPE_READ_RECORD_RESPONSE,
        WAITING_FOR_MAG_STRIPE_FIRST_WRITE_FLAG,
        WAITING_FOR_CCC_RESPONSE_1,
        WAITING_FOR_CCC_RESPONSE_2,
        WAITING_FOR_INTERNAL_AUTHENTICATE,
        WAITING_FOR_ONLINE_RESPONSE,
        WAITING_FOR_EXTERNAL_AUTHENTICATE,
        WAITING_FOR_SCRIPT_PROCESSING,

        NONE,
    }
    public enum ActionsEnum
    {
        Execute_Idle,
        Execute_WaitingForPDOLData,
        Execute_WaitingForGPOResponse,
        Execute_WaitingForExchangeRelayResistanceDataResponse,
        Execute_WaitingForEMVReadRecordResponse,
        Execute_WaitingForGetPinReponse,
        Execute_WaitingForGetPinTryCounter,
        Execute_WaitingForTerminalRiskManagement,
        Execute_WaitingForGetChallenge,
        Execute_WaitingForVerify,
        Execute_TerminateOnNextRA,
        Execute_WaitingForGetDataResponse,
        Execute_WaitingForCVMProcessing,
        Execute_WaitingForEMVModeFirstWriteFlag,
        Execute_WaitingForMagStripeReadRecordResponse,
        Execute_WaitingForMagStripeModeFirstWriteFlag,
        Execute_WaitingForGenerateACResponse_1,
        Execute_WaitingForRecoverACResponse,
        Execute_WaitingForGenerateACResponse_2,
        Execute_WaitingForPutDataResponseBeforeGenerateAC,
        Execute_WaitingForCCCResponse_1,
        Execute_WaitingForCCCResponse_2,
        Execute_WaitingForPutDataResponseAfterGenerateAC,
        Execute_WaitingForInternalAuthenticate,
        Execute_WaitingForOnlineAuth,
        Execute_WaitingForExternalAuthenticate,
        Execute_WaitingForScriptProcessing,

        Execute_EXIT,
    }

    public abstract class KernelBase : StateEngine, IKernel
    {
        public event EventHandler ExceptionOccured;
        public KernelQ KernelQ { get; }

        protected KernelDatabaseBase database;
        protected Stopwatch sw;
        protected CardQProcessor cardQProcessor;
        
        protected PublicKeyCertificateManager publicKeyCertificateManager;
        protected EntryPointPreProcessingIndicators processingIndicatorsForSelected;
        protected CardExceptionManager cardExceptionManager;
        protected IConfigurationProvider configProvider;

        public KernelBase(CardQProcessor cardQProcessor, PublicKeyCertificateManager publicKeyCertificateManager, EntryPointPreProcessingIndicators processingIndicatorsForSelected, CardExceptionManager cardExceptionManager, IConfigurationProvider configProvider)
        {
            KernelQ = new KernelQ(1000);
            this.cardQProcessor = cardQProcessor;
            this.publicKeyCertificateManager = publicKeyCertificateManager;
            this.processingIndicatorsForSelected = processingIndicatorsForSelected;
            this.cardExceptionManager = cardExceptionManager;
            this.configProvider = configProvider;
        }

        protected void OnExceptionOccured(Exception e)
        {
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }

        public virtual void StartNewTransaction()
        {
            try
            {
                KernelRequest kRequest = KernelQ.DequeueFromInput(true);

                switch (kRequest.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.ACT:
                        sw = new Stopwatch();
                        DoStateChange(SignalsEnum.START);
                        break;

                    default:
                        throw new Exception("Unknown Kernel1ReaderTerminalServiceResponseEnum");
                }
            }
            catch (Exception ex)
            {
                OnExceptionOccured(ex);
            }
        }

        protected override ActionsEnum MapSignalToAction(SignalsEnum signal)
        {
            switch (signal)
            {
                case SignalsEnum.START:
                    return ActionsEnum.Execute_Idle;
                case SignalsEnum.STOP:
                    return ActionsEnum.Execute_EXIT;
                case SignalsEnum.TEMINATE_ON_NEXT_RA:
                    return ActionsEnum.Execute_TerminateOnNextRA;
                case SignalsEnum.WAITING_FOR_EXCHANGE_RELAY_RESISTANCE_DATA_RESPONSE:
                    return ActionsEnum.Execute_WaitingForExchangeRelayResistanceDataResponse;
                case SignalsEnum.WAITING_FOR_GPO_REPONSE:
                    return ActionsEnum.Execute_WaitingForGPOResponse;
                case SignalsEnum.WAITING_FOR_PDOL_DATA:
                    return ActionsEnum.Execute_WaitingForPDOLData;
                case SignalsEnum.WAITING_FOR_GET_DATA_RESPONSE:
                    return ActionsEnum.Execute_WaitingForGetDataResponse;
                case SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE:
                    return ActionsEnum.Execute_WaitingForEMVReadRecordResponse;
                case SignalsEnum.WAITING_FOR_EMV_MODE_FIRST_WRITE_FLAG:
                    return ActionsEnum.Execute_WaitingForEMVModeFirstWriteFlag;
                //these signals are only used within the procedures, and dealt with locally
                //case SignalsEnum.WAITING_FOR_POST_GEN_AC_BALANCE:
                //    return ActionsEnum.
                //case SignalsEnum.WAITING_FOR_PRE_GEN_AC_BALANCE:
                //    return ActionsEnum.
                case SignalsEnum.WAITING_FOR_RECOVER_AC:
                    return ActionsEnum.Execute_WaitingForRecoverACResponse;
                case SignalsEnum.WAITING_FOR_GEN_AC_1:
                    return ActionsEnum.Execute_WaitingForGenerateACResponse_1;
                case SignalsEnum.WAITING_FOR_GEN_AC_2:
                    return ActionsEnum.Execute_WaitingForGenerateACResponse_2;
                case SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC:
                    return ActionsEnum.Execute_WaitingForPutDataResponseAfterGenerateAC;
                case SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_BEFORE_GEN_AC:
                    return ActionsEnum.Execute_WaitingForPutDataResponseBeforeGenerateAC;
                case SignalsEnum.WAITING_FOR_MAG_STRIPE_READ_RECORD_RESPONSE:
                    return ActionsEnum.Execute_WaitingForMagStripeReadRecordResponse;
                case SignalsEnum.WAITING_FOR_MAG_STRIPE_FIRST_WRITE_FLAG:
                    return ActionsEnum.Execute_WaitingForMagStripeModeFirstWriteFlag;
                case SignalsEnum.WAITING_FOR_CCC_RESPONSE_1:
                    return ActionsEnum.Execute_WaitingForCCCResponse_1;
                case SignalsEnum.WAITING_FOR_CCC_RESPONSE_2:
                    return ActionsEnum.Execute_WaitingForCCCResponse_2;
                case SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE:
                    return ActionsEnum.Execute_WaitingForInternalAuthenticate;
                case SignalsEnum.WAITING_FOR_GET_CHALLENGE:
                    return ActionsEnum.Execute_WaitingForGetChallenge;
                case SignalsEnum.WAITING_FOR_VERIFY:
                    return ActionsEnum.Execute_WaitingForVerify;
                case SignalsEnum.WAITING_FOR_PIN_RESPONSE:
                    return ActionsEnum.Execute_WaitingForGetPinReponse;
                case SignalsEnum.WAITING_FOR_GET_PIN_TRY_COUNTER:
                    return ActionsEnum.Execute_WaitingForGetPinTryCounter;
                case SignalsEnum.WAITING_FOR_CVM_PROCESSING:
                    return ActionsEnum.Execute_WaitingForCVMProcessing;
                case SignalsEnum.WAITING_FOR_TERMINAL_RISK_MANAGEMENT:
                    return ActionsEnum.Execute_WaitingForTerminalRiskManagement;
                case SignalsEnum.WAITING_FOR_ONLINE_RESPONSE:
                    return ActionsEnum.Execute_WaitingForOnlineAuth;
                case SignalsEnum.WAITING_FOR_EXTERNAL_AUTHENTICATE:
                    return ActionsEnum.Execute_WaitingForExternalAuthenticate;
                case SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING:
                    return ActionsEnum.Execute_WaitingForScriptProcessing;

                default:
                    throw new EMVProtocolException("Invalid SignalsEnum value:" + signal);
            }
        }

        protected static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}
