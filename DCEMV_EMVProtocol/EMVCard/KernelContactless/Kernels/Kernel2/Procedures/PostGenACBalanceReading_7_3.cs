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

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class PostGenACBalanceReading_7_3
    {
        internal static SignalsEnum PostGenACBalanceReading(KernelDatabaseBase database, KernelQ qManager, CardQ cardQManager)
        {
            APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag) &&
                aci.Value.SupportForBalanceReading))
            {
                return SignalsEnum.NONE;
            }

            if (!database.IsPresent(EMVTagsEnum.BALANCE_READ_AFTER_GEN_AC_DF8105_KRN2.Tag))
            {
                return SignalsEnum.NONE;
            }

            EMVGetDataRequest request = new EMVGetDataRequest(Formatting.HexStringToByteArray(EMVTagsEnum.OFFLINE_ACCUMULATOR_BALANCE_9F50_KRN2.Tag));
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));

            SignalsEnum result = SignalsEnum.WAITING_FOR_POST_GEN_AC_BALANCE;
            while (result == SignalsEnum.WAITING_FOR_POST_GEN_AC_BALANCE)
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                result = State_17_WaitingForPostGenACBalance_7_4.Execute_State_17_WaitingForPostGenACBalance(database, qManager,cardQManager);
            }

            return result;
        }
    }
}


