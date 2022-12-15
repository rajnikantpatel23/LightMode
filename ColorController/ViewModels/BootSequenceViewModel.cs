using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class BootSequenceViewModel : Abstractions.BaseViewModel
    {
        private List<Item> _items;
        public List<Item> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(nameof(Items)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BootSequenceViewModel()
        {
            Items = new List<Item>
            {
               new Item{ Text = "BOOT SEQUENCE 1"},
               new Item{ Text = "BOOT SEQUENCE 2", IsSelected = true},
               new Item{ Text = "BOOT SEQUENCE 3"},
               new Item{ Text = "NO BOOT SEQUENCE"},
            };
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
    }
}
