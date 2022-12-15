using Acr.UserDialogs;
using ColorController.Controls;
using ColorController.Enums;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.StringResources;
using ColorController.ViewModels;
using ColorController.Views;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
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
using Xamarin.Forms.Internals;

namespace ColorController.Helpers
{
    /// <summary>
    /// NOTE: Always send message after setting App.ConnectionButtonState value
    /// </summary>
    public class BLEHelper
    {
        //private static BLEHelper instance = null;
        //public static BLEHelper Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new BLEHelper();
        //        }
        //        return instance;
        //    }
        //}

        private string _serviceId = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
        private string _characteristicsId = "beb5483e-36e1-4688-b7f5-ea07361b26a8";

        List<IDevice> devices;

        public BLEHelper(bool startEvent = true)
        {
            if (startEvent)
            {
                devices = new List<IDevice>();

                CrossBluetoothLE.Current.Adapter.DeviceDiscovered += (s, a) =>
                {
                    if (!string.IsNullOrWhiteSpace(a.Device.Name) && a.Device.Name.Contains("LightMode Controller"))
                    {
                        var result = devices.Contains(a.Device);
                        if (!result)
                        {
                            devices.Add(a.Device);
                        }
                    }
                };

                CrossBluetoothLE.Current.Adapter.DeviceDisconnected += OnDeviceDisconnected;
                CrossBluetoothLE.Current.Adapter.DeviceConnectionLost += OnDeviceConnectionLost;
                CrossBluetoothLE.Current.Adapter.DeviceConnected += OnDeviceConnected;
            }
        }

        private void OnDeviceConnected(object sender, DeviceEventArgs e)
        {
            Debug.WriteLine("DeviceConnected");
        }

        private async void OnDeviceConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            Debug.WriteLine("OnDeviceConnectionLost Start");
            await Disconnect(false);
            Debug.WriteLine("OnDeviceConnectionLost End");
        }

