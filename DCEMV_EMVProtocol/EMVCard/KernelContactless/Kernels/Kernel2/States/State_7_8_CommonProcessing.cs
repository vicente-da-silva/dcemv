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
using System.Diagnostics;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_7_8_CommonProcessing
    {
        private static SignalsEnum Do78_3(string source, Kernel2Database database, KernelQ qManager, Stopwatch sw)
        {
            #region 78.3
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                    database.TagsToReadYet.RemoveFromList(tlv);
                }
            }
            #endregion
            #region 78.4
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) ||
                database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) &&
                database.TagsToReadYet.Count == 0
                )
            #endregion
            {
                #region 78.5
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion
            }
            
            #region 78.6
            sw.Start();
            #endregion
            return SignalsEnum.WAITING_FOR_MAG_STRIPE_FIRST_WRITE_FLAG;
        }
        internal static SignalsEnum DoCommonProcessing(string source, Kernel2Database database, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            if (database.IsEmpty(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag))
            {
                DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                #region 78.2
                dataNeeded.Value.Tags.Add(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag);
                #endregion
                dataNeeded.UpdateDB();

                return Do78_3(source, database, qManager, sw);
            }
            else
            {
                #region 78.7
                if (database.IsPresent(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag) &&
                    database.Get(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2).Value[0] == 0x00)
                #endregion
                {
                    #region 78.3
                    return Do78_3(source, database, qManager, sw);
                    #endregion
                }
                else
                {
                    #region 78.8
                    if (database.IsEmpty(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag))
                    #endregion
                    {
                        #region 78.9
                        CommonRoutines.CreateEMVDiscretionaryData(database);
                        return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.AMOUNT_NOT_PRESENT);
                        #endregion
                    }
                    #region 78.10
                    long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
                    long rctl = database.ReaderContactlessTransactionLismit;
                    if (aa > rctl)
                    #endregion
                    {
                        #region 78.11
                        CommonRoutines.CreateEMVDiscretionaryData(database);
                        return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.MAX_LIMIT_EXCEEDED, L3Enum.NOT_SET);
                        #endregion
                    }
                   
                    #region 78.12
                    foreach(TLV tlv in database.TagsToReadYet)
                    {
                        if (database.IsPresent(tlv.Tag.TagLable))
                            database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                        else
                            if (database.IsKnown(tlv.Tag.TagLable))
                                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(TLV.Create(tlv.Tag.TagLable));

                        database.TagsToReadYet.RemoveFromList(tlv);
                    }
                    #endregion

                    #region 78.13
                    if (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag))
                    #endregion
                    {
                        #region 78.14
                        CommonRoutines.PostDEK(database, qManager);
                        database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                        database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                        #endregion
                    }

                    #region 78.15
                    //The 8-nUN most significant digits must be set to zero
                    TLV rNN = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_NUMERIC_9F6A_KRN2);
                    if (rNN == null)
                    {
                        database.AddToList(TLV.Create(EMVTagsEnum.UNPREDICTABLE_NUMBER_NUMERIC_9F6A_KRN2.Tag));
                        rNN = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_NUMERIC_9F6A_KRN2);
                    }
                    rNN.Value = Formatting.GetRandomNumberNumeric(8-database.NUN);
                    #endregion

                    #region 78.16
                    APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
                    if (aip.Value.OnDeviceCardholderVerificationIsSupported)
                    #endregion
                    {
                        #region 78.19
                        if (aa > rctl)
                        #endregion
                        {
                            #region 78.20
                            byte msi = database.Get(EMVTagsEnum.MOBILE_SUPPORT_INDICATOR_9F7E_KRN2).Value[0];
                            msi = (byte)(msi | 0x02);
                            database.Get(EMVTagsEnum.MOBILE_SUPPORT_INDICATOR_9F7E_KRN2).Value[0] = msi;
                            #endregion
                        }

                        #region 78.21
                        EMVComputeCryptographicChecksumRequest request = new EMVComputeCryptographicChecksumRequest(CommonRoutines.PackUdolRelatedDataTag(database));
                        #endregion

                        #region 78.22
                        cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                        #endregion

                        return SignalsEnum.WAITING_FOR_CCC_RESPONSE_2;
                    }
                    else
                    {
                        #region 78.17
                        EMVComputeCryptographicChecksumRequest request = new EMVComputeCryptographicChecksumRequest(CommonRoutines.PackUdolRelatedDataTag(database));
                        #endregion

                        #region 78.17
                        cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                        #endregion

                        return SignalsEnum.WAITING_FOR_CCC_RESPONSE_1;
                    }
                }
            }
        }
        
       
    }
}
