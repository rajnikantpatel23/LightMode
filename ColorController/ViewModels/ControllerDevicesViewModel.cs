using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using ColorController.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class ControllerDevicesViewModel : Abstractions.BaseViewModel
    {
        public ICommand DeviceTappedCommand { get; set; }

        private ObservableCollection<Controller> _devices;
        private INavigation _navigation;

        public ObservableCollection<Controller> Controllers
        {
            get { return _devices; }
            set { _devices = value; OnPropertyChanged(nameof(Controllers)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerDevicesViewModel(INavigation navigation)
        {
            DeviceTappedCommand = new Command(DeviceTapped);
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
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
        /// Thia will be called to unubscribe BLEHelper to receive connection status notifications
        /// </summary>
        internal void UnubscribeBLEHelperToReceiveConnectionStatusNotifications()
        {
            MessagingCenter.Unsubscribe<BLEHelper, string>(this, StringResource.Connection);
        }

        /// <summary>
        /// Will be called on list item tapped
        /// </summary>
        /// <param name="obj">Controller object</param>
        private async void DeviceTapped(object obj)
        {
            var controller = obj as Controller;
            await _navigation.PushAsync(new ControllerDetailPage(controller, this));
        }

        /// <summary>
        /// Will be called on page appearing
        /// </summary>
        /// <returns></returns>
        public async override Task OnPageAppering()
        {
            var controllers = await App.Database.GetControllers();
            if (controllers != null && controllers.Count > 0)
            {
                Controllers = new ObservableCollection<Controller>(controllers);
            }
        }
    }
}
