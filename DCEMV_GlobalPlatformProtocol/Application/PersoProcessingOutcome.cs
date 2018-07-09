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
using DCEMV.GlobalPlatformProtocol;
using System;
using DCEMV.TLVProtocol;

namespace DCEMV.Shared
{
    public enum EMVPersoPreProcessingStateEnum
    {
        ProtocolActivation_StartB,
        EndProcess,
    }

    public class PersoProcessingOutcome
    {
        public EMVPersoPreProcessingStateEnum NextProcessState { get; set; }
        public bool UIRequestOnOutcomePresent { get; set; }
        public UserInterfaceRequest UserInterfaceRequest { get; set; }
        public bool UIRequestOnRestartPresent { get; set; }
        public GPRegistry GPRegistry { get; set; }
        public String CardData { get; set; }
        public TLVList TestOutCome { get; set; }
    }

    public class PersoProcessingOutcomeEventArgs : EventArgs
    {
        public PersoProcessingOutcome TerminalProcessingOutcome { get; set; }
    }
}
