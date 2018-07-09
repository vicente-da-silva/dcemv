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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DCEMV.TLVProtocol
{
    public class TLVList 
    {
        protected List<TLV> listToManage;

        public TLVList()
        {
            listToManage = new List<TLV>();
        }

        public void Initialize()
        {
            listToManage.Clear();
        }
        
        public TLV GetFirst()
        {
            if (listToManage.Count > 0)
                return listToManage[0];
            else
                return null;
        }

        public byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>();
            foreach(TLV tlv in listToManage)
                result.Add(tlv.Serialize());
            return result.SelectMany(a => a).ToArray();
        }

        public void Deserialize(byte[] rawTLV, bool withDup = false)
        {
            for (int i = 0; i < rawTLV.Length;)
                AddToList(TLV.Create(rawTLV, ref i), withDup);
        }
        
        public string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbTab = new StringBuilder();
            for (int j = 0; j <= depth; j++)
                sbTab.Append("\t");

            for (int i = 0; i < listToManage.Count; i++)
                if(i == listToManage.Count - 1)
                    sb.Append(sbTab.ToString() + listToManage[i].ToPrintString(ref depth));
                else
                    sb.AppendLine(sbTab.ToString() + listToManage[i].ToPrintString(ref depth));

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listToManage.Count; i++)
                sb.Append(listToManage[i].ToString());
            return sb.ToString();
        }

        public bool AreAnyItemsConstructed()
        {
            foreach (TLV tlv in listToManage)
                if (tlv.Tag.IsConstructed)
                    return true;

            return false;
        }

        public IEnumerator<TLV> GetEnumerator()
        {
            return listToManage.GetEnumerator();
        }

        public int Count { get { return listToManage.Count; } }
        
        public void AddToListIncludeDuplicates(TLV tlv)
        {
            listToManage.Add(tlv);
        }

        public void RemoveFromList(TLV tlv)
        {
            if (tlv == null)
                return;
            List<TLV> result = listToManage.Where(x => x.Tag.TagLable == tlv.Tag.TagLable).ToList();
            if (result.Count != 0)
            {
                listToManage.Remove(result[0]);
            }
        }
        
        public virtual void AddListToList(TLVList list, bool withDup = false, Func<TLV,bool> function = null)
        {
            foreach (TLV tlv in list.listToManage)
                AddToList(tlv, withDup, function);
        }
        public virtual void AddToList(TLV tlv, bool withDup = false, Func<TLV,bool> function = null)
        {
            if (function != null)
            {
                if (function.Invoke(tlv))
                    return;
            }

            if (withDup == false)
            {
                List<TLV> result = listToManage.Where(x => x.Tag.TagLable == tlv.Tag.TagLable).ToList();
                if (result.Count != 0)
                {
                    TLV tlvFound = result[0];
                    tlvFound.Value = tlv.Value;
                }
                else
                    listToManage.Add(tlv);
            }
            else
                listToManage.Add(tlv);
        }

        public TLV GetFirstAndRemoveFromList()
        {
            TLV result = null;
            if (listToManage.Count > 0)
            {
                result = listToManage[0];
                listToManage.RemoveAt(0);
            }
            return result;
        }

        public TLV GetLastAndRemoveFromList()
        {
            TLV result = null;
            if (listToManage.Count > 0)
            {
                result = listToManage[listToManage.Count - 1];
                listToManage.RemoveAt(listToManage.Count - 1);
            }
            return result;
        }


        public TLV GetNextGetDataTagFromList()
        {
            if (listToManage.Count == 0)
                return null;

            TLV tlvFound = listToManage.First();
            if (tlvFound != null)
            {
                listToManage.Remove(tlvFound);
                return tlvFound;
            }
            else
                return null;
        }

        public TLV Get(string tag)
        {
            List<TLV> result = listToManage.Where(x => x.Tag.TagLable == tag).ToList();
            if (result.Count == 0)
                return null;
            if (result.Count > 1)
                throw new Exception("TLVList:GetTLV Duplicate tag found:" + result[0].Tag.TagLable);
            return result[0];
        }

        public TLVList FindAll(string tag)
        {
            List<TLV> result = listToManage.FindAll(x => x.Tag.TagLable == tag).ToList();
            return new TLVList() { listToManage = result };
        }

        public bool IsEmpty()
        {
            return listToManage.Count == 0 ? true : false;
        }

        public bool IsNotEmpty()
        {
            return listToManage.Count > 0 ? true : false;
        }

        public bool IsNotEmpty(string tag)
        {
            return IsNotEmpty(tag);
        }
        public bool IsEmpty(string tag)
        {
            TLV result = Get(tag);
            if (result == null)
                return false;
            else
            {
                if (result.Value.Length == 0)
                    return true;
                else
                    return false;
            }

        }

        public bool IsPresent(string tag)
        {
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
    }

    public class TLVListXML
    {
        public static Logger Logger = new Logger(typeof(TLVListXML));

        //[XmlElement (ElementName = "tlvxml")]
        public class TLVXML
        {
            //[XmlAttribute(AttributeName = "tag")]
            //public string Tag { get; set; }
            //[XmlElement(DataType = "hexBinary", ElementName = "value")]
            //public byte[] Value { get; set; }
            //[XmlElement(ElementName = "children")]
            //public List<TLVXML> Children { get; set; }

            public string Tag { get; set; }
            [XmlElement(DataType = "hexBinary")]
            public byte[] Value { get; set; }
            public List<TLVXML> Children { get; set; }
        }

        public static string XmlSerialize(TLVList list)
        {
            return XMLUtil<List<TLVXML>>.Serialize(SerList(list));
        }

        public static TLVList XmlDeserialize(string list)
        {
            return DeserList(XMLUtil<List<TLVXML>>.Deserialize(list));
        }

        private static List<TLVXML> SerList(TLVList list)
        {
            IEnumerator<TLV> e = list.GetEnumerator();
            List<TLVXML> serXML = new List<TLVXML>();

            while (e.MoveNext())
            {
                TLV value = e.Current;
                TLVXML tlvXML = new TLVXML() { Tag = value.Tag.TagLable };
                if (value.Tag.IsConstructed)
                    tlvXML.Children = SerList(value.Children);
                else
                    tlvXML.Value = value.Value;

                serXML.Add(tlvXML);
            }
            return serXML;
        }

        private static TLVList DeserList(List<TLVXML> serXML)
        {
            TLVList result = new TLVList();
            serXML.ForEach((x) =>
            {
                TLV tlvToAdd = TLV.Create(x.Tag);
                result.AddToList(tlvToAdd);
                if (x.Children.Count > 0)
                    tlvToAdd.Children.AddListToList(DeserList(x.Children));
                else
                    if (x.Value != null)
                    tlvToAdd.Value = x.Value;
            });
            return result;
        }
    }
}
