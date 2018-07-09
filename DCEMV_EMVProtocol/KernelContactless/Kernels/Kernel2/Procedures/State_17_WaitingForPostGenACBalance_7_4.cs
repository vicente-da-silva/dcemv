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
    public static class State_17_WaitingForPostGenACBalance_7_4
    {
        public static SignalsEnum Execute_State_17_WaitingForPostGenACBalance(
            KernelDatabaseBase database,
            KernelQ qManager,
            CardQ cardQManager)
        {
            if (qManager.GetOutputQCount() > 0) //there is a pending request to the terminal
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP();

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
                        return EntryPointL1RSP();

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * S17.2
        */
        private static SignalsEnum EntryPointRA(KernelDatabaseBase database, CardResponse cardResponse)
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
                database.Get(EMVTagsEnum.BALANCE_READ_AFTER_GEN_AC_DF8105_KRN2).Value = bal;
            }

            return SignalsEnum.NONE;
        }
        /*
         * S17.1
         */
        private static SignalsEnum EntryPointL1RSP()
        {
            return SignalsEnum.NONE;
        }
        /*
        * S17.4
        */
        private static SignalsEnum EntryPointDET()
        {
            return SignalsEnum.WAITING_FOR_POST_GEN_AC_BALANCE;
        }
        /*
         * S17.3
         */
        private static SignalsEnum EntryPointSTOP()
        {
            return SignalsEnum.WAITING_FOR_POST_GEN_AC_BALANCE;
        }

        
    }
}
