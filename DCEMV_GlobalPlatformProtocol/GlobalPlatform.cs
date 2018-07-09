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
using DCEMV.EMVProtocol;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GlobalPlatform
    {
        private int blockSize = 255;
        private SCPWrapper wrapper;
        private CardQProcessor cardInterface;
        private GPSpec spec = GPSpec.GP211;
        bool doForceInstallApplet = false;
        bool isDefaultApplet = false;
        bool isAppletTerminate = false;

        public GlobalPlatform(CardQProcessor cardInterface)
        {
            this.cardInterface = cardInterface;
        }

        private GPResponse SendCommand(GPCommand apduCommand)
        {
            if (wrapper == null)
            {
                ApduResponse response = cardInterface.SendCommand(apduCommand);
                if (response is GPResponse)
                    return (GPResponse)response;
            }
            else
            {
                GPCommand wrapped = Activator.CreateInstance(apduCommand.GetType()) as GPCommand;
                wrapped.ApduResponseType = apduCommand.ApduResponseType;
                wrapped.Deserialize(wrapper.Wrap(apduCommand));
                ApduResponse response = cardInterface.SendCommand(wrapped);
                if (response is GPResponse)
                    return (GPResponse)response;
            }

            throw new PersoException("Error returned from card");
        }

        private EMVResponse SendCommand(EMVCommand apduCommand)
        {
            ApduResponse response = cardInterface.SendCommand(apduCommand);
            if (response is EMVResponse)
                return (EMVResponse)response;
            
            throw new PersoException("Error returned from card");
        }

        public void OpenSecureChannel(GPPlaintextKeys keys, List<APDUMode> securityLevel, byte[] hostChallenge = null, byte[] initUpdateResponse = null, byte[] externalAuthReponse = null)
        {
            if (securityLevel.Contains(APDUMode.ENC) && !securityLevel.Contains(APDUMode.MAC))
                securityLevel.Add(APDUMode.MAC);

            if (hostChallenge == null)
            {
                // Generate host challenge
                hostChallenge = new byte[8];
                SecureRandom sr = new SecureRandom();
                sr.NextBytes(hostChallenge);
            }

            GPInitializeUpdateReqest initUpdate = new GPInitializeUpdateReqest(keys.GetKeysetVersion(), keys.GetKeysetID(), hostChallenge);
            //System.Diagnostics.Debug.WriteLine(initUpdate.ToPrintString());
            GPInitializeUpdateResponse response;
            if (initUpdateResponse != null)
            {
                response = new GPInitializeUpdateResponse();
                response.Deserialize(initUpdateResponse);
                System.Diagnostics.Debug.WriteLine(response.ToPrintString());
            }
            else
                response = (GPInitializeUpdateResponse)SendCommand(initUpdate);


            // Detect and report locked cards in a more sensible way.
            if ((response.SW == (ushort)ISO7816ReturnCodes.SW_SECURITY_STATUS_NOT_SATISFIED) || (response.SW == (ushort)ISO7816ReturnCodes.SW_AUTHENTICATION_METHOD_BLOCKED))
                throw new Exception("INITIALIZE UPDATE failed, Card possibly locked.");

            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("INITIALIZE UPDATE failed");

            // Verify response length (SCP01/SCP02 + SCP03 + SCP03 w/ pseudorandom)
            if (response.ResponseData.Length != 28 && response.ResponseData.Length != 29 && response.ResponseData.Length != 32)
                throw new Exception("Invalid INITIALIZE UPDATE response length: " + response.ResponseData.Length);

            System.Diagnostics.Debug.WriteLine("Host challenge: " + Formatting.ByteArrayToHexString(hostChallenge));
            System.Diagnostics.Debug.WriteLine("Card challenge: " + Formatting.ByteArrayToHexString(response.CardChallenge));

            // Verify response
            // If using explicit key version, it must match.
            if ((keys.GetKeysetVersion() > 0) && (response.KeyVersionNumber != keys.GetKeysetVersion()))
                throw new Exception("Key version mismatch: " + keys.GetKeysetVersion() + " != " + response.KeyVersionNumber);

            System.Diagnostics.Debug.WriteLine("Card reports SCP0" + response.SCPId + " with version " + response.KeyVersionNumber + " keys");
            if (response.SCPId == 3) System.Diagnostics.Debug.WriteLine("SCP03 i=" + response.SCPI);

            SCPVersions scpVersion;
            // Derive session keys
            GPKeySet sessionKeys;
            if (response.SCPId == 1)
            {
                if (securityLevel.Contains(APDUMode.RMAC))
                    throw new Exception("SCP01 does not support RMAC");

                scpVersion = SCPVersions.SCP_01_05;
                sessionKeys = keys.GetSessionKeys(response.SCPId, response.KeyDiversificationData, hostChallenge, response.CardChallenge);
            }
            else if (response.SCPId == 2)
            {
                scpVersion = SCPVersions.SCP_02_15;
                sessionKeys = keys.GetSessionKeys(response.SCPId, response.KeyDiversificationData, response.CardChallengeSeq);
            }
            else if (response.SCPId == 3)
            {
                scpVersion = SCPVersions.SCP_03;
                sessionKeys = keys.GetSessionKeys(response.SCPId, response.KeyDiversificationData, hostChallenge, response.CardChallenge);
            }
            else
                throw new Exception("Unsupported scpVersion: " + response.SCPId);

            // Verify card cryptogram
            byte[] my_card_cryptogram = null;
            byte[] cntx;
            if (response.SCPId == 2)
                cntx = Formatting.ConcatArrays(hostChallenge, response.CardChallengeSeq, response.CardChallenge);
            else
                cntx = Arrays.Concatenate(hostChallenge, response.CardChallenge);

            if (response.SCPId == 1 || response.SCPId == 2)
                my_card_cryptogram = SCP0102Wrapper.Mac_3des_nulliv(sessionKeys.GetKey(KeySessionType.ENC), cntx);
            else
                my_card_cryptogram = SCP03Wrapper.Scp03_kdf(sessionKeys.GetKey(KeySessionType.MAC), (byte)0x00, cntx, 64);

            // This is the main check for possible successful authentication.
            if (!Arrays.AreEqual(response.CardCryptogram, my_card_cryptogram))
            {
                string message = "Card cryptogram invalid." +
                    "\nCard: " + Formatting.ByteArrayToHexString(response.CardCryptogram) +
                    "\nCalculated: " + Formatting.ByteArrayToHexString(my_card_cryptogram) +
                    "\nRetrying the same parameters may disable the card";

                System.Diagnostics.Debug.WriteLine(message);
                throw new Exception(message);

            }
            else
                System.Diagnostics.Debug.WriteLine("Verified card cryptogram: " + Formatting.ByteArrayToHexString(my_card_cryptogram));

            // Calculate host cryptogram and initialize SCP wrapper
            byte[] host_cryptogram = null;
            if (response.SCPId == 1 || response.SCPId == 2)
            {
                if (response.SCPId == 2)
                    host_cryptogram = SCP0102Wrapper.Mac_3des_nulliv(sessionKeys.GetKey(KeySessionType.ENC), Formatting.ConcatArrays(response.CardChallengeSeq, response.CardChallenge, hostChallenge));
                else
                    host_cryptogram = SCP0102Wrapper.Mac_3des_nulliv(sessionKeys.GetKey(KeySessionType.ENC), Arrays.Concatenate(response.CardChallenge, hostChallenge));
                wrapper = new SCP0102Wrapper(sessionKeys, scpVersion, new List<APDUMode>() { APDUMode.MAC }, null, null, blockSize);
            }
            else
            {
                host_cryptogram = SCP03Wrapper.Scp03_kdf(sessionKeys.GetKey(KeySessionType.MAC), (byte)0x01, cntx, 64);
                wrapper = new SCP03Wrapper(sessionKeys, scpVersion, new List<APDUMode>() { APDUMode.MAC }, null, null, blockSize);
            }
            System.Diagnostics.Debug.WriteLine("Calculated host cryptogram: " + Formatting.ByteArrayToHexString(host_cryptogram));
            
            GPExternalAuthenticateReqest externalAuthenticate = new GPExternalAuthenticateReqest(GetSetValue(securityLevel), host_cryptogram);
            GPExternalAuthenticateResponse response2;
            if (externalAuthReponse != null)
            {
                response2 = new GPExternalAuthenticateResponse();
                response2.Deserialize(externalAuthReponse);
                System.Diagnostics.Debug.WriteLine(response2.ToPrintString());
            }
            else
                response2 = (GPExternalAuthenticateResponse)SendCommand(externalAuthenticate);


            if (response2.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("External authenticate failed");

            wrapper.SetSecurityLevel(securityLevel);
        }

        public GPRegistry getRegistry()
        {
            GPRegistry registry = new GPRegistry();

            // Issuer security domain
            byte[] data = getConcatenatedStatus(registry, 0x80, new byte[] { 0x4F, 0x00 });
            registry.parse(0x80, data, Kind.IssuerSecurityDomain, spec);

            // Apps and security domains
            data = getConcatenatedStatus(registry, 0x40, new byte[] { 0x4F, 0x00 });
            registry.parse(0x40, data, Kind.Application, spec);

            // Load files
            data = getConcatenatedStatus(registry, 0x20, new byte[] { 0x4F, 0x00 });
            registry.parse(0x20, data, Kind.ExecutableLoadFile, spec);

            return registry;
        }

       

        public void deleteAID(AID aid, bool deleteDeps)
        {
            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            try
            {
                bo.Write((byte)0x4F);
                bo.Write((byte)aid.getLength());
                bo.Write(aid.getBytes());
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }
            GPInstallRequest delete = new GPInstallRequest(GPInstructionEnum.Delete, bo.ToByteArray(), 0x00, (byte)(deleteDeps ? 0x80 : 0x00));
            GPInstallResponse response = (GPInstallResponse)SendCommand(delete);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("Deletion failed");

        }

        public void LockApp(AID instanceAID)
        {
            GPSetStatusRequest delete = new GPSetStatusRequest(instanceAID.getBytes(),true);
            GPSetStatusResponse response = (GPSetStatusResponse)SendCommand(delete);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("Lock App failed");
        }
        public void UnLockApp(AID instanceAID)
        {
            GPSetStatusRequest delete = new GPSetStatusRequest(instanceAID.getBytes(), false);
            GPSetStatusResponse response = (GPSetStatusResponse)SendCommand(delete);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("UnLock App failed");
        }

        public String GetData()
        {
            StringBuilder sb = new StringBuilder();

            GPGetDataRequest requestCPLC = new GPGetDataRequest(DataType.CPLC);
            GPGetDataResponse responseCPLC = (GPGetDataResponse)SendCommand(requestCPLC);
            if (responseCPLC.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                sb.AppendLine("Get CPLC failed");
            else
            {
                CPLC cplc = new CPLC(responseCPLC.ResponseData);
                sb.AppendLine("CPLC Data");
                sb.AppendLine(cplc.ToString());
            }
            
            GPGetDataRequest requestCardData = new GPGetDataRequest(DataType.CardData);
            GPGetDataResponse responseCardData = (GPGetDataResponse)SendCommand(requestCardData);
            if (responseCPLC.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                sb.AppendLine("Get Card Data failed");
            else
            {
                //sb.AppendLine(unpackKeyData(responseCardData.ResponseData));
                TLVList tlvList = new TLVList();
                tlvList.Deserialize(responseCardData.ResponseData);
                sb.AppendLine("Card Data");
                int depth = 0;
                sb.AppendLine(tlvList.ToPrintString(ref depth));
            }

            GPGetDataRequest requestKeyInfo = new GPGetDataRequest(DataType.KeyInfoTemplate);
            GPGetDataResponse responseKeyInfo = (GPGetDataResponse)SendCommand(requestKeyInfo);
            if (responseCPLC.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                sb.AppendLine("Get Key Data failed");
            else
            {
                TLVList tlvList = new TLVList();
                tlvList.Deserialize(responseKeyInfo.ResponseData);
                sb.AppendLine("Key Data");
                int depth = 0;
                sb.AppendLine(tlvList.ToPrintString(ref depth));
            }

            return sb.ToString();
        }

        private String unpackKeyData(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            TLVList tlvList = new TLVList();
            tlvList.Deserialize(data);
            sb.AppendLine("Card Data");
            try
            {
                String oidVersion = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("06").Value);
                String version = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("60").Children.Get("06").Value);
                String tag3 = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("63").Children.Get("06").Value);
                String scpVersion = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("64").Children.Get("06").Value);
                String tag5 = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("65").Children.Get("06").Value);
                String tag6 = Formatting.ByteArrayToASCIIString(tlvList.Get("66").Children.Get("73").Children.Get("66").Children.Get("06").Value);

                sb.AppendLine("OID Version:" + oidVersion);
                sb.AppendLine("Version:" + version);
                sb.AppendLine("Tag3:" + tag3);
                sb.AppendLine("SCP Version:" + scpVersion);
                sb.AppendLine("Tag5:" + tag5);
                sb.AppendLine("Tag6:" + tag6);
            }
            catch(Exception ex)
            {
                sb.AppendLine("Error Parsing Data:" + ex.Message);
            }

            return sb.ToString();
        }

        public void InstallForLoad(String aid, MemoryStream capFile)
        {
            CapFile instcap = new CapFile(capFile);
            loadCapFile(aid, instcap);
        }

        public void installCapFile(MemoryStream capFile)
        {
            //final File capfile;
            //capfile = (File)args.valueOf(OPT_INSTALL);

            CapFile instcap = new CapFile(capFile);

            // Only install if cap contains a single applet
            if (instcap.getAppletAIDs().Count == 0)
            {
                throw new Exception("No applets in CAP");
            }
            if (instcap.getAppletAIDs().Count > 1)
            {
                throw new Exception("CAP contains more than one applet");
            }

            GPRegistry reg = getRegistry();
            Privileges privs = getInstPrivs(isDefaultApplet, isAppletTerminate);
            // Remove existing default app
            if (doForceInstallApplet && (reg.getDefaultSelectedAID() != null && privs.has(Privilege.CardReset)))
            {
                deleteAID(reg.getDefaultSelectedAID(), false);
            }
            // Remove existing load file
            if (doForceInstallApplet && reg.allPackageAIDs().Contains(instcap.getPackageAID()))
            {
                deleteAID(instcap.getPackageAID(), true);
            }

            try
            {
                loadCapFile("",instcap);
                //System.err.println("CAP loaded");
            }
            catch (Exception e)
            {
                //if (e.sw == 0x6985 || e.sw == 0x6A80)
                //{
                //    System.err.println("Applet loading failed. Are you sure the CAP file (JC version, packages) is compatible with your card?");
                //}
                throw e;
            }

            // Take the applet AID from CAP but allow to override
            AID appaid = instcap.getAppletAIDs()[0];
            //if (args.has(OPT_APPLET))
            //{
            //    appaid = (AID)args.valueOf(OPT_APPLET);
            //}
            //if (args.has(OPT_CREATE))
            //{
            //    appaid = (AID)args.valueOf(OPT_CREATE);
            //}
            if (getRegistry().allAIDs().Contains(appaid))
            {
                //System.err.println("WARNING: Applet " + appaid + " already present on card");
                throw new Exception("Applet " + appaid + " already present on card");
            }
            installAndMakeSelectable(instcap.getPackageAID(), appaid, null, privs, getInstParams(null), null);
        }

        private static byte[] getInstParams(byte[] args)
        {
            byte[] paramsVal = null;
            if (args != null)
            {
                paramsVal = args;
                // Simple use: only application parameters without tag, prepend 0xC9
                if (paramsVal[0] != (byte)0xC9)
                {
                    byte[] newparams = new byte[paramsVal.Length + 2];
                    newparams[0] = (byte)0xC9;
                    newparams[1] = (byte)paramsVal.Length;
                    Array.Copy(paramsVal, 0, newparams, 2, paramsVal.Length);
                    paramsVal = newparams;
                }
            }
            return paramsVal;
        }

        private static Privileges getInstPrivs(bool isDefault, bool doTerminate)
        {
            Privileges privs = new Privileges();
            if (isDefault)
            {
                privs.add(Privilege.CardReset);
            }
            if (doTerminate)
            {
                privs.add(Privilege.CardLock);
                privs.add(Privilege.CardTerminate);
            }
            return privs;
        }

        private static Privileges addPrivs(Privileges privs, String v)
        {
            String[] parts = v.Split(',');
            foreach (String s in parts)
            {
                bool found = false;
                foreach (Privilege p in Privileges.getEnumList())
                {
                    if (s.Trim() == Enum.GetName(typeof(Privilege), p))
                    {
                        found = true;
                        privs.add(p);
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception("Unknown privilege: " + s.Trim());
                }
            }
            return privs;
        }

        private byte[] getConcatenatedStatus(GPRegistry reg, byte p1, byte[] data)
        {
            // By default use tags
            byte p2 = (byte)(reg.tags ? 0x02 : 0x00);

            GPGetStatusRequest cmd = new GPGetStatusRequest(data, p1, p2);
            GPGetStatusResponse response = (GPGetStatusResponse)SendCommand(cmd);

            // Workaround for legacy cards, like SCE 6.0 FIXME: this does not work properly
            // Find a different way to adjust the response parser without touching the overall spec mode

            // If ISD-s are asked and none is returned, it could be either
            // - SSD
            // - no support for tags
            if (p1 == 0x80 && response.SW == 0x6A86)
            {
                if (p2 == 0x02)
                {
                    // If no support for tags. Re-issue command without requesting tags
                    reg.tags = false;
                    return getConcatenatedStatus(reg, p1, data);
                }
            }

            int sw = response.SW;
            if ((sw != (int)ISO7816ReturnCodes.SW_NO_ERROR) && (sw != 0x6310))
            {
                // Possible values:
                if (sw == 0x6A88)
                {
                    // No data to report
                    return response.ResponseData;

                }
                // 0x6A86 - no tags support or ISD asked from SSD
                // 0a6A81 - Same as 6A88 ?
                //logger.warn("GET STATUS failed for " + HexUtils.bin2hex(cmd.getBytes()) + " with " + Integer.toHexString(sw));
                return response.ResponseData;
            }

            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            try
            {
                bo.Write(response.ResponseData);

                while (response.SW == 0x6310)
                {
                    cmd = new GPGetStatusRequest(data, p1, (byte)(p2 | 0x01));
                    response = (GPGetStatusResponse)SendCommand(cmd);

                    sw = response.SW;
                    if ((sw != (int)ISO7816ReturnCodes.SW_NO_ERROR) && (sw != 0x6310))
                    {
                        throw new Exception("GET STATUS failed for " + Formatting.ByteArrayToHexString(cmd.Serialize()));
                    }
                    bo.Write(response.ResponseData);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return bo.ToByteArray();
        }

        public void InstallForInstallAndMakeSelectable(String packageAID, String appletAID, String instanceId)
        {
            installAndMakeSelectable(new AID(packageAID), new AID(appletAID), new AID(instanceId), 
                getInstPrivs(false,false), getInstParams(null), null);
        }

        public void SelectApplication(byte[] aid)
        {
            GPSelectRequest request = new GPSelectRequest(aid);
            GPSelectResponse response = SendCommand(request) as GPSelectResponse;

            if (!response.Succeeded)
                throw new Exception("Failure selecting application, SW=" + response.SW + " (" + response.SWTranslation + ")");
        }

        public TLVList DoGPOTest(String data)
        {
            TLV tlv = TLV.Create("DF8111");
            tlv.Value = Formatting.HexStringToByteArray(data);
            EMVGetProcessingOptionsRequest request = new EMVGetProcessingOptionsRequest(tlv);
            EMVGetProcessingOptionsResponse response = SendCommand(request) as EMVGetProcessingOptionsResponse;
            if (!response.Succeeded)
                throw new Exception("Failure doing GPO, SW=" + response.SW + " (" + response.SWTranslation + ")");

            return response.GetResponseTags();
        }

        public void StoreDataToApplication(GPStoreData irfi)
        {
            byte p1 = (byte)StoreDataRequestP1Enum.DGIformatofthecommanddatafield;
            if (irfi.IsLastBlock)
                p1 = (byte)(p1 | (byte)StoreDataRequestP1Enum.LastOrOnlyCommand);

            GPStoreDataReqest ir = new GPStoreDataReqest(irfi.Serialize(), p1, irfi.DataBlock);
            GPStoreDataResponse response = SendCommand(ir) as GPStoreDataResponse;
            if (!response.Succeeded)
                throw new Exception("Failure storing data to application, SW=" + response.SW + " (" + response.SWTranslation + ")");
        }


        public void InstallForPerso(String instanceId)
        {
            AID instance = new AID(instanceId);
            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            try
            {
                bo.Write((byte)0x00);
                bo.Write((byte)0x00);

                bo.Write((byte)instance.getLength());
                bo.Write(instance.getBytes());

                bo.Write((byte)0x00);
                bo.Write((byte)0x00);
                bo.Write((byte)0x00);
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }
            
            GPInstallRequest install = new GPInstallRequest((byte)InstallRequestP1Enum.LastOrOnlyCommand | (byte)InstallRequestP1Enum.ForPersonalization)
            {
                CommandData = bo.ToByteArray()
            };
            //System.Diagnostics.Debug.WriteLine(install.ToPrintString());
            GPInstallResponse response = (GPInstallResponse)SendCommand(install);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("Install for Perso failed");
        }

        private void installAndMakeSelectable(AID packageAID, AID appletAID, AID instanceAID, Privileges privileges, byte[] installParams, byte[] installToken)
        {
            if (instanceAID == null)
            {
                instanceAID = appletAID;
            }
            //if (getRegistry().allAppletAIDs().Contains(instanceAID))
            //{
                //giveStrictWarning("Instance AID " + instanceAID + " is already present on card");
            //}
            if (installParams == null)
            {
                installParams = new byte[] { (byte)0xC9, 0x00 };
            }
            if (installToken == null)
            {
                installToken = new byte[0];
            }
            byte[] privs = privileges.toBytes();
            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            try
            {
                bo.Write((byte)packageAID.getLength());
                bo.Write(packageAID.getBytes());

                bo.Write((byte)appletAID.getLength());
                bo.Write(appletAID.getBytes());

                bo.Write((byte)instanceAID.getLength());
                bo.Write(instanceAID.getBytes());

                bo.Write((byte)privs.Length);
                bo.Write(privs);

                bo.Write((byte)installParams.Length);
                bo.Write(installParams);

                bo.Write((byte)installToken.Length);
                bo.Write(installToken);
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }
            GPInstallRequest install = new GPInstallRequest(GPInstructionEnum.Install, bo.ToByteArray(), 0x0C, 0x00);
            //System.Diagnostics.Debug.WriteLine(install.ToPrintString());
            GPInstallResponse response = (GPInstallResponse)SendCommand(install);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("Install for Install and make selectable failed");
        }

        private void loadCapFile(String aid, CapFile cap)
        {
            loadCapFile(aid, cap, false, false, false, false);
        }

        private void loadCapFile(String sAID,CapFile cap, bool includeDebug, bool separateComponents, bool loadParam, bool useHash)
        {
            //if (getRegistry().allAIDs().Contains(cap.getPackageAID()))
            //{
            //    giveStrictWarning("Package with AID " + cap.getPackageAID() + " is already present on card");
            //}
            byte[] hash = useHash ? cap.getLoadFileDataHash("SHA1", includeDebug) : new byte[0];
            int len = cap.getCodeLength(includeDebug);
            // FIXME: parameters are optional for load
            byte[] loadParams = loadParam ? new byte[] { (byte) 0xEF, 0x04, (byte) 0xC6, 0x02, (byte) ((len & 0xFF00) >> 8),
                (byte) (len & 0xFF) } : new byte[0];

            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            try
            {
                bo.Write((byte)cap.getPackageAID().getLength());
                bo.Write(cap.getPackageAID().getBytes());

                AID aid = new AID(sAID);
                bo.Write((byte)aid.getLength());
                bo.Write(aid.getBytes());

                bo.Write((byte)hash.Length);
                bo.Write(hash);

                bo.Write((byte)loadParams.Length);
                bo.Write(loadParams);

                bo.Write((byte)0x00); //no load token
            }
            catch (IOException ioe)
            {
                throw new Exception(ioe.Message);
            }

            GPInstallRequest installForLoad = new GPInstallRequest(GPInstructionEnum.Install, bo.ToByteArray(), 0x02, 0x00);
            //System.Diagnostics.Debug.WriteLine(installForLoad.ToPrintString());
            GPInstallResponse response = (GPInstallResponse)SendCommand(installForLoad);
            if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                throw new Exception("Install for Load failed");

            List<byte[]> blocks = cap.getLoadBlocks(includeDebug, separateComponents, wrapper.getBlockSize());
            for (int i = 0; i < blocks.Count; i++)
            {
                GPInstallRequest load = new GPInstallRequest(GPInstructionEnum.Load, blocks[i], (byte)((i == (blocks.Count - 1)) ? 0x80 : 0x00), (byte)i);
                //System.Diagnostics.Debug.WriteLine(load.ToPrintString());
                response = (GPInstallResponse)SendCommand(load);
                if (response.SW != (ushort)ISO7816ReturnCodes.SW_NO_ERROR)
                    throw new Exception("Load failed");
            }
        }

        private static byte GetSetValue(List<APDUMode> s)
        {
            byte v = 0x00;
            foreach (APDUMode m in s)
                v |= (byte)m;
            return v;
        }
    }
}
