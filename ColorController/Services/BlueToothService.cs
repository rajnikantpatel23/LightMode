using ColorController.Enums;
using ColorController.Helpers;
using ColorController.PopupPages;
using ColorController.StringResources;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.Services
{
    public class BlueToothService : IBlueToothService
    {
        public Guid ServiceId => new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
        public Guid CharacteristicsId => new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8");

        public async Task ScanAndConnectDevice()
        {
            //Check Bluetooth ON/OFF status
            //iOS: If Bluetooth ON then Start Scanning
            //Android: Check Location Permission
            //Android: If Location Permission disabled then ask for permission
            //Android: If Location permission is denied then display confirmation alert and navigate to Settings 
            //Android: Check GPS ON/OFF status
            //Android: If GPS is OFF then display alert to turn ON GPS.
            //Android: If GPS is ON then start scanning.

            if (!CrossBluetoothLE.Current.IsOn)
            {
                await PopupNavigation.Instance.PushAsync(new AlertPopupPage(StringResource.PleaseTurnONBT));
                return;
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (CrossGeolocator.IsSupported)
                {
                    if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                    {
                        locationAlwaysPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }
                    if (locationAlwaysPermissionStatus == PermissionStatus.Disabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                    }
                    if (locationAlwaysPermissionStatus == PermissionStatus.Denied)
                    {
                        await PopupNavigation.Instance.PushAsync(new AlertPopupPageWithOkCancel("You must grant this App permission to access your location in order to pair.", (response) =>
                        {
                            if (response)
                            {
                                var locSettings = DependencyService.Get<ILocSettings>();
                                locSettings.OpenSettings();
                            }
                        }));
                        return;
                    }

                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                        return;
                    }
                }

                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    return;
                }
            }

            //Search for devices 
            //Find Light Mode Controller & Connect
            //If Controller is connected first time then Display Name Popup
            BLEHelper bLEHelper = new BLEHelper();

            await bLEHelper.StopScanning();
            var devices = await bLEHelper.ScanDevices(30 * 1000, true);

            //If Device is already connected then do notihing
            if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
            {
                Debug.WriteLine("FavoriteViewModel: Already Connected....");
                return;
            }

            if (devices != null && devices.Count == 0)
            {
                Debug.WriteLine("FavoriteViewModel: Devices not found");
            }
            else
            {
                var isConnected = await bLEHelper.ConnectController(devices);
                if (!isConnected)
                {
                    Debug.WriteLine("FavoriteViewModel: Not Connected....");
                }
                else
                {
                    Debug.WriteLine("FavoriteViewModel: Connected....");
                }
            }
        }

        public async Task<List<ICharacteristic>> GetCharacteristics()
        {
            var devices = CrossBluetoothLE.Current.Adapter.ConnectedDevices;
            List<ICharacteristic> characteristics = new List<ICharacteristic>();
            foreach (var device in devices)
            {
                var service = await device.GetServiceAsync(ServiceId);
                var characteristic = await service.GetCharacteristicAsync(CharacteristicsId);
                characteristics.Add(characteristic);
            }

            return characteristics;
        }

        public async Task SendCommandToController(string command, bool executeStartUpdate = true)
        {
            var characteristics = await GetCharacteristics();

            foreach (var characteristic in characteristics)
            {
                var commandArray = Encoding.ASCII.GetBytes(command);

                if (MainThread.IsMainThread)
                {
                    if (executeStartUpdate)
                    {
                        await characteristic.StartUpdatesAsync();
                    }
                    var result = await characteristic.WriteAsync(commandArray);
                }
                else
                {
                    Debug.WriteLine($"====================CommandHelper MainThread.InvokeOnMainThreadAsync : {command}===============================");

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (executeStartUpdate)
                        {
                            await characteristic.StartUpdatesAsync();
                        }
                        var result = await characteristic.WriteAsync(commandArray);
                    });
                }
            }
        }

        public async Task SendHueSaturationToController(Color color)
        {
            var hueXXX = GetXXXHueValue(color.Hue);
            if (hueXXX.Length == 1)
            {
                hueXXX = $"00{hueXXX}";
            }
            if (hueXXX.Length == 2)
            {
                hueXXX = $"0{hueXXX}";
            }

            var saturationValue = GetSaturationValue(color.Luminosity);
            var saturationXXX = $"{saturationValue * 100}";
            if (saturationXXX.Length == 1)
            {
                saturationXXX = $"00{saturationXXX}";
            }
            if (saturationXXX.Length == 2)
            {
                saturationXXX = $"0{saturationXXX}";
            }

            await SendCommandToController($"OPTS {hueXXX}{saturationXXX}", false);
        }

        private string GetXXXHueValue(double hue)
        {
            double value = 0;
            if (hue < 0.5)
            {
                value = (hue) * 256 + 128;
            }
            if (hue >= 0.5)
            {
                value = (hue) * 256 - 128;
            }
            value = Math.Round(value);
            return value.ToString();
        }

        public double GetSaturationValue(double luminosity)
        {
            double value = 0;
            if (luminosity > 0 && luminosity <= 0.525)
            {
                value = 1;
            }
            if (luminosity > 0.525 && luminosity <= 0.550)
            {
                value = 0.95;
            }
            if (luminosity > 0.550 && luminosity <= 0.575)
            {
                value = 0.90;
            }
            if (luminosity > 0.575 && luminosity <= 0.600)
            {
                value = 0.85;
            }
            if (luminosity > 0.600 && luminosity <= 0.625)
            {
                value = 0.80;
            }
            if (luminosity > 0.625 && luminosity <= 0.650)
            {
                value = 0.75;
            }
            if (luminosity > 0.650 && luminosity <= 0.675)
            {
                value = 0.70;
            }
            if (luminosity > 0.675 && luminosity <= 0.700)
            {
                value = 0.65;
            }
            if (luminosity > 0.700 && luminosity <= 0.725)
            {
                value = 0.60;
            }
            if (luminosity > 0.725 && luminosity <= 0.750)
            {
                value = 0.55;
            }
            if (luminosity > 0.750 && luminosity <= 0.775)
            {
                value = 0.50;
            }
            if (luminosity > 0.775 && luminosity <= 0.800)
            {
                value = 0.45;
            }
            if (luminosity > 0.800 && luminosity <= 0.825)
            {
                value = 0.40;
            }
            if (luminosity > 0.825 && luminosity <= 0.850)
            {
                value = 0.35;
            }
            if (luminosity > 0.850 && luminosity <= 0.875)
            {
                value = 0.30;
            }
            if (luminosity > 0.875 && luminosity <= 0.900)
            {
                value = 0.25;
            }
            if (luminosity > 0.900 && luminosity <= 0.925)
            {
                value = 0.20;
            }
            if (luminosity > 0.925 && luminosity <= 0.950)
            {
                value = 0.15;
            }
            if (luminosity > 0.950 && luminosity <= 0.975)
            {
                value = 0.10;
            }
            if (luminosity > 0.975 && luminosity <= 1)
            {
                value = 0;
            }

            return value;
        }

        public async Task SendCommandToUpdateFirmware(string command, bool executeStartUpdate = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    CommonUtils.WriteLog($"Sending Command: {command}");

                    cancellationToken.ThrowIfCancellationRequested();
                    var characteristics = await GetCharacteristics();
                    var characteristic = characteristics.FirstOrDefault();
                    if (characteristic != null)
                    {
                        var commandArray = Encoding.ASCII.GetBytes(command);

                        if (MainThread.IsMainThread)
                        {
                            if (executeStartUpdate)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await characteristic.StartUpdatesAsync();
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                            var result = await characteristic.WriteAsync(commandArray, cancellationToken);
                        }
                        else
                        {
                            CommonUtils.WriteLog($"====================CommandHelper MainThread.InvokeOnMainThreadAsync : {command}===============================");

                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                if (executeStartUpdate)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    await characteristic.StartUpdatesAsync();
                                }

                                cancellationToken.ThrowIfCancellationRequested();
                                var result = await characteristic.WriteAsync(commandArray);
                            });
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"SendCommandToController(): {command} command : {ex.Message}");
                throw;
            }
        }
    }
}
