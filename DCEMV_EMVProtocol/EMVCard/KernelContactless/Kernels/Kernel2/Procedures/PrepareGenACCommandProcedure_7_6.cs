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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class PrepareGenACCommandProcedure_7_6
    {
        private static ACTypeEnum GetDSACType(KernelDatabaseBase database)
        {
            return ((Kernel2Database)database).ACType.Value.DSACTypeEnum;
        }

        private static void SetDSACType(KernelDatabaseBase database, ACTypeEnum acTypeEnum)
        {
            ((Kernel2Database)database).ACType.Value.DSACTypeEnum = acTypeEnum;
        }
        private static byte GetODAStatus(KernelDatabaseBase database)
        {
            return ((Kernel2Database)database).ODAStatus;
        }
        private static void SetODAStatus(KernelDatabaseBase database, byte status)
        {
            ((Kernel2Database)database).ODAStatus = status;
        }

        internal static EMVGenerateACRequest PrepareGenACCommand(KernelDatabaseBase database, KernelQ qManager, CardQ cardQManager)
        {
            #region GAC.1
            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
            if (ids.Value.IsRead)
            #endregion
            {
                #region GAC.2
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                if (tvr.Value.CDAFailed)
                #endregion
                {
                    return DoGAC22_26(database);
                }
                else
                {
                    #region GAC.3
                    if (database.IsNotEmpty(EMVTagsEnum.DS_ODS_INFO_DF62_KRN2.Tag))
                    #endregion
                    {
                        #region GAC.4
                        if (database.IsNotEmpty(EMVTagsEnum.DSDOL_9F5B_KRN2.Tag))
                        {
                            #region GAC.5
                            if (database.IsNotEmpty(EMVTagsEnum.DS_AC_TYPE_DF8108_KRN2.Tag) && database.IsNotEmpty(EMVTagsEnum.DS_ODS_INFO_FOR_READER_DF810A_KRN2.Tag))
                            #endregion
                            {
                                #region GAC.7
                                DS_AC_TYPE_DF8108_KRN2 dsactype = new DS_AC_TYPE_DF8108_KRN2(database);
                                if (dsactype.Value.DSACTypeEnum == ACTypeEnum.AAC ||
                                    GetDSACType(database) == dsactype.Value.DSACTypeEnum ||
                                    (dsactype.Value.DSACTypeEnum == ACTypeEnum.ARQC && GetDSACType(database) == ACTypeEnum.TC))
                                {
                                    #region GAC.8
                                    SetDSACType(database, dsactype.Value.DSACTypeEnum);
                                    #endregion
                                    #region GAC.40
                                    return DoIDSWrite(database);
                                    #endregion
                                }
                                else
                                {
                                    #region GAC.9
                                    DS_ODS_INFO_FOR_READER_DF810A_KRN2 dsodsifr = new DS_ODS_INFO_FOR_READER_DF810A_KRN2(database);
                                    if ((GetDSACType(database) == ACTypeEnum.AAC && dsodsifr.Value.UsableForAAC) ||
                                        (GetDSACType(database) == ACTypeEnum.ARQC && dsodsifr.Value.UsableForARQC))
                                    #endregion
                                    {
                                        #region GAC.40
                                        return DoIDSWrite(database);
                                        #endregion
                                    }
                                    else
                                    {
                                        #region GAC.10
                                        if (dsodsifr.Value.StopIfNoDSODSTerm)
                                        #endregion
                                        {
                                            #region GAC.11
                                            return GenerateErrorOutput(database, L2Enum.IDS_NO_MATCHING_AC, qManager);
                                            #endregion
                                        }
                                        else
                                        {
                                            #region GAC.27
                                            return DoIDSReadOnly(database);
                                            #endregion
                                        }
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                #region GAC.6
                                return GenerateErrorOutput(database, L2Enum.IDS_DATA_ERROR, qManager);
                                #endregion
                            }
                        }
                        else
                        {
                            #region GAC.27
                            return DoIDSReadOnly(database);
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region GAC.27
                        return DoIDSReadOnly(database);
                        #endregion
                    }
                }
            }
            else
            {
                #region GAC.20
                return DoNoIDS(database);
                #endregion
            }
        }
        private static EMVGenerateACRequest DoGAC26(KernelDatabaseBase database)
        {
            #region GAC.26
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcp.Value.ACTypeEnum = GetDSACType(database);
            rcp.UpdateDB();
            return DoPart3(database);
            #endregion
        }
        private static EMVGenerateACRequest DoGAC22_26(KernelDatabaseBase database)
        {
            #region GAC.22
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            KERNEL_CONFIGURATION_DF811B_KRN2 kc = new KERNEL_CONFIGURATION_DF811B_KRN2(database);
            if (aip.Value.OnDeviceCardholderVerificationIsSupported && kc.Value.OnDeviceCardholderVerificationSupported)
            #endregion
            {
                #region GAC.23
                SetDSACType(database, ACTypeEnum.AAC);
                #endregion
            }

            return DoGAC26(database);
        }
        private static EMVGenerateACRequest DoNoIDS(KernelDatabaseBase database)
        {
            #region GAC.20
            if (GetODAStatus(database) == 0x80)
            {
                #region GAC.21
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                if (tvr.Value.CDAFailed)
                #endregion
                {
                    return DoGAC22_26(database);
                }
                else
                {
                    #region GAC.24
                    if (GetDSACType(database) == ACTypeEnum.AAC)
                    #endregion
                    {
                        APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                        #region GAC.25
                        if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag) &&
                            aci.Value.CDAIndicatorEnum == CDAIndicatorEnum.CDA_SUPPORTED_OVER_TC_ARQC_AAC)
                        {
                            #region GAC.27
                            return DoIDSReadOnly(database);
                            #endregion
                        }
                        else
                        {
                            return DoGAC26(database);
                        }
                        #endregion
                    }
                    else
                    {
                        #region GAC.27
                        return DoIDSReadOnly(database);
                        #endregion
                    }
                }
            }
            else
            {
                return DoGAC22_26(database);
            }
            #endregion
        }
        private static EMVGenerateACRequest DoPart3(KernelDatabaseBase database)
        {
            TLV cdol1 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN);
            CommonRoutines.PackRelatedDataTag(database, EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2, cdol1);

            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpST = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcpST.Value.ACTypeEnum = GetDSACType(database);
            rcpST.Value.CDASignatureRequested = GetODAStatus(database) == 0x80 ? true : false;
            rcpST.UpdateDB();

            EMVGenerateACRequest request = new EMVGenerateACRequest(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2), null, rcpST);
            return request;
        }
        
        private static EMVGenerateACRequest DoPart4(KernelDatabaseBase database)
        {
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpST = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcpST.Value.ACTypeEnum = GetDSACType(database);
            rcpST.Value.CDASignatureRequested = GetODAStatus(database) == 0x80 ? true : false;
            rcpST.UpdateDB();

            TLV cdol1 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN);

            CommonRoutines.PackRelatedDataTag(database,EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2,cdol1);
            CommonRoutines.PackDSDOLRelatedDataTag(database);

            EMVGenerateACRequest request = new EMVGenerateACRequest(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2), database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2), rcpST);

            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
            ids.Value.IsWrite = true;
            ids.UpdateDB();

            return request;

        }
        private static EMVGenerateACRequest DoCDAFailed(KernelDatabaseBase database)
        {
            return DoGAC22_26(database);
        }
        private static EMVGenerateACRequest DoIDSReadOnly(KernelDatabaseBase database)
        {
            #region GAC.27
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcp.Value.ACTypeEnum = GetDSACType(database);
            rcp.Value.CDASignatureRequested = true;
            rcp.UpdateDB();
            return DoPart3(database);
            #endregion
        }
        private static EMVGenerateACRequest DoIDSWrite(KernelDatabaseBase database)
        {
            #region GAC.40
            TLVList tags = TLV.DeserializeChildrenWithNoV(database.Get(EMVTagsEnum.DSDOL_9F5B_KRN2).Value, 0);
            if (tags.Get(EMVTagsEnum.DS_DIGEST_H_DF61_KRN2.Tag) != null)
            #endregion
            {
                #region GAC.41
                if (database.IsPresent(EMVTagsEnum.DS_INPUT_TERM_DF8109_KRN2.Tag))
                #endregion
                {
                    #region GAC.42
                    APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                    TLV dsit = database.Get(EMVTagsEnum.DS_INPUT_TERM_DF8109_KRN2);
                    TLV dsdh = database.Get(EMVTagsEnum.DS_DIGEST_H_DF61_KRN2.Tag);
                    if (aci.Value.DataStorageVersionNumberEnum == DataStorageVersionNumberEnum.VERSION_1)
                    #endregion
                    {
                        #region GAC.43
                        dsdh.Value = OWHF2.OWHF2_8_2(database, dsit.Value);
                        #endregion
                    }
                    else
                    {
                        #region GAC.44
                        dsdh.Value = OWHF2.OWHF2AES_8_3(database, dsit.Value);
                        #endregion
                    }
                    return DoPart4(database);
                }
                else
                {
                    #region GAC.45
                    return DoPart4(database);
                    #endregion
                }
            }
            else
            {
                #region GAC.45
                return DoPart4(database);
                #endregion
            }
        }
        private static EMVGenerateACRequest GenerateErrorOutput(KernelDatabaseBase database, L2Enum l2ErrorEnum, KernelQ qManager)
        {
            #region GAC.12, GAC.13
            CommonRoutines.CreateEMVDiscretionaryData(database);
            CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                null,
                l2ErrorEnum,
                L3Enum.NOT_SET);
            return null;
            #endregion
        }
    }
}


