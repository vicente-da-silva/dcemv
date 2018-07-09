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
namespace DCEMV.SPDHProtocol
{
    internal class HeaderDeviceType : HeaderEntryBase
    {
        public HeaderDeviceType() : base("Device Type", 1, 2, 2, FormatEnum.Hex)
        {
        }
    }
    internal class HeaderTransmissionNumber : HeaderEntryBase
    {
        public HeaderTransmissionNumber() : base("Transmission Number", 3, 4, 2, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderTerminalID : HeaderEntryBase
    {
        public HeaderTerminalID() : base("Terminal ID", 5, 20, 16, FormatEnum.Hex)
        {
        }
    }
    internal class HeaderEmployeeID : HeaderEntryBase
    {
        public HeaderEmployeeID() : base("Employee ID", 21, 26, 6, FormatEnum.Hex)
        {
        }
    }
    internal class HeaderCurrentDate : HeaderEntryBase
    {
        public HeaderCurrentDate() : base("Current Date", 27, 32, 6, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderCurrentTime : HeaderEntryBase
    {
        public HeaderCurrentTime() : base("Current Time", 33, 38, 6, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderMessageType : HeaderEntryBase
    {
        public HeaderMessageType() : base("Message Type", 39, 39, 1, FormatEnum.Hex)
        {
        }
    }
    internal class HeaderMessageSubtype : HeaderEntryBase
    {
        public HeaderMessageSubtype() : base("Message Subtype", 40, 40, 1, FormatEnum.Hex)
        {
        }
    }
    internal class HeaderTransactionCode : HeaderEntryBase
    {
        public HeaderTransactionCode() : base("Transaction Code", 41, 42, 2, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderProcessingFlag1 : HeaderEntryBase
    {
        public HeaderProcessingFlag1() : base("Processing Flag 1", 43, 43, 1, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderProcessingFlag2 : HeaderEntryBase
    {
        public HeaderProcessingFlag2() : base("Processing Flag 2", 44, 44, 1, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderProcessingFlag3 : HeaderEntryBase
    {
        public HeaderProcessingFlag3() : base("Processing Flag 3", 45, 45, 1, FormatEnum.Numeric)
        {
        }
    }
    internal class HeaderResponseCode : HeaderEntryBase
    {
        public HeaderResponseCode() : base("Response Code", 46, 48, 3, FormatEnum.Numeric)
        {
        }
    }
    public enum HeaderEntryEnum
    {
        DeviceType,
        TransmissionNumber,
        TerminalID,
        EmployeeID,
        CurrentDate,
        CurrentTime,
        MessageType,
        MessageSubtype,
        TransactionCode,
        ProcessingFlag1,
        ProcessingFlag2,
        ProcessingFlag3,
        ResponseCode,

    }
    internal class Header : HeaderBase
    {
        public Header()
        {
            Entries.Add(HeaderEntryEnum.DeviceType, new HeaderDeviceType());
            Entries.Add(HeaderEntryEnum.TransmissionNumber, new HeaderTransmissionNumber());
            Entries.Add(HeaderEntryEnum.TerminalID, new HeaderTerminalID());
            Entries.Add(HeaderEntryEnum.EmployeeID, new HeaderEmployeeID());
            Entries.Add(HeaderEntryEnum.CurrentDate, new HeaderCurrentDate());
            Entries.Add(HeaderEntryEnum.CurrentTime, new HeaderCurrentTime());
            Entries.Add(HeaderEntryEnum.MessageType, new HeaderMessageType());
            Entries.Add(HeaderEntryEnum.MessageSubtype, new HeaderMessageSubtype());
            Entries.Add(HeaderEntryEnum.TransactionCode, new HeaderTransactionCode());
            Entries.Add(HeaderEntryEnum.ProcessingFlag1, new HeaderProcessingFlag1());
            Entries.Add(HeaderEntryEnum.ProcessingFlag2, new HeaderProcessingFlag2());
            Entries.Add(HeaderEntryEnum.ProcessingFlag3, new HeaderProcessingFlag3());
            Entries.Add(HeaderEntryEnum.ResponseCode, new HeaderResponseCode());
        }
    }

}
