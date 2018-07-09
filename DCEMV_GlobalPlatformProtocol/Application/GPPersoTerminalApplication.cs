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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.ISO7816Protocol;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPPersoTerminalApplication
    {
        public static Logger Logger = new Logger(typeof(GPPersoTerminalApplication));

        protected CardQProcessor cardQProcessor;
        protected bool cardInField = false;

        public event EventHandler ExceptionOccured;
        public event EventHandler UserInterfaceRequest;
        public event EventHandler ProcessCompleted;

        protected CancellationTokenSource cancellationTokenForPreProcessing;
        
        public GPPersoTerminalApplication(CardQProcessor cardInterface)
        {
            try
            {
                TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();
                this.cardQProcessor = cardInterface;
                this.cardQProcessor.ExceptionOccured += CardInterface_ExceptionOccured;
                this.cardQProcessor.CardInterfaceManger.CardPutInField += CardReader_CardPutInField;
                this.cardQProcessor.CardInterfaceManger.CardRemovedFromField += CardReader_CardRemovedFromField;
                cardInterface.StartServiceQProcess();
            }
            catch(Exception ex)
            {
                cardInterface.StopServiceQProcess();
                throw ex;
            }
        }

        private void OnExceptionOccured(Exception e)
        {
            cardQProcessor.StopServiceQProcess();
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }
        protected virtual void CardInterface_ExceptionOccured(object sender, EventArgs e)
        {
            //StopServiceQProcess();
            ExceptionOccured?.Invoke(this, e);
        }
        protected virtual void CardReader_CardRemovedFromField(object sender, EventArgs e)
        {
            cardInField = false;
            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.CardRemoved,StatusEnum.EndProcessing));
        }
        protected virtual void CardReader_CardPutInField(object sender, EventArgs e)
        {
            cardInField = true;
        }
        protected void OnUserInterfaceRequest(UIMessageEventArgs e)
        {
            UserInterfaceRequest?.Invoke(this, e);
        }
        protected void OnProcessCompleted(PersoProcessingOutcome po)
        {
            try
            {
                if (po == null) //exception occurred
                    return;

                switch (po.NextProcessState)
                {
                    case EMVPersoPreProcessingStateEnum.EndProcess:
                        cardQProcessor.StopServiceQProcess();
                        ProcessCompleted?.Invoke(this, new PersoProcessingOutcomeEventArgs() { TerminalProcessingOutcome = po });
                        break;

                    default:
                        throw new PersoException("Unimplemeted TerminalPreProcessingStateEnum in OnProcessCompleted");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in OnProcessCompleted:" + ex.Message);
                return;
            }
        }
        public void CancelTransactionRequest()
        {
            cardQProcessor.StopServiceQProcess();
            //StopServiceQProcess();
            if (cancellationTokenForPreProcessing != null)
                cancellationTokenForPreProcessing.Cancel();

            PersoProcessingOutcome processingOutcomeOUT = new PersoProcessingOutcome()
            {
                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                UIRequestOnOutcomePresent = true
            };
            UserInterfaceRequest uird = new UserInterfaceRequest()
            {
                MessageIdentifier = MessageIdentifiersEnum.ClearDisplay,
                Status = StatusEnum.ReadyToRead
            };
            processingOutcomeOUT.UserInterfaceRequest = uird;
            OnProcessCompleted(processingOutcomeOUT);
        }
        
        public void DoXMLPerso(string xml, string secDomainAID, string masterKey)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBXMLPerso(xml, secDomainAID, masterKey);
        }
        public void GetApps(string secDomainAID, string masterKey)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBGetApps(secDomainAID, masterKey);
        }
        public void LoadCapfile(String path, string secDomainAID, string masterKey)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBLoadApp(path, secDomainAID, masterKey);
        }
        public void InstallApp(string secDomainAID, string masterKey, string packageAid, string execModuleAid, string instanceAid)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBInstallApp(secDomainAID, masterKey, packageAid, execModuleAid, instanceAid);
        }
        public void RemoveApp(string secDomainAID, string masterKey, string instanceAid)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBRemoveApp(secDomainAID, masterKey, instanceAid);
        }
        public void LockApp(string secDomainAID, string masterKey, string instanceAid)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBLockApp(secDomainAID, masterKey, instanceAid);
        }
        public void UnLockApp(string secDomainAID, string masterKey, string instanceAid)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBUnLockApp(secDomainAID, masterKey, instanceAid);
        }
        public void GetCardData()
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBGetCardData();
        }
        public void TestApp(String instanceID)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBTestApp(instanceID);
        }
        public void All(String path, string secDomainAID, string masterKey, string packageAid, string execModuleAid, string instanceAid)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointBAll(path, secDomainAID, masterKey, packageAid, execModuleAid, instanceAid);
        }
        
        private void DoEntryPointBTestApp(String instanceID)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            TLVList tlvList = pxml.TestApp(instanceID);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                                TestOutCome = tlvList,
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBUnLockApp(string secDomainAID, string masterKey, string instanceAid)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.UnLockApp(secDomainAID, masterKey, instanceAid);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,

                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBLockApp(string secDomainAID, string masterKey, string instanceAid)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.LockApp(secDomainAID, masterKey, instanceAid);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,

                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBRemoveApp(string secDomainAID, string masterKey, string instanceAid)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.RemoveApp(secDomainAID, masterKey, instanceAid);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                                
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBXMLPerso(string xml,string secDomainAID, string masterKey)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }
                            
                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.PersoFromXml(xml, secDomainAID, masterKey);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBGetApps(string secDomainAID, string masterKey)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            GPRegistry reg = pxml.GetAppList(secDomainAID,masterKey);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                                GPRegistry = reg
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        private void DoEntryPointBGetCardData()
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            String cardData = pxml.GetCardData();

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                                CardData = cardData,
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        public void DoEntryPointBLoadApp(String path,string secDomainAID, string masterKey)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);

                            using (MemoryStream capFile = new MemoryStream())
                            {

                                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                                {
                                    byte[] data = new byte[fs.Length];
                                    fs.Read(data, 0, (int)fs.Length);
                                    capFile.Write(data, 0, (int)fs.Length);
                                }
                                pxml.LoadCapFile(capFile, secDomainAID,masterKey);
                            }

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        public void DoEntryPointBInstallApp(string secDomainAID, string masterKey, string packageAid, string execModuleAid, string instanceAid)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.InstallAndMakeSelectable(secDomainAID, masterKey, packageAid, execModuleAid, instanceAid);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }
        public void DoEntryPointBAll(String path, string secDomainAID, string masterKey, string packageAid, string execModuleAid, string instanceAid)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            PersoProcessing pxml = new PersoProcessing(cardQProcessor);
                            pxml.RemoveApp(secDomainAID, masterKey, packageAid);

                            using (MemoryStream capFile = new MemoryStream())
                            {

                                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                                {
                                    byte[] data = new byte[fs.Length];
                                    fs.Read(data, 0, (int)fs.Length);
                                    capFile.Write(data, 0, (int)fs.Length);
                                }
                                pxml.LoadCapFile(capFile, secDomainAID, masterKey);
                            }

                            pxml.InstallAndMakeSelectable(secDomainAID, masterKey, packageAid, execModuleAid, instanceAid);

                            PersoProcessingOutcome processingOutcome = new PersoProcessingOutcome()
                            {
                                NextProcessState = EMVPersoPreProcessingStateEnum.EndProcess,
                                UIRequestOnOutcomePresent = true,
                                UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.ClearDisplay, Status = StatusEnum.ReadyToRead },
                                UIRequestOnRestartPresent = false,
                            };

                            OnProcessCompleted(processingOutcome);
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }

        protected async Task ProtocolActivation_B()
        {
            cardInField = false;

            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.PresentCard, StatusEnum.ReadyToRead));

            await Task.Run(() =>
            {
                while (!cardInField)
                {
                    if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
            return;
        }
    }
}
