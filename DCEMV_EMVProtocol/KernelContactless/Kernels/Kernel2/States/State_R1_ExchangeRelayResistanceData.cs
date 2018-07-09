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
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_R1_ExchangeRelayResistanceData
    {
        public static SignalsEnum Execute(
            Kernel2Database database,
            KernelQ qManager, 
            CardQ cardQManager,
            Stopwatch sw)
        {
            if (qManager.GetOutputQCount() > 0) //there is a pending request to the terminal
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP(database, qManager, sw);

                    case KernelTerminalReaderServiceRequestEnum.DET:
                        return EntryPointDET(database,kernel1Request);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_R1_ExchangeRelayResistanceData:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager,cardQManager,sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager,sw);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_R1_ExchangeRelayResistanceData:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        /*
         * SR1.9 - SR1.32
         */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            #region SR1.10
            sw.Stop();
            #endregion

            #region SR1.11
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region SR1.12 - SR1.13
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
                #endregion
            }

            #region SR1.14
            bool parsingResult = false;
            EMVExchangeRelayResistanceDataResponse emverrdr = (EMVExchangeRelayResistanceDataResponse)cardResponse.ApduResponse;
            if ((cardResponse.ApduResponse.ResponseData.Length > 11 && cardResponse.ApduResponse.ResponseData[0] == 0x80))// && cardResponse.ApduResponse.ResponseData.Length == 10)
            {
                database.AddListToList(emverrdr.GetResponseTags());
                parsingResult = true;
            }
            #region SR1.15
            if (!parsingResult)
            #endregion
            {
                #region SR1.16, SR1.17
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
                    null,
                    L2Enum.PARSING_ERROR,
                    L3Enum.NOT_SET);
                #endregion
            }

            #region SR1.18
            int a = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.TERMINAL_EXPECTED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_CAPDU_DF8134_KRN2).Value);
            int b = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.TERMINAL_EXPECTED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8135_KRN2).Value);
            int c = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.DEVICE_ESTIMATED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8305_KRN2).Value);

            int measuredRelayResistanceProcessingTime = ((int)sw.ElapsedMilliseconds / 100) - a - Math.Min(c, b);
            #endregion

            int d = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.MINIMUM_RELAY_RESISTANCE_GRACE_PERIOD_DF8132_KRN2).Value);
            int e = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.MIN_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8303_KRN2).Value);

            #region SR1.19
            if (measuredRelayResistanceProcessingTime < (e - d))
            #endregion
            {
                #region SR1.20, SR1.21
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
                    null,
                    L2Enum.CARD_DATA_ERROR,
                    L3Enum.NOT_SET);
                #endregion
            }

            int g = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.MAX_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8304_KRN2).Value);
            int f = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.RRP_COUNTER_DF8307_KRN2).Value);
            #region SR1.22
            if ((f < 2) && (measuredRelayResistanceProcessingTime > (g + d)))
            #endregion
            {
                #region SR1.23 - SR1.27
                database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value = Formatting.GetRandomNumber();
                database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value;
                f++;
                database.Get(EMVTagsEnum.RRP_COUNTER_DF8307_KRN2).Value = BitConverter.GetBytes(f);
                EMVExchangeRelayResistanceDataRequest request = new EMVExchangeRelayResistanceDataRequest(database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value);
                sw.Start();
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_EXCHANGE_RELAY_RESISTANCE_DATA_RESPONSE;
                #endregion
            }

            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            #region SR1.28
            if (measuredRelayResistanceProcessingTime > (g + d))
            #endregion
            {
                #region SR1.29
                tvr.Value.RelayResistanceTimeLimitsExceeded = true;
                #endregion
            }

            int h = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.RELAY_RESISTANCE_TRANSMISSION_TIME_MISMATCH_THRESHOLD_DF8137_KRN2).Value);
            int i = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.RELAY_RESISTANCE_ACCURACY_THRESHOLD_DF8136_KRN2).Value);
            #region SR1.30
            if (((c / b) * 100 < h) || ((b / c) * 100 < h) || ((measuredRelayResistanceProcessingTime - e) > i))
            {
                tvr.Value.RelayResistanceThresholdExceeded = true;
            }
            else
            {
                tvr.Value.RelayResistancePerformedEnum = RelayResistancePerformedEnum.RRP_PERFORMED;
            }
            tvr.UpdateDB();
            #endregion
            #endregion

            return State_3_R1_CommonProcessing.DoCommonProcessing("State_R1_ExchangeRelayResistanceData", database, qManager, cardQManager, cardResponse);
        }
        /*
         * SR1.1 - SR1.2
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_EXCHANGE_RELAY_RESISTANCE_DATA_RESPONSE;
        }
        /*
         * SR1.3 - SR1.5
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, Stopwatch sw)
        {
            sw.Stop();

            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.TRY_AGAIN,
                KernelStatusEnum.READY_TO_READ,
                new byte[] { 0x00, 0x00, 0x00 },
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.B,
                false,
                KernelMessageidentifierEnum.TRY_AGAIN,
                L1Enum.NOT_SET,
                null,
                L2Enum.CARD_DATA_ERROR,
                L3Enum.NOT_SET);
        }
        /*
         * SR1.6 - SR1.8
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager, Stopwatch sw)
        {
            sw.Stop();

            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}
