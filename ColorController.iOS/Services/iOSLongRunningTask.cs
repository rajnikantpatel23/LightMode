using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Services;
using Plugin.BLE;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;

namespace ColorController.iOS.Services
{
    public class iOSLongRunningTask
    {
        nint _taskId;
        CancellationTokenSource _cts;
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>(); 

        public async Task Start()
        {
            _cts = new CancellationTokenSource();
            _taskId = UIApplication.SharedApplication.BeginBackgroundTask("LongRunningTask", OnExpiration);

            try
            {
                //If there is no saved devices then don't start scanning
                var savedDevices = await App.Database.GetControllers();
                if (!savedDevices.Any())
                {
                    //Send Message to display 'Searching for devices...' Text at Top Bar 
                    MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
                    CommonUtils.WriteLog("BackgroundJob: OnStartJob() : If there are no saved devices.");
                    return;
                }

                //If Bluetooth is Off then Stop Searching
                var isBluetoothOn = await BlueToothService.IsBluetoothON();

                //Android: Check Location Permission
                var isLocationPermissionAllowed = await BlueToothService.IsLocationPermissionAllowed();

                //If all saved devices are connected then Stop Searching
                var areAllSavedDeviceConnected = await BlueToothService.AreAllSavedDevicesConnected();

                if (!isBluetoothOn || areAllSavedDeviceConnected || !isLocationPermissionAllowed)
                {
                    //Send Message to display 'Searching for devices...' Text at Top Bar 
                    MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
                    CommonUtils.WriteLog("BackgroundJob: OnStartJob() : !isBluetoothOn || areAllSavedDeviceConnected || !isLocationPermissionAllowed");
                    return;
                }

                MessagingCenter.Send<object, bool>(this, nameof(MessageType.DisplaySearchingForDevicesText), true);

                //If all is fine then start connecting saved devices
                foreach (var controller in savedDevices)
                {
                    await BlueToothService.ConnectToKnownDeviceInBackground(controller);
                }

                //Connect saved and new devices
                //await BlueToothService.ScanAndConnectDevice();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {

            }
            finally
            {
                App.IsBackgroundTaskRunning = false;
            }

            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private void OnExpiration()
        {
            _cts?.Cancel();
        }
    }

    public class iOSLongRunningTask_ForConnectionButton
    {
        nint _taskId;
        CancellationTokenSource _cts;
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        public async Task Start()
        {
            _cts = new CancellationTokenSource();
            _taskId = UIApplication.SharedApplication.BeginBackgroundTask("iOSLongRunningTask_ForConnectionButton", OnExpiration);

            try
            {
                CommonUtils.WriteLog("BackgroundJob_ToDisplayConnectionButton: OnStartJob()");

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    try
                    {
                        Task.Run(async () =>
                        {
                            await DisplayConnectionButton();
                        });
                    }
                    catch (Exception)
                    {
                        CommonUtils.WriteLog("Exception: StartTimerToRunBackgroundTask() Inside Device.StartTimer()");
                    }

                    return true;
                });

            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                if (_cts.IsCancellationRequested)
                {

                }

                App.IsBackgroundTaskRunning = false;
            }

            UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        }       

        public void Stop()
        {
            _cts?.Cancel();
        }

        private void OnExpiration()
        {
            _cts?.Cancel();
        }

        private async Task DisplayConnectionButton()
        {
            var savedDeviceCount = await App.Database.GetSavedDeviceCount();
            //- Condition #1: No saved devices & app/controller NOT connected                   =>  PAIR NEW DEVICE
            if (savedDeviceCount == 0 && !BlueToothService.IsAppConnectedWithDevice)
            {
                // PAIR NEW DEVICE
                MessagingCenter.Send<object, string>(this, nameof(MessageType.NaviBarConnectionButton), "pair_new_device_small.png");
            }
            //- Condition #2: At least 1 device saved & app/controller NOT connected            =>  CONNECT
            else if (savedDeviceCount > 0 && !BlueToothService.IsAppConnectedWithDevice)
            {
                //CONNECT
                var buttonImage = CrossBluetoothLE.Current.IsOn ? "navBtnConnect.png" : "navBtnConnectOff.png";
                MessagingCenter.Send<object, string>(this, nameof(MessageType.NaviBarConnectionButton), buttonImage);
            }
            //- Condition #3: app is connected to one or more devices.                          =>  DISCONNECT 
            else if (BlueToothService.IsAppConnectedWithDevice)
            {
                //DISCONNECT
                MessagingCenter.Send<object, string>(this, nameof(MessageType.NaviBarConnectionButton), "btnDisconnectOn.png");
            }
        }
    }
}