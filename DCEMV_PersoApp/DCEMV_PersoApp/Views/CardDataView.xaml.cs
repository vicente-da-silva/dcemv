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
using DCEMV.GlobalPlatformProtocol;
using DCEMV.ISO7816Protocol;
using System;
using Xamarin.Forms;

namespace DCEMV.PersoApp
{
    public partial class CardDataView : ModalPage
    {
        private ICardInterfaceManger cardInterfaceManger;
        private GPPersoTerminalApplication cardApp;
       
        public CardDataView(ICardInterfaceManger cardInterfaceManger)
		{
			InitializeComponent();
            this.cardInterfaceManger = cardInterfaceManger;
            gridProgress.IsVisible = false;
        }

        private void StartCardScanner()
        {
            try
            {
                if (cardApp == null)
                {
                    cardApp = new GPPersoTerminalApplication(new CardQProcessor(cardInterfaceManger, SessionSingleton.DeviceId));
                    cardApp.UserInterfaceRequest += Ta_UserInterfaceRequest;
                    cardApp.ProcessCompleted += Ta_ProcessCompleted;
                    cardApp.ExceptionOccured += Ta_ExceptionOccured;
                }
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }
        private void StopCardScanner()
        {
            if (cardApp != null)
            {
                cardApp.CancelTransactionRequest();
                cardApp = null;
            }
        }

        private void Ta_ProcessCompleted(object sender, EventArgs e)
        {
            try
            {
                PersoProcessingOutcome tpo = (e as PersoProcessingOutcomeEventArgs).TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;

                if (tpo is PersoProcessingOutcome)
                {
                    SetStatusLabel("Complete");

                    if (tpo.CardData != null)
                    {
                        SetCardData(tpo.CardData);
                    }
                }
                else
                {
                    SetStatusLabel(string.Format("{0}\n{1}", tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status));
                }
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }
        private void Ta_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel((e as UIMessageEventArgs).MakeMessage());
        }
        private void Ta_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel((e as ExceptionEventArgs).Exception.Message);
            }
            catch
            {
                SetStatusLabel("Terminal Error Occurred");
            }
        }

        private void Log(String message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
      
        private void SetStatusLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblStatus.Text = text; });
        }
        private void SetCardData(string text)
        {
            Device.BeginInvokeOnMainThread(() => 
            {
                 lblCardData.Text = text;
            });
        }
      
        private void cmdOK_Clicked(object sender, EventArgs e)
        {
            StopCardScanner();
            StartCardScanner();
            cardApp.GetCardData();
        }
        
        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            StopCardScanner();
            ClosePage();
        }
    }
}
