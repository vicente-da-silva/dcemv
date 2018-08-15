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
using DCEMV.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using DCEMV.TLVProtocol;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol.Contactless
{
    public class EMVContactlessCombinationSelection
    {
        public static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>> 
            CombinationSelection_FromA(CardQProcessor cardInterfaceManager, TLV iad, TLV scriptData)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();

            EMVSelectPPSEResponse responsePPSE = FinalCombinationSelection_Step1(cardInterfaceManager);

            #region 3.3.2.4
            if (responsePPSE.Succeeded && responsePPSE.GetDirectoryEntries_61() != null)
            #endregion
            {
                #region 3.3.2.3
                if (responsePPSE.Succeeded)
                    candidates = FinalCombinationSelection_Step2(responsePPSE.GetDirectoryEntries_61());
                #endregion

                #region 3.3.2.7
                if (candidates.Count == 0)
                #endregion
                {
                    return Tuple.Create(
                    new EMVTerminalProcessingOutcome()
                    {
                        NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                        UIRequestOnOutcomePresent = true,
                        UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard, Status = StatusEnum.ReadyToRead },
                        UIRequestOnRestartPresent = false,
                    }, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null, 
                    (List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>)null);
                }
            }

            return ProcessCandidates(cardInterfaceManager,candidates, iad, scriptData);
        }

        public static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
            CombinationSelection_FromB(CardQProcessor cardInterfaceManager,
            Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> lastCandidateSelected,
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates,
            TLV iad, TLV scriptData)
        {
            /*
            If Issuer Authentication Data and/or Issuer Script is present,
            then processing shall continue at requirement 3.3.3.3 of Final
            Combination Selection with the Combination that was
            selected during the previous Final Combination Selection.
            */
            if (iad != null || scriptData != null)
            {
                Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination> 
                    result = FinalCombinationSelection_Step3_From_3_3_3_3(cardInterfaceManager, lastCandidateSelected.Item3, lastCandidateSelected.Item4, iad, scriptData);
                return Tuple.Create(result.Item1, result.Item2, result.Item3, lastCandidateSelected.Item4, candidates); 
            }
            else
            {
                return CombinationSelection_FromA(cardInterfaceManager, iad, scriptData);
            }
        }

        public static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
            CombinationSelection_FromC(CardQProcessor cardInterfaceManager,List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates)
        {
            return ProcessCandidates(cardInterfaceManager, candidates, null, null);
        }

        private static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
           ProcessCandidates(CardQProcessor cardInterfaceManager, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates, TLV iad, TLV scriptData)
        {
        BackToStep3:
            Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> candidate = FinalCombinationSelection_Step3_StartC(candidates);

            if (candidate.Item1 == null)
            {
                return Tuple.Create(
                new EMVTerminalProcessingOutcome()
                {
                    NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                    UIRequestOnOutcomePresent = true,
                    UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard, Status = StatusEnum.ReadyToRead },
                    UIRequestOnRestartPresent = false,
                }, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null,
                (List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>)null);
            }

            Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination> result = FinalCombinationSelection_Step3_From_3_3_3_3(cardInterfaceManager, candidate.Item1, candidate.Item2, iad, scriptData);

            if (result.Item1.NextProcessState == EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC)
            {
                candidates.Remove(candidate);
                if (candidates.Count == 0)
                {
                    return Tuple.Create(
                    new EMVTerminalProcessingOutcome()
                    {
                        NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                        UIRequestOnOutcomePresent = true,
                        UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard, Status = StatusEnum.ReadyToRead },
                        UIRequestOnRestartPresent = false,
                    }, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null,
                    (List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>)null);
                }
                else
                    goto BackToStep3;
            }

            return Tuple.Create(result.Item1, result.Item2, result.Item3, candidate.Item2, candidates);
        }

        private static Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> 
            FinalCombinationSelection_Step3_StartC(List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList)
        {
            #region 3.3.3.1
            if (candidateList.Count == 1)
            #endregion
            {
                return candidateList.ElementAt(0);
            }

            #region 3.3.3.2 
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> filteredNoZero = candidateList.Where(x => {
                if (x.Item2.ApplicationPriorityIndicatorTag != null && (x.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F) != 0x00)
                    return true;
                else
                    return false;
            }).ToList();

            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> filteredZero = candidateList.Where(x => {
                if (x.Item2.ApplicationPriorityIndicatorTag == null || (x.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F) == 0x00)
                    return true;
                else
                    return false;
            }).ToList();


            List<IGrouping<int, Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>> groupedOrderedFilteredNoZero = filteredNoZero.GroupBy(x => {
                return (x.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F);
            }).OrderBy(y => y.Key).ToList();

            //0 no priority
            //1 highest priority
            //15 lowest priority

            if (groupedOrderedFilteredNoZero.Count == 0)
            {
                if (filteredZero.Count == 0)
                {
                    //problem, nothing to select
                    return Tuple.Create((TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null);
                }

                else if (filteredZero.Count == 1)
                {
                    return filteredZero[0];
                }

                else // (filteredZero.Count > 1)
                {
                    //TODO: make sure the items in the list are in they same order as retreived from the ppse
                    return filteredZero[0]; //use this list, the first item in the filteredZero list will be the highest priority
                }
            }
            else
            {
                if (groupedOrderedFilteredNoZero.Count == 1)
                {
                    return groupedOrderedFilteredNoZero[0].ElementAt(0);
                }

                else //if (groupedOrderedFilteredNoZero.Count > 1)
                {
                    //check the groups
                    if (groupedOrderedFilteredNoZero[0].Key == 1) //only 1 item in highest priority group
                    {
                        return groupedOrderedFilteredNoZero[0].ElementAt(0);
                    }
                    else //TODO: make sure the items in the list are in they same order as retreived from the ppse
                    {
                        return groupedOrderedFilteredNoZero[0].ElementAt(0);  //use this highest priority group list, the first item in the filteredZero list will be the highest priority
                    }

                }
            }
            #endregion
        }

        private static EMVSelectPPSEResponse FinalCombinationSelection_Step1(CardQProcessor cardInterfaceManager)
        {
            //Step 1
            //3.3.2.2
            ApduResponse cardResponse = cardInterfaceManager.SendCommand(new EMVSelectPPSERequest("2PAY.SYS.DDF01"));
            if (cardResponse is EMVSelectPPSEResponse)
                return (EMVSelectPPSEResponse)cardResponse;
            else
                throw new EMVProtocolException("Pre-processing transmit error: try again");
        }

        private static List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> 
            FinalCombinationSelection_Step2(TLVList directoryEntries)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();

            foreach (TLV tlv in directoryEntries)
            {
                TLV adfNameTag = tlv.Children.Get(EMVTagsEnum.APPLICATION_DEDICATED_FILE_ADF_NAME_4F_KRN.Tag);
                TLV applicationPriorityIndicatorTag = tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIORITY_INDICATOR_87_KRN.Tag);
                Optional<TLV> extendedSelectionTag = Optional<TLV>.Create(tlv.Children.Get(EMVTagsEnum.EXTENDED_SELECTION_9F29_KRN.Tag));

                //A
                if (adfNameTag == null)
                    break;
                if (adfNameTag.Value.Length < 5 || adfNameTag.Value.Length > 16)
                    break;

                #region 3.3.2.5
                foreach (TerminalSupportedKernelAidTransactionTypeCombination kc in TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactlessCombinations)
                #endregion
                {
                    string terminalAID = Enum.GetName(typeof(RIDEnum), ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)kc).RIDEnum);
                    string cardAid = Formatting.ByteArrayToHexString(adfNameTag.Value);

                    //B
                    if (terminalAID != cardAid && !cardAid.StartsWith(terminalAID))
                        continue;
                   
                    if (terminalAID == cardAid)
                    {
                        //full match
                    }

                    if (cardAid.StartsWith(terminalAID))
                    {
                        //matching aid, partial match
                    }

                    //C
                    TLV kernelID = tlv.Children.Get(EMVTagsEnum.KERNEL_IDENTIFIER_9F2A_KRN.Tag);
                    byte kernelIDByte = 0x00;
                    if (kernelID == null || kernelID.Value.Length == 0 || kernelID.Value.Length > 8)
                    {
                        //use default
                        kernelIDByte = AIDKernelIDMapDefaults.FindEntry(terminalAID).KernelID;
                    }
                    else
                    {
                        byte[] kernelIdBytes = kernelID.Value;

                        if ((kernelIdBytes[0] & 0xC0) == 0x00 || (kernelIdBytes[0] & 0xC0) == 0x40) //international kernel
                            kernelIDByte = kernelIdBytes[0];

                        if ((kernelIdBytes[0] & 0xC0) == 0x80 || (kernelIdBytes[0] & 0xC0) == 0xC0)//domestic kernel
                        {
                            if (kernelIdBytes.Length < 3)
                                continue;

                            kernelIDByte = (byte)(kernelIdBytes[0] & 0x3F); //short kernel id

                            if (kernelIDByte != 0x00)
                                kernelIDByte = (byte)(kernelIdBytes[0] | kernelIdBytes[1] | kernelIdBytes[2]); //byte [1], byte [2] are the extended kernel id
                            else
                                continue;
                        }
                    }
                    //D
                    if (kernelIDByte != 0x00 && (byte)((TerminalSupportedContactlessKernelAidTransactionTypeCombination)kc).KernelEnum != kernelIDByte)
                    {
                        continue;
                    }

                    if (kernelIDByte == 0x00)
                    {
                        //supported
                    }

                    if ((byte)((TerminalSupportedContactlessKernelAidTransactionTypeCombination)kc).KernelEnum == kernelIDByte)
                    {
                        //supported
                    }

                    //E
                    candidateList.Add(Tuple.Create(kc, new CardKernelAidCombination()
                    {
                        AdfNameTag = adfNameTag,
                        ApplicationPriorityIndicatorTag = applicationPriorityIndicatorTag,
                        ExtendedSelectionTag = extendedSelectionTag,
                    }));

                }
            }

            return candidateList;
        }

        private static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination> 
            FinalCombinationSelection_Step3_From_3_3_3_3(CardQProcessor cardInterfaceManager,
            TerminalSupportedKernelAidTransactionTypeCombination supportedKernelAidCombination, CardKernelAidCombination candidate, TLV iad, TLV scriptData)
        {
            string finalAdfName = Formatting.ByteArrayToHexString(candidate.AdfNameTag.Value);
            #region 3.3.3.3
            if (candidate.ExtendedSelectionTag.IsPresent() && ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)supportedKernelAidCombination).ExtendedSelectionSupportFlag != null 
                    && (bool)((TerminalSupportedContactlessKernelAidTransactionTypeCombination)supportedKernelAidCombination).ExtendedSelectionSupportFlag)
                finalAdfName = finalAdfName + Formatting.ByteArrayToHexString(candidate.ExtendedSelectionTag.Get().Value);
            #endregion

            #region 3.3.3.4
            ApduResponse cardResponse = cardInterfaceManager.SendCommand(new EMVSelectApplicationRequest(finalAdfName)) as EMVSelectApplicationResponse;
            EMVSelectApplicationResponse response;
            if (cardResponse is EMVSelectApplicationResponse)
                response = (EMVSelectApplicationResponse)cardResponse;
            else
                throw new EMVProtocolException("Pre-processing transmit error: try again");
            #endregion

                #region 3.3.3.5
            if (!response.Succeeded)
            #endregion
            {
                if (iad != null || scriptData != null)
                {
                    return Tuple.Create(
                    new EMVTerminalProcessingOutcome()
                    {
                        NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                        UIRequestOnOutcomePresent = true,
                        UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard, Status = StatusEnum.ReadyToRead },
                        UIRequestOnRestartPresent = false
                    }, response, supportedKernelAidCombination);
                }
                else
                    return Tuple.Create(
                        new EMVTerminalProcessingOutcome()
                        {
                            NextProcessState = EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC
                        }, response, supportedKernelAidCombination);
            }

            TLV ttqInPDOL = response.GetPDOLTags().Get(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag);
            #region 3.3.3.6
            if (finalAdfName.StartsWith(Enum.GetName(typeof(RIDEnum), RIDEnum.A000000003)))
            #endregion
            {
                if (((TerminalSupportedContactlessKernelAidTransactionTypeCombination)supportedKernelAidCombination).KernelEnum == KernelEnum.Kernel3)
                {
                    if (response.GetPDOLTags().Count == 0 || ttqInPDOL == null)
                    {
                        if (TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactlessCombinations.Where(x => x.KernelEnum == KernelEnum.Kernel1).ToList().Count > 0)
                        {
                            //switch kernel to enable to kernel 1
                            TerminalSupportedKernelAidTransactionTypeCombination val = TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactlessCombinations.Where(x => x.KernelEnum == KernelEnum.Kernel1).ElementAt(0);
                            return Tuple.Create(
                                new EMVTerminalProcessingOutcome()
                                {
                                    NextProcessState = EMVTerminalPreProcessingStateEnum.KernelActivation_StartD
                                }, response, val);
                        }
                        else
                            return Tuple.Create(
                                new EMVTerminalProcessingOutcome()
                                {
                                    NextProcessState = EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC
                                }, response, supportedKernelAidCombination);
                    }
                }
            }

            //we will not support Application Selection Registered Proprietary Data (ASRPD) and continue as if the data was not present

            return Tuple.Create(
                new EMVTerminalProcessingOutcome()
                {
                    NextProcessState = EMVTerminalPreProcessingStateEnum.KernelActivation_StartD
                }, response, supportedKernelAidCombination);
        }
    }
}
