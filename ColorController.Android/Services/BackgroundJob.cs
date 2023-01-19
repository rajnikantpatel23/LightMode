using Android.App;
using Android.App.Job;
using Android.Content;
using ColorController.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using ColorController.Enums;
using ColorController.Helpers;
using Plugin.BLE;

namespace ColorController.Droid.Services
{
    [Service(Name = "ColorController.Droid.Services.BackgroundJob",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BackgroundJob : JobService
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        public override bool OnStartJob(JobParameters jobParams)
        {
            CommonUtils.WriteLog("BackgroundJob: OnStartJob()");
            Task.Run(async () =>
            {
                // Work is happening asynchronously
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
                catch (System.OperationCanceledException)
                {

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    App.IsBackgroundTaskRunning = false;
                }

                // Have to tell the JobScheduler the work is done. 
                JobFinished(jobParams, false);
            });

            // Return true because of the asynchronous work
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            CommonUtils.WriteLog("BackgroundJob: OnStopJob()");
            // we don't want to reschedule the job if it is stopped or cancelled.
            MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
            return false;
        }
    }

    public static class JobSchedulerHelpers
    {
        public static JobInfo.Builder CreateJobBuilderUsingJobId<T>(this Context context, int jobId) where T : JobService
        {
            var javaClass = Java.Lang.Class.FromType(typeof(T));
            var componentName = new ComponentName(context, javaClass);
            return new JobInfo.Builder(jobId, componentName);
        }
    }

    [Service(Name = "ColorController.Droid.Services.BackgroundJob_ToDisplayConnectionButton",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BackgroundJob_ToDisplayConnectionButton : JobService
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        public override bool OnStartJob(JobParameters jobParams)
        {
            CommonUtils.WriteLog("BackgroundJob_ToDisplayConnectionButton: OnStartJob()");

            Task.Run(() =>
            {
                // Work is happening asynchronously
                try
                {
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
                catch (Exception ex)
                {

                }
                finally
                {
                    
                }

                // Have to tell the JobScheduler the work is done. 
                JobFinished(jobParams, false);
            });

            // Return true because of the asynchronous work
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            CommonUtils.WriteLog("BackgroundJob_ToDisplayConnectionButton: OnStopJob()");
            // we don't want to reschedule the job if it is stopped or cancelled.
            return false;
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