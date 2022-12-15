using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using Plugin.LatestVersion;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class AppUpdateDetailViewModel : BaseViewModel
    {
        private bool _isUpdatesAvailable;
        public bool IsUpdatesAvailable
        {
            get { return _isUpdatesAvailable; }
            set { _isUpdatesAvailable = value; OnPropertyChanged(nameof(IsUpdatesAvailable)); }
        }

        private bool _isUpdated;
        public bool IsUpdated
        {
            get { return _isUpdated; }
            set { _isUpdated = value; OnPropertyChanged(nameof(IsUpdated)); }
        }

        public ICommand UpdateButtonCommand { get; set; }

        private Item _item;
        public Item Item
        {
            get { return _item; }
            set { _item = value; OnPropertyChanged(); }
        }

        public AppUpdateDetailViewModel(Item item)
        {
            Item = item;
            UpdateButtonCommand = new Command(OpenStoreToUpdateApp);
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
        }

        public async override Task LoadData()
        {
            await CheckLatestVersion();
        }

        private async Task CheckLatestVersion()
        {
            try
            {
                var isLatestApp = await CommonUtils.GetLatestAppVersionNumber();
                if (!isLatestApp)
                {
                    DisplayUpdateControls(true);
                }
                else
                {
                    DisplayUpdateControls(false);
                }
            }
            catch (Exception)
            {
                DisplayUpdateControls(false);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Will be called to hide/show update controlls
        /// </summary>
        /// <param name="updatesAvailable">Should update controls visible or not</param>
        private void DisplayUpdateControls(bool updatesAvailable)
        {
            IsUpdatesAvailable = updatesAvailable;
            IsUpdated = !updatesAvailable;
        }

        /// <summary>
        /// Will be called on Update button click
        /// </summary>
        /// <param name="obj">Arguments</param>
        private async void OpenStoreToUpdateApp(object obj)
        {
            await CrossLatestVersion.Current.OpenAppInStore(AppInfo.PackageName);
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
                        //StartSearchingTimer();
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
            MessagingCenter.Unsubscribe<object, string>(this, StringResource.Connection);
        }
    }
}
