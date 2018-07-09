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
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_3_R1_CommonProcessing
    {
        public static SignalsEnum DoCommonProcessing(string source, Kernel2Database database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse)
        {
            #region S3R1.1
            TLV nextTLV = database.TagsToReadYet.GetNextGetDataTagFromList();
            if (nextTLV != null)
                database.ActiveTag = nextTLV.Tag.TagLable;
            else
                database.ActiveTag = null;

            if (database.ActiveTag != null)
            {
                #region S3R1.2
                EMVGetDataRequest request = new EMVGetDataRequest(Formatting.HexStringToByteArray(database.ActiveTag));
                #endregion
                #region S3R1.3
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                #endregion
                #region S3R1.3
                database.NextCommandEnum = NextCommandEnum.GET_DATA;
                #endregion
            }
            #endregion
            else
            {
                #region S3R1.5
                if (database.ActiveAFL == null)
                #endregion
                {
                    #region S3R1.6
                    return DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                    #endregion
                }
                else
                {
                    #region S3R1.7
                    EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                    #endregion
                    #region S3R1.8
                    cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                    #endregion
                    #region S3R1.9
                    database.NextCommandEnum = NextCommandEnum.READ_RECORD;
                    #endregion
                }
            }

            #region S3R1.10
            TLVList dataToSend = database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children;
            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
            if (ids.Value.IsRead) 
            #endregion
            {
                #region S3R1.11
                if (database.IsNotEmpty(EMVTagsEnum.DS_SLOT_AVAILABILITY_9F5F_KRN2.Tag))
                {
                    dataToSend.AddToList(database.Get(EMVTagsEnum.DS_SLOT_AVAILABILITY_9F5F_KRN2));
                }
                if (database.IsNotEmpty(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2.Tag))
                {
                    dataToSend.AddToList(database.Get(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2));
                }
                if (database.IsNotEmpty(EMVTagsEnum.DS_UNPREDICTABLE_NUMBER_9F7F_KRN2.Tag))
                {
                    dataToSend.AddToList(database.Get(EMVTagsEnum.DS_UNPREDICTABLE_NUMBER_9F7F_KRN2));
                }
                if (database.IsNotEmpty(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2.Tag))
                {
                    dataToSend.AddToList(database.Get(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2));
                }
                if (database.IsNotEmpty(EMVTagsEnum.DS_ODS_CARD_9F54_KRN2.Tag))
                {
                    dataToSend.AddToList(database.Get(EMVTagsEnum.DS_ODS_CARD_9F54_KRN2));
                }
                dataToSend.AddToList(database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN));
                #endregion
                #region S3R1.12
                if (!((database.IsNotEmpty(EMVTagsEnum.DS_SLOT_AVAILABILITY_9F5F_KRN2.Tag) &&
                   database.IsNotEmpty(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2.Tag) &&
                   database.IsNotEmpty(EMVTagsEnum.DS_UNPREDICTABLE_NUMBER_9F7F_KRN2.Tag) &&
                   database.IsNotPresent(EMVTagsEnum.DS_ODS_CARD_9F54_KRN2.Tag)) 
                   ||
                   (database.IsNotEmpty(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2.Tag) &&
                   database.IsPresent(EMVTagsEnum.DS_ODS_CARD_9F54_KRN2.Tag))
                   ))
                {
                    #region S3R1.13
                    ids = new IDS_STATUS_DF8128_KRN2(database);
                    ids.Value.IsRead = false;
                    ids.UpdateDB();
                    #endregion
                }
                #endregion
            }

            #region S3R1.14
            TLVList tagsToRemove = new TLVList();
            foreach (TLV ttry in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(ttry.Tag.TagLable))
                {
                    dataToSend.AddToList(ttry);
                    tagsToRemove.AddToList(ttry);
                }
            }
            foreach (TLV ttr in tagsToRemove)
                database.TagsToReadYet.RemoveFromList(ttr);

            #endregion

            #region S3R1.15
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) || 
                (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0))
            {
                #region S3R1.16
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion
            }
            #endregion

            #region S3R1.17
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);

            if(!(aip.Value.CDASupported && tc.Value.CDACapable))
            {
                #region S3R1.18
                ids = new IDS_STATUS_DF8128_KRN2(database);
                if (!ids.Value.IsRead)
                {
                    #region S3R1.20
                    TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                    tvr.Value.OfflineDataAuthenticationWasNotPerformed = true;
                    tvr.UpdateDB();
                    #endregion
                }
                else
                {
                    #region S3R1.19
                    database.ODAStatus = 0x80;
                    #endregion
                }
                #endregion
            }
            else
            {
                #region S3R1.19
                database.ODAStatus = 0x80;
                #endregion
            }
            #endregion

            if(database.NextCommandEnum == NextCommandEnum.READ_RECORD)
                return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
            else
                return SignalsEnum.WAITING_FOR_GET_DATA_RESPONSE;
        }

        public static SignalsEnum DoInvalidReponse(Kernel2Database database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            #region 3.90.1, 3.90.2
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true, KernelMessageidentifierEnum.ERROR_OTHER_CARD, 
                l1Enum, 
                null,
                l2Enum,
                l3Enum);
            #endregion
        }
    }
}
