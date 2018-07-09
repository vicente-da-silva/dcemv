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
using System.Linq;
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class CoreSetConfigCommand : CoreCommand
    {
        private ConfigParameter[] parameters;

        public ConfigParameter[] ConfigParameters
        {
            get { return parameters; }
            set { parameters = value; }
        }

        public CoreSetConfigCommand(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeCoreIdentifierEnum.CORE_SET_CONFIG_CMD) { }

        public CoreSetConfigCommand(PacketBoundryFlagEnum pbf, OpcodeCoreIdentifierEnum type) : base(pbf,type) { }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            parameters = new ConfigParameter[payLoad[0]];
            byte pos = 1;
            for (int i = 0; i < payLoad[0]; i++)
            {
                parameters[i] = new ConfigParameter();
                pos = parameters[i].deserialize(payLoad, pos);
            }
        }

        public override byte[] serialize()
        {
            byte[][] parametersArray = new byte[parameters.Length + 1][]; //+1 for length value
            for (byte i = 0; i < parameters.Length; i++)
            {
                ConfigParameter paramObject = parameters[i];
                parametersArray[i+1] = paramObject.serialize();
            }
            parametersArray[0] = new byte[] { (byte)parameters.Length };
            payLoad = parametersArray.SelectMany(x => x).ToArray(); //flatten array
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("NumberOfParameters: " + parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                sb.AppendLine("Parameter: " + (i + 1));
                sb.AppendLine(parameters[i].ToString());
            }
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
