using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class ControllerUpdateDetailViewModel : BaseViewModel
    {
        private Item _item;
        public Item Item
        {
            get { return _item; }
            set { _item = value; OnPropertyChanged(nameof(Item)); }
        }

        private string _displayText;
        public string DisplayText
        {
            get { return _displayText; }
            set { _displayText = value; OnPropertyChanged(nameof(DisplayText)); }
        }

        private bool _updateButtonVisibility;
        public bool UpdateFirmwareButtonVisibility
        {
            get { return _updateButtonVisibility; }
            set { _updateButtonVisibility = value; OnPropertyChanged(nameof(UpdateFirmwareButtonVisibility)); }
        }

        private bool _updateAppButtonVisibility;
        public bool UpdateAppButtonVisibility
        {
            get { return _updateAppButtonVisibility; }
            set { _updateAppButtonVisibility = value; OnPropertyChanged(nameof(UpdateAppButtonVisibility)); }
        }

        Views.ControllerUpdateDetailPage _controllerUpdateDetailPage; 
        public ControllerUpdateDetailViewModel(Item item, Views.ControllerUpdateDetailPage controllerUpdateDetailPage)
        {
            Item = item;
            _controllerUpdateDetailPage = controllerUpdateDetailPage;
            UpdateAppButtonVisibility = false;
            UpdateFirmwareButtonVisibility = false;
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
                    case "ShowConnect":
                        DisplayTextToPairAppWitController();
                        break;
                    case "ShowDisconnect":
                        UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
                        _controllerUpdateDetailPage.Navigation.PopToRootAsync();
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

        public async override Task LoadData()
        {
            await DisplayTextBasedOnStatus();
        }
        
        private async Task DisplayTextBasedOnStatus()
        {
            switch (App.ConnectionState)
            {
                case ConnectionButtonState.ShowConnect:
                    DisplayTextToPairAppWitController();
                    break;

                case ConnectionButtonState.ShowDisconnect:
                    await DisplayTextBasedOnAppAndFirmwareVersion();
                    break;
                default:
                    break;
            }
        }
        
        private void DisplayTextToPairAppWitController()
        {
            DisplayText = "Please pair your controller to the app to check if a firmware update is available.";
            UpdateAppButtonVisibility = false;
            UpdateFirmwareButtonVisibility = false;
        }

        private async Task DisplayTextBasedOnAppAndFirmwareVersion()
        {
            var isLatestAppVersion = await CommonUtils.GetLatestAppVersionNumber();
            if (!isLatestAppVersion)
            {
                DisplayText = "There may be a new firmware version available. Update the LightMode app and repair to your controller to find out.";
                UpdateAppButtonVisibility = true;
                UpdateFirmwareButtonVisibility = false;
            }
            else
            {
                UpdateAppButtonVisibility = false;
                if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion) && (new Version(App.ConnectedControllerVersion) < App.LatestFirmwareVersion))
                {
                    DisplayText = "A new firmware update is available!";
                    UpdateFirmwareButtonVisibility = true;
                }
                else
                {
                    DisplayText = "You have the latest firmware installed on your controller. Continue being awesome.";
                    UpdateFirmwareButtonVisibility = false;
                }
            } 
        } 
    }
}
//Notes:
//Condition-1: If App version is latest and firmware version is latest
//"You have the latest firmware installed on your controller.
//Continue being awesome."
//hide update button

//Condition-2: If App version is latest and firmware version is old.
//"A new firmware update is available!"
//Display update button
