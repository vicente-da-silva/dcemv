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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DCEMV.TLVProtocol
{
    public sealed class TLVasJSON
    {
        public string Tag { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public List<TLVasJSON> Children { get; set; }

        public TLVasJSON()
        {
            Children = new List<TLVasJSON>();
        }

        public static TLVasJSON Convert(TLV tlv)
        {
            TLVasJSON json = new TLVasJSON()
            {
                Tag = tlv.Tag.TagLable,
                Name = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(tlv.Tag.TagLable),
            };

            if (tlv.Tag.IsConstructed)
            {
                foreach (TLV tlvChild in tlv.Children)
                {
                    json.Children.Add(Convert(tlvChild));
                }
            }
            else
            {
                json.Value = FormattingUtils.Formatting.ByteArrayToHexString(tlv.Value);
            }
            return json;
        }
        public static TLV Convert(TLVasJSON tlvasJSON)
        {
            TLV tlv = TLV.Create(tlvasJSON.Tag);
            if (tlv.Tag.IsConstructed)
            {
                foreach (TLVasJSON tlvChild in tlvasJSON.Children)
                {
                    tlv.Children.AddToList(Convert(tlvChild));
                }
            }
            else
            {
                tlv.Value = FormattingUtils.Formatting.HexStringToByteArray(tlvasJSON.Value);
            }
            return tlv;
        }

        public static string ToJSON(TLV tlv)
        {
            return JsonConvert.SerializeObject(Convert(tlv));
        }
        public static TLV FromJSON(string json)
        {
            return Convert(JsonConvert.DeserializeObject<TLVasJSON>(json));
        }
    }
}
