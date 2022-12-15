using Acr.UserDialogs;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.StringResources;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class ControllerDetailViewModel : Abstractions.BaseViewModel
    {
        private bool _isDefault;
        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; OnPropertyChanged(nameof(IsDefault)); }
        }

        private Controller _controller;
        private ControllerDevicesViewModel _controllerDevicesViewModel;

        public ICommand ForgetDeviceBtnTappedCommand { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controller">Controller object</param>
        public ControllerDetailViewModel(Controller controller, ControllerDevicesViewModel controllerDevicesViewModel)
        {
            if (controller.Name.Length <= 7)
            {
                Title = controller.Name;
            }
            else
            {
                var name = controller.Name.Substring(0, 5);
                Title = $"{name}...";
            }
            IsDefault = controller.IsDefault;
            _controller = controller;
            _controllerDevicesViewModel = controllerDevicesViewModel;
            ForgetDeviceBtnTappedCommand = new Command(ForgetDeviceBtnTapped);
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
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

        public async Task ChangeSwitchStatus()
        {
            try
            {
                UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                var defaultController = await App.Database.GetDefaultController();
                if (IsDefault && defaultController!=null)
                {
                    //Make previous controller non default
                    defaultController.IsDefault = false;
                    await App.Database.UpdateController(defaultController);
                }

                //Make current controller as Default
                _controller.IsDefault = IsDefault;
                var result = await App.Database.UpdateController(_controller);
            }
            catch (Exception)
            {
                 
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Will be called when forget device button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void ForgetDeviceBtnTapped(object obj)
        {            
            await OpenPopupPage(new ForgetDeviceConfirmationPopupPage(_controller, _controllerDevicesViewModel));
        }
    }
}
