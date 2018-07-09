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
using System;

namespace DCEMV.Shared
{
    public enum MessageIdentifiersEnum
    {
        PleaseInsertOrSwipeCard,// = 0x18,
        PresentCard,// = 0x15,
        PleasePresentOneCardOnly,// = 0x19,
        InsertSwipeOrTryAnotherCard,// = 0x1C,
        TryAnotherCard,
        ClearDisplay,
        CardRemoved,
        CardInserted,
        TransmissionError,
        Approved,
        CardReadOk,
        TryAgain,
        ApprovedSign,
        Declined,
        SeePhone,
        Authorizing,
        NA,
        RemoveCard,
    }
    public enum StatusEnum
    {
        ProcessingError,
        ReadyToRead,
        ContactlessCollisionDetected_ProcessingError,
        EndProcessing,
        NotReady,
        Idle,
        CardReadOk,
        NA,
    }

    public class UserInterfaceRequest
    {
        public MessageIdentifiersEnum MessageIdentifier { get; set; }
        public StatusEnum Status { get; set; }
    }
    
    public class UIMessageEventArgs : EventArgs
    {
        public MessageIdentifiersEnum MessageIdentifiers { get; }
        public StatusEnum Status { get; }
        
        public UIMessageEventArgs(MessageIdentifiersEnum messageIdentifiers, StatusEnum status)
        {
            MessageIdentifiers = messageIdentifiers;
            Status = status;
        }

        public string MakeMessage()
        {
            if(!string.IsNullOrEmpty(AdditionalMessage))
                return string.Format("{0} : {1} : {2}", MessageIdentifiers, Status, AdditionalMessage.ToLower());
            else
                return string.Format("{0} : {1}", MessageIdentifiers, Status);
        }

        public uint HoldTime { get; set; }
        public string AdditionalMessage { get; set; }
    }
}
