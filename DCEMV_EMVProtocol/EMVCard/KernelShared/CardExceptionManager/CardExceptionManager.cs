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
using System.Collections.Generic;
using System.Linq;

namespace DCEMV.EMVProtocol.Kernels
{
    public class HotCard
    {
        public string PAN { get; set; }
    }

    public class CardExceptionManager
    {
        public static Logger Logger = new Logger(typeof(CardExceptionManager));
        private List<HotCard> TerminalExceptionFile { get; set; }

        public CardExceptionManager(IConfigurationProvider configProvider)
        {
            LoadTerminalExceptionFile(configProvider);
        }
        public bool CheckForCardException(string pan)
        {
            if (TerminalExceptionFile.Count(x => x.PAN == pan) == 0)
                return false;
            else
                return true;
        }

        public void LoadTerminalExceptionFile(IConfigurationProvider configProvider)
        {
            Logger.Log("Terminal Exception File:");
            TerminalExceptionFile = XMLUtil<List<HotCard>>.Deserialize(configProvider.GetExceptionFileXML());
        }
    }
}
