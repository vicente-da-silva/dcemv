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

namespace DCEMV.EMVProtocol.Kernels.K1K3
{
    public static class State_3_R1_CommonProcessing
    {
        public static SignalsEnum DoCommonProcessing(string source, KernelDatabaseBase database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse)
        {
            if (database.ActiveAFL == null)
            {
                return DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
            }
            else
            {
                EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                database.NextCommandEnum = NextCommandEnum.READ_RECORD;
            }
            
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) || 
                (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0))
            {
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
            }

            return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
        }

        public static SignalsEnum DoInvalidReponse(KernelDatabaseBase database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true, KernelMessageidentifierEnum.ERROR_OTHER_CARD, 
                L1Enum.NOT_SET, 
                null,
                L2Enum.NOT_SET,
                L3Enum.NOT_SET);
        }
    }
}
