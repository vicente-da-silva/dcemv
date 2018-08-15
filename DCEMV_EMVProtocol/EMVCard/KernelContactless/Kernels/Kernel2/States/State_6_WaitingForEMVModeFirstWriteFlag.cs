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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_6_WaitingForEMVModeFirstWriteFlag
    {
        public static SignalsEnum Execute(
           Kernel2Database database,
           KernelQ qManager,
           CardQ cardQManager,
           TornTransactionLogManager tornTransactionLogManager,
           Stopwatch sw)
        {
            KernelRequest kernel1Request = qManager.DequeueFromInput(false);

            switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
            {
                case KernelTerminalReaderServiceRequestEnum.STOP:
                    return EntryPointSTOP(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.DET:
                    return EntryPointDET(database, kernel1Request, qManager, cardQManager, tornTransactionLogManager, sw);

                case KernelTerminalReaderServiceRequestEnum.TIMEOUT:
                    return EntryPointTIMEOUT(database, qManager);

                default:
                    throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_6_WaitingForEMVModeFirstWriteFlag:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
            }
        }

        /*
         * S6.5 - S6.7
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw)
        {
            #region 6.6
            database.UpdateWithDETData(kernel1Request.InputData);
            #endregion

            #region 6.7
            sw.Stop();
            #endregion

            #region 6.8
            TLV nextTLV = database.TagsToReadYet.GetNextGetDataTagFromList();
            if (nextTLV != null)
                database.ActiveTag = nextTLV.Tag.TagLable;
            else
                database.ActiveTag = null;
            if (database.ActiveTag != null)
            #endregion
            {
                #region 6.9 - 6.12
                EMVGetDataRequest request = new EMVGetDataRequest(Formatting.HexStringToByteArray(database.ActiveTag));
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                database.NextCommandEnum = NextCommandEnum.GET_DATA;
                #endregion
            }
            else
            {
                #region 6.12
                database.NextCommandEnum = NextCommandEnum.NONE;
                #endregion
            }
            return State_4_5_6_CommonProcessing.DoCommonProcessing("State_6_WaitingForEMVModeFirstWriteFlag", database, qManager, cardQManager, sw, tornTransactionLogManager);
        }

        /*
        * S6.1, S6.3
        */
        private static SignalsEnum EntryPointTIMEOUT(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.TIME_OUT);
        }

        /*
         * S6.2 - 6.4
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
            
        }
    }
}
