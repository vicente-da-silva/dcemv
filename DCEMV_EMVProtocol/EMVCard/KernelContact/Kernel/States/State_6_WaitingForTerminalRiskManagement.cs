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
using DCEMV.EMVProtocol.Contact;
using DCEMV.EMVProtocol.Kernels.K2;
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_6_WaitingForTerminalRiskManagement
    {
        public static SignalsEnum Execute(
           KernelDatabase database,
           KernelQ qManager,
           CardQ cardQManager,
           EMVSelectApplicationResponse emvSelectApplicationResponse,
           Stopwatch sw)
        {
            if (cardQManager.GetInputQCount() > 0) //there is a pending request to the card
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_6_WaitingForTerminalRiskManagement:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }

            }
            else
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP(database, qManager);

                    case KernelTerminalReaderServiceRequestEnum.DET:
                        return EntryPointDET(database, kernel1Request);

                    case KernelTerminalReaderServiceRequestEnum.TRM:
                        return EntryPointTRM(database, kernel1Request, cardQManager, qManager, emvSelectApplicationResponse);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_6_WaitingForTerminalRiskManagement:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
        }


        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            EMVGetDataResponse response = (EMVGetDataResponse)cardResponse.ApduResponse;
            if (!response.Succeeded)
            {
                if (response.SW1 == 0x6A && response.SW2 == 0x88) //data not found
                {
                    //do nothing
                    return SignalsEnum.WAITING_FOR_TERMINAL_RISK_MANAGEMENT;
                }
                else
                {
                    CommonRoutines.CreateEMVDiscretionaryData(database);
                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        KernelStatusEnum.NOT_READY,
                        null,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        L1Enum.NOT_SET,
                        cardResponse.ApduResponse.SW12,
                        L2Enum.STATUS_BYTES,
                        L3Enum.NOT_SET);
                }
            }
            if (response.GetResponseTag().Tag.TagLable == EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag)
                database.AddToList(response.GetResponseTag());
            if (response.GetResponseTag().Tag.TagLable == EMVTagsEnum.LAST_ONLINE_APPLICATION_TRANSACTION_COUNTER_ATC_REGISTER_9F13_KRN.Tag)
                database.AddToList(response.GetResponseTag());

            return SignalsEnum.WAITING_FOR_TERMINAL_RISK_MANAGEMENT;
        }

        private static SignalsEnum EntryPointTRM(KernelDatabase database, KernelRequest kernel1Request, CardQ cardQManager, KernelQ qManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
            tsi.Value.TerminalRiskmanagementWasPerformed = true;
            tsi.UpdateDB();

            //The state works as follows:
            //State_5_WaitingForCVMProcessing ends by possibly adding 2 messages to the card q
            //and 1 or 2 messages in the terminal q (get floor limit (optional) and do trm)
            //the card messages are processed by this state first, then the get floor limit by DEK if it
            //was in the q and then this method

            //Random Transaction Selection determined by terminal, not kernel
            //Terminal can also force transaction online
            //EMVTagsEnum.MAXIMUM_TARGET_PERCENTAGE_TO_BE_USED_FOR_BIASED_RANDOM_SELECTION_INTERNAL_KRN
            //EMVTagsEnum.THRESHOLD_VALUE_FOR_BIASED_RANDOM_SELECTION_INTERNAL_KRN
            //EMVTagsEnum.MAXIMUM_TARGET_PERCENTAGE_TO_BE_USED_FOR_BIASED_RANDOM_SELECTION_INTERNAL_KRN

            switch (((KernelTRMRequest)kernel1Request).KernelTRMRequestType)
            {
                case KernelTRMRequestType.GoOnlineForRandomSelection:
                    tvr.Value.TransactionSelectedRandomlyForOnlineProcessing = true;
                    tvr.UpdateDB();
                    break;
                case KernelTRMRequestType.GoOnline:
                    tvr.Value.MerchantForcedTransactionOnline = true;
                    tvr.UpdateDB();
                    break;
            }

            //Floor limit check done by kernel, no need for it to be done by terminal
            if (database.IsNotEmpty(EMVTagsEnum.TERMINAL_FLOOR_LIMIT_9F1B_KRN.Tag))
            {
                long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
                long fl = Formatting.BcdToLong(database.Get(EMVTagsEnum.TERMINAL_FLOOR_LIMIT_9F1B_KRN).Value);
                if (aa > fl)
                {
                    tvr.Value.TransactionExceedsFloorLimit = true;
                    tvr.UpdateDB();
                }
            }

            //Velocity Check
            TLV lcol = database.Get(EMVTagsEnum.LOWER_CONSECUTIVE_OFFLINE_LIMIT_9F14_KRN);
            TLV ucol = database.Get(EMVTagsEnum.UPPER_CONSECUTIVE_OFFLINE_LIMIT_9F23_KRN);
            TLV atcTLV = database.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
            TLV lastOnlineATCRegisterTLV = database.Get(EMVTagsEnum.LAST_ONLINE_APPLICATION_TRANSACTION_COUNTER_ATC_REGISTER_9F13_KRN.Tag);

            bool doVelocity = false;
            if (lcol != null && ucol != null)
            {
                if (database.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) != null &&
                    database.Get(EMVTagsEnum.LAST_ONLINE_APPLICATION_TRANSACTION_COUNTER_ATC_REGISTER_9F13_KRN.Tag) != null)
                {
                    if (Formatting.ConvertToInt32(atcTLV.Value) >
                        Formatting.ConvertToInt32(lastOnlineATCRegisterTLV.Value))
                    {
                        doVelocity = true;
                    }
                    else
                    {
                        // Set both the ‘Lower consecutive offline limit exceeded’ and the ‘Upper consecutive offline limit exceeded’ bits in the TVR to 1.
                        // Not set the ‘New card’ indicator in the TVR unless the Last Online ATC Register is returned and equals zero.
                        // End velocity checking for this transaction
                        tvr.Value.LowerConsecutiveOfflineLimitExceeded = true;
                        tvr.Value.UpperConsecutiveOfflineLimitExceeded = true;

                        if (lastOnlineATCRegisterTLV != null)
                        {
                            if (Formatting.ConvertToInt32(lastOnlineATCRegisterTLV.Value) == 0)
                            {
                                tvr.Value.NewCard = true;
                            }
                        }
                        tvr.UpdateDB();
                    }
                }
            }
            if (doVelocity == true)
            {
                uint atc = Formatting.ConvertToInt32(atcTLV.Value);
                uint lastATC = Formatting.ConvertToInt32(lastOnlineATCRegisterTLV.Value);

                uint atcDiff = atc - lastATC;
                uint lcolInt = Formatting.ConvertToInt32(lcol.Value);
                uint ucolInt = Formatting.ConvertToInt32(ucol.Value);
                if (atcDiff > lcolInt)
                {
                    //set ‘Lower consecutive offline limit exceeded’ bit in the TVR to 1
                    tvr.Value.LowerConsecutiveOfflineLimitExceeded = true;
                    if (atcDiff > ucolInt)
                    {
                        //set the ‘Upper consecutive offline limit exceeded’ bit in the TVR to 1.
                        tvr.Value.UpperConsecutiveOfflineLimitExceeded = true;
                    }
                }
                if (lastATC == 0)
                {
                    //set ‘New card’ bit in the TVR to 1.
                    tvr.Value.NewCard = true;
                }
                tvr.UpdateDB();
            }

            #region Book 3 Section 10.7
            //Terminal Action Analysis of kernel 2 being used
            database.ACType.Value.DSACTypeEnum = TerminalActionAnalysis_7_8.TerminalActionAnalysis(database);
            #endregion

            #region Book 3 Section 10.8
            //Card Action Analysis
            return CardActionAnalysis.InitiateCardActionAnalysis(database, qManager, cardQManager, emvSelectApplicationResponse);
            #endregion
        }

        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_TERMINAL_RISK_MANAGEMENT;
        }

        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
        {
            CommonRoutines.InitializeDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.TRY_AGAIN,
                KernelStatusEnum.READY_TO_READ,
                new byte[] { 0x00, 0x00, 0x00 },
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.B,
                false,
                KernelMessageidentifierEnum.TRY_AGAIN,
                L1Enum.NOT_SET,
                cardResponse.ApduResponse.SW12,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
        }

        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static bool DoDEKIfNeeded(KernelDatabaseBase database, KernelQ qManager)
        {
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) ||
                (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag)))
            {
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                return true;
            }
            return false;
        }


    }
}
