using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.StringResources;
using ColorController.Views;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using Sharpnado.HorizontalListView.Paging;
using Sharpnado.HorizontalListView.Services;
using Sharpnado.HorizontalListView.ViewModels;
using Sharpnado.Presentation.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class FavoritesViewModel : BaseViewModel
    {
        private bool _firstAnimationAdded;
        private const int _pageSize = 10;

        public Paginator<FavoriteAnimation> AnimationPaginator { get; }
        public TaskLoaderNotifier<IReadOnlyCollection<FavoriteAnimation>> AnimationLoaderNotifier { get; }

        public ICommand TapCommand { get; set; }
        public ICommand DragAndDropEndedCommand { get; set; }
       
        public List<AnimationModel> _animationList;

        private ObservableRangeCollection<FavoriteAnimation> _animations;
        public ObservableRangeCollection<FavoriteAnimation> Animations
        {
            get { return _animations; }
            set { _animations = value; OnPropertyChanged(nameof(Animations)); }
        }
        
        private bool _isBottomMessageVisible;
        public bool IsBottomMessageVisible
        {
            get { return _isBottomMessageVisible; }
            set { _isBottomMessageVisible = value; OnPropertyChanged(nameof(IsBottomMessageVisible)); }
        }
         
        /// <summary>
        /// Constructor
        /// </summary>
        public FavoritesViewModel()
        {
            TapCommand = new Command(ItemTapped);
            DragAndDropEndedCommand = new Command(DragAndDropEnded);
            Animations = new ObservableRangeCollection<FavoriteAnimation>();
            SubscribeAnimationPageToRceiveAnimations();
            AnimationPaginator = new Paginator<FavoriteAnimation>(
                LoadAnimationsAsync,
                pageSize: _pageSize,
                loadingThreshold: 0.25f);
            AnimationLoaderNotifier = new TaskLoaderNotifier<IReadOnlyCollection<FavoriteAnimation>>();
            StartTimerToRunBackgroundTask();
        }

        private void Load()
        {
            Animations.Clear();
            AnimationLoaderNotifier.Load(async () => (await AnimationPaginator.LoadPage(1)).Items);
        }

        private async Task<PageResult<FavoriteAnimation>> LoadAnimationsAsync(int pageNumber, int pageSize, bool isRefresh)
        {
            UserDialogs.Instance.ShowLoading();
            var resultPage = await GetAnimationsPage(pageNumber, pageSize);
            Animations.AddRange(resultPage.Items);
            UserDialogs.Instance.HideLoading();
            return resultPage;
        }

        private async Task<PageResult<FavoriteAnimation>> GetAnimationsPage(int pageNumber, int pageSize)
        {
            var animations = await App.Database.GetFavoriteAnimationsAsync();
            var pageResult = new PageResult<FavoriteAnimation>(
                 animations.Count,
                 animations.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
            
            return pageResult;
        }

        private void SubscribeAnimationPageToRceiveAnimations()
        {
            MessagingCenter.Unsubscribe<AnimationPage, FavoriteAnimation>(this, StringResource.AddFavorite);
            MessagingCenter.Subscribe<AnimationPage, FavoriteAnimation>(this, StringResource.AddFavorite, (sender, args) =>
            {
                if (args!=null)
                { 
                    if (Animations.Count == 0)
                    {
                        _firstAnimationAdded = true;
                    }
                    Animations.Add(args);
                    DisplayBottomMessage();
                }
            });
        }

        /// <summary>
        /// Below method will be called to display message at bottom of the screen 
        /// Message: Click on the Animation tab ( <animation_icon> ) to start adding favorites.
        /// </summary>
        internal void DisplayBottomMessage()
        {
            if (BlueToothService.IsAppConnectedWithDevice && (Animations == null || Animations.Count == 0))
            {
                IsBottomMessageVisible = true;
            }
            else
            {
                IsBottomMessageVisible = false;
            }
        }

        /// <summary>
        /// Will be call just after item is dragged to sort Index of items & update in Local DB
        /// </summary>
        /// <param name="obj">DragAndDropInfo object</param>
        private async void DragAndDropEnded(object obj)
        {
            try
            {
                var favoriteAnimations = Animations.Select((favoriteAnimation, index) => new FavoriteAnimation
                {
                    Index = index,
                    Id = favoriteAnimation.Id,
                    Title = favoriteAnimation.Title,
                    Detail = favoriteAnimation.Detail,
                    AnimationType = favoriteAnimation.AnimationType,
                    BaseColorJson = favoriteAnimation.BaseColorJson,
                    IsFavorite = true,
                    IsSelected = false
                }).ToList();

                await App.Database.UppdateFavoriteAnimations(favoriteAnimations);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Will be call when list item tapped
        /// </summary>
        /// <param name="obj">DragAndDropInfo object</param>
        private void ItemTapped(object obj)
        {

        }

        public override Task LoadData()
        { 
            Load();
            return base.LoadData();
        }

        public override Task OnPageAppering()
        {
            if (_firstAnimationAdded)
            {
                _firstAnimationAdded = false;
                Load();
            }
            return base.OnPageAppering();
        }

        private void StartTimerToRunBackgroundTask()
        {
            try
            {               
                Device.StartTimer(TimeSpan.FromSeconds(10), () =>
                {
                    try
                    {
                        var message = Preferences.Get("search_in_background", false) ? 
                                        nameof(MessageType.StartLongRunningTaskMessage) : 
                                        nameof(MessageType.StopLongRunningTaskMessage);

                        MessagingCenter.Send<object>(this, message);
                        
                        Task.Run(() =>
                        {
                            //awaitable task
                        });
                    }
                    catch (Exception)
                    {
                        CommonUtils.WriteLog("Exception: StartTimerToRunBackgroundTask() Inside Device.StartTimer()");
                    }

                    return true;
                });
            }
            catch (Exception ex)
            {
                CommonUtils.WriteLog("Exception: StartTimerToRunBackgroundTask()");
            }
        }
    }
}
