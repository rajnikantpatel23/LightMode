using ColorController.Services;
using System;
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
                //Connect saved devices
                var savedDevices = await App.Database.GetControllers();
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
    }
}