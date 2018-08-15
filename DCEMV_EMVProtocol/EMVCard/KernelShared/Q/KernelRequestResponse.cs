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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum KernelReaderTerminalServiceResponseEnum
    {
        QUERY_REPLY,
        OUT, //kernel is complete
        STOP_ACK,
        DEK, //when kernel wants more data from terminal
        UI, //kernel wants to display a message
        PIN, //kernel wants terminal to prompt for pin
        TRM, //kernel wants terminal to perform terminal risk management
        ONLINE, //kernel wants terminal to go online
    }
    public enum KernelTerminalReaderServiceRequestEnum
    {
        UPD,
        QUERY,
        ACT,
        STOP,
        ABORT,
        MSG,
        DET,
        CLEAN,
        TIMEOUT,
        PIN, //Terminal is informing kernel of pin response
        TRM,//Terminal is informing kernel of trm response
        ONLINE, //Terminal is informing kernel of online response
    }

    public abstract class KernelResponseBase
    {
        public KernelReaderTerminalServiceResponseEnum KernelReaderTerminalServiceResponseEnum { get; protected set; }
    }

    public class KernelRequest
    {
        public KernelTerminalReaderServiceRequestEnum KernelTerminalReaderServiceRequestEnum { get; }
        public TLVList InputData { get; }

        public KernelRequest(KernelTerminalReaderServiceRequestEnum enumVal, TLVList inputData)
        {
            this.KernelTerminalReaderServiceRequestEnum = enumVal;
            this.InputData = inputData;
        }
    }

    public enum KernelTRMRequestType
    {
        GoOnline,
        GoOnlineForRandomSelection,
        Decline,
        Approve,
        DontCare
    }
    public enum KernelOnlineResponseType
    {
        UnableToGoOnline,
        Decline,
        Approve,
    }
    public class KernelOnlineRequest : KernelRequest
    {
        public KernelOnlineResponseType OnlineApprovalStatus { get; }
        public KernelOnlineRequest(TLVList inputData, KernelOnlineResponseType onlineApprovalStatus) : base(KernelTerminalReaderServiceRequestEnum.ONLINE, inputData)
        {
            OnlineApprovalStatus = onlineApprovalStatus;
        }
    }

    public class KernelTRMRequest : KernelRequest
    {
        public KernelTRMRequestType KernelTRMRequestType { get; set; }
        public KernelTRMRequest(TLVList inputData) : base(KernelTerminalReaderServiceRequestEnum.TRM, inputData)
        {

        }
    }

    public class KernelDEKResponse: KernelResponseBase
    {
        public DATA_TO_SEND_FF8104_KRN2 DataToSend { get; }
        public DATA_NEEDED_DF8106_KRN2 DataNeeded { get; }

        public KernelDEKResponse(DATA_TO_SEND_FF8104_KRN2 dataToSend, DATA_NEEDED_DF8106_KRN2 dataNeeded)
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.DEK;

            DataNeeded = dataNeeded;
            DataToSend = dataToSend;
        }
    }

    public class KernelOnlineResponse : KernelResponseBase
    {
        public TLV data { get; }
        public TLV discretionaryData { get; }
        public KernelOnlineResponse(TLV data, TLV discretionaryData)
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.ONLINE;
            this.data = data;
            this.discretionaryData = discretionaryData;
        }
    }
    public class KernelTRMResponse : KernelResponseBase
    {
        public KernelTRMResponse()
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.TRM;
        }
    }
    public class KernelPinResponse : KernelResponseBase
    {
        public KernelPinResponse()
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.PIN;
        }
    }

    public class KernelUIResponse : KernelResponseBase
    {
        public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 UserInterfaceRequest_DF8116 { get; }

        public KernelUIResponse(TLV userInterfaceRequest)
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.UI;
            UserInterfaceRequest_DF8116 = (USER_INTERFACE_REQUEST_DATA_DF8116_KRN2)userInterfaceRequest;
        }
    }

    public class KernelOUTResponse: KernelResponseBase
    {
        public TLV DataRecord_FF8105 { get; }
        public TLV DiscretionaryData_FF8106 { get; }

        public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 UserInterfaceRequest_DF8116 { get; }

        public OUTCOME_PARAMETER_SET_DF8129_KRN2 OutcomeParameterSet_DF8129 { get; }

        public ERROR_INDICATION_DF8115_KRN2 ErrorIndication_DF8115 { get; }

        public KernelOUTResponse(TLV outcomeParameterSet, 
            TLV errorIndication,
            TLV dataRecord, 
            TLV discretionaryData, 
            TLV userInterfaceRequest)
        {
            KernelReaderTerminalServiceResponseEnum = KernelReaderTerminalServiceResponseEnum.OUT;

            OutcomeParameterSet_DF8129 = new OUTCOME_PARAMETER_SET_DF8129_KRN2(outcomeParameterSet);

            if (dataRecord != null)
            {
                DataRecord_FF8105 = dataRecord;
                OutcomeParameterSet_DF8129.Value.DataRecordPresent = true;
            }
            if (discretionaryData != null)
            {
                DiscretionaryData_FF8106 = discretionaryData;
                OutcomeParameterSet_DF8129.Value.DiscretionaryDataPresent = true;
            }
            if (userInterfaceRequest != null)
            {
                if (!OutcomeParameterSet_DF8129.Value.UIRequestOnRestartPresent)
                    OutcomeParameterSet_DF8129.Value.UIRequestOnOutcomePresent = true;

                UserInterfaceRequest_DF8116 = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(userInterfaceRequest);
            }

            ErrorIndication_DF8115 = new ERROR_INDICATION_DF8115_KRN2(errorIndication);
        }

    }
}
