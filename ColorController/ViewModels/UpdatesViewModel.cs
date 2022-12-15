using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using ColorController.Views;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class UpdatesViewModel : Abstractions.BaseViewModel
    {
        private INavigation _navigation;

        public ICommand ItemtappedCommad { get; set; }

        private List<Item> _items;
        public List<Item> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(); }
        }

        public UpdatesViewModel(INavigation navigation)
        {
            _navigation = navigation;
            ItemtappedCommad = new Command(Itemtapped);
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
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
                        UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
                        _navigation.PopToRootAsync();
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

        /// <summary>
        /// Will be called on list item tapped
        /// </summary>
        /// <param name="obj">Selected list item object</param>
        private async void Itemtapped(object obj)
        {
            if(Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                ShowToast("Please connect to the internet and try again");
                return; 
            }

            var item = obj as Item;
            if (item.Text == "App")
            {
                await _navigation.PushAsync(new AppUpdateDetailPage(item));
            }
            else
            {
                await _navigation.PushAsync(new ControllerUpdateDetailPage(item));
            }
        }

        /// <summary>
        /// Will be called on page appearing
        /// </summary>
        /// <returns></returns>
        public override Task LoadData()
        {
            var versionName = string.Empty;
            if (App.ConnectedControllerVersion != null && App.ConnectionState == ConnectionButtonState.ShowDisconnect)
            {
                versionName = $"Version {App.ConnectedControllerVersion}";
            }
            else
            {
                versionName = "Not connected";
            }

            Items = new List<Item>
            {
                new Item { Text = "App" , Description = $"Version {Xamarin.Essentials.VersionTracking.CurrentVersion}"},
                new Item { Text = "Controller" , Description = versionName},
            };

            return base.LoadData();
        } 
    }
}
