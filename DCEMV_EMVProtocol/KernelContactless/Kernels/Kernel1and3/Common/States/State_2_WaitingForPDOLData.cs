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
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Kernels.K1K3
{
    public static class State_2_WaitingForPDOLData
    {
        public static SignalsEnum Execute(
            KernelDatabaseBase database, 
            KernelQ qManager, 
            CardQ cardQManager, 
            Stopwatch sw)
        {
            KernelRequest kernel1Request = qManager.DequeueFromInput(false);

            switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
            {
                case KernelTerminalReaderServiceRequestEnum.STOP:
                    return EntryPointSTOP(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.TIMEOUT:
                    return EntryPointTIMEOUT(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.DET:
                    return EntryPointDET(database, kernel1Request, cardQManager, sw);

                default:
                    throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in Execute_1_Idle:" + Enum.GetName(typeof(KernelTerminalReaderServiceRequestEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
            }
        }

        private static SignalsEnum EntryPointDET(KernelDatabaseBase database, KernelRequest kernel1Request, CardQ cardQManager, Stopwatch sw)
        {
            database.UpdateWithDETData(kernel1Request.InputData);

            bool missingPDOLData = false;
            TLV _9f38 = database.Get(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN);
            TLVList pdolList = TLV.DeserializeChildrenWithNoV(_9f38.Value, 0);
            DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
            foreach (TLV tlv in pdolList)
            {
                if (database.IsEmpty(tlv.Tag.TagLable))
                {
                    missingPDOLData = true;
                    dataNeeded.Value.Tags.Add(tlv.Tag.TagLable);
                }
            }
            dataNeeded.UpdateDB();

            if (missingPDOLData)
                return SignalsEnum.WAITING_FOR_PDOL_DATA;

            database.Initialize(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2.Tag);
            CommonRoutines.PackRelatedDataTag(database,EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2, pdolList);
            EMVGetProcessingOptionsRequest request = new EMVGetProcessingOptionsRequest(database.Get(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2));
            sw.Stop();
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));

            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }

        private static SignalsEnum EntryPointSTOP(KernelDatabaseBase database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static SignalsEnum EntryPointTIMEOUT(KernelDatabaseBase database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.TIME_OUT);
            
        }
    }
}
