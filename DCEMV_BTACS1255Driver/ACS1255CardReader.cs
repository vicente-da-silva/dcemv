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
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Com.Acs.Bluetooth;
using static Com.Acs.Bluetooth.BluetoothReaderGattCallback;
using static Android.Bluetooth.BluetoothAdapter;
using System.Collections.ObjectModel;
using DCEMV.FormattingUtils;
using System.Threading;

namespace DCEMV.CardReaders.BTACS1255Driver
{
    public enum DriverState
    {
        ReaderDiconnected,
        ReaderConnected,
        ReaderDetected,
        ReaderAuthenticated,
        ReaderPolling,
        Transition
    }
    public enum ReaderState
    {
        Started,
        Stopped
    }

    public class ACS1255LibWrapper
    {
        public event EventHandler StateChanged;
        public event EventHandler CardPutInField;
        public event EventHandler CardRemovedFromField;

        private BluetoothGatt mBluetoothGatt;
        private BluetoothReaderGattCallback mGattCallback;
        private static BluetoothReaderManager mBluetoothReaderManager;
        private static BluetoothReader mBluetoothReader;

        private Context ctx;
        private BroadcastReceiver mBroadcastReceiver = new MyBroadcastReceiver();

        private byte[] adpuResponse = null;
        private static String mDeviceAddress;

        private bool firstCardStateCalled = false;

        private LeScanCallback leScanCallback = new LeScanCallback();
        private byte[] masterKey = Formatting.HexStringToByteArray("41435231323535552D4A312041757468");

        private static byte[] AUTO_POLLING_START = { (byte)0xE0, 0x00, 0x00, 0x40, 0x01 };
        private static byte[] AUTO_POLLING_STOP = { (byte)0xE0, 0x00, 0x00, 0x40, 0x00 };
        private static byte[] TURN_OFF_SLEEP_MODE = { (byte)0xE0, 0x00, 0x00, 0x48, 0x04 };

        private static ObservableCollection<string> btDevices = new ObservableCollection<string>();

        public class MyBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                BluetoothAdapter bluetoothAdapter = null;
                BluetoothManager bluetoothManager = null;
                String action = intent.Action;

                if (!(mBluetoothReader is Acr3901us1Reader))
                {
                    /* Only ACR3901U-S1 require bonding. */
                    return;
                }

