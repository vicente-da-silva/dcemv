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
namespace DCEMV.EMVProtocol.Kernels
{
    public class EntryPointPreProcessingIndicators
    {
        public bool StatusCheckRequested { get; set; }
        public bool ContactlessApplicationNotAllowed { get; set; }
        public bool ZeroAmount { get; set; }
        //Reader CVM Required Limit Exceeded
        public bool ReaderContactlessFloorLimitExceeded { get; set; }
        public bool ReaderCVMRequiredLimitExceeded { get; set; }
        //Copy of TTQ(if present as part of configuration data);
        public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN TTQ { get; set; }
    }
}
