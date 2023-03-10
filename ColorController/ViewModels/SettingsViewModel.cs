using ColorController.Helpers;
using ColorController.StringResources;
using ColorController.Views;
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

        public ICommand BootSequenceTappedCommand { get; set; }
        public ICommand DevicesTappedCommand { get; set; }
        public ICommand UpdatesTappedCommand { get; set; }

        private INavigation _navigation;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsViewModel(INavigation navigation)
        {
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();

            BootSequenceTappedCommand = new Command(BootSequenceTapped);
            DevicesTappedCommand = new Command(DevicesTapped);
            UpdatesTappedCommand = new Command(UpdatesTapped);
            _navigation = navigation;
        }

        /// <summary>
        /// Call this method to receive connection notifications from BLEHelper 
        /// </summary>
        private void SubscribeBLEHelperToReceiveConnectionStatusNotifications()
        {
            MessagingCenter.Unsubscribe<BLEHelper, string>(this, StringResource.Connection);
            MessagingCenter.Subscribe<BLEHelper, string>(this, StringResource.Connection, (sender, args) =>
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
