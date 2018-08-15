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
using System.Diagnostics;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_8_WaitingForMagStripeFirstWriteFlag
    {
        public static SignalsEnum Execute(
            Kernel2Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
            Stopwatch sw)
        {
            KernelRequest kernel2Request = qManager.DequeueFromInput(false);

            switch (kernel2Request.KernelTerminalReaderServiceRequestEnum)
            {
                case KernelTerminalReaderServiceRequestEnum.STOP:
                    return EntryPointSTOP(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.TIMEOUT:
                    return EntryPointTIMEOUT(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.DET:
                    return EntryPointDET(database, kernel2Request,qManager,cardQManager, sw);

                default:
                    throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_8_WaitingForMagStripeFirstWriteFlag:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel2Request.KernelTerminalReaderServiceRequestEnum));
            }
        }

        /*
        * 8.5
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel2Request, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            database.UpdateWithDETData(kernel2Request.InputData);
            sw.Stop();
            return State_7_8_CommonProcessing.DoCommonProcessing("State_8_WaitingForMagStripeFirstWriteFlag", database, qManager, cardQManager, sw);
        }
        /*
        * 8.1
        */
        private static SignalsEnum EntryPointTIMEOUT(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateMSDiscretionaryDataRecord(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.TIME_OUT);
        }
        /*
         * 8.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateMSDiscretionaryDataRecord(database);
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
