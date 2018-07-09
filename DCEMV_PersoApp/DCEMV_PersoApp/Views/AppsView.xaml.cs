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
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace DCEMV.PersoApp
{
    public partial class AppsView : ModalPage
    {
        public class AppVM
        {
            public String AID { get; set; }
            public String Type { get; set; }
            public String Privileges { get; set; }
            public String Version { get; set; }
            public String LifeCycle { get; set; }
        }

        private ICardInterfaceManger cardInterfaceManger;
        private GPPersoTerminalApplication cardApp;
        private ObservableCollection<AppVM> list = new ObservableCollection<AppVM>();

        public AppsView(ICardInterfaceManger cardInterfaceManger)
		{
			InitializeComponent();
            this.cardInterfaceManger = cardInterfaceManger;
            gridProgress.IsVisible = false;
            lstApps.ItemsSource = list;

            gridMain.BindingContext = SessionSingleton.Instance;
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

                    if (tpo.GPRegistry != null)
                    {
                        GPRegistry reg = tpo.GPRegistry;
                        PrintRegistry(reg);
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
        private void AddToList(AppVM vm)
        {
            Device.BeginInvokeOnMainThread(() => 
            {
                list.Add(vm);
            });
        }
        private void PrintRegistry(GPRegistry reg)
        {
            foreach (GPRegistryEntry e in reg.entries.Values)
            {
                AID aid = e.getAID();
                Log(e.getType() + ": " + Formatting.ByteArrayToHexString(aid.getBytes()) + " (" + e.getLifeCycleString() + ")");
                if (e.getDomain() != null)
                    Log("\t" + "Parent:  " + e.getDomain());
                else
                    Log("\t" + "No Parent");

                AppVM vm = new AppVM();
                vm.AID = Formatting.ByteArrayToHexString(aid.getBytes());
                vm.Type = e.getType().ToString();
                AddToList(vm);

                if (e.getType() == Kind.ExecutableLoadFile)
                {
                    GPRegistryEntryPkg pkg = (GPRegistryEntryPkg)e;
                    if (pkg.getVersion() != null) 
                        Log("\t" + "Version: " + pkg.getVersionString());
                    else
                        Log("\t" + "No Version");

                    vm.Version = "Version: " + pkg.getVersionString();

                    if (pkg.getModules().Count == 0)
                        Log("\t" + "No Executable Modules");

                    vm.LifeCycle = pkg.getLifeCycleString();

                    foreach (AID a in pkg.getModules())
                    {
                        Log("\t" + "Applet:  " + Formatting.ByteArrayToHexString(a.getBytes()));
                        Log(" (" + Formatting.ByteArrayToHexString(a.getBytes()) + ")");
                    }
                }
                else
                {
                    GPRegistryEntryApp app = (GPRegistryEntryApp)e;
                    if (app.getLoadFile() != null)
                        Log("\t" + "From:    " + app.getLoadFile());
                    else
                        Log("\t" + "No Load File");


                    if (!app.getPrivileges().isEmpty())
                    {
                        Log("\t" + "Privs:   " + app.getPrivileges());
                        vm.Privileges = app.getPrivileges().ToString();
                    }
                    else
                    {
                        Log("\t" + "No Privileges");
                        vm.Privileges = "No Privileges";
                    }

                    vm.LifeCycle = app.getLifeCycleString();
                }
            }
        }
        private void SetStatusLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblStatus.Text = text; });
        }
        
        private async void cmdRemove_Clicked(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            AppVM selected = (AppVM)lstApps.SelectedItem;

            StopCardScanner();
            StartCardScanner();
            cardApp.RemoveApp(txtSecurityDomain.Text, txtMasterKey.Text, selected.AID);
        }
        private async void cmdLock_Clicked(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            AppVM selected = (AppVM)lstApps.SelectedItem;

            StopCardScanner();
            StartCardScanner();
            cardApp.LockApp(txtSecurityDomain.Text, txtMasterKey.Text, selected.AID);
        }
        private async void cmdUnLock_Clicked(object sender, EventArgs e)
        {
            if (lstApps.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            AppVM selected = (AppVM)lstApps.SelectedItem;

            StopCardScanner();
            StartCardScanner();
            cardApp.UnLockApp(txtSecurityDomain.Text, txtMasterKey.Text, selected.AID);
        }
        
        private void cmdOK_Clicked(object sender, EventArgs e)
        {
            list.Clear();
            StopCardScanner();
            StartCardScanner();
            cardApp.GetApps(txtSecurityDomain.Text,txtMasterKey.Text);
        }
        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            StopCardScanner();
            ClosePage();
        }


    }
}
