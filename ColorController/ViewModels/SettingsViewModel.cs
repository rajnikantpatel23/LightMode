using ColorController.Helpers;
using ColorController.Services;
using ColorController.StringResources;
using ColorController.Views;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class SettingsViewModel : Abstractions.BaseViewModel
    {
        private bool _switchStatus;
        public bool SwitchStatus
        {
            get { return _switchStatus; }
            set { _switchStatus = value; OnPropertyChanged(nameof(SwitchStatus)); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        private bool _isAnimationPlaying;
        public bool IsAnimationPlaying
        {
            get { return _isAnimationPlaying; }
            set { _isAnimationPlaying = value; OnPropertyChanged(nameof(IsAnimationPlaying)); }
        }

        private string _pairNewDeviceButtonImage = "pairNewDevice.png";
        public string PairNewDeviceButtonImage
        {
            get { return _pairNewDeviceButtonImage; }
            set { _pairNewDeviceButtonImage = value; OnPropertyChanged(nameof(PairNewDeviceButtonImage)); }
        }

        public ICommand DetailsTappedCommand { get; set; }
        public ICommand BootSequenceTappedCommand { get; set; }
        public ICommand DevicesTappedCommand { get; set; }
        public ICommand UpdatesTappedCommand { get; set; }
        public ICommand PairNewDeviceCommand { get; set; }

        private INavigation _navigation;
        private bool _isClicked;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsViewModel(INavigation navigation)
        {
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();

            BootSequenceTappedCommand = new Command(BootSequenceTapped);
            DevicesTappedCommand = new Command(DevicesTapped);
            UpdatesTappedCommand = new Command(UpdatesTapped);
            DetailsTappedCommand = new Command(DetailsTapped);
            PairNewDeviceCommand = new Command(PairNewDevice);
            _navigation = navigation;
        }

        CancellationTokenSource _cancellationTokenSource; 

        private void PairNewDevice()
        {
            try
            {
                Task.Run(async () => 
                {
                    try
                    {
                        if (_isClicked)
                        {
                            _cancellationTokenSource.Cancel();
                            return;
                        }
                        _isClicked = true;

                        if (_cancellationTokenSource != null)
                        {
                            _cancellationTokenSource.Cancel();
                        }
                        _cancellationTokenSource = new CancellationTokenSource();

                        IsAnimationPlaying = true;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PairNewDeviceButtonImage = "searchingGIF2.png";
                        });

                        await BlueToothService.ScanAndConnectDevice_2(_cancellationTokenSource.Token);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            PairNewDeviceButtonImage = "pairNewDevice.png";
                        });

                        IsAnimationPlaying = false;
                        _isClicked = false;
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                 
            }
        }

        private void DetailsTapped(object obj)
        {
            IsExpanded = false;
        }

        /// <summary>
        /// Call this method to receive connection notifications from BLEHelper 
        /// </summary>
        private void SubscribeBLEHelperToReceiveConnectionStatusNotifications()
        {
            MessagingCenter.Unsubscribe<object, string>(this, StringResource.Connection);
            MessagingCenter.Subscribe<object, string>(this, StringResource.Connection, (sender, args) =>
            {
                switch (args)
                {
                    case "ShowSearching":
                        break;

                    case "ShowConnect":
                        break;

                    case "ShowConnecting":
                        break;

                    case "ShowDisconnect":
                        break;

                    case "QUIT":
                        break;

                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// Will be called on Updates tabbed
        /// </summary>
        private async void UpdatesTapped()
        {
            await _navigation.PushAsync(new UpdatesPage());
        }

        /// <summary>
        /// Will be called on Devices tabbed
        /// </summary>
        private async void DevicesTapped()
        {
            await _navigation.PushAsync(new ControllerDevicesPage());
        }

        /// <summary>
        /// Will be called on Boot Sequence tabbed
        /// </summary>
        private async void BootSequenceTapped()
        {
            await _navigation.PushAsync(new BootSequencePage());
        }
    }
}
