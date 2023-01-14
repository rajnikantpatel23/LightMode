using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using ColorController.Enums;
using ColorController.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ColorController.Droid.Services
{
    [Service(Label = "LongRunningTaskService")]
    public class LongRunningTaskService : Service
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>(); 
        CancellationTokenSource _cts;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    //If there is no saved devices then don't start scanning
                    var savedDevices = await App.Database.GetControllers();
                    if (!savedDevices.Any())
                    {
                        //Send Message to display 'Searching for devices...' Text at Top Bar 
                        MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
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
                        return;
                    }

                    //If all is fine then start connecting saved devices
                    foreach (var controller in savedDevices)
                    {
                        await BlueToothService.ConnectToKnownDeviceInBackground(controller);
                    }

                    //Connect saved and new devices
                    //await BlueToothService.ScanAndConnectDevice();
                }
                catch (System.OperationCanceledException)
                {

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (_cts.IsCancellationRequested)
                    {
                        
                    }
                    App.IsBackgroundTaskRunning = false;
                }
            }, _cts.Token);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _cts?.Cancel();
            base.OnDestroy();
        }
    }
}

//References links:
//https://www.youtube.com/watch?v=Z1YzyreS4-o&ab_channel=XamarinUniversity
//https://www.youtube.com/watch?v=KPMT8FN5A1Y