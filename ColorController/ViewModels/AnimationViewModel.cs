using ColorController.Abstractions;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class AnimationViewModel : BaseViewModel
    {
        public bool _refreshList;

        private ObservableCollection<AnimationModel> _animations;
        public ObservableCollection<AnimationModel> Animations
        {
            get { return _animations; }
            set { _animations = value; OnPropertyChanged(nameof(Animations)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationViewModel()
        {
            _refreshList = true;
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
            SubscribeColorPageToReceiveMessageOnColorChange();
        }

        private void SubscribeColorPageToReceiveMessageOnColorChange()
        {
            //MessagingCenter.Unsubscribe<ColorsPage, bool>(this, StringResource.ReloadAnimation);
            //MessagingCenter.Subscribe<ColorsPage, bool>(this, StringResource.ReloadAnimation, async (sender, args) =>
            //{
            //    if (args)
            //    {
            //        Animations.Clear();
            //        //Acr.UserDialogs.UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
            //        var animations = await App.Database.GetAnimationsAsync();
            //        Animations = new ObservableCollection<AnimationModel>(animations);
            //        //Acr.UserDialogs.UserDialogs.Instance.HideLoading();
            //    }
            //});
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
        /// Will be called on page appearing
        /// </summary>
        /// <returns>Task</returns>
        public override Task OnPageAppering()
        {
            LoadAnimations();
            return base.OnPageAppering();
        }

        public void LoadAnimations()
        {
            try
            {
                if (_refreshList)
                {
                    _refreshList = false;
                    var animationList = new ObservableCollection<AnimationModel>(GetAnimations());
                    Animations = animationList;
                }
            }
            catch (Exception)
            {
            }
        }

        private List<AnimationModel> GetAnimations()
        {
            var selectedColorJson = JsonConvert.SerializeObject(App.CurrentSelectedColor);
            Preferences.Set("SelectedColor", selectedColorJson);

            var animations = Constants.GetAnimations();

            return animations;
        }
    }
}