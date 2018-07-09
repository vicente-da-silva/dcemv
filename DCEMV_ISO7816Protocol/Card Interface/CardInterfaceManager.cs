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
using DCEMV.FormattingUtils;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCEMV.ISO7816Protocol
{
    public enum CardinterfaceServiceRequestEnum
    {
        ADPU,
    }
    public enum CardInterfaceServiceResponseEnum
    {
        RA,
        L1RSP,
    }
    public enum L1Enum
    {
        NOT_SET = 0xFF,

        OK = 0x00,
        TIME_OUT_ERROR = 0x01,
        TRANSMISSION_ERROR = 0x02,
        PROTOCOL_ERROR = 0x3,
        RETURN_CODE = 0x4,
    }

    public class CardQ : InOutQsBase<CardRequest, CardResponse>
    {
        public CardQ(int timeoutMS) : base(timeoutMS)
        {
        }
    }

    public class CardRequest
    {
        public CardinterfaceServiceRequestEnum CardinterfaceServiceRequestEnum { get; }
        public ApduCommand ApduCommand { get; }

        public CardRequest(ApduCommand apduCommand, CardinterfaceServiceRequestEnum kernel1CardinterfaceServiceRequestEnum)
        {
            this.ApduCommand = apduCommand;
            this.CardinterfaceServiceRequestEnum = kernel1CardinterfaceServiceRequestEnum;
        }
    }

    public class CardResponse
    {
        public CardInterfaceServiceResponseEnum CardInterfaceServiceResponseEnum { get; }
        public L1Enum L1Enum { get; }
        public ApduResponse ApduResponse { get; }
        public CardResponse(ApduResponse apduResponse, CardInterfaceServiceResponseEnum kernel1CardinterfaceServiceResponseEnum)
        {
            this.ApduResponse = apduResponse;
            this.CardInterfaceServiceResponseEnum = kernel1CardinterfaceServiceResponseEnum;
        }
    }

    public class CardQProcessor
    {
        public static Logger Logger = new Logger(typeof(CardQProcessor));

        public event EventHandler ExceptionOccured;

        private string deviceId;

        private void OnExceptionOccured(Exception e)
        {
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }

        public CardQ CardQ { get; }

        public ICardInterfaceManger CardInterfaceManger { get; }

        protected CancellationTokenSource cancellationTokenForCardInterfaceManager;

        public CardQProcessor(ICardInterfaceManger cardInterfaceManger, string deviceId)
        {
            this.deviceId = deviceId;
            cancellationTokenForCardInterfaceManager = new CancellationTokenSource();
            CardQ = new CardQ(1000);
            CardInterfaceManger = cardInterfaceManger;
        }

        public void StartServiceQProcess()
        {
            Task.Run(()=> CardInterfaceManger.StartCardReaderAsync(deviceId)).Wait();
            Task.Run(() => StartServicingeQ(), cancellationTokenForCardInterfaceManager.Token);
        }

        public void StopServiceQProcess()
        {
            try
            {
                if(!cancellationTokenForCardInterfaceManager.IsCancellationRequested)
                    cancellationTokenForCardInterfaceManager.Cancel();
                CardInterfaceManger.StopCardReaderAsync();
            }
            catch
            {
                //do nothing
            }
        }

        private async Task StartServicingeQ()
        {
            //run for as long as card reader is active
            while (1 == 1)
            {
                try
                {
                    CardRequest cardRequest = CardQ.DequeueFromInput(false, true);
                    if (cardRequest == null) //timeout
                    {
                        if (cancellationTokenForCardInterfaceManager.Token.IsCancellationRequested)
                        {
                            cancellationTokenForCardInterfaceManager.Dispose();
                            break;
                        }
                        else
                            continue;
                    }

                    CardResponse response;
                    try
                    {
                        response = new CardResponse(await Transceive(cardRequest.ApduCommand), CardInterfaceServiceResponseEnum.RA);
                    }
                    catch
                    {
                        ApduResponse apduRes = new ApduResponse();
                        byte[] dataOut = new byte[] { 0x00, 0x00 };
                        apduRes.Deserialize(dataOut);
                        response = new CardResponse(apduRes, CardInterfaceServiceResponseEnum.L1RSP);
                    }
                    CardQ.EnqueueToOutput(response);
                }
                catch (Exception ex)
                {
                    StopServiceQProcess();
                    OnExceptionOccured(ex);
                }
            }
        }

        public ApduResponse SendCommand(ApduCommand apduCommand)
        {
            CardQ.EnqueueToInput(new CardRequest(apduCommand, CardinterfaceServiceRequestEnum.ADPU));

            CardResponse response = CardQ.DequeueFromOutput(false); //this will poll indefinetely

            return response.ApduResponse;
        }

        private async Task<ApduResponse> Transceive(ApduCommand apduCommand)
        {
            ApduResponse apduRes = Activator.CreateInstance(apduCommand.ApduResponseType) as ApduResponse;
            byte[] dataIn = apduCommand.Serialize();

            bool debugOut = true;

            if (debugOut)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("********************************************************************************************************");
                sb.AppendLine(apduCommand.ToString());
                sb.Append("Raw Data Sent:" + Formatting.ByteArrayToHexString(dataIn));
                Logger.Log(sb.ToString());
            }
            
            byte[] dataOut = await CardInterfaceManger.TransmitAsync(dataIn);

            if (debugOut)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Raw Data Received:" + Formatting.ByteArrayToHexString(dataOut));
                sb.AppendLine("********************************************************************************************************");
                Logger.Log(sb.ToString());
            }

            apduRes.Deserialize(dataOut);

            if (apduRes.SW1 == 0x61)
            {
                GetResponseRequest getResponseRequest = new GetResponseRequest(apduRes.SW2);
                ApduResponse getResponseResponse = await Transceive(getResponseRequest);
                if (getResponseResponse.Succeeded || (getResponseResponse.SW1 == 0x62 && getResponseResponse.SW2 == 0x83))
                {
                    apduRes.ResponseData = Formatting.ConcatArrays(apduRes.ResponseData, getResponseResponse.ResponseData, new byte[] { 0x90,0x00});
                    apduRes.Deserialize(apduRes.ResponseData);
                }
                else
                    throw new Exception("GetResponse failed");
            }
            if (apduRes.SW1 == 0x6C)
            {
                //repeat command with correct Le
                apduCommand.Le = apduRes.SW2;
                apduRes = await Transceive(apduCommand);
            }

            return apduRes;
        }
    }
}
