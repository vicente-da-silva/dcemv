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
using DCEMV.FormattingUtils;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class TerminalActionAnalysis_7_8
    {
        public static Logger Logger = new Logger(typeof(TerminalActionAnalysis_7_8));

        public static ACTypeEnum TerminalActionAnalysis(KernelDatabaseBase database)
        {
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            ulong tvrAsNumber = Formatting.ConvertToInt64(tvr.Value.Value);
            ulong tacAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.TERMINAL_ACTION_CODE_DENIAL_DF8121_KRN2).Value);
            #region taa.1
            if (database.IsNotEmpty(EMVTagsEnum.ISSUER_ACTION_CODE_DENIAL_9F0E_KRN.Tag))
            #endregion
            {
                #region taa.4
                ulong iacAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.ISSUER_ACTION_CODE_DENIAL_9F0E_KRN).Value);
                if (!(
                    ((tacAsNumber | iacAsNumber ) & tvrAsNumber) == 0)
                    )
                #endregion
                {
                    #region taa.5
                    Logger.Log("Declining: !(((tacAsNumber | iacAsNumber) & tvrAsNumber) == 0)):" + ((tacAsNumber | iacAsNumber) & tvrAsNumber));
                    Logger.Log("tacAsNumber: " + tacAsNumber);
                    Logger.Log("iacAsNumber: " + iacAsNumber);
                    int depth = 0;
                    Logger.Log(tvr.ToPrintString(ref depth));
                    return ACTypeEnum.AAC;
                    #endregion
                }
            }
            else
            {
                #region taa.2
                if (!((tacAsNumber & tvrAsNumber) == 0))
                #endregion
                {
                    #region taa.3
                    Logger.Log("Declining: !((tacAsNumber & tvrAsNumber) == 0)");
                    Logger.Log("tacAsNumber: " + tacAsNumber);
                    int depth = 0;
                    Logger.Log(tvr.ToPrintString(ref depth));
                    return ACTypeEnum.AAC;
                    #endregion
                }
            }

            #region taa.4.1
            TERMINAL_TYPE_9F35_KRN tt = new TERMINAL_TYPE_9F35_KRN(database);
            if (tt.Value.TerminalType.Code == 0x11 || tt.Value.TerminalType.Code == 0x21 ||
                tt.Value.TerminalType.Code == 0x14 || tt.Value.TerminalType.Code == 0x24 ||
                tt.Value.TerminalType.Code == 0x34)
            #endregion
            {
                #region taa.4.2
                return ACTypeEnum.ARQC;
                #endregion
            }

            #region taa.6
            if (tt.Value.TerminalType.Code == 0x23 || tt.Value.TerminalType.Code == 0x26 ||
                tt.Value.TerminalType.Code == 0x36 || tt.Value.TerminalType.Code == 0x13 ||
                tt.Value.TerminalType.Code == 0x16)
            #endregion
            {
                #region taa.13
                if (database.IsNotEmpty(EMVTagsEnum.ISSUER_ACTION_CODE_DEFAULT_9F0D_KRN.Tag))
                #endregion
                {
                    ulong iacDefaultNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.ISSUER_ACTION_CODE_DEFAULT_9F0D_KRN).Value); 
                    ulong taDefaultAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.TERMINAL_ACTION_CODE_DEFAULT_DF8120_KRN2).Value);  
                    #region taa.16
                    if (((iacDefaultNumber | taDefaultAsNumber) & tvrAsNumber) == 0)
                    #endregion
                    {
                        #region taa.18
                        return ACTypeEnum.TC;
                        #endregion
                    }
                    else
                    {
                        #region taa.17
                        Logger.Log("Declining: ((iacDefaultNumber | taDefaultAsNumber) & tvrAsNumber) == 0");
                        Logger.Log("iacDefaultNumber: " + iacDefaultNumber);
                        Logger.Log("taDefaultAsNumber: " + taDefaultAsNumber);
                        int depth = 0;
                        Logger.Log(tvr.ToPrintString(ref depth));
                        return ACTypeEnum.AAC;
                        #endregion
                    }
                }
                else
                {
                    #region taa.14
                    if (tvrAsNumber == 0)
                    #endregion
                    {
                        #region taa.15
                        return ACTypeEnum.TC;
                        #endregion
                    }
                    else
                    {
                        #region taa.17
                        Logger.Log("Declining: tvrAsNumber == 0");
                        int depth = 0;
                        Logger.Log(tvr.ToPrintString(ref depth));
                        return ACTypeEnum.AAC;
                        #endregion
                    }
                }
            }
            else
            {
                #region taa.7
                if (database.IsNotEmpty(EMVTagsEnum.ISSUER_ACTION_CODE_ONLINE_9F0F_KRN.Tag))
                #endregion
                {
                    ulong iacOnlineNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.ISSUER_ACTION_CODE_ONLINE_9F0F_KRN).Value);
                    ulong tacOnlineAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.TERMINAL_ACTION_CODE_ONLINE_DF8122_KRN2).Value);
                    #region taa.10
                    if (((tacOnlineAsNumber | iacOnlineNumber ) & tvrAsNumber) == 0)
                    #endregion
                    {
                        #region taa.12
                        return ACTypeEnum.TC;
                        #endregion
                    }
                    else
                    {
                        #region taa.11
                        return ACTypeEnum.ARQC;
                        #endregion
                    }
                }
                else
                {
                    #region taa.8
                    if (tvrAsNumber == 0)
                    #endregion
                    {
                        #region taa.9
                        return ACTypeEnum.TC;
                        #endregion
                    }
                    else
                    {
                        #region taa.11
                        return ACTypeEnum.ARQC;
                        #endregion
                    }
                }
            }
        }
    }
}
