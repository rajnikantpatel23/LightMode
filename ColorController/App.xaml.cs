using Acr.UserDialogs;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.Services;
using ColorController.Views;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ColorController.StringResources;
using ColorController.PopupPages;
using Rg.Plugins.Popup.Services;
using ColorController.Abstractions;
using FFImageLoading;
using BLESample1.Views;

namespace ColorController
{
    public partial class App : Application
    {
        //Don't update until not confirm by Thomas
        public static Version LatestFirmwareVersion = new Version("3.1.14");

        static Database.LightModeDatabase _animationDatabase;

        public static ControllerUpdatePage ControllerUpdatePage { get; internal set; }

        // Create the database connection as a singleton.
        public static Database.LightModeDatabase Database
        {
            get
            {
                if (_animationDatabase == null)
                {
                    _animationDatabase = new Database.LightModeDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LightMode.db3"));
                }
                return _animationDatabase;
            }
        }

        public static bool ColorPaletteFocused { get; internal set; }
        public static BaseContentPage ColorsPage { get; internal set; }
        public static Dashboard DashboardInstance { get; internal set; }
        public static int CurrentIndex { get; internal set; }
        public static ICharacteristic Characteristic { get; internal set; }
        public static bool ContinueFetchingBatteryDetail { get; internal set; }
        public static string BatterPercentages { get; internal set; }
        public static ConnectionButtonState ConnectionState { get; internal set; }
        public static bool IsScanningAlreadyGoingOn { get; internal set; }
        //public static bool ConnectButtonClicked { get; internal set; }
        public static Color CurrentSelectedColor { get; internal set; }
        public static bool IsAutoScanningGoingOn { get; internal set; }
        //public static bool ConnectingButtonTapped { get; internal set; }
       
        /// <summary>
        /// Will be used to check Bluetooth on iOS (On first time App start Plugin.BLE not detecting bluetooth status)
        /// </summary>
        public static bool IsiOSBluetoothOn { get; set; }
        public static string ConnectedControllerVersion { get; set; }

        /// <summary>
        /// This will be used to perform operations when user manually touch the color picker to select color
        /// </summary>
        public static bool IsColorPickerTouchedManually { get; internal set; }
        public static Controller ConnectedController { get; set; }
        public static bool LMDSentSuccessfully { get; internal set; }
        public static string StoredAnimationId { get; internal set; }

        public static int SIZE = 240;

        public App()
        {
            InitializeComponent();
            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            Sharpnado.HorizontalListView.Initializer.Initialize(true, false);
            DependencyService.Register<MockDataStore>();
            MainPage = new Dashboard();
            GetLastSelectedColor();
        }

        protected async override void OnStart()
        {
            InitializeAppCenter();
            //await CheckPermissions();
            StartAutoConnectingInBackground();
            await PreloadButtonPressGif();
        }

        private void InitializeAppCenter()
        {
            try
            {
                AppCenter.Start("ios=e2569202-911f-4835-a814-021b915a8f85;" +
              "uwp={Your UWP App secret here};" +
              "android=3e47ab16-0579-400b-9f56-76d887605dac",
              typeof(Analytics), typeof(Crashes));
            }
            catch (Exception)
            {
                //if (ex != null)
                //{
                //    UserDialogs.Instance.Alert(ex.Message);
                //}
                //else
                //{
                //    UserDialogs.Instance.Alert("AppCenter Error");
                //}
            }
        }

        private async Task PreloadButtonPressGif()
        {
            try
            {
                await ImageService.Instance.LoadCompiledResource("ButtonPressGifV7.gif")
                       .DownSample(0, 0, true)
                       .Success((info, result) =>
                       {

                       })
                       .PreloadAsync();
            }
            catch (Exception)
            {

            }
        }

