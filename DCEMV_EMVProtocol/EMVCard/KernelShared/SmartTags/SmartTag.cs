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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public abstract class SmartTag : TLV
    {
        protected EMVTagMeta EMVTagMeta { get; set; }
        private KernelDatabaseBase database;

        public SmartTag(EMVTagMeta emvTagMeta, V value)
            : base()
        {
            EMVTagMeta = emvTagMeta;
            Tag = new T(EMVTagMeta.Tag);
            Val = value;
        }

        public SmartTag(TLV tlv, EMVTagMeta emvTagMeta, V value)
            : base()
        {
            EMVTagMeta = emvTagMeta;

            Tag = new T(EMVTagMeta.Tag);
            Val = value;

            if (tlv != null && tlv.Val.Value.Length > 0)
            {
                Val.Deserialize(tlv.Val.Serialize(), 0);
            }
        }

        public SmartTag(KernelDatabaseBase database, EMVTagMeta emvTagMeta, V value)
            :base()
        {
            this.database = database;
            EMVTagMeta = emvTagMeta;

            Tag = new T(EMVTagMeta.Tag);
            Val = value;

            TLV tlv = database.Get(EMVTagMeta);
            if(tlv != null && tlv.Val.Value.Length > 0)
            {
                Val.Deserialize(tlv.Val.Serialize(),0);
            }
        }

        public virtual void UpdateDB()
        {
            if (database == null)
                throw new EMVProtocolException("cannot call UpdateDB with database = null");

            TLV tlv = database.Get(EMVTagMeta);

            if (tlv != null)
            {
                tlv.Val.Deserialize(Val.Serialize(), 0);
            }
            else
            {
                Val.Serialize();
                database.AddToList(TLV.Create(this.Tag.TagLable,Val.Value));
            }
        }
    }
}
