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
using System;
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class CPLC
    {

        public enum Field
        {
            ICFabricator,
            ICType,
            OperatingSystemID,
            OperatingSystemReleaseDate,
            OperatingSystemReleaseLevel,
            ICFabricationDate,
            ICSerialNumber,
            ICBatchIdentifier,
            ICModuleFabricator,
            ICModulePackagingDate,
            ICCManufacturer,
            ICEmbeddingDate,
            ICPrePersonalizer,
            ICPrePersonalizationEquipmentDate,
            ICPrePersonalizationEquipmentID,
            ICPersonalizer,
            ICPersonalizationDate,
            ICPersonalizationEquipmentID
        };

        private Dictionary<Field, byte[]> values = null;

        public CPLC(byte[] data)
        {
            if (data == null || data.Length < 3 || data[2] != 0x2A)
                throw new Exception("CPLC must be 0x2A bytes long");
           
            short offset = 3;
            values = new Dictionary<Field, byte[]>();
            values.Add(Field.ICFabricator, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICType, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.OperatingSystemID, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.OperatingSystemReleaseDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.OperatingSystemReleaseLevel, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICFabricationDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICSerialNumber, Formatting.copyOfRange(data, offset, offset + 4)); offset += 4;
            values.Add(Field.ICBatchIdentifier, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICModuleFabricator, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICModulePackagingDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICCManufacturer, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICEmbeddingDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICPrePersonalizer, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICPrePersonalizationEquipmentDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICPrePersonalizationEquipmentID, Formatting.copyOfRange(data, offset, offset + 4)); offset += 4;
            values.Add(Field.ICPersonalizer, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICPersonalizationDate, Formatting.copyOfRange(data, offset, offset + 2)); offset += 2;
            values.Add(Field.ICPersonalizationEquipmentID, Formatting.copyOfRange(data, offset, offset + 4)); offset += 4;
        }

        public override String ToString()
        {
            String s = "Card CPLC:";
            foreach (KeyValuePair<Field, byte[]> f in values)
            {
                s += "\n" + f.Key + ": " + Formatting.ByteArrayToHexString(f.Value);
            }
            return s;
        }
    }
}
