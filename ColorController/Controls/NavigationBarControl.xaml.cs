using Acr.UserDialogs;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.PopupPages;
using ColorController.Services;
using ColorController.StringResources;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationBarControl : Grid
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>(); 

        public event EventHandler BackButtonEvent;

        public static readonly BindableProperty ImageBtnConnectSourceProperty = BindableProperty.Create(nameof(ImageBtnConnectSource), typeof(ImageSource), typeof(NavigationBarControl), null);
        public ImageSource ImageBtnConnectSource
        {
            get { return (ImageSource)GetValue(ImageBtnConnectSourceProperty); }
            set { SetValue(ImageBtnConnectSourceProperty, value); }
        }

        public static readonly BindableProperty SearchingTextProperty = BindableProperty.Create(nameof(SearchingText), typeof(string), typeof(NavigationBarControl), string.Empty);
        public string SearchingText
        {
            get { return (string)GetValue(SearchingTextProperty); }
            set { SetValue(SearchingTextProperty, value); }
        }
          
        public static readonly BindableProperty DottedTextProperty = BindableProperty.Create(nameof(DottedText), typeof(string), typeof(NavigationBarControl), string.Empty);
        public string DottedText
        {
            get { return (string)GetValue(DottedTextProperty); }
            set { SetValue(DottedTextProperty, value); }
        }

        public static readonly BindableProperty IsBluetoothOnProperty = BindableProperty.Create(nameof(IsBluetoothOn), typeof(bool), typeof(CustomButtonControl), false);
        public bool IsBluetoothOn
        {
            get { return (bool)GetValue(IsBluetoothOnProperty); }
            set
            {
                SetValue(IsBluetoothOnProperty, value);
            }
        } 
        
        public static readonly BindableProperty ImageBtnConnectIsVisibleProperty = BindableProperty.Create(nameof(ImageBtnConnectIsVisible), typeof(bool), typeof(CustomButtonControl), false);
        public bool ImageBtnConnectIsVisible
        {
            get { return (bool)GetValue(ImageBtnConnectIsVisibleProperty); }
            set
            {
                SetValue(ImageBtnConnectIsVisibleProperty, value);
            }
        }
           
        public static readonly BindableProperty ImageBatteryVisibleProperty = BindableProperty.Create(nameof(ImageBatteryVisible), typeof(bool), typeof(CustomButtonControl), false);
        public bool ImageBatteryVisible
        {
            get { return (bool)GetValue(ImageBatteryVisibleProperty); }
            set
            {
                SetValue(ImageBatteryVisibleProperty, value);
            }
        }

        public static readonly BindableProperty ShowBackButtonProperty = BindableProperty.Create(nameof(ShowBackButton), typeof(bool), typeof(CustomButtonControl), false);
        public bool ShowBackButton
        {
            get { return (bool)GetValue(ShowBackButtonProperty); }
            set { SetValue(ShowBackButtonProperty, value); }
        }

        public static readonly BindableProperty IsAnimationPlayingProperty = BindableProperty.Create(nameof(IsAnimationPlaying), typeof(bool), typeof(CustomButtonControl), false);
        public bool IsAnimationPlaying
        {
            get { return (bool)GetValue(IsAnimationPlayingProperty); }
            set { SetValue(IsAnimationPlayingProperty, value); }
        }

        public static readonly BindableProperty ButtonTextColorProperty = BindableProperty.Create(nameof(ButtonTextColor), typeof(Color), typeof(CustomButtonControl), (Color)App.Current.Resources["ButtonTextColor"]);
        public Color ButtonTextColor
        {
            get { return (Color)GetValue(ButtonTextColorProperty); }
            set { SetValue(ButtonTextColorProperty, value); }
        }

        public static readonly BindableProperty BluetoothColorProperty = BindableProperty.Create(nameof(BluetoothColor), typeof(Color), typeof(CustomButtonControl), (Color)App.Current.Resources["BluetoothOffColor"]);
        public Color BluetoothColor
        {
            get { return (Color)GetValue(BluetoothColorProperty); }
            set { SetValue(BluetoothColorProperty, value); }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(NavigationBarControl), string.Empty);
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty StatusTextProperty = BindableProperty.Create(nameof(StatusText), typeof(string), typeof(NavigationBarControl), string.Empty);
        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public static readonly BindableProperty BatteryPercentagesProperty = BindableProperty.Create(nameof(BatteryPercentages), typeof(string), typeof(NavigationBarControl), string.Empty);
        public string BatteryPercentages
        {
            get { return (string)GetValue(BatteryPercentagesProperty); }
            set { SetValue(BatteryPercentagesProperty, value); }
        }

        private bool _backButtonClicked;
        private bool _isClicked;

        public NavigationBarControl()
        {
            InitializeComponent();
            CrossBluetoothLE.Current.StateChanged += BluetoothStateChanged;
            ShowBluetoothColor(CrossBluetoothLE.Current.State);
            DisplayBatteryStatus(App.BatterPercentages);
            SubscribeBLEHelperToReceiveBatteryNotifications();
            DisplayConnectionButton();
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
            StartSearchingTimer();
        }

        private void DisplayConnectionButton()
        {
            switch (App.ConnectionState)
            {
                case ConnectionButtonState.ShowConnect:
                    ShowConnectButton();
                    break; 
                case ConnectionButtonState.ShowSearchingButton:
                    ShowConnectingButton();
                    break;
                case ConnectionButtonState.ShowSearchingText:
                    ShowConnectingText();
                    break;
                case ConnectionButtonState.ShowDisconnect:
                    ShowDisconnectButton();
                    break;
                default:
                    break;
            }
        }

        private void SubscribeBLEHelperToReceiveBatteryNotifications()
        {
            MessagingCenter.Unsubscribe<object, string>(this, StringResource.POWR);
            MessagingCenter.Subscribe<object, string>(this, StringResource.POWR, (sender, batteryPercentage) =>
            {
                DisplayBatteryStatus(batteryPercentage);
            });
        }

        /// <summary>
        /// Call this method to receive connection notifications from BLEHelper 
        /// </summary>
        private void SubscribeBLEHelperToReceiveConnectionStatusNotifications()
        {
            MessagingCenter.Unsubscribe<object, string>(this, StringResource.Connection);
            MessagingCenter.Subscribe<object, string>(this, StringResource.Connection, async (sender, args) =>
            {
                switch (args)
                {
                    case "ShowSearchingButton":
                        ShowConnectingButton();
                        HideBatteryIndicator();
                        break;

                    case "ShowSearchingText":
                        ShowConnectingText();
                        StartSearchingTimer();
                        HideBatteryIndicator();
                        break;                    
                    
                    //case "ShowSearching":
                    //    if (App.ConnectButtonClicked)
                    //    {
                    //        ShowConnectingButton();
                    //    }
                    //    else
                    //    {
                    //        ShowConnectingText();
                    //        StartSearchingTimer();
                    //    }
                    //    HideBatteryIndicator();
                    //    break;

                    //case "ShowConnecting":
                    //    if (App.ConnectButtonClicked)
                    //    {
                    //        ShowConnectingButton();
                    //    }
                    //    else
                    //    { 
                    //        ShowConnectingText();
                    //        StartSearchingTimer();
                    //    }
                    //    HideBatteryIndicator();
                    //    break;

                    case "ShowConnect":
                        ShowConnectButton();
                        HideBatteryIndicator();
                        break;

                    case "ShowDisconnect":

                        if (string.IsNullOrWhiteSpace(SearchingText))
                        {
                            SearchingText = string.Empty;
                            DottedText = string.Empty;
                            ImageBtnConnectSource = "navBtnConnected.png";
                        }
                        else
                        {
                            DottedText = string.Empty;
                            SearchingText = "CONNECTED!";
                        } 
                        await Task.Delay(1000);
                        SearchingText = string.Empty;
                        ShowDisconnectButton();
                        break;

                    case "QUIT":
                        ShowConnectButton();
                        HideBatteryIndicator();
                        break;

                    default:
                        break;
                }
            });
        }

        private void HideBatteryIndicator()
        {
            try
            {
                BatteryPercentages = string.Empty;
                //ImageBattery.Source = null;
                ImageBatteryVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Inside HideBatteryIndicator() Error:" + ex.Message);
            }
        }

        private void BluetoothStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            ShowBluetoothColor(e.NewState);
        }

        private void ShowBluetoothColor(BluetoothState bluetoothState)
        {
            ImageBtnConnectIsVisible = true;

            switch (bluetoothState)
            {
                case BluetoothState.Unknown:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                case BluetoothState.Unavailable:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                case BluetoothState.Unauthorized:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                case BluetoothState.TurningOn:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                case BluetoothState.On:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOnColor"];
                    ImageBtnConnectSource = "navBtnConnect.png";
                    break;
                case BluetoothState.TurningOff:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOnColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                case BluetoothState.Off:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
                default:
                    BluetoothColor = (Color)App.Current.Resources["BluetoothOffColor"];
                    ImageBtnConnectSource = "navBtnConnectOff.png";
                    break;
            }

            if (Device.RuntimePlatform == Device.iOS && App.IsiOSBluetoothOn)
            {
                BluetoothColor = (Color)App.Current.Resources["BluetoothOnColor"];
                ImageBtnConnectSource = "navBtnConnect.png";
            }
        }

        private void DisplayBatteryStatus(string percentage)
        {
            //UserDialogs.Instance.Alert("Percentages:" + percentage);
            if (!string.IsNullOrWhiteSpace(percentage))
            {
                var batteryPercentage = int.Parse(percentage);
                BatteryPercentages = $"{batteryPercentage}%";
                if (batteryPercentage >= 0 && batteryPercentage <= 10)
                {
                    ImageBattery.Source = "Battery10.png";
                }
                if (batteryPercentage > 10 && batteryPercentage <= 20)
                {
                    ImageBattery.Source = "Battery20.png";
                }
                if (batteryPercentage > 20 && batteryPercentage <= 30)
                {
                    ImageBattery.Source = "Battery30.png";
                }
                if (batteryPercentage > 30 && batteryPercentage <= 40)
                {
                    ImageBattery.Source = "Battery40.png";
                }
                if (batteryPercentage > 40 && batteryPercentage <= 50)
                {
                    ImageBattery.Source = "Battery50.png";
                }
                if (batteryPercentage > 50 && batteryPercentage <= 60)
                {
                    ImageBattery.Source = "Battery60.png";
                }
                if (batteryPercentage > 60 && batteryPercentage <= 70)
                {
                    ImageBattery.Source = "Battery70.png";
                }
                if (batteryPercentage > 70 && batteryPercentage <= 80)
                {
                    ImageBattery.Source = "Battery80.png";
                }
                if (batteryPercentage > 80 && batteryPercentage <= 90)
                {
                    ImageBattery.Source = "Battery90.png";
                }
                if (batteryPercentage > 90 && batteryPercentage <= 100)
                {
                    ImageBattery.Source = "Battery100.png";
                }
            }
        }

        private void BackButtonTapped(object sender, EventArgs e)
        {
            if (ShowBackButton)
            {
                _backButtonClicked = true;
                //await Navigation.PopAsync();
                BackButtonEvent?.Invoke(sender, e);
            }
        }

        private async void ConnectBtnTapped(object sender, EventArgs e)
        {
            try
            {
                var imageSource = (FFImageLoading.Forms.CachedImage)sender;
                if (imageSource.Source != null)
                {
                    var selectedImage = imageSource.Source as FileImageSource;

                    if (selectedImage.File == "navBtnConnectOff.png" && !CrossBluetoothLE.Current.IsOn)
                    {
                        await PopupNavigation.Instance.PushAsync(new AlertPopupPage(StringResource.PleaseTurnONBT));
                        return;
                    }
                    else if (selectedImage.File == "navBtnConnect.png")
                    {
                        await BlueToothService.ScanAndConnectDevice();
                    }
                    else if (selectedImage.File == "navSearchingGIF2.gif")
                    {
                        UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                        await Task.Delay(2000);
                        var bleHelper = new BLEHelper(false);
                        await bleHelper.StopScanning();
                        if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                        {
                            Debug.WriteLine("Already Connected....");
                        }
                        else
                        {
                            Debug.WriteLine("SendMessageToDisplayConnectButton()");
                            bleHelper.SendMessageToDisplayConnectButton();
                        }
                        UserDialogs.Instance.HideLoading();
                    }
                    else if (selectedImage.File == "btnDisconnectOn.png")
                    {
                        await new BLEHelper(false).Disconnect();
                    }
                }
            }
            catch (Exception)
            {
                 
            }
        }

        private void ShowDisconnectButton()
        { 
            ImageBtnConnectIsVisible = true;
            ImageBatteryVisible = true;
            ImageBtnConnectSource = "btnDisconnectOn.png";
            SearchingText = string.Empty;
            DottedText = string.Empty;
        }

        private void ShowConnectButton()
        {
            ImageBtnConnectIsVisible = true;
            IsAnimationPlaying = false;
            ImageBatteryVisible = false;
            if (CrossBluetoothLE.Current.IsOn)
            {
                ImageBtnConnectSource = "navBtnConnect.png";
            }
            else
            {
                ImageBtnConnectSource = "navBtnConnectOff.png";
            }
            SearchingText = string.Empty;
            DottedText = string.Empty;
        }

        private void ShowConnectingText()
        {
            ImageBtnConnectIsVisible = false;
            IsAnimationPlaying = false;
            ImageBatteryVisible = false;
            ImageBtnConnectSource = null;
            var defaultControllerName = Preferences.Get("defaultControllerName", string.Empty);
            SearchingText = $"Searching for {defaultControllerName}";
        }

        private void ShowConnectingButton()
        {
            if (!ImageBtnConnectIsVisible)
            {
                ImageBtnConnectIsVisible = true;
            }
            ImageBatteryVisible = false;

            var selectedImage = ImageBtnConnectSource as FileImageSource;
            if (selectedImage != null && selectedImage.File != "navSearchingGIF2.gif")
            {
                ImageBtnConnectSource = "navSearchingGIF2.gif";
            }
            if (selectedImage == null)
            {
                ImageBtnConnectSource = "navSearchingGIF2.gif";
            }

            if (!IsAnimationPlaying)
            {
                IsAnimationPlaying = true;
            }
            SearchingText = string.Empty;
            DottedText = string.Empty;
        }

        private async void DisconnectBtnTapped(object sender, EventArgs e)
        {
            await new BLEHelper(false).Disconnect();
        }

        private async void ConnectingBtnTapped(object sender, EventArgs e)
        {
            if (!_isClicked)
            {
                _isClicked = true;
                UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                await Task.Delay(2000);
                var bleHelper = new BLEHelper(false);
                await bleHelper.StopScanning();
                if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                {
                    Debug.WriteLine("Already Connected....");
                }
                else
                {
                    Debug.WriteLine("SendMessageToDisplayConnectButton()");
                    bleHelper.SendMessageToDisplayConnectButton();
                }
                UserDialogs.Instance.HideLoading();
                _isClicked = false;
            }
        }

        public void StartSearchingTimer()
        {
            if (App.ConnectionState == ConnectionButtonState.ShowSearchingText)
            {
                ImageBtnConnectIsVisible = false;

                var defaultControllerName = Preferences.Get("defaultControllerName", string.Empty);
                SearchingText = $"Searching for {defaultControllerName}";

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (App.ConnectionState != ConnectionButtonState.ShowSearchingText)
                    {
                        SearchingText = string.Empty;
                        DottedText = string.Empty;
                        return false;
                    }

                    if (!App.IsAutoScanningGoingOn)
                    {
                        SearchingText = string.Empty;
                        DottedText = string.Empty;
                        return false;
                    }

                    if (_backButtonClicked)
                    {
                        _backButtonClicked = false;
                        SearchingText = string.Empty;
                        DottedText = string.Empty;
                        return false;
                    }
                     
                    switch (DottedText)
                    {
                        case "":
                            DottedText = ".";
                            break;

                        case ".":
                            DottedText = "..";
                            break;

                        case "..":
                            DottedText = "...";
                            break;

                        case "...":
                            DottedText = "";
                            break;

                        default:
                            SearchingText = "";
                            break;
                    }
                    var value = App.ConnectionState == ConnectionButtonState.ShowSearchingText;
                    if (!value)
                    {
                        SearchingText = string.Empty;
                        DottedText = string.Empty;
                    }
                    return value;
                });
            }
        }
    }
}