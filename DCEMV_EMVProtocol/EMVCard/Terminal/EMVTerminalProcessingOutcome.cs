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
using DCEMV.TLVProtocol;
using DCEMV.EMVProtocol.EMVQRCode;
using DCEMV_QRDEProtocol;

namespace DCEMV.EMVProtocol
{
    public class EMVTerminalProcessingOutcome : TerminalProcessingOutcome
    {
        public TLV DataRecord { get; set; }
        public TLV DiscretionaryData { get; set; }
        public QRDEList QRData { get; set; }
        public KernelCVMEnum CVM { get; set; }
    }
}
