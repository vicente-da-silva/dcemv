using Common;
using DesFireProtocol;
using FormattingUtils;
using ISO7816Protocol;
using SPDHProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPIPDriver;
using TLVProtocol;
using Xamarin.Forms;

namespace XTerminal
{
    public partial class DesFireMain : ContentPage
    {
        public static Logger Logger = new Logger(typeof(DesFireMain));

        private DesfireTerminalApplicationBase ta;
        private ICardInterfaceManger cardInterfaceManger;

        public DesFireMain()
        {
            InitializeComponent();
        }
        public DesFireMain(ICardInterfaceManger cardInterfaceManger)
        {
            InitializeComponent();

            this.cardInterfaceManger = cardInterfaceManger;
        }

        private async void CmdProcess_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ids = (await cardInterfaceManger.GetCardReaders()).Where(x => x.ToLower().Contains("contactless")).ToList();
                if (ids.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No contactless reader found", "OK");
                    return;
                }
                ta = new DesfireTerminalApplicationBase(new CardQProcessor(cardInterfaceManger, ids[0]));
                ta.UserInterfaceRequest += Ta_UserInterfaceRequest;
                ta.ProcessCompleted += Ta_ProcessCompleted;
                ta.ExceptionOccured += Ta_ExceptionOccured;
                ta.StartTransactionRequest(DesFireTransactionTypeEnum.ProcessTransaction);
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }

        private async void CmdInstall_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> ids = (await cardInterfaceManger.GetCardReaders()).Where(x => x.ToLower().Contains("contactless")).ToList();
                if (ids.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No contactless reader found", "OK");
                    return;
                }
                ta = new DesfireTerminalApplicationBase(new CardQProcessor(cardInterfaceManger, ids[0]));
                ta.UserInterfaceRequest += Ta_UserInterfaceRequest;
                ta.ProcessCompleted += Ta_ProcessCompleted;
                ta.ExceptionOccured += Ta_ExceptionOccured;
                ta.StartTransactionRequest(DesFireTransactionTypeEnum.InstallApp);
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if(ta != null)
                    ta.CancelTransactionRequest();
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
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
            finally
            {
                SetProcessButtonEnabled(true);
            }
        }

        private void Ta_ProcessCompleted(object sender, EventArgs e)
        {
            try
            {
                TerminalProcessingOutcome tpo = (e as TerminalProcessingOutcomeEventArgs).TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;


                if (tpo.UIRequestOnOutcomePresent)
                    SetStatusLabel(string.Format("{0}\n{1}", tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status));
                
            }
            catch (Exception ex)
            {
                ta.CancelTransactionRequest();
                SetStatusLabel(ex.Message);
            }
            finally
            {
                SetProcessButtonEnabled(true);
                SetInstallButtonEnabled(true);
            }
        }

        private void Ta_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel((e as UIMessageEventArgs).Message);
            Task.Run(async () => await Task.Delay((int)(e as UIMessageEventArgs).HoldTime)).Wait();
        }

        private void SetStatusLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblStatus.Text = text; });
        }
        private void SetProcessButtonEnabled(bool enabled)
        {
            Device.BeginInvokeOnMainThread(() => { cmdProcess.IsEnabled = enabled; });
        }
        private void SetInstallButtonEnabled(bool enabled)
        {
            Device.BeginInvokeOnMainThread(() => { cmdInstallApp.IsEnabled = enabled; });
        }
    }
}