        private void OnDeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Debug.WriteLine("OnDeviceDisconnected");
        }

        int timer = 0;
         
        public async Task<List<IDevice>> ScanDevices(int timeOut = 1000, bool loop = false, bool isExecuted = false, int endTime = 5)
        {
            try
            {
                if (!isExecuted)
                {
                    try
                    {
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            // Do something
                            timer++;
                            if (App.Characteristic != null)
                            {
                                loop = false;
                                timer = 0;
                                CloseButtonPressPopupPage();
                            }

                            if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                            {
                                loop = false;
                                timer = 0;
                                CloseButtonPressPopupPage();
                            }

                            if (App.ConnectionState == ConnectionButtonState.ShowConnect)
                            {
                                loop = false;
                                timer = 0;
                                CloseButtonPressPopupPage();
                            }

                            if (timer == endTime)
                            {
                                loop = false;
                                timer = 0;

                                if (CrossBluetoothLE.Current.IsOn && App.ConnectionState != ConnectionButtonState.ShowDisconnect)
                                {
                                    //SendMessageToDisplayConnectButton();
                                    if (!PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == typeof(DoubleClickPopupPage)))
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            PopupNavigation.Instance.PushAsync(new DoubleClickPopupPage());
                                        });
                                    }
                                    MessagingCenter.Send(this, StringResource.Connection, "RestartScanning");
                                }
                            }

                            return loop;
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"BLEHelper: {ex.Message}");
                    }

                    MessagingCenter.Send(this, StringResource.Connection, "STOP");

                    //SendMessageToDisplayConnectButton();
                    await StopScanning();
                    SendMessageToDisplayConnectingButton();
                }

                if (!CrossBluetoothLE.Current.Adapter.IsScanning)
                {
                    CrossBluetoothLE.Current.Adapter.ScanMode = ScanMode.LowLatency;
                    CrossBluetoothLE.Current.Adapter.ScanTimeout = timeOut;
                    await CrossBluetoothLE.Current.Adapter.StartScanningForDevicesAsync();
                }
                //await StopScanning(); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLEHelper: {ex.Message}");
                App.ConnectionState = ConnectionButtonState.ShowConnect;
                MessagingCenter.Send(this, StringResource.Connection, StringResource.ShowConnect);
            }

            return devices;
        }

        public async Task<bool> ConnectController(List<IDevice> deviceList)
        {
            if (!CrossBluetoothLE.Current.IsOn)
            {
                return false;
            }

            App.ConnectionState = ConnectionButtonState.ShowSearchingButton;
            MessagingCenter.Send(this, StringResource.Connection, StringResource.ShowConnecting);

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
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        parameters = new ConnectParameters(forceBleTransport: true);
                    }
                    else
                    {
                        parameters = new ConnectParameters(true, false);
                    }
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(15000);
                    await CrossBluetoothLE.Current.Adapter.ConnectToDeviceAsync(device, parameters, cancellationTokenSource.Token);

                    //Request to increates MTU
                    await device.RequestMtuAsync(App.SIZE);

                    SendMessageToDisplayDisconnectButton();

                    CloseButtonPressPopupPage();

                    //Get service
                    var services = await device.GetServicesAsync();
                    var service = services.FirstOrDefault(x => x.Id == new Guid(_serviceId));

                    //Get characteristic
                    var characteristics = await service.GetCharacteristicsAsync();
                    var characteristic = characteristics.FirstOrDefault(x => x.Id == new Guid(_characteristicsId));
                    App.Characteristic = characteristic;

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
                        await ExecuteCommands(characteristic);
                    }
                }
                else
                {
                    isConneted = false;
                    SendMessageToDisplayConnectButton();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLEHelper: {ex.Message}");
                isConneted = false;
                SendMessageToDisplayConnectButton();
            }

            return isConneted;
        }

        private void StartTimerToFetchBatteryStatus()
        {
            Device.StartTimer(TimeSpan.FromMinutes(2), () =>
            {
                try
                {
                    if (!App.ContinueFetchingBatteryDetail)
                    {
                        return false;
                    }

                    MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await CommandHelper.SendCommandToController(StringResource.POWR);
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

        public async Task Disconnect(bool executeCommand = true)
        {
            try
            {
                Debug.WriteLine("Disconnect Start");
                App.ContinueFetchingBatteryDetail = false;
                UserDialogs.Instance.ShowLoading("Disconnecting...");
                if (executeCommand)
                {
                    Debug.WriteLine("QUIT Executing...");
                    await App.Characteristic?.StartUpdatesAsync();
                    var commandArray = Encoding.ASCII.GetBytes(StringResource.QUIT);
                    await App.Characteristic?.WriteAsync(commandArray);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error:" + ex.Message);
            }
            finally
            {
                App.ConnectionState = ConnectionButtonState.ShowConnect;
                MessagingCenter.Send(this, StringResource.Connection, "QUIT");
                App.Characteristic = null;
                App.BatterPercentages = null;
                UserDialogs.Instance.HideLoading();
                App.ConnectedControllerVersion = null;
                Debug.WriteLine("Disconnect End");
            }
        }

        public async Task StopScanning()
        {
            try
            {
                App.IsScanningAlreadyGoingOn = false;
                await CrossBluetoothLE.Current.Adapter.StopScanningForDevicesAsync();
            }
            catch (Exception)
            {

            }
        }

        public void SendMessageToDisplayConnectButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowConnect;
            App.IsScanningAlreadyGoingOn = false;
            MessagingCenter.Send(this, StringResource.Connection, StringResource.ShowConnect);
        }

        internal void SendMessageToDisplayDisconnectButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowDisconnect;
            App.IsScanningAlreadyGoingOn = false;
            MessagingCenter.Send(this, StringResource.Connection, StringResource.ShowDisconnect);
        }

        internal void SendMessageToDisplayConnectingButton()
        {
            App.ConnectionState = ConnectionButtonState.ShowSearchingButton;
            App.IsScanningAlreadyGoingOn = true;
            MessagingCenter.Send(this, StringResource.Connection, "ShowSearchingButton");
        }

        public async Task<bool> ConnectToKnownDevice(Guid guid)
        {
            bool isConneted = false;

            if (!CrossBluetoothLE.Current.IsOn && !App.IsiOSBluetoothOn)
            {
                return false;
            }

            App.ConnectionState = ConnectionButtonState.ShowSearchingText;
            MessagingCenter.Send(this, StringResource.Connection, "ShowSearchingText");

            try
            {
                ConnectParameters parameters;
                if (Device.RuntimePlatform == Device.Android)
                {
                    parameters = new ConnectParameters(forceBleTransport: true);
                }
                else
                {
                    parameters = new ConnectParameters(true, false);
                }
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(30000);
                await CrossBluetoothLE.Current.Adapter.ConnectToKnownDeviceAsync(guid, parameters, cancellationTokenSource.Token);
                var connectedDevice = CrossBluetoothLE.Current.Adapter.ConnectedDevices;
                var device = connectedDevice.FirstOrDefault();

                //Request to increates MTU
                await device.RequestMtuAsync(App.SIZE);

                SendMessageToDisplayDisconnectButton();

                //Get service
                var services = await device.GetServicesAsync();
                var service = services.FirstOrDefault(x => x.Id == new Guid(_serviceId));

                //Get characteristic
                var characteristics = await service.GetCharacteristicsAsync();
                var characteristic = characteristics.FirstOrDefault(x => x.Id == new Guid(_characteristicsId));
                App.Characteristic = characteristic;

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
                    SendMessageToDisplayDisconnectButton();
                    await DisplayConnectedToDeviceNamePopupFor1Second();
                }
                else
                {
                    isConneted = false;
                    SendMessageToDisplayConnectButton();
                }

                if (isConneted)
                {
                    await ExecuteCommands(characteristic);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BLEHelper: {ex.Message}");
                isConneted = false;
                //Send message only of Searching  is not going on
                if (App.ConnectionState == ConnectionButtonState.ShowSearchingText)
                {
                    SendMessageToDisplayConnectButton();
                }
            }

            return isConneted;
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

        private async Task DisplayConnectedToDeviceNamePopupFor1Second()
        {
            CloseUpdatingFirmwarePopupPage();
            await PopupNavigation.Instance.PushAsync(new ConnectedPopupPage());
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

        private async Task ExecuteCommands(ICharacteristic characteristic)
        {
            #region VERS command
            try
            {
                await Task.Delay(500);
                WriteLog("Sending Command: VERS");
                await CommandHelper.SendCommandToController("VERS");
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
                    await CommandHelper.SendCommandToController(britCommand);
                }
                else
                {
                    //If first time login then send command to play 'Standard' brightness
                    await CommandHelper.SendCommandToController("BRIT 040");
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
                    await CommandHelper.SendCommandToController("DFUQ");

                    //Set SendDFUQCommand value to false so this Popup will not appear again.
                    //Preferences.Set(StringResource.SendDFUQCommand, false);
                }
            }
            catch (Exception)
            {
            }
            #endregion
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
                await CommandHelper.SendCommandToController("BOTS 0");


                await Task.Delay(500);
                WriteLog("Sending Command: ON--");
                await CommandHelper.SendCommandToController("ON--");
            }
            catch (Exception)
            {
            }
        }

        private void WriteLog(string message)
        {
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
            Debug.WriteLine(message);
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
        }


        private void Characteristic_ValueUpdated(object o, CharacteristicUpdatedEventArgs args)
        {
            try
            {
                //Call below method just after receiving response from controller
                if (App.Characteristic != null)
                {
                    App.Characteristic.StopUpdatesAsync();
                }

                var bytes = args.Characteristic.Value;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (bytes == null)
                        return;

                    var response = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        WriteLog($"Controller Response: {response}");

                        if (App.LMDSentSuccessfully && response.Contains("VRFY"))
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    WriteLog($"LMD Saving in local DB");
                                    var result = await App.Database.SaveLMD(new LMD { Id = Guid.NewGuid().ToString(), ControllerId = App.ConnectedController.Id, AnimationId = App.StoredAnimationId });
                                    App.LMDSentSuccessfully = false;
                                    WriteLog($"LMD Saved in local DB : Result : {result}");
                                }
                                catch (Exception ex)
                                {
                                    WriteLog($"-----------------------------{ex.Message}------------------------------------------");
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
                            MessagingCenter.Send(this, StringResource.POWR, batteryPercentages);
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
    }
}