                if (BluetoothDevice.ActionBondStateChanged == intent.Action)
                {
                    LogMessage("ACTION_BOND_STATE_CHANGED");

                    /* Get bond (pairing) state */
                    if (mBluetoothReaderManager == null)
                    {
                        LogMessage("Unable to initialize BluetoothReaderManager.");
                        return;
                    }

                    bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
                    if (bluetoothManager == null)
                    {
                        LogMessage("Unable to initialize BluetoothManager.");
                        return;
                    }

                    if (bluetoothManager.Adapter == null)
                    {
                        LogMessage("Unable to initialize BluetoothAdapter.");
                        return;
                    }

                    BluetoothDevice device = bluetoothAdapter.GetRemoteDevice(mDeviceAddress);

                    if (device == null)
                    {
                        return;
                    }

                    LogMessage("BroadcastReceiver - getBondState. state = " + device.BondState);

                    /* Enable notification */
                    if (device.BondState == Bond.Bonded)
                    {
                        if (mBluetoothReader != null)
                        {
                            mBluetoothReader.EnableNotification(true);
                        }
                    }

                    /* Progress Dialog */
                    if (device.BondState == Bond.Bonding)
                    {
                        //mProgressDialog = ProgressDialog.show(context,"ACR3901U-S1", "Bonding...");
                    }
                    else
                    {
                        //if (mProgressDialog != null)
                        //{
                        //    mProgressDialog.dismiss();
                        //    mProgressDialog = null;
                        //}
                    }
                }
            }
        }
        public class LeScanCallback : Java.Lang.Object, ILeScanCallback
        {
            public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
            {
                string desc = device.Name == "" ? "No Name" : device.Name + "=" + device.Address;
                if (!btDevices.Contains(desc))
                    btDevices.Add(desc);
            }
        }
        public class StateChangedEventArgs : EventArgs
        {
            public DriverState State { get; set; }
        }

        public ACS1255LibWrapper(Context ctx)
        {
            this.ctx = ctx;
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }
        protected virtual void OnCardPutInField(EventArgs e)
        {
            CardPutInField?.Invoke(this, e);
        }
        protected virtual void OnCardRemovedFromField(EventArgs e)
        {
            CardRemovedFromField?.Invoke(this, e);
        }

        public async Task<ObservableCollection<string>> GetCardReaders()
        {
            btDevices.Clear();
            BluetoothManager bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);
            if (bluetoothManager.Adapter == null)
                throw new Exception("BT not supported");

            //scanning will start and each time a device is found it is added to 
            //btDevices, which is an Observable Collection therefore UI will update
            //in real time as devices are found
            bluetoothManager.Adapter.StartLeScan(leScanCallback);

            //stop scanning after 10 seconds
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                bluetoothManager.Adapter.StopLeScan(leScanCallback);
            });

            return btDevices;
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            firstCardStateCalled = false;
            mDeviceAddress = deviceId.Split('=')[1];
        }

        public void StopCardReaderAsync()
        {
            Disconnect("StopCardReaderAsync");
        }

        public void Disconnect(String source)
        {
            if (mBluetoothGatt == null)
                return;

            mBluetoothGatt.Disconnect();

            LogMessage("Disconnect called from: " + source);
            OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderDiconnected });
        }
        public void Connect()
        {
            BluetoothManager bluetoothManager = (BluetoothManager)ctx.GetSystemService(Context.BluetoothService);
            if (bluetoothManager == null)
            {
                LogMessage("Unable to initialize BluetoothManager.");
                throw new Exception("Unable to initialize BluetoothManager.");
            }

            if (bluetoothManager.Adapter == null)
            {
                LogMessage("Unable to obtain a BluetoothAdapter.");
                throw new Exception("Unable to obtain a BluetoothAdapter.");
            }

            //stop scanning if scanning still in progress
            bluetoothManager.Adapter.StopLeScan(leScanCallback);

            mGattCallback = new BluetoothReaderGattCallback();

            mGattCallback.ConnectionStateChange += MGattCallback_ConnectionStateChange;

            mBluetoothReaderManager = new BluetoothReaderManager();

            mBluetoothReaderManager.ReaderDetection += MBluetoothReaderManager_ReaderDetection;

            if (mBluetoothGatt != null)
            {
                LogMessage("Clear old GATT connection");
                mBluetoothGatt.Disconnect();
                mBluetoothGatt.Close();
                mBluetoothGatt = null;
            }

            BluetoothDevice device = bluetoothManager.Adapter.GetRemoteDevice(mDeviceAddress);
            if (device == null)
            {
                LogMessage("Device not found. Unable to connect.");
                OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderDiconnected });
                throw new Exception("Device not found. Unable to connect.");
            }

            //Connect gat will connect and call mGattCallback which in turn calls detect reader
            mBluetoothGatt = device.ConnectGatt(ctx, false, mGattCallback);
        }
        public void Detect()
        {
            bool ret = mBluetoothReaderManager.DetectReader(mBluetoothGatt, mGattCallback);
            if (!ret)
                LogMessage("Could not start reader detection:" + mBluetoothGatt.Device.Address);
            else
                LogMessage("Connecting to reader:" + mBluetoothGatt.Device.Address);
        }
        public void Authenticate()
        {
            mBluetoothReader.Authenticate(masterKey);
        }
        public void StartPoll()
        {
            bool ret = mBluetoothReader.TransmitEscapeCommand(AUTO_POLLING_START);
            if (ret)
                LogMessage("Polling Started");
            else
                LogMessage("Polling Start Request Failed");
        }
        public void StopPoll()
        {
            bool ret = mBluetoothReader.TransmitEscapeCommand(AUTO_POLLING_STOP);
            if (ret)
                LogMessage("Polling Stopped");
            else
                LogMessage("Polling Stop Request Failed");
        }
        public void TurnOffSleepMode()
        {
            bool ret = mBluetoothReader.TransmitEscapeCommand(TURN_OFF_SLEEP_MODE);
            if (ret)
                LogMessage("Sleep Off Enabled");
            else
                LogMessage("Sleep Off Request Failed");
        }

        public async Task<byte[]> TransmitAsync(byte[] inputData)
        {
            adpuResponse = null;
            bool ret = mBluetoothReader.TransmitApdu(inputData);
            if (!ret)
                throw new Exception("Error Tramsmitting adpu");

            CancellationTokenSource ct = new CancellationTokenSource();

            Task t = Task.Run(async () =>
            {
                while (adpuResponse == null)
                {
                    await Task.Delay(10);
                }
            }, ct.Token);

            if (!t.Wait(1000))
            {
                if (!ct.IsCancellationRequested)
                    ct.Cancel();
                throw new TimeoutException();
            }

            return adpuResponse;
        }

        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }

        #region Event Handlers
        private void MBluetoothReaderManager_ReaderDetection(object sender, BluetoothReaderManager.ReaderDetectionEventArgs e)
        {
            if (e.P0 is Acr3901us1Reader)
                LogMessage("On Acr3901us1Reader Detected.");
            else if (e.P0 is Acr1255uj1Reader)
                LogMessage("On Acr1255uj1Reader Detected.");
            else
            {
                LogMessage("Disconnect reader");
                Disconnect("ReaderDetection");
                throw new Exception("Unknown BT Reader");
            }

            OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderDetected });
            mBluetoothReader = e.P0;

            //connect listeners
            if (mBluetoothReader is Acr3901us1Reader)
            {
                ((Acr3901us1Reader)mBluetoothReader).BatteryStatusChange += ACS1255_BatteryStatusChange;
                ((Acr3901us1Reader)mBluetoothReader).BatteryStatusAvailable += ACS1255_BatteryStatusAvailable;
            }
            else if (mBluetoothReader is Acr1255uj1Reader)
            {
                ((Acr1255uj1Reader)mBluetoothReader).BatteryLevelChange += ACS1255_BatteryLevelChange;
                ((Acr1255uj1Reader)mBluetoothReader).BatteryLevelAvailable += ACS1255_BatteryLevelAvailable;
            }

            mBluetoothReader.CardStatusChange += Reader_CardStatusChange;

            mBluetoothReader.AuthenticationComplete += Reader_AuthenticationComplete;

            mBluetoothReader.AtrAvailable += Reader_AtrAvailable;

            mBluetoothReader.CardPowerOffComplete += Reader_CardPowerOffComplete;

            mBluetoothReader.ResponseApduAvailable += Reader_ResponseApduAvailable;

            mBluetoothReader.EscapeResponseAvailable += Reader_EscapeResponseAvailable;

            mBluetoothReader.DeviceInfoAvailable += Reader_DeviceInfoAvailable;

            mBluetoothReader.EnableNotificationComplete += Reader_EnableNotificationComplete;

            //activate reader
            if (mBluetoothReader is Acr3901us1Reader)
            {
                /* Start pairing to the reader. */
                ((Acr3901us1Reader)mBluetoothReader).StartBonding();
            }
            else if (mBluetoothReader is Acr1255uj1Reader)
            {
                /* Enable notification. */
                mBluetoothReader.EnableNotification(true);
            }

            //Authenticate();
        }

        private void MGattCallback_ConnectionStateChange(object sender, ConnectionStateChangeEventArgs e)
        {
            if (e.P1 != (int)GattStatus.Success)
            {
                //OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderDiconnected });
                if (e.P2 == BluetoothReader.StateConnected)
                {
                    LogMessage("Connect Failed");
                }
                else if (e.P2 == BluetoothReader.StateDisconnected)
                {
                    LogMessage("Disconnect Failed");
                }
                return;
            }

            if (e.P2 == (int)ProfileState.Connected)
            {
                OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderConnected });
                /* Detect the connected reader. */
                //if (mBluetoothReaderManager != null)
                //{
                //    //Detect();
                //}
            }
            else if (e.P2 == (int)ProfileState.Disconnected)
            {
                //Disconnect("ConnectionStateChange");
                mBluetoothReader = null;
                if (mBluetoothGatt != null)
                {
                    mBluetoothGatt.Close();
                    mBluetoothGatt = null;
                }
            }
        }

        private void Reader_DeviceInfoAvailable(object sender, BluetoothReader.DeviceInfoAvailableEventArgs e)
        {
            if (e.P3 != (int)GattStatus.Success)
            {
                return;
            }
            LogMessage("Info:" + e.P2);
        }

        private void Reader_AuthenticationComplete(object sender, BluetoothReader.AuthenticationCompleteEventArgs e)
        {
            if (e.P1 == BluetoothReader.ErrorSuccess)
            {
                LogMessage("Authenticated to reader:" + mBluetoothGatt.Device.Address);
                OnStateChanged(new StateChangedEventArgs() { State = DriverState.ReaderAuthenticated });
                //StartPoll();
            }
            else
            {
                LogMessage("Could not authenticat to reader:" + mBluetoothGatt.Device.Address);
            }
        }

        private void Reader_AtrAvailable(object sender, BluetoothReader.AtrAvailableEventArgs e)
        {
            //e.P2 is error code
            //e.P1 is atr
        }

        private void Reader_CardPowerOffComplete(object sender, BluetoothReader.CardPowerOffCompleteEventArgs e)
        {
            //e.P1 is error code
        }

        private void Reader_ResponseApduAvailable(object sender, BluetoothReader.ResponseApduAvailableEventArgs e)
        {
            //e.P1 is response
            //e.P2 is error code
            adpuResponse = Formatting.copyOfRange(e.P1, 0, e.P1.Length);
        }

        private void Reader_EscapeResponseAvailable(object sender, BluetoothReader.EscapeResponseAvailableEventArgs e)
        {
            //e.P1 is response
            //e.P2 is error code
        }

        private void Reader_CardStatusChange(object sender, BluetoothReader.CardStatusChangeEventArgs e)
        {
            //e.P1 is CardName status and error code?
            switch (e.P1)
            {
                case BluetoothReader.CardStatusAbsent:
                    if (firstCardStateCalled == false)
                    {
                        firstCardStateCalled = true;
                    }
                    else
                    { 
                        OnCardRemovedFromField(new EventArgs());
                    }
                    break;

                case BluetoothReader.CardStatusPresent:
                    OnCardPutInField(new EventArgs());
                    break;

                case BluetoothReader.CardStatusPowered:
                    break;

                case BluetoothReader.CardStatusPowerSavingMode:
                    break;
            }
        }

        private void Reader_EnableNotificationComplete(object sender, BluetoothReader.EnableNotificationCompleteEventArgs e)
        {
            //e.P1 is error code
        }

        private void ACS1255_BatteryLevelChange(object sender, Acr1255uj1Reader.BatteryLevelChangeEventArgs e)
        {
            //e.P1 is level
        }

        private void ACS1255_BatteryStatusChange(object sender, Acr3901us1Reader.BatteryStatusChangeEventArgs e)
        {
            //e.P1 is status
        }

        private void ACS1255_BatteryStatusAvailable(object sender, Acr3901us1Reader.BatteryStatusAvailableEventArgs e)
        {
            //e.P1 is status
        }

        private void ACS1255_BatteryLevelAvailable(object sender, Acr1255uj1Reader.BatteryLevelAvailableEventArgs e)
        {
            //e.P1 is level
        }
        #endregion
    }

    public class ACS1255CardReader : ICardInterfaceManger
    {
        protected CancellationTokenSource cancellationTokenForDriverManager;
        private DriverState currentDriverState = DriverState.ReaderDiconnected;
        private DriverState desiredDriverState = DriverState.ReaderDiconnected;

        public event EventHandler CardPutInField;
        public event EventHandler CardRemovedFromField;

        private static ACS1255LibWrapper libWrapper;

        bool isDriverManagerStarting = true;

        public ACS1255CardReader(Context ctx)
        {
            libWrapper = new ACS1255LibWrapper(ctx);
            cancellationTokenForDriverManager = new CancellationTokenSource();
            libWrapper.StateChanged += LibWrapper_StateChanged;
            libWrapper.CardPutInField += LibWrapper_CardPutInField;
            libWrapper.CardRemovedFromField += LibWrapper_CardRemovedFromField;
        }

        ~ACS1255CardReader()
        {
            desiredDriverState = DriverState.ReaderDiconnected;
            cancellationTokenForDriverManager.Cancel();
            libWrapper.StopCardReaderAsync();
        }

        private void LibWrapper_CardRemovedFromField(object sender, EventArgs e)
        {
            OnCardRemovedFromField(e);
        }
        private void LibWrapper_CardPutInField(object sender, EventArgs e)
        {
            OnCardPutInField(e);
        }
        private void LibWrapper_StateChanged(object sender, EventArgs e)
        {
            UpdateState((e as ACS1255LibWrapper.StateChangedEventArgs).State);
        }

        private void UpdateState(DriverState state)
        {
            currentDriverState = state;
            LogMessage("===> Driver State Changed: " + state);
        }

        private void StartDriverManager()
        {
            Task.Run(() => StartStateTransitionManager(), cancellationTokenForDriverManager.Token);
        }
        private void StopStateTransitionManager()
        {
            if (!cancellationTokenForDriverManager.IsCancellationRequested)
                cancellationTokenForDriverManager.Cancel();
        }
        private void DoTaskWithTimeout(DriverState driverState, int timeout)
        {
            CancellationTokenSource ct = new CancellationTokenSource();
            Task t = Task.Run(async () =>
            {
                while (currentDriverState != driverState)
                {
                    await Task.Delay(100);
                }
            }, ct.Token);

            if (!t.Wait(timeout))
            {
                if (!ct.IsCancellationRequested)
                    ct.Cancel();
                throw new TimeoutException();
            }

            //currentDriverState = driverState;
        }
        private void StartStateTransitionManager()
        {
            while (1 == 1)
            {
                try
                {
                    if (cancellationTokenForDriverManager.Token.IsCancellationRequested)
                    {
                        cancellationTokenForDriverManager.Dispose();
                        break;
                    }

                    //any reader event/exception causing a disconect must set DriverState to ReaderDiconnected
                    if (desiredDriverState != currentDriverState)
                    {
                        switch (desiredDriverState)
                        {
                            case DriverState.ReaderDiconnected:
                                DoTaskWithTimeout(doDisconnect(), 5000);
                                break;
                            case DriverState.ReaderConnected:
                                DoTaskWithTimeout(doConnect(), 5000);
                                desiredDriverState = DriverState.ReaderDetected;//move to next state
                                break;
                            case DriverState.ReaderDetected:
                                DoTaskWithTimeout(doDetect(), 5000);
                                desiredDriverState = DriverState.ReaderAuthenticated;//move to next state
                                Task.Delay(1000).Wait();//wait after detection, before trying to auth
                                break;
                            case DriverState.ReaderAuthenticated:
                                if (currentDriverState == DriverState.ReaderPolling)
                                {
                                    doStopPoll();
                                    currentDriverState = DriverState.ReaderAuthenticated;
                                    break;
                                }
                                DoTaskWithTimeout(doAuth(), 5000);
                                if (isDriverManagerStarting)
                                {
                                    Task.Run(() => { doTurnOffSleepMode(); });
                                    Task.Delay(1000).Wait();//wait for sleep mode turned on
                                    desiredDriverState = DriverState.ReaderPolling;
                                    isDriverManagerStarting = false;
                                }
                                break;
                            case DriverState.ReaderPolling:
                                currentDriverState = doStartPoll();
                                //fire an ReaderStarted event 
                                break;
                        }
                    }
                    else
                        Task.Delay(100).Wait();
                }
                catch (Exception ex)
                {
                    LogMessage("Error in StateTransitionManager:" + ex.Message);
                    //if (currentDriverState == DriverState.ReaderAuthenticated || currentDriverState == DriverState.ReaderPolling)
                    //{
                    //    //restart the state engine
                    //    desiredDriverState = DriverState.ReaderConnected;
                    //}
                    //else
                    //{
                    //    //stop the state engine
                    //    desiredDriverState = DriverState.ReaderDiconnected;
                    //}
                    desiredDriverState = DriverState.ReaderConnected;
                }
            }
        }

        private DriverState doDisconnect()
        {
            libWrapper.Disconnect("doDisconnect");
            return DriverState.ReaderDiconnected;
        }
        private DriverState doConnect()
        {
            libWrapper.Connect();
            return DriverState.ReaderConnected;
        }
        private DriverState doDetect()
        {
            libWrapper.Detect();
            return DriverState.ReaderDetected;
        }
        private void doTurnOffSleepMode()
        {
            libWrapper.TurnOffSleepMode();
        }
        private DriverState doAuth()
        {
            libWrapper.Authenticate();
            return DriverState.ReaderAuthenticated;
        }
        private DriverState doStartPoll()
        {
            libWrapper.StartPoll();
            return DriverState.ReaderPolling;
        }
        private DriverState doStopPoll()
        {
            libWrapper.StopPoll();
            return DriverState.ReaderAuthenticated;
        }

        #region Interface Imp

        protected virtual void OnCardPutInField(EventArgs e)
        {
            CardPutInField?.Invoke(this, e);
        }

        protected virtual void OnCardRemovedFromField(EventArgs e)
        {
            CardRemovedFromField?.Invoke(this, e);
        }

        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public async Task<ObservableCollection<string>> GetCardReaders()
        {
            return await libWrapper.GetCardReaders();
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            if(isDriverManagerStarting)
            {
                await libWrapper.StartCardReaderAsync(deviceId);
                StartDriverManager();
                desiredDriverState = DriverState.ReaderConnected;
            }
            else
            {
                if (currentDriverState == DriverState.ReaderAuthenticated)
                    desiredDriverState = DriverState.ReaderPolling;
            }
        }

        public void StopCardReaderAsync()
        {
            desiredDriverState = DriverState.ReaderAuthenticated;
        }

        public async Task<byte[]> TransmitAsync(byte[] inputData)
        {
            return await libWrapper.TransmitAsync(inputData);
        }
        #endregion
    }
}
