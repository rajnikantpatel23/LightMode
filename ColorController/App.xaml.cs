using ColorController.Enums;
using ColorController.Models;
using ColorController.Services;
using ColorController.Views;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ColorController.Abstractions;
using FFImageLoading;
using System.Threading;

namespace ColorController
{
    public partial class App : Application
    {
        //Don't update until not confirm by Thomas
        public static Version LatestFirmwareVersion = new Version("3.1.14");

        public static CancellationTokenSource CancellationTokenSource { get; set; }
        
        public static void ResetCancellationToken()
        {
            if (App.CancellationTokenSource != null)
            {
                App.CancellationTokenSource.Cancel();
            }
            App.CancellationTokenSource = new CancellationTokenSource();
        }

        // Create the database connection as a singleton.
        static Database.LightModeDatabase _animationDatabase;
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
        public static bool ContinueFetchingBatteryDetail { get; internal set; }
        public static string BatterPercentages { get; internal set; }
        public static bool IsScanningAlreadyGoingOn { get; internal set; }
        //public static bool ConnectButtonClicked { get; internal set; }
        public static Color CurrentSelectedColor { get; internal set; }
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
        public static bool IsBackgroundTaskRunning { get; set; }
        public static AppState AppState { get; set; }

        public static int SIZE = 240;
       
        public App()
        {
            InitializeComponent();
            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            Sharpnado.HorizontalListView.Initializer.Initialize(true, false);
            DependencyService.Register<MockDataStore>();
            DependencyService.Register<BlueToothService>();
            MainPage = new Dashboard();
            GetLastSelectedColor();
        }

        protected async override void OnStart()
        {
            InitializeAppCenter();
            await PreloadButtonPressGif();
            AppState = AppState.Foreground;
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
            AppState = AppState.Background;
        }

        protected override void OnResume()
        {
            AppState = AppState.Foreground;
        }
    }
}
