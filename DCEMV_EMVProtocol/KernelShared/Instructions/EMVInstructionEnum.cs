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
namespace DCEMV.EMVProtocol
{
    public enum EMVInstructionEnum : byte
    {
        SelectPPSE = 0xA4,
        GetProcessingOptions = 0xA8,
        ReadRecord = 0xB2,
        ExchangeRelayResistanceData = 0xEA,
        GetData = 0xCA,
        PutData = 0xDA,
        GenerateAC = 0xAE,
        RecoverAC = 0xD0,
        ComputeCryptographickChecksum = 0x2A,
        InternalAuthenticate = 0x88,
        ExternalAuthenticate = 0x82,
        GetResponse = 0xC0,
        GetChallenge = 0x84,
        Verify = 0x20,
    };
}
