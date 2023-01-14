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
}