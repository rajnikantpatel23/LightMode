using ColorController.Helpers;
using ColorController.iOS.Services;
using ColorController.Services;
using CoreBluetooth;
using CoreFoundation;
using CoreLocation;
using Foundation;
using System;
using SystemConfiguration;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformSpecific))]
namespace ColorController.iOS.Services
{
    public class PlatformSpecific : IPlatformSpecific
    {

        public bool GetBluetoothStatus()
        {
            var bluetoothManager = new CBCentralManager(new CbCentralDelegate(), DispatchQueue.DefaultGlobalQueue,
                                                    new CBCentralInitOptions { ShowPowerAlert = true });
            return bluetoothManager.State == CBCentralManagerState.PoweredOn;
        }

        private void GetLocationConsent()
        {
            var manager = new CLLocationManager();
            manager.AuthorizationChanged += (sender, args) =>
            {
                Console.WriteLine("Authorization changed to: {0}", args.Status);
            };
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            { 
                //manager.RequestWhenInUseAuthorization();
            }
        }

        public string GetCurrentWiFi()
        {
            GetLocationConsent();
            string ssid = "";
            try
            {
                string[] supportedInterfaces;
                StatusCode status;
                if ((status = CaptiveNetwork.TryGetSupportedInterfaces(out supportedInterfaces)) != StatusCode.OK)
                {
                }
                else
                {
                    foreach (var item in supportedInterfaces)
                    {
                        NSDictionary info;
                        status = CaptiveNetwork.TryCopyCurrentNetworkInfo(item, out info);
                        if (status != StatusCode.OK)
                        {
                            continue;
                        }
                        ssid = info[CaptiveNetwork.NetworkInfoKeySSID].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return ssid;
        }

        public bool IsActivityFinishing()
        {
            //Android specific method
            return false;
        }
    }

    public class CbCentralDelegate : CBCentralManagerDelegate
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>(); 

        public override void UpdatedState(CBCentralManager central)
        {
            if (central.State == CBCentralManagerState.PoweredOn)
            {
                System.Console.WriteLine("Powered On");
                App.IsiOSBluetoothOn = true;
            }
            else
            {
                System.Console.WriteLine("Powered Off");
                App.IsiOSBluetoothOn = false;
                BlueToothService.SendMessageToDisplayConnectButton();
            }
        }
    }
}