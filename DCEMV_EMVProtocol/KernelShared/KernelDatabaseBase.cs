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
using System;
using DCEMV.TLVProtocol;
using System.Text;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum NextCommandEnum
    {
        READ_RECORD = 0x00,
        GET_DATA = 0x01,
        NONE = 0x10,
    }
    public class StaticDataToBeAuthenticatedList : TLVList
    {
        public static Logger Logger = new Logger(typeof(StaticDataToBeAuthenticatedList));
        private KernelDatabaseBase database; 

        public StaticDataToBeAuthenticatedList(KernelDatabaseBase database)
        {
            this.database = database;
        }
        
        public override void AddListToList(TLVList list, bool withDup = false, Func<TLV, bool> function = null)
        {
            base.AddListToList(list, true);//, (x) => { return TLVMetaDataSourceSingleton.Instance.DataSource.GetName(x.Tag.TagLable) == "Invalid tag"; });

            int depth = 0;
            Logger.Log("Added to StaticDataToBeAuthenticatedList: \n" + ToPrintString(ref depth));
        }

        public byte[] BuildStaticDataToBeAuthenticated()
        {
            TLV aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
            if (aip != null)
            {
                TLVList newList = new TLVList();
                foreach (TLV tlv in listToManage)
                {
                    if (tlv.Tag.TagLable != aip.Tag.TagLable)
                        newList.AddToList(tlv, true);
                }
                if (database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN.Tag))
                {
                    StringBuilder sb = new StringBuilder();
                    int depth = 0;
                    sb.Append("Final StaticDataToBeAuthenticatedList (AIP and STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN in DB): \n");
                    sb.AppendLine(newList.ToPrintString(ref depth));
                    depth = 1;
                    sb.AppendLine(aip.ToPrintString(ref depth));
                    Logger.Log(sb.ToString());
                    return Formatting.ConcatArrays(newList.Serialize(), aip.Value);
                }
                else
                {
                    int depth = 0;
                    Logger.Log("Final StaticDataToBeAuthenticatedList: (AIP in DB but No STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN in DB)\n" + ToPrintString(ref depth));
                    return newList.Serialize();
                }
            }
            else
            {
                int depth = 0;
                Logger.Log("Final StaticDataToBeAuthenticatedList (No AIP in DB): \n" + ToPrintString(ref depth));
                return Serialize();
            }

            
        }
    }
    public abstract class KernelDatabaseBase
    {
        public static Logger Logger = new Logger(typeof(KernelDatabaseBase));

        private TLVList DataObjects { get; set; }
        public PublicKeyCertificateManager PublicKeyCertificateManager { get; }
        public KernelConfigurationData KernelConfigurationData { get; protected set; }
        public APPLICATION_FILE_LOCATOR_AFL_94_KRN ActiveAFL { get; set; }
        public TLVList TagsToReadYet { get; }
        public StaticDataToBeAuthenticatedList StaticDataToBeAuthenticated { get; set; }
        public NextCommandEnum NextCommandEnum { get; set; }

        public int CVMCurrentlySelectedCounter { get; set; }

        public KernelDatabaseBase(PublicKeyCertificateManager publicKeyCertificateManager)
        {
            PublicKeyCertificateManager = publicKeyCertificateManager;
            KernelConfigurationData = new KernelConfigurationData();
            TagsToReadYet = new TLVList();
            StaticDataToBeAuthenticated = new StaticDataToBeAuthenticatedList(this);
        }

        public void AddToList(TLV tlv)
        {
            DataObjects.AddToList(tlv);
        }
        public void AddListToList(TLVList tlv)
        {
            DataObjects.AddListToList(tlv);
        }

        public void RemoveFromList(TLV tlv)
        {
            DataObjects.RemoveFromList(tlv);
        }

        public void Initialize(string tagLabel)
        {
            TLV tlv = DataObjects.Get(tagLabel);
            if (tlv != null)
                DataObjects.Get(tagLabel).Initialize();
            else
                AddToList(TLV.Create(tagLabel));
        }
        public TLV GetDefault(EMVTagMeta tag)
        {
            return Get(tag);
            //TLV result = Kernel2ConfigurationData.Get(tag.Tag, TransactionRequest.GetTransactionType_9C());
            //if (result == null)
            //    return null;
            //else
            //    return result;
        }
        public TLV Get(EMVTagMeta tag)
        {
            TLV result = DataObjects.Get(tag.Tag);
            if (result == null)
                return null;
            else
                return result;
        }
        public TLV Get(string tag)
        {
            TLV result = DataObjects.Get(tag);
            if (result == null)
                return null;
            else
                return result;
        }
        public int? GetLength(string tag)
        {
            TLV result = Get(tag);
            if (result == null)
                return null;
            else
                return result.Value.Length;
        }
        public bool ParseAndStoreCardResponse(byte[] input)
        {
            TLVList tlvs = new TLVList();
            tlvs.Deserialize(input);

            if (tlvs.Count != 1)
                return false;

            TLV tlvFirst = tlvs.GetFirst();

            bool parsingResult = ParseAndStoreCardResponse(tlvFirst);
            Logger.Log("Parsing Result:" + parsingResult);
            return parsingResult;
        }
        private bool PersistTLV(TLV tlv, string parentTemplate)
        {
            bool updateConditionsOfTIncludeRASignal = EMVTagsEnum.DoesTagIncludesPermission(tlv.Tag.TagLable, UpdatePermissionEnum.RA);
            bool isKnown = IsKnown(tlv.Tag.TagLable);
            if (!(isKnown && tlv.Tag.TagType == TagTypeEnum.Private && !updateConditionsOfTIncludeRASignal))
            {
                if (isKnown)
                {
                    bool lengthCorrect = false;
                    if (tlv.ValidateLength())
                        lengthCorrect = true;

                    bool tlvIsIncludedInTheCorrectTemplate = true;
                    if (!string.IsNullOrWhiteSpace(parentTemplate))
                        tlvIsIncludedInTheCorrectTemplate = EMVTagsEnum.CompareTemplate(tlv.Tag.TagLable, parentTemplate);

                    if ((IsNotPresent(tlv.Tag.TagLable) || IsEmpty(tlv.Tag.TagLable))
                        && updateConditionsOfTIncludeRASignal
                        && lengthCorrect
                        && tlvIsIncludedInTheCorrectTemplate)
                    {
                        AddToList(tlv);
                    }
                    else
                        return false;
                }
                else
                {
                    if (IsPresent(tlv.Tag.TagLable))
                    {
                        if (IsEmpty(tlv.Tag.TagLable) && updateConditionsOfTIncludeRASignal)
                        {
                            AddToList(tlv);
                        }
                        else
                            return false;
                    }
                }
                return true;
            }
            else
                return false;
        }
        public bool ParseAndStoreCardResponse(TLV tlvFirst)
        {
            try
            {
                TLVList tlvs = new TLVList();
                string parentTemplate = "";

                if (tlvFirst.Tag.IsConstructed)
                {
                    parentTemplate = tlvFirst.Tag.TagLable;

                    bool response = false;
                    if (tlvFirst.Children.Count == 0)
                        return true;
                    foreach (TLV tlv in tlvFirst.Children)
                    {
                        response = ParseAndStoreCardResponse(tlv);
                        if (!response) return response;
                    }
                    return response;
                }
                else
                    return PersistTLV(tlvFirst, "");
            }
            catch
            {
                return false;
            }
        }
        public bool IsKnown(string tag)
        {
            if (TLVMetaDataSourceSingleton.Instance.DataSource.GetName(tag) == null)
                return false;
            else
                return true;
        }
        public bool IsPresent(string tag)
        {
            //this will check if the object is present, objects may be added to the dictionaly after kernel instantiation
            //this checks the all the values including the default values
            TLV tlv = Get(tag);
            if (tlv == null)
                return false;
            else
                return true;
        }
        public bool IsNotPresent(string tag)
        {
            return !IsPresent(tag);
        }
        public bool IsNotEmpty(string tag)
        {
            return !IsEmpty(tag);
        }
        public bool IsEmpty(string tag)
        {
            TLV result = Get(tag);
            if (result == null)
                return true;
            else
            {
                if (result.Value.Length == 0)
                    return true;
                else
                    return false;
            }

        }
        public bool IsEmptyList(string tag)
        {
            return IsEmpty(tag);
        }
        public bool IsNotEmptyList(string tag)
        {
            return !IsEmpty(tag);
        }

        

        public void InitializeDefaultDataObjects(TransactionTypeEnum tt, IConfigurationProvider configProvider)
        {
            DataObjects = new TLVList();
            LoadKernelDefaultConfigurationDataObjects(tt, configProvider);
            TLVList defaults = KernelConfigurationData.Get(tt);
            foreach (TLV tlv in defaults)
            {
                byte[] val = new byte[tlv.Value.Length];
                Array.Copy(tlv.Value, 0, val, 0, val.Length);
                AddToList(TLV.Create(tlv.Tag.TagLable, val));
            }

            ActiveAFL = new APPLICATION_FILE_LOCATOR_AFL_94_KRN(this);
        }

        public virtual void UpdateWithDETData(TLVList terminalSentData)
        {
            try
            {
                foreach (TLV tlv in terminalSentData)
                {
                    int depth = 0;
                    bool updateConditionsOfTIncludeDETSignal = EMVTagsEnum.DoesTagIncludesPermission(tlv.Tag.TagLable, UpdatePermissionEnum.DET);
                    if ((IsKnown(tlv.Tag.TagLable) || IsPresent(tlv.Tag.TagLable)) && updateConditionsOfTIncludeDETSignal)
                    {
                        AddToList(tlv);
                        Logger.Log("Tag Added to DB: " + tlv.ToPrintString(ref depth));
                    }
                    else
                        Logger.Log("Tag Not Added to DB: " + tlv.ToPrintString(ref depth));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected abstract void LoadKernelDefaultConfigurationDataObjects(TransactionTypeEnum transactionTypeEnum, IConfigurationProvider configProvider);

        protected void AddDefaultTLV(TLVList list, EMVTagMeta tag, string hexValue)
        {
            list.AddToList(TLV.Create(tag.Tag, Formatting.HexStringToByteArray(hexValue)));
        }
    }
}
