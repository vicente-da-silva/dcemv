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

namespace DCEMV.EMVProtocol.Contact
{
    public class EMVContactCombinationSelection
    {
        public static Tuple<EMVTerminalProcessingOutcome, 
            EMVSelectApplicationResponse, 
            TerminalSupportedKernelAidTransactionTypeCombination, 
            CardKernelAidCombination, 
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
           CombinationSelection_FromA(CardQProcessor cardInterfaceManager, IUICallbackProvider uiProvider)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();
            
            #region 12.3.2 Book1 12.2.2 and 12.3.2 Step 1
            EMVSelectPPSEResponse responsePPSE = SelectPSEApplication(cardInterfaceManager);
            
            //card blocked
            if (responsePPSE.SW1 == 0x6A && responsePPSE.SW2 == 0x81)
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
            #endregion

            //cards supports selection by PSE
            if (responsePPSE.Succeeded && responsePPSE.GetSFI_88() != null)
            {
                #region 12.3.2 12.3.2 Book1 Step 2
                TLVList records = ReadAllPSERecords(responsePPSE.GetSFI_88(), cardInterfaceManager);
                #endregion

                //do matching
                #region 12.3.2 Book1 12.3.2 Step 3
                candidates = CreateCandidateListFromPSERecords(records);
                #endregion

                #region 12.3.2 Book1 Step 4 and 5
                if (candidates.Count != 0)
                {
                    return ProcessCandidates(cardInterfaceManager, candidates, uiProvider);
                }
                #endregion
            }
            candidates = CreateCandidateListByDirMethod(cardInterfaceManager);
            if (candidates.Count == 0)
            {
                return Tuple.Create(
                   new EMVTerminalProcessingOutcome()
                   {
                       NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                       UIRequestOnOutcomePresent = true,
                       UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.TryAnotherCard, Status = StatusEnum.ReadyToRead },
                       UIRequestOnRestartPresent = false,
                   }, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null,
                   (List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>)null);
            }
            return ProcessCandidates(cardInterfaceManager, candidates, uiProvider);
        }

        public static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
            CombinationSelection_FromB(CardQProcessor cardInterfaceManager,
            Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> lastCandidateSelected,
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates,
            IUICallbackProvider uiProvider)
        {
            return CombinationSelection_FromA(cardInterfaceManager, uiProvider);
        }

        public static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
            CombinationSelection_FromC(CardQProcessor cardInterfaceManager, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates, IUICallbackProvider uiProvider)
        {
            return ProcessCandidates(cardInterfaceManager, candidates, uiProvider);
        }

        private static TLVList ReadAllPSERecords(TLV sfi, CardQProcessor cardInterfaceManager)
        {
            byte record = 1;
            TLVList _61 = new TLVList();

            while (record != 0)
            {
                EMVReadRecordRequest request = new EMVReadRecordRequest(sfi.Value[0], record);

                //do read record commands starting with record 1 untill the card returns 6A83
                ApduResponse cardResponse = cardInterfaceManager.SendCommand(request);
                if (cardResponse is EMVReadRecordResponse)
                {
                    if (cardResponse.Succeeded)
                    {
                        TLVList tags = (cardResponse as EMVReadRecordResponse).GetResponseTags();
                        foreach (TLV tlv in tags)
                            _61.AddToList(tlv, true);
                        record++;
                    }
                    else
                        record = 0;
                }
                else
                    throw new EMVProtocolException("ReadAllRecords transmit error");
            }
            return _61;
        }
       
            private static Tuple<EMVTerminalProcessingOutcome, 
            EMVSelectApplicationResponse, 
            TerminalSupportedKernelAidTransactionTypeCombination, 
            CardKernelAidCombination, 
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
          ProcessCandidates(CardQProcessor cardInterfaceManager, 
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> 
            candidates, IUICallbackProvider uiProvider)
        {
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

            BackToStep3:
            //prioritize if more than 1 candidate
            Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidate = CandidatesPrioritizeAndPromptUser(candidates, uiProvider);
            if (!candidate.IsPresent())
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

            //select the application
            Tuple<EMVTerminalProcessingOutcome,
                EMVSelectApplicationResponse,
                TerminalSupportedKernelAidTransactionTypeCombination> result = SelectAppChosen(cardInterfaceManager, candidate.Get().Item1, candidate.Get().Item2);

            if (result.Item1.NextProcessState == EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC)
            {
                candidates.Remove(candidate.Get());
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

            return Tuple.Create(result.Item1, result.Item2, result.Item3, candidate.Get().Item2, candidates);
        }

        private static Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> AskCardHolderToSelect(List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList, IUICallbackProvider uiProvider)
        {
            List<String> candidatesStrings = candidateList.Select(x => Formatting.ByteArrayToASCIIString(x.Item2.ApplicationLabel.Value) + ":" + Formatting.ByteArrayToHexString(x.Item2.AdfNameTag.Value)).ToList();
            String selected = uiProvider.DisplayApplicationList(candidatesStrings);
            if (!string.IsNullOrEmpty(selected))
            {
                Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> selectedTuple =
                    candidateList.Where(x => Formatting.ByteArrayToHexString(x.Item2.AdfNameTag.Value) == selected).First();
                return Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>.Create(selectedTuple);
            }
            else
                return Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>.CreateEmpty();
        }

        private static Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>
           CandidatesPrioritizeAndPromptUser(List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList, IUICallbackProvider uiProvider)
        {
            if (candidateList.Count == 1)
            {
                #region 12.3.2 Book1 12.4 Step 2
                if ((candidateList.ElementAt(0).Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x80) == 0x00)
                {
                    return Optional<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>.Create(new Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>(candidateList.ElementAt(0).Item1, candidateList.ElementAt(0).Item2));
                }
                #endregion
            }

            #region 12.3.2 Book1 12.4 Step 4
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> filteredNoZero = candidateList.Where(x =>
            {
                if (x.Item2.ApplicationPriorityIndicatorTag != null)
                    if ((x.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F) != 0x00) //0 = no priority
                        return true;
                    else
                        return false;
                else
                    return false;
            }).OrderBy(y => y.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F).ToList();

            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> filteredZero = candidateList.Where(x =>
            {
                if (x.Item2.ApplicationPriorityIndicatorTag == null)
                    return true;
                else
                     if ((x.Item2.ApplicationPriorityIndicatorTag.Value[0] & 0x0F) == 0x00)
                    return true;
                else
                    return false;
            }).ToList();

            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> result = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();
            result.AddRange(filteredNoZero);
            result.AddRange(filteredZero);
            #endregion

            #region 12.3.2 Book1 12.4 Step 3
            return AskCardHolderToSelect(result, uiProvider);
            #endregion
        }

        private static Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination>
            SelectAppChosen(CardQProcessor cardInterfaceManager,
            TerminalSupportedKernelAidTransactionTypeCombination supportedKernelAidCombination, CardKernelAidCombination candidate)
        {
            string finalAdfName = Formatting.ByteArrayToHexString(candidate.AdfNameTag.Value);
            
            ApduResponse cardResponse = cardInterfaceManager.SendCommand(new EMVSelectApplicationRequest(finalAdfName)) as EMVSelectApplicationResponse;
            EMVSelectApplicationResponse response;
            if (cardResponse is EMVSelectApplicationResponse)
                response = (EMVSelectApplicationResponse)cardResponse;
            else
                throw new EMVProtocolException("Pre-processing transmit error: try again");

            
            if (!response.Succeeded)
            {
                return Tuple.Create(
                        new EMVTerminalProcessingOutcome()
                        {
                            NextProcessState = EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC
                        }, response, supportedKernelAidCombination);
            }

            //TLV ttqInPDOL = response.GetPDOLTags().Get(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag);
            
            return Tuple.Create(
                new EMVTerminalProcessingOutcome()
                {
                    NextProcessState = EMVTerminalPreProcessingStateEnum.KernelActivation_StartD
                }, response, supportedKernelAidCombination);
        }


        private static EMVSelectPPSEResponse SelectPSEApplication(CardQProcessor cardInterfaceManager)
        {
            //same as contactless except for name
            ApduResponse cardResponse = cardInterfaceManager.SendCommand(new EMVSelectPPSERequest("1PAY.SYS.DDF01"));
            
            if (cardResponse is EMVSelectPPSEResponse)
                return (EMVSelectPPSEResponse)cardResponse;
            else
                throw new EMVProtocolException("Pre-processing transmit error: try again");
        }

        private static List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>
            CreateCandidateListByDirMethod(CardQProcessor cardInterfaceManager)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();

            foreach (TerminalSupportedKernelAidTransactionTypeCombination kc in TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactCombinations)
            {
                string finalAdfName = Enum.GetName(typeof(AIDEnum), ((TerminalSupportedContactKernelAidTransactionTypeCombination)kc).AIDEnum);
                #region 12.3.3 Step 1
                ApduResponse cardResponse = cardInterfaceManager.SendCommand(new EMVSelectApplicationRequest(finalAdfName)) as EMVSelectApplicationResponse;
                #endregion
                if (!(cardResponse is EMVSelectApplicationResponse))
                    throw new EMVProtocolException("Pre-processing transmit error: try again");

                EMVSelectApplicationResponse response = (EMVSelectApplicationResponse)cardResponse;

NextAID:
                #region 12.3.3 Step 2
                if (response.SW1 == 0x6A && response.SW2 == 0x81) //card blocked
                {
                    throw new Exception("Card blocked");
                }
                if (response.SW1 == 0x6A && response.SW2 == 0x81) //command not supported
                {
                    throw new Exception("Select command not supported");
                }
                if (response.SW1 == 0x6A && response.SW2 == 0x82) //file not found
                {
                    continue;
                }
                #endregion

                if (!response.Succeeded && !(response.SW1 == 0x62 && response.SW2 == 0x83))
                {
                    throw new Exception("Select command failed");
                }

                if (response.GetDFName() == finalAdfName)
                {
                    //full match
                    if (response.Succeeded) //i.e. app not blocked 0x6283
                    {
                        TLV adfNameTag = response.GetDFNameTag();
                        TLV applicationPriorityIndicatorTag = response.GetApplicationPriorityindicatorTag();
                        TLV preferedName = response.GetPreferredNameTag();
                        TLV appLabel = response.GetApplicationLabelTag();
                        Optional<TLV> extendedSelectionTag = Optional<TLV>.CreateEmpty();

                        candidateList.Add(Tuple.Create(kc, new CardKernelAidCombination()
                        {
                            AdfNameTag = adfNameTag,
                            ApplicationPriorityIndicatorTag = applicationPriorityIndicatorTag,
                            ApplicationPreferredName = preferedName,
                            ApplicationLabel = appLabel,
                            ExtendedSelectionTag = extendedSelectionTag,
                        }));
                        continue;
                    }
                }
                if (response.GetDFName().StartsWith(finalAdfName))
                {
                    //partial match
                    if(((TerminalSupportedContactKernelAidTransactionTypeCombination)kc).ApplicationSelectionIndicator)
                    {
                        if (response.Succeeded) //i.e. app not blocked 0x6283
                        {
                            TLV adfNameTag = response.GetDFNameTag();
                            TLV applicationPriorityIndicatorTag = response.GetApplicationPriorityindicatorTag();
                            TLV preferedName = response.GetPreferredNameTag();
                            TLV appLabel = response.GetApplicationLabelTag();
                            Optional<TLV> extendedSelectionTag = Optional<TLV>.CreateEmpty();

                            candidateList.Add(Tuple.Create(kc, new CardKernelAidCombination()
                            {
                                AdfNameTag = adfNameTag,
                                ApplicationPriorityIndicatorTag = applicationPriorityIndicatorTag,
                                ExtendedSelectionTag = extendedSelectionTag,
                                ApplicationPreferredName = preferedName,
                                ApplicationLabel = appLabel,
                            }));
                            continue;
                        }
                        else
                        {
                            ApduResponse cardResponseNext = cardInterfaceManager.SendCommand(new EMVSelectApplicationRequest(finalAdfName, true)) as EMVSelectApplicationResponse;
                            if (!(cardResponse is EMVSelectApplicationResponse))
                                throw new EMVProtocolException("Pre-processing transmit error: try again");

                            response = (EMVSelectApplicationResponse)cardResponse;
                            if(response.Succeeded || response.SW1 == 0x62 || response.SW1 == 0x63)
                                goto NextAID;
                        }
                    }
                }
            }
            return candidateList;
        }

        private static List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>
            CreateCandidateListFromPSERecords(TLVList directoryEntries)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidateList = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>();
            
            foreach (TLV tlv in directoryEntries)
            {
                TLV adfNameTag = tlv.Children.Get(EMVTagsEnum.APPLICATION_DEDICATED_FILE_ADF_NAME_4F_KRN.Tag);
                TLV applicationPriorityIndicatorTag = tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIORITY_INDICATOR_87_KRN.Tag);
                Optional<TLV> extendedSelectionTag = Optional<TLV>.Create(tlv.Children.Get(EMVTagsEnum.EXTENDED_SELECTION_9F29_KRN.Tag));
                TLV preferedName = tlv.Children.Get(EMVTagsEnum.APPLICATION_PREFERRED_NAME_9F12_KRN.Tag);
                TLV appLabel = tlv.Children.Get(EMVTagsEnum.APPLICATION_LABEL_50_KRN.Tag);

                if (adfNameTag == null)
                    break;

                if (adfNameTag.Value.Length < 5 || adfNameTag.Value.Length > 16)
                    break;

                foreach (TerminalSupportedKernelAidTransactionTypeCombination kc in TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactCombinations)
                {
                    string terminalAID = Enum.GetName(typeof(AIDEnum), ((TerminalSupportedContactKernelAidTransactionTypeCombination)kc).AIDEnum);
                    string cardAid = Formatting.ByteArrayToHexString(adfNameTag.Value);

                    if (terminalAID != cardAid && !cardAid.StartsWith(terminalAID))
                        continue;

                    if (terminalAID == cardAid)
                    {
                        //full match
                    }

                    if (cardAid.StartsWith(terminalAID))
                    {
                        //matching aid, partial match, check if partial match allowed
                        if (!((TerminalSupportedContactKernelAidTransactionTypeCombination)kc).ApplicationSelectionIndicator)
                            continue;
                    }

                    candidateList.Add(Tuple.Create(kc, new CardKernelAidCombination()
                    {
                        AdfNameTag = adfNameTag,
                        ApplicationPriorityIndicatorTag = applicationPriorityIndicatorTag,
                        ApplicationPreferredName = preferedName,
                        ApplicationLabel = appLabel,
                        ExtendedSelectionTag = extendedSelectionTag,
                    }));

                }
            }

            return candidateList;
        }

    }
}
