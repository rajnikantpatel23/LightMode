using Acr.UserDialogs;
using ColorController.Controls;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.StringResources;
using ColorController.ViewModels;
using ColorController.Views;
using Microsoft.AppCenter;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.Services
{
    public class BlueToothService : IBlueToothService
    {
        List<IDevice> _devices;

        public BlueToothService()
        {
            //MessagingCenter.Subscribe<object, string>(this, StringResource.Connection, async (s, args) =>
            //{
            //    if (args == "RestartScanning")
            //    {
            //        await ScanAndConnectDevice();
            //    }
            //});
        }

        public Guid ServiceId => new Guid("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
        public Guid CharacteristicsId => new Guid("beb5483e-36e1-4688-b7f5-ea07361b26a8");

        //public bool IsAppConnectedWithDevice => CrossBluetoothLE.Current.Adapter.ConnectedDevices != null && CrossBluetoothLE.Current.Adapter.ConnectedDevices.Any();
        public bool IsAppConnectedWithDevice => ConnectedDeviceCount > 0;

        public int ConnectedDeviceCount
        {
            get
            {
                var connectedDevices = GetConnectedDevices();
                return connectedDevices.Count();
            }
        }

        public IReadOnlyList<IDevice> GetConnectedDevices()
        {
            var connectedDevices = CrossBluetoothLE.Current.Adapter.ConnectedDevices.Where(x => x.State == DeviceState.Connected).Distinct();
            return connectedDevices.ToList();
        }


        //Add New Device
        public async Task ScanAndConnectDevice(CancellationToken cancellationToken = default)
        {
            try
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

                if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
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
                await StopScanning();

                //var devices1 = await new BLEHelper().ScanDevices(10 * 1000, true);
                var devices = await ScanDevices(10 * 1000, true, cancellationToken: cancellationToken);

                //If Device is already connected then do notihing
                if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                {
                    CommonUtils.WriteLog("FavoriteViewModel: Already Connected....");
                    return;
                }

                if (devices != null && devices.Count == 0)
                {
                    //await ScanAndConnectDevice();
                    CommonUtils.WriteLog("FavoriteViewModel: Devices not found");
                }
                else
                {
                    var isConnected = await ConnectController(devices, cancellationToken);
                    if (!isConnected)
                    {
                        CommonUtils.WriteLog("FavoriteViewModel: Not Connected....");
                    }
                    else
                    {
                        CommonUtils.WriteLog("FavoriteViewModel: Connected....");
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                CommonUtils.WriteLog("ScanAndConnectDevice: OperationCanceledException" + oce.Message);
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog("ScanAndConnectDevice: Exception" + ex.Message);
            }
        }
               

        public async Task<List<ICharacteristic>> GetCharacteristics()
        {
            var devices = GetConnectedDevices();
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
                    CommonUtils.WriteLog($"====================CommandHelper MainThread.InvokeOnMainThreadAsync : {command}===============================");

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

        #region ConnectToKnownDevice
        public async Task<bool> ConnectToKnownDeviceInBackground(Controller controller)
        {
            MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), true);
            var isConneted = await ConnectToKnownDeviceInBackground_V2(controller);
            if (isConneted)
            {
                MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
            }
            else
            {
                RemoveEventHandlers();
            }
            //if (!isConnected && App.ConnectionState != ConnectionButtonState.ShowDisconnect)
            //{
            //    StartAutoConnectingInBackground();
            //}

            //MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
            return isConneted;
        }
        #endregion

        public async Task<bool> AreAllSavedDevicesConnected()
        {
            bool connected = true;
            var savedDevices = await App.Database.GetControllers();
            var connectedDeviceIds = GetConnectedDevices().Select(x => x.Id);

            //if (savedDevices.Count != connectedDeviceIds.Count())
            //    return false;

            foreach ( var device in savedDevices) 
            {
                var contains = connectedDeviceIds.Any(x => x.ToString() == device.Id);
                if (!contains)
                {
                    connected = false;
                    WriteLog("All Saved Devices Are Not Connetected");
                    break;
                }
            }

            //foreach (var savedDevice in savedDevices)
            //{
            //    if (!connectedDeviceIds.Any(x => x == new Guid(savedDevice.Id)))
            //    {
            //        //Continue Background Task
            //        //MessagingCenter.Send<object, bool>(this, MessageType.DisplaySearchingForDevicesText.ToString(), false);
            //    }
            //    else
            //    {
            //        connected = true;
            //    }
            //}

            return connected;
        }

        public async Task<bool> IsBluetoothON()
        {
            //Get bluetooth status on App Starts
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS && !CrossBluetoothLE.Current.IsOn)
            {
                var platformSpecific = DependencyService.Get<IPlatformSpecific>();
                platformSpecific.GetBluetoothStatus();
                await Task.Delay(500);
            }

            return CrossBluetoothLE.Current.IsOn || App.IsiOSBluetoothOn;
        }

        public async Task<bool> IsLocationPermissionAllowed()
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
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
                        return false;
                    }

                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                        return false;
                    }
                }

                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    return false;
                }
            }

            return true;
        }

       
        
        
        
        
        
        
        
        
        //BLEHElpe Methods
        
        private IAdapter _adapter;
        public IAdapter Adapter
        {
            get
            {
                if (_adapter == null)
                {
                    _adapter = CrossBluetoothLE.Current.Adapter;
                    CrossBluetoothLE.Current.Adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
                    CrossBluetoothLE.Current.Adapter.DeviceConnected += Adapter_DeviceConnected;
                    CrossBluetoothLE.Current.Adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
                    CrossBluetoothLE.Current.Adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
                }
                return _adapter;
            }
        }

        public void RemoveEventHandlers()
        {
            //Adapter.DeviceDiscovered -= Adapter_DeviceDiscovered;
            //Adapter.DeviceConnected -= Adapter_DeviceConnected;
            //Adapter.DeviceDisconnected -= Adapter_DeviceDisconnected;
            //Adapter.DeviceConnectionLost -= Adapter_DeviceConnectionLost;
            //CommonUtils.WriteLog("Event Handlers Removed");
        }

        #region ConnectToKnownDeviceInBackground_V2
        public async Task<bool> ConnectToKnownDeviceInBackground_V2(Controller controller)
        {
            bool isConneted = false;

            if (!await IsBluetoothON())
            {
                return false;
            }

            try
            {
                var parameters = new ConnectParameters(autoConnect: Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android,
                                                       forceBleTransport: Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android);

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var isAlreadyConnected = GetConnectedDevices().Any(x => x.Id == new Guid(controller.Id));
                if (isAlreadyConnected)
                    return false;

                var device = await Adapter.ConnectToKnownDeviceAsync(new Guid(controller.Id), parameters, cancellationTokenSource.Token);

                isConneted = true;
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"BLEHelper ConnectToKnownDeviceInBackground_V2() Exception: {ex.Message}");
            }

            return isConneted;
        } 
        #endregion

        #region Adaptor Events Methods
        public void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Device.Name) && e.Device.Name.Contains("LightMode Controller"))
            {
                var result = _devices.Contains(e.Device);
                if (!result)
                {
                    _devices.Add(e.Device);
                }
            }
        }

        public async void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
        {
            CommonUtils.WriteLog("Device Connected!");

            var device = e.Device;

            //Request to increates MTU
            await device.RequestMtuAsync(App.SIZE);

            //Get service
            var service = await device.GetServiceAsync(ServiceId);

            //Get characteristic
            var characteristic = await service.GetCharacteristicAsync(CharacteristicsId);

            characteristic.ValueUpdated -= Characteristic_ValueUpdated;
            characteristic.ValueUpdated += Characteristic_ValueUpdated;

            SendMessageToDisplayDisconnectButton();

            var savedDevices = await App.Database.GetControllers();
            var savedDevice = savedDevices.FirstOrDefault(x => x.Id == e.Device.Id.ToString());
            if (savedDevice != null)
            {
                await DisplayConnectedToDeviceNamePopupFor1Second(savedDevice.Name);
            }

            await ExecuteCommandsAfterConnection(characteristic);
        }

        public async void Adapter_DeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            CommonUtils.WriteLog("OnDeviceConnectionLost Start");
            await Disconnect(false);
            CommonUtils.WriteLog("OnDeviceConnectionLost End");
        }

        public void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            CommonUtils.WriteLog("OnDeviceDisconnected");
        }

        #endregion

        #region ExecuteCommandsAfterConnection
        private async Task ExecuteCommandsAfterConnection(ICharacteristic characteristic)
        {
            #region VERS command
            try
            {
                await Task.Delay(500);
                WriteLog("Sending Command: VERS");
                await SendCommandToController("VERS");
            }
            catch (Exception)
            {
            }
            #endregion

            #region BRITXXXcommand
            try
            {
                WriteLog("Sending Command: BRIT");
                var britCommand = Preferences.Get("LastPlayedBritCommand", null);
                if (!string.IsNullOrWhiteSpace(britCommand))
                {
                    await SendCommandToController(britCommand);
                }
                else
                {
                    //If first time login then send command to play 'Standard' brightness
                    await SendCommandToController("BRIT 040");
                }
                await Task.Delay(500);
            }
            catch (Exception)
            {
            }
            #endregion

            #region DFUQ Command
            try
            {
                var firmwareUpdated = Preferences.Get(StringResource.SendDFUQCommand, false);
                if (firmwareUpdated)
                {
                    await Task.Delay(500);
                    WriteLog("Sending Command: DFUQ");
                    await SendCommandToController("DFUQ");

                    //Set SendDFUQCommand value to false so this Popup will not appear again.
                    //Preferences.Set(StringResource.SendDFUQCommand, false);
                }
            }
            catch (Exception)
            {
            }
            #endregion
        } 
        #endregion

        public async Task Disconnect(bool executeCommand = true)
        {
            try
            {
                CommonUtils.WriteLog("Disconnect Start");
                App.ContinueFetchingBatteryDetail = false;
                UserDialogs.Instance.ShowLoading("Disconnecting...");
                if (executeCommand)
                {
                    CommonUtils.WriteLog("QUIT Executing...");
                    var commandArray = Encoding.ASCII.GetBytes(StringResource.QUIT);
                    await SendCommandToController(StringResource.QUIT);
                }
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog("Error:" + ex.Message);
            }
            finally
            {
                App.ConnectionState = ConnectionButtonState.ShowConnect;
                MessagingCenter.Send<object, string>(this, StringResource.Connection, "QUIT");
                App.BatterPercentages = null;
                UserDialogs.Instance.HideLoading();
                App.ConnectedControllerVersion = null;
                CommonUtils.WriteLog("Disconnect End");
            }
        }

        private async Task DisplayConnectedToDeviceNamePopupFor1Second(string controllerName = null)
        {
            CloseUpdatingFirmwarePopupPage();
            UserDialogs.Instance.Toast(new ToastConfig($"Connected to {controllerName}!") { Position = ToastPosition.Top, Duration = TimeSpan.FromSeconds(1) });
            //await Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    if (PopupNavigation.Instance.PopupStack.Any(x => x != null && x.GetType() == typeof(ConnectedPopupPage)))
            //    {
            //        await PopupNavigation.Instance.PopAsync();
            //    }

            //    await PopupNavigation.Instance.PushAsync(new ConnectedPopupPage(controllerName));
            //});
        }

        private async void Characteristic_ValueUpdated(object o, CharacteristicUpdatedEventArgs args)
        {
            try
            {
                //To-Do: Need to work here...
                //Call below method just after receiving response from controller
                //if (App.Characteristic != null)
                //{
                //    App.Characteristic.StopUpdatesAsync();
                //}

                var bytes = args.Characteristic.Value;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (bytes == null)
                        return;

                    var response = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        CommonUtils.WriteLog($"Controller Response: {response}");

                        if (App.LMDSentSuccessfully && response.Contains("VRFY"))
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    CommonUtils.WriteLog($"LMD Saving in local DB");
                                    var result = await App.Database.SaveLMD(new LMD { Id = Guid.NewGuid().ToString(), ControllerId = App.ConnectedController.Id, AnimationId = App.StoredAnimationId });
                                    App.LMDSentSuccessfully = false;
                                    CommonUtils.WriteLog($"LMD Saved in local DB : Result : {result}");
                                }
                                catch (Exception ex)
                                {
                                    CommonUtils.WriteLog($"-----------------------------{ex.Message}------------------------------------------");
                                }
                            });
                        }

                        //Below commented code is just for development & not for actual App
                        //if (App.ControllerUpdatePage != null && App.ControllerUpdatePage.OutputSource != null)
                        //{
                        //    App.ControllerUpdatePage.OutputSource.Add(response);
                        //}

                        if (response.Contains(StringResource.POWR))
                        {
                            //BatterPOWR 11\r\n
                            var responseArray = response.Split(' ');
                            var batteryPercentages = Regex.Replace(responseArray[1], "\n|\r", string.Empty);
                            MessagingCenter.Send<object, string>(this, StringResource.POWR, batteryPercentages);
                            App.BatterPercentages = batteryPercentages;
                        }

                        GetVersionOfController(response);

                        DisplayLastDFUResponse(response);
                    }
                });
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string> { { "Characteristic_ValueUpdated", $"{ex.StackTrace}" } });
            }
        }

         private void GetVersionOfController(string response)
        {
            try
            {
                if (response.StartsWith("v"))
                {
                    var response1 = response.Replace("v", "");
                    var vers = Regex.Replace(response1, "\n|\r", string.Empty);
                    App.ConnectedControllerVersion = vers;

                    UpdateAnimationsList();
                }
            }
            catch (Exception)
            {

            }
        }

        private void UpdateAnimationsList()
        {
            var dashboard = App.Current.MainPage as Dashboard;
            if (dashboard != null)
            {
                var extendedTabbedPage = dashboard as ExtendedTabbedPage;
                if (extendedTabbedPage != null)
                {
                    var favoritesPage = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(NavigationPage) && ((NavigationPage)x).RootPage.GetType() == typeof(FavoritesPage));
                    if (favoritesPage != null)
                    {
                        var viewModel = ((NavigationPage)favoritesPage).RootPage.BindingContext as FavoritesViewModel;
                        if (viewModel != null)
                        {
                            if (viewModel.Animations != null && viewModel.Animations.Any())
                            {
                                var connectedControllerVersion = new Version(App.ConnectedControllerVersion);

                                foreach (var animation in viewModel.Animations)
                                {
                                    var animationControllerVersion = new Version(animation.ControllerVersion);

                                    if (animationControllerVersion > connectedControllerVersion)
                                    {
                                        animation.IsShieldVisible = true;
                                    }
                                    if (animationControllerVersion <= connectedControllerVersion)
                                    {
                                        animation.IsShieldVisible = false;
                                    }
                                }
                            }
                        }
                    }

                    var animationPage = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(NavigationPage) && ((NavigationPage)x).RootPage.GetType() == typeof(AnimationPage));
                    if (animationPage != null)
                    {
                        var viewModel = ((NavigationPage)animationPage).RootPage.BindingContext as AnimationViewModel;
                        if (viewModel != null)
                        {
                            if (viewModel.Animations != null && viewModel.Animations.Any())
                            {
                                var connectedControllerVersion = new Version(App.ConnectedControllerVersion);

                                foreach (var animation in viewModel.Animations)
                                {
                                    var animationControllerVersion = new Version(animation.ControllerVersion);

                                    if (animationControllerVersion > connectedControllerVersion)
                                    {
                                        animation.IsShieldVisible = true;
                                    }
                                    if (animationControllerVersion <= connectedControllerVersion)
                                    {
                                        animation.IsShieldVisible = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void DisplayLastDFUResponse(string response)
        {
            if (response.StartsWith("DFUQ"))
            {
                Preferences.Set(StringResource.SendDFUQCommand, false);
                await SendBOTSZeroCommand();

                if (response.StartsWith("DFUQ 1"))
                {
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware updated successfully."));
                }

                if (response.StartsWith("DFUQ 2"))
                {
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to LM_SERVER_DFU_FAILED."));
                }

                if (response.StartsWith("DFUQ 3"))
                {
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to DFU timed out."));
                }

                if (response.StartsWith("DFUQ 4"))
                {
                    await PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to dne."));
                }

                if (response.StartsWith("DFUQ 5"))
                {
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to WiFi connection timeout."));
                }
            }

            switch (response)
            {
                case "":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to DFU terminated prematurely / unable to connect wifi."));
                    break;
                case "1":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware updated successfully."));
                    break;
                case "2":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to LM_SERVER_DFU_FAILED."));
                    break;
                case "3":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to DFU timed out."));
                    break;
                case "4":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to dne."));
                    break;
                case "5":
                    Preferences.Set(StringResource.SendDFUQCommand, false);
                    await SendBOTSZeroCommand();
                    PopupNavigation.Instance.PushAsync(new AlertPopupPage("Your firmware update failed due to WiFi connection timeout."));
                    break;
                default:
                    break;
            }
        }

        private void CloseUpdatingFirmwarePopupPage()
        {
            var updatingPopupPage = PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x != null && x.GetType() == typeof(UpdatingPopupPage));
            if (updatingPopupPage != null)
            {
                CommonUtils.WriteLog("Closing updatingPopupPage");
                CommonUtils.ClosePopup();
                CommonUtils.WriteLog("Closed updatingPopupPage");
            }
        }

        private async Task SendBOTSZeroCommand()
        {
            try
            {
                await Task.Delay(500);
                WriteLog("Sending Command: BOTS 0");
                await SendCommandToController("BOTS 0");


                await Task.Delay(500);
                WriteLog("Sending Command: ON--");
                await SendCommandToController("ON--");
            }
            catch (Exception)
            {
            }
        }

        private void WriteLog(string message)
        {
            CommonUtils.WriteLog($"---------------------------------------------------------------------------------------");
            CommonUtils.WriteLog(message);
            CommonUtils.WriteLog($"---------------------------------------------------------------------------------------");
        }

        public async Task StopScanning()
        {
            try
            {
                App.IsScanningAlreadyGoingOn = false;
                await Adapter.StopScanningForDevicesAsync();
            }
            catch (Exception)
            {

            }
        }
        #region Send Messages
        public void SendMessageToDisplayConnectButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowConnect;
            App.IsScanningAlreadyGoingOn = false;
            MessagingCenter.Send<object, string>(this, StringResource.Connection, StringResource.ShowConnect);
        }

        public void SendMessageToDisplayDisconnectButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowDisconnect;
            App.IsScanningAlreadyGoingOn = false;
            MessagingCenter.Send<object, string>(this, StringResource.Connection, StringResource.ShowDisconnect);
        }

        public void SendMessageToDisplayConnectingButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowSearchingButton;
            App.IsScanningAlreadyGoingOn = true;
            MessagingCenter.Send<object, string>(this, StringResource.Connection, "ShowSearchingButton");
        }
        #endregion

        public async Task<bool> ConnectController(List<IDevice> deviceList, CancellationToken cancellationToken)
        {
            if (!CrossBluetoothLE.Current.IsOn)
            {
                return false;
            }

            App.ConnectionState = ConnectionButtonState.ShowSearchingButton;
            MessagingCenter.Send<object, string>(this, StringResource.Connection, StringResource.ShowConnecting);

            bool isConneted = false;
            IDevice device = null;
            try
            {
                await StopScanning();

                var lightModeControllers = new List<IDevice>();
                foreach (var item in deviceList)
                {
                    if (item.Name != null && item.Name != string.Empty && item.Name.Contains("LightMode Controller"))
                    {
                        lightModeControllers.Add(item);
                    }
                }

                //'Connect' buttons and 'Auto-Connect' have slightly different protocols.
                //Connect Buttons: If two unsaved controllers are found, randomly connect to either.
                //Auto-Connect: If one, two, three, etc controllers are found, ONLY connect to the default controller. If user has no default controller, then Auto-Connect should not pair.
                var defaultController = await App.Database.GetDefaultController();
                if (defaultController != null)
                {
                    //If default controller is available then connect to default controller
                    foreach (var item in lightModeControllers)
                    {
                        //if (Device.RuntimePlatform == Device.iOS)
                        //{
                        //    if (item.Id == new Guid(defaultController.Id))
                        //    {
                        //        device = item;
                        //    }
                        //}
                        //else
                        //{
                        //    var deviceBase = item as DeviceBase;
                        //    var macAddress = deviceBase.NativeDevice.ToString();
                        //    if (macAddress == defaultController.Id)
                        //    {
                        //        device = item;
                        //    }
                        //}

                        if (item.Id == new Guid(defaultController.Id))
                        {
                            device = item;
                        }
                    }

                    //If default controller from saved device list not found
                    if (device == null)
                    {
                        device = lightModeControllers.FirstOrDefault();
                    }
                }
                else
                {
                    if (App.IsAutoScanningGoingOn)
                    {
                        //Don't connect any device
                    }
                    else
                    {
                        //If default controller not available then connect to first controller
                        device = lightModeControllers.FirstOrDefault();
                    }
                }

                if (device != null)
                {
                    //Find whether connected controller exist in local DB or not.
                    var isNewConnection = await FindWhetherControllerExistOnNotInLocalDB(device);

                    //Get device which contains 'LightMode Controller' AdvertisementRecord
                    //foreach (var item in deviceList)
                    //{
                    //    var bytes = Encoding.ASCII.GetBytes("LightMode Controller");
                    //    var result = item.AdvertisementRecords.Any(x => x.Data.SequenceEqual(bytes));
                    //    if (result)
                    //    {
                    //        device = item;
                    //        break;
                    //    }
                    //}

                    ConnectParameters parameters;
                    if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                    {
                        parameters = new ConnectParameters(forceBleTransport: true);
                    }
                    else
                    {
                        parameters = new ConnectParameters(true, false);
                    }

                    //var cancellationTokenSource = new CancellationTokenSource();
                    //cancellationTokenSource.CancelAfter(15000);
                    await Adapter.ConnectToDeviceAsync(device, parameters, cancellationToken: cancellationToken);

                    //Request to increates MTU
                    await device.RequestMtuAsync(App.SIZE);

                    SendMessageToDisplayDisconnectButton();

                    CloseButtonPressPopupPage();

                    //Get service
                    var service = await device.GetServiceAsync(ServiceId);

                    //Get characteristic
                    var characteristic = await service.GetCharacteristicAsync(CharacteristicsId);

                    characteristic.ValueUpdated -= Characteristic_ValueUpdated;
                    characteristic.ValueUpdated += Characteristic_ValueUpdated;

                    await characteristic.StartUpdatesAsync();
                    var commandArray = Encoding.ASCII.GetBytes(StringResource.POWR);
                    await characteristic.WriteAsync(commandArray);
                    App.ContinueFetchingBatteryDetail = true;
                    //Start timer to check battery percentages of the Controller
                    StartTimerToFetchBatteryStatus();

                    //If BT is ON then only ask to enter device name.
                    if (CrossBluetoothLE.Current.IsOn)
                    {
                        isConneted = true;
                        //SendMessageToDisplayDisconnectButton();

                        if (isNewConnection)
                        {
                            PopupNavigation.PushAsync(new EnterDeviceNamePopupPage(device));
                        }
                        else
                        {
                            await DisplayConnectedToDeviceNamePopupFor1Second();
                        }
                    }
                    else
                    {
                        isConneted = false;
                        SendMessageToDisplayConnectButton();
                    }

                    if (isConneted)
                    {
                        await ExecuteCommandsAfterConnection(characteristic);
                    }
                }
                else
                {
                    isConneted = false;
                    SendMessageToDisplayConnectButton();
                }
            }
            catch (OperationCanceledException ex2)
            {
                CommonUtils.WriteLog($"BLEHelper OperationCanceledException: {ex2.Message}");
                isConneted = false;
                SendMessageToDisplayConnectButton();
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"BLEHelper: {ex.Message}");
                isConneted = false;
                SendMessageToDisplayConnectButton();
            }

            return isConneted;
        }

        private void StartTimerToFetchBatteryStatus()
        {
            Xamarin.Forms.Device.StartTimer(TimeSpan.FromMinutes(2), () =>
            {
                try
                {
                    if (!App.ContinueFetchingBatteryDetail)
                    {
                        return false;
                    }

                    MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await SendCommandToController(StringResource.POWR);
                    });
                }
                catch (Exception)
                {

                }

                return App.ContinueFetchingBatteryDetail;
            });
        }

        /// <summary>
        /// Find whether connected controller exist in local DB or not.
        /// </summary>
        /// <param name="device">Connected device</param>
        /// <returns>Device exists or not</returns>
        private async Task<bool> FindWhetherControllerExistOnNotInLocalDB(IDevice device)
        {
            Controller existingController = null;
            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    var deviceUDID = device.Id.ToString();
            //    existingController = await App.Database.GetController(deviceUDID);
            //}
            //else
            //{
            //    var deviceBase = device as DeviceBase;
            //    var deviceMacAddress = deviceBase.NativeDevice.ToString();
            //    existingController = await App.Database.GetController(deviceMacAddress);
            //}
            var deviceUDID = device.Id.ToString();
            existingController = await App.Database.GetController(deviceUDID);

            //If existingController not found it means connected controller is connecting firsttime
            var isNewConnection = existingController == null;
            if (!isNewConnection)
            {
                Preferences.Set("defaultControllerName", existingController.Name);

                App.ConnectedController = existingController;
            }
            return isNewConnection;
        }
        public void CloseButtonPressPopupPage()
        {
            try
            {
                if (PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == typeof(DoubleClickPopupPage)))
                {
                    var doubleClickPopupPage = PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x.GetType() == typeof(DoubleClickPopupPage));
                    PopupNavigation.Instance.RemovePageAsync(doubleClickPopupPage);
                    //PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception)
            {

            }
        }

        int timer = 0;
        public async Task<List<IDevice>> ScanDevices(int timeOut = 1000, bool loop = false, bool isExecuted = false, int endTime = 5, CancellationToken cancellationToken = default)
        {
            _devices = new List<IDevice>();
            try
            {
                if (!isExecuted)
                {
                    try
                    {
                        Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                // Do something
                                timer++;

                                //To-Do: Need to work here...
                                //if (App.Characteristic != null || App.ConnectionState == ConnectionButtonState.ShowDisconnect || App.ConnectionState == ConnectionButtonState.ShowConnect)
                                //{
                                //    loop = false;
                                //    timer = 0;
                                //    CloseButtonPressPopupPage();
                                //}

                                if (timer == endTime)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    loop = false;
                                    timer = 0;

                                    if (CrossBluetoothLE.Current.IsOn && App.ConnectionState != ConnectionButtonState.ShowDisconnect)
                                    {
                                        //SendMessageToDisplayConnectButton();
                                        if (!PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == typeof(DoubleClickPopupPage)))
                                        {
                                            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                                            {
                                                //PopupNavigation.Instance.PushAsync(new DoubleClickPopupPage());
                                            });
                                        }
                                        MessagingCenter.Send<object, string>(this, StringResource.Connection, "RestartScanning");
                                    }
                                }
                            }
                            catch (OperationCanceledException oce)
                            {
                                loop = false;
                                CommonUtils.WriteLog($"BLEHelper: ScanDevices => Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(1) {oce.Message}");
                            }
                            catch (Exception ex)
                            {
                                loop = false;
                                CommonUtils.WriteLog($"BLEHelper: ScanDevices => Xamarin.Forms.Device.StartTimer(TimeSpan.FromSeconds(1) {ex.Message}");
                            }

                            return loop;
                        });
                    }
                    catch (OperationCanceledException oce)
                    {
                        CommonUtils.WriteLog($"BLEHelper: ScanDevices {oce.Message}");
                    }
                    catch (Exception ex)
                    {
                        CommonUtils.WriteLog($"BLEHelper: ScanDevices {ex.Message}");
                    }

                    MessagingCenter.Send<object, string>(this, StringResource.Connection, "STOP");

                    //SendMessageToDisplayConnectButton();
                    await StopScanning();
                    SendMessageToDisplayConnectingButton();
                }

                if (!Adapter.IsScanning)
                {
                    Adapter.ScanMode = ScanMode.LowLatency;
                    Adapter.ScanTimeout = timeOut;
                    await Adapter.StartScanningForDevicesAsync(cancellationToken: cancellationToken);
                }
                //await StopScanning(); 
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog($"BLEHelper: {ex.Message}");
                App.ConnectionState = ConnectionButtonState.ShowConnect;
                MessagingCenter.Send<object, string>(this, StringResource.Connection, StringResource.ShowConnect);
            }

            return _devices;
        }












        public async Task<bool> CheckPermissions()
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
                return false;
            }

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
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
                        return false;
                    }

                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                        return false;
                    }
                }

                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    return false;
                }
            }

            return true;
        }


        private async Task ScanDevice(CancellationToken cancellationToken = default, DateTime startTime = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if ((DateTime.Now - startTime).Seconds == 5)
            {
                await PopupNavigation.Instance.PushAsync(new DoubleClickPopupPage());
            }

            await Adapter.StopScanningForDevicesAsync();

            if (!Adapter.IsScanning)
            {
                Adapter.ScanMode = ScanMode.LowLatency;
                Adapter.ScanTimeout = 5 * 1000;
                await Adapter.StartScanningForDevicesAsync(cancellationToken: cancellationToken);
            }

            if (_devices == null || !_devices.Any())
            {
                await ScanDevice(cancellationToken: cancellationToken, startTime);
                CommonUtils.WriteLog("ScanAndConnectDevice_2: No Device Found.");
                return;
            }
        }
         

        public async Task ScanAndConnectDevice_2(CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _devices = new List<IDevice>();

                var isValid = await CheckPermissions();
                if (!isValid)
                {
                    CommonUtils.WriteLog("CheckPermissions: Permission not granted/Bluetooth is not ON/GPS Not enabled....");
                    return;
                }

                await ScanDevice(cancellationToken: cancellationToken, startTime: DateTime.Now);

                if (_devices == null || !_devices.Any())
                {
                    await ScanDevice(cancellationToken: cancellationToken);
                    CommonUtils.WriteLog("ScanAndConnectDevice_2: No Device Found.");
                    return;
                }

                var isConnected = await ConnectController(_devices, cancellationToken);
                if (!isConnected)
                    CommonUtils.WriteLog("FavoriteViewModel: Not Connected....");
                else
                    CommonUtils.WriteLog("FavoriteViewModel: Connected....");
            }
            catch (OperationCanceledException oce)
            {
                CommonUtils.WriteLog("ScanAndConnectDevice: OperationCanceledException" + oce.Message);
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog("ScanAndConnectDevice: Exception" + ex.Message);
            }
        }
    }
}
