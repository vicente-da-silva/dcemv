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
using DCEMV.ISO7816Protocol;
using System;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_16_WaitingForPreGenACBalance_7_2
    {
        public static SignalsEnum Execute_State_16_WaitingForPreGenACBalance(
            Kernel2Database database, 
            KernelQ qManager,
            CardQ cardQManager)
        {
            if (qManager.GetOutputQCount() > 0) //there is a pending request to the terminal
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP(database, qManager);

                    case KernelTerminalReaderServiceRequestEnum.DET:
                        return EntryPointDET();

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * S16.4
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse)
        {
            if (!cardResponse.ApduResponse.Succeeded)
                return SignalsEnum.NONE;

            if (cardResponse.ApduResponse.ResponseData.Length == 9 &&
                cardResponse.ApduResponse.ResponseData[0] == 0x9F &&
                cardResponse.ApduResponse.ResponseData[1] == 0x50 &&
                cardResponse.ApduResponse.ResponseData[2] == 0x06)
            {
                byte[] bal = new byte[5];
                Array.Copy(cardResponse.ApduResponse.ResponseData, 3, bal, 0, bal.Length);
                database.Get(EMVTagsEnum.BALANCE_READ_BEFORE_GEN_AC_DF8104_KRN2).Value = bal;
            }

            return SignalsEnum.NONE;
        }
        /*
         * S16.1
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
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
                cardResponse.ApduResponse.SW12,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
        }
        /*
        * S16.5
        */
        private static SignalsEnum EntryPointDET()
        {
            return SignalsEnum.WAITING_FOR_PRE_GEN_AC_BALANCE;
        }
        /*
         * S16.6
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
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
