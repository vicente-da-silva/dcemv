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
using DataFormatters;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public enum C9_ApplicationInstance
    {
        Main,
        Alias
    }
    public enum C9_PinSharing
    {
        NoPinSharingOrAliasNotApplicable = 0x00,
        PinSharingBetweenInstances = 0x40,
        GlobalPinSharing = 0x20,
    }
    public enum C9_InterfacesAvailable
    {
        DualInterfaceContactAndContactless = 0x01,
        DualInterfaceContactOnly = 0x02,
        DualInterfaceContactlessOnly = 0x03,
        ContactOnly = 0x00
    }

    public class INSTALL_PARAM_C9_GP : TLV
    {
        protected EMVTagMeta GPTagMeta { get; private set; }

        public class INSTALL_PARAM_C9_GP_VALUE : V
        {
            public INSTALL_PARAM_C9_GP_VALUE(DataFormatterBase dataFormatter)
                : base(dataFormatter, new byte[0])
            {

            }

            public C9_ApplicationInstance ApplicationInstance { get; set; }
            public C9_PinSharing PinSharing { get; set; }
            public C9_InterfacesAvailable InterfacesAvailable { get; set; }
            public byte PDEMaxLength{ get; set; }
            public bool PDEPresent { get; set; }


            public override byte[] Serialize()
            {
                if (PDEPresent)
                    Value = new byte[2];
                else
                    Value = new byte[1];

                Value[0] = 0x00;
                Formatting.SetBitPosition(ref Value[0], ApplicationInstance == C9_ApplicationInstance.Main ? true : false, 8);
                Value[0] = (byte)(Value[0] | (byte)PinSharing);
                Value[0] = (byte)(Value[0] | (byte)InterfacesAvailable);

                if(PDEPresent)
                    Value[1] = PDEMaxLength;

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                if (Value.Length == 0)
                    return pos;

                ApplicationInstance = Formatting.GetBitPosition(Value[0], 8) ? C9_ApplicationInstance.Main : C9_ApplicationInstance.Alias;
                if (ApplicationInstance == C9_ApplicationInstance.Alias)
                    PinSharing = C9_PinSharing.NoPinSharingOrAliasNotApplicable;
                else
                {
                    if ((Value[0] & 0x60) == 0x00)
                        PinSharing = C9_PinSharing.NoPinSharingOrAliasNotApplicable;
                    if ((Value[0] & 0x60) == 0x20)
                        PinSharing = C9_PinSharing.GlobalPinSharing;
                    if ((Value[0] & 0x60) == 0x40)
                        PinSharing = C9_PinSharing.PinSharingBetweenInstances;
                }
                if ((Value[0] & 0x03) == 0x00)
                    InterfacesAvailable = C9_InterfacesAvailable.ContactOnly;
                if ((Value[0] & 0x03) == 0x01)
                    InterfacesAvailable = C9_InterfacesAvailable.DualInterfaceContactAndContactless;
                if ((Value[0] & 0x03) == 0x02)
                    InterfacesAvailable = C9_InterfacesAvailable.DualInterfaceContactOnly;
                if ((Value[0] & 0x03) == 0x03)
                    InterfacesAvailable = C9_InterfacesAvailable.DualInterfaceContactlessOnly;

                if (Value.Length > 1)
                {
                    PDEPresent = true;
                    PDEMaxLength = Value[1];
                }

                return pos;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                
                if (Value.Length == 0)
                    sb.Append("V:[]");
                else
                {
                    sb.AppendLine(base.ToString());
                    sb.AppendLine(string.Format("{0,-30}:{1}", "C9_ApplicationInstance", ApplicationInstance));
                    sb.AppendLine(string.Format("{0,-30}:{1}", "C9_PinSharing", PinSharing));
                    sb.AppendLine(string.Format("{0,-30}:{1}", "C9_InterfacesAvailable", InterfacesAvailable));
                    if(PDEPresent)
                        sb.Append(string.Format("{0,-30}:{1:X}", "C9_PDEMaxLength", PDEMaxLength));
                    else
                        sb.Append(string.Format("{0,-30}:{1}", "C9_PDEMaxLength", "Not Present"));
                }

                return sb.ToString();
            }
        }
        public new INSTALL_PARAM_C9_GP_VALUE Value { get { return (INSTALL_PARAM_C9_GP_VALUE)Val; } }

        public INSTALL_PARAM_C9_GP()
            : base()
        {
            GPTagMeta = EMVTagsEnum.INSTALL_PARAM_OR_APPLICATION_CONTACTLESS_USAGE_C9_GP;
            Tag = new T(GPTagMeta.Tag);
            Val = new INSTALL_PARAM_C9_GP_VALUE(GPTagMeta.DataFormatter);
        }

        public override int Deserialize(byte[] rawTlv, int pos)
        {
            pos = base.Deserialize(rawTlv, pos);
            byte[] valCurrent = Val.Serialize();

            Val = new INSTALL_PARAM_C9_GP_VALUE(GPTagMeta.DataFormatter);
            if (valCurrent.Length > 2)
                ((INSTALL_PARAM_C9_GP_VALUE)Val).PDEPresent = true;
            Val.Deserialize(valCurrent,0);

            return pos;
        }

    }
}
