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

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_1_Idle
    {
        public static SignalsEnum Execute(
            KernelDatabase database,
            KernelQ qManager,
            CardQ cardQManager,
            Stopwatch sw)
        {
            if (qManager.GetInputQCount() == 0)
                throw new EMVProtocolException("Execute_1_Idle: Kernel Input Q empty");

            KernelRequest kernelRequest = qManager.DequeueFromInput(false);

            switch (kernelRequest.KernelTerminalReaderServiceRequestEnum)
            {
                case KernelTerminalReaderServiceRequestEnum.STOP:
                    return EntryPointSTOP(database, qManager);
                    
                case KernelTerminalReaderServiceRequestEnum.ACT:
                    return EntryPointACT(database, kernelRequest, qManager, cardQManager, sw);

                default:
                    throw new EMVProtocolException("Invalid KernelTerminalReaderServiceRequestEnum in Execute_1_Idle:" + Enum.GetName(typeof(KernelTerminalReaderServiceRequestEnum), kernelRequest.KernelTerminalReaderServiceRequestEnum));
            }
        }

      
        private static SignalsEnum EntryPointACT(KernelDatabase database, KernelRequest kernelRequest, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            foreach (TLV tlv in kernelRequest.InputData)
            {
                if (tlv.Tag.TagLable == EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                {
                    if (!database.ParseAndStoreCardResponse(tlv))
                    {
                        return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                    }
                }
                else
                {
                    if ((database.IsKnown(tlv.Tag.TagLable) || database.IsPresent(tlv.Tag.TagLable)) && EMVTagsEnum.DoesTagIncludesPermission(tlv.Tag.TagLable, UpdatePermissionEnum.ACT))
                        database.AddToList(tlv);
                }
            }

            database.AddToList(TLV.Create(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag, new byte[] { 0x00, 0x00, 0x00, 0x00 }));
            database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value = Formatting.GetRandomNumber();

            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvmr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);
            cvmr.UpdateDB();

            database.ACType = new DS_AC_TYPE_DF8108_KRN2(database);
            database.ACType.Value.DSACTypeEnum = ACTypeEnum.TC;

            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            tvr.UpdateDB();

            TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
            tsi.UpdateDB();

            TERMINAL_CAPABILITIES_9F33_KRN _9f33 = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            _9f33.UpdateDB();

            database.Initialize(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag);
            database.Initialize(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag);

            DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
            DATA_TO_SEND_FF8104_KRN2 dataToSend = new DATA_TO_SEND_FF8104_KRN2(database);

            database.TagsToReadYet.Initialize();

            if (database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2.Tag))
                database.TagsToReadYet.AddListToList(database.Get(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2).Children);
            else
            {
                dataNeeded.Value.AddTag(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2);
                dataNeeded.UpdateDB();
            }

            bool MissingPDOLDataFlag = false;

            TLV _9f38 = database.Get(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN);
            TLVList pdolList = TLV.DeserializeChildrenWithNoV(_9f38.Value, 0);
            foreach (TLV tlv in pdolList)
            {
                if (database.IsEmpty(tlv.Tag.TagLable))
                {
                    MissingPDOLDataFlag = true;
                    dataNeeded.Value.AddTag(tlv.Tag.TagLable);
                }
            }
            dataNeeded.UpdateDB();

            if (!MissingPDOLDataFlag)
            {
                database.Initialize(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2.Tag);
                CommonRoutines.PackRelatedDataTag(database, EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2, pdolList);
                EMVGetProcessingOptionsRequest request = new EMVGetProcessingOptionsRequest(database.Get(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2));
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            }

            TLVList toRemove = new TLVList();
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                    toRemove.AddToList(tlv);
                }
            }

            foreach (TLV tlv in toRemove)
                database.TagsToReadYet.RemoveFromList(tlv);

            dataNeeded.UpdateDB();

            if (MissingPDOLDataFlag)
            {
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();

                sw.Restart();
                return SignalsEnum.WAITING_FOR_PDOL_DATA;
            }

            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }
        

        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }
    }
}