        private void GetLastSelectedColor()
        {
            if (Preferences.ContainsKey("SelectedColor"))
            {
                var colorStringJson = Preferences.Get("SelectedColor", null);
                if (!string.IsNullOrWhiteSpace(colorStringJson) && !colorStringJson.Equals("{\"IsDefault\":true,\"A\":0.0,\"R\":0.0,\"G\":0.0,\"B\":0.0,\"Hue\":0.0,\"Saturation\":0.0,\"Luminosity\":0.0}"))
                {
                    var colorModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ColorModel>(colorStringJson);
                    App.CurrentSelectedColor = Color.FromRgba(colorModel.R, colorModel.G, colorModel.B, colorModel.A);
                }
                else
                {
                    App.CurrentSelectedColor = Color.Red;
                }
            }
            else
            {
                App.CurrentSelectedColor = Color.Red;
            }
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {
            StartAutoConnectingInBackground();
        }

        private async Task ConnectController()
        {
            var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
            {
                locationAlwaysPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            if (locationAlwaysPermissionStatus == PermissionStatus.Disabled)
            { 
                await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
            }
            if (locationAlwaysPermissionStatus == PermissionStatus.Granted)
            {
                //Search for devices 
                //Find Light Mode Controller & Connect
                //If Controller is connected first time then Display Name Popup
                var bLEHelper = new BLEHelper();

                var devices = await bLEHelper.ScanDevices(1000, true);
                if (devices != null && devices.Count == 0)
                {
                    await bLEHelper.StopScanning();
                    bLEHelper.SendMessageToDisplayConnectButton();
                }
                else
                {
                    var isConnected = await bLEHelper.ConnectController(devices);
                    if (!isConnected)
                    {
                    }
                    else
                    {
                    }
                }
            }
        }

        /// <summary>
        /// This will be called if user has turned on the 'Auto-Connect' toggle button in settings page
        /// </summary>
        private void StartAutoConnectingInBackground()
        {
            Task.Run(async () =>
            {
                var autoConnect = Preferences.Get("auto_connect", false);

                //Get bluetooth status on App Starts
                if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS && !CrossBluetoothLE.Current.IsOn)
                {
                    var platformSpecific = DependencyService.Get<IPlatformSpecific>();
                    platformSpecific.GetBluetoothStatus();
                    await Task.Delay(500);
                }

                //Auto-Connect should not start if no default controller found
                var defaultController = await App.Database.GetDefaultController();

                if (autoConnect && defaultController != null && (CrossBluetoothLE.Current.IsOn || App.IsiOSBluetoothOn) && !App.IsScanningAlreadyGoingOn && App.Characteristic == null)
                {
                    App.ConnectedController = defaultController;

                    Preferences.Set("defaultControllerName", defaultController.Name);
                    // Auto - Connect should not start:
                    //   1.If Bluetooth is off.
                    //   2.If user did not select it in the Settings page.
                    //   3.If app is already connected to a controller.
                    //   4.Auto-Connect should not start if not start if no default controller found

                    IsScanningAlreadyGoingOn = true;
                    IsAutoScanningGoingOn = true;
                    //await ConnectController();
                    await ConnectDefaultController(new Guid(defaultController.Id));
                    IsScanningAlreadyGoingOn = false;
                    IsAutoScanningGoingOn = false;
                }
            }).ConfigureAwait(false);
        }

        private static async Task CheckPermissions()
        {
            try
            {
                if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                {

                    var networkStatePermissionStatus = await Permissions.CheckStatusAsync<Permissions.NetworkState>();
                    if (networkStatePermissionStatus != PermissionStatus.Granted)
                    {
                        var networkStateStatus = await Permissions.RequestAsync<Permissions.NetworkState>();
                    }

                    var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                    {
                        var locationAlwaysStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    } 
                }
            }
            catch (Exception)
            {

            }
        }

        private async Task ConnectDefaultController(Guid guid)
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    locationAlwaysPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                if (locationAlwaysPermissionStatus == PermissionStatus.Disabled)
                {
                    await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                }
                if (locationAlwaysPermissionStatus == PermissionStatus.Granted)
                {
                    var bLEHelper = new BLEHelper(false);
                    var isConnected = await bLEHelper.ConnectToKnownDevice(guid);
                }
            }
            else
            {
                var bLEHelper = new BLEHelper(false);
                var isConnected = await bLEHelper.ConnectToKnownDevice(guid);
            }
        }
    }
}
