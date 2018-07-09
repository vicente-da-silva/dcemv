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
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol
{
    public class TerminalConfigurationData
    {
        public TLVList TerminalConfigurationDataObjects { get; protected set; }
        public static Logger Logger = new Logger(typeof(TerminalConfigurationData));

        public TerminalConfigurationData()
        {
        }

        public void LoadTerminalConfigurationDataObjects(KernelEnum kernel, IConfigurationProvider configProvider)
        {
            TerminalConfigurationDataObjects = TLVListXML.XmlDeserialize(configProvider.GetTerminalConfigurationDataXML(Formatting.ByteArrayToHexString(new byte[] { (byte)kernel })));

            int depth = 0;
            Logger.Log("Using Terminal Defaults: \n" + TerminalConfigurationDataObjects.ToPrintString(ref depth));
        }
        
        private void AddDefaultTLV(EMVTagMeta tag, string hexValue)
        {
            TerminalConfigurationDataObjects.AddToList(TLV.Create(tag.Tag, Formatting.HexStringToByteArray(hexValue)));
        }

        private void AddDefaultTLV(EMVTagMeta tag)
        {
            TerminalConfigurationDataObjects.AddToList(TLV.Create(tag.Tag));
        }
    }
}
