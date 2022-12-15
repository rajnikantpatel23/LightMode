using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Controls;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.Services;
using ColorController.StringResources;
using ColorController.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnimationPage : BaseContentPage
    {
        private IPlatformSpecific _platformSpecific = DependencyService.Get<IPlatformSpecific>();
        private bool _isClicked;
        private AnimationModel _lastSelectedItem;
        private AnimationViewModel _viewModel;
        int _delay = 50;

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new AnimationViewModel();
        }

        /// <summary>
        /// Will be called when Animation will be selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AnimationTapped(object sender, EventArgs e)
        { 
            if (!_isClicked)
            {
                _isClicked = true;
                var tappedEventArgs = e as TappedEventArgs;
                var selectedAnimation = tappedEventArgs.Parameter as AnimationModel;
                DisplayBlueBorderAroundSelectedAnimation(selectedAnimation);
                DeselectAnimationFromFavoritePageList();

                if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                {
                    var animationControllerVersion = new Version(selectedAnimation.ControllerVersion);
                    var connectedControllerVersion = new Version(App.ConnectedControllerVersion);

                    if (connectedControllerVersion >= animationControllerVersion)
                    {
                        await Play(selectedAnimation);
                    }
                    else
                    {
                        if (!_platformSpecific.IsActivityFinishing())
                        {
                            await PopupNavigation.Instance.PushAsync(new UpdateControllerConfirmationPopUpPage(Navigation));
                        }
                    }

                    await Task.Delay(500);
                }

                _isClicked = false;
            }
        }

        private void DisplayBlueBorderAroundSelectedAnimation(AnimationModel selectedAnimation)
        {
            foreach (var item in _viewModel.Animations)
            {
                item.IsSelected = false;
            }
            if (_lastSelectedItem != null)
            {
                _lastSelectedItem.IsSelected = false;
            }

            selectedAnimation.IsSelected = !selectedAnimation.IsSelected;
            _lastSelectedItem = selectedAnimation;
        }

        private async Task Play(AnimationModel selectedAnimation)
        {
            try
            {
                //if (App.Characteristic != null && App.ConnectionState == Enums.ConnectionButtonState.ShowDisconnect)
                //{
                    #region OFF- Command
                    ////UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                    //Debug.WriteLine($"OFF Command executing.........{DateTime.Now.ToString("hh:mm:ss")}");
                    //await BlueToothService.SendCommandToController("OFF-");
                    //Debug.WriteLine($"OFF Command executed.........{DateTime.Now.ToString("hh:mm:ss")}");
                    //await Task.Delay(_delay); 
                    #endregion

                    #region OPTS Command
                    if (selectedAnimation.AnimationType != Enums.AnimationType.Technetium)
                    {
                        try
                        {
                            Debug.WriteLine($"Hue Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                            await BlueToothService.SendHueSaturationToController(selectedAnimation.SelectedColor1);
                            Debug.WriteLine($"Hue Command executed for {selectedAnimation.Title}.......{DateTime.Now.ToString("hh:mm:ss")}");
                        }
                        catch (Exception)
                        {
                            try
                            {
                                await Task.Delay(_delay);
                                Debug.WriteLine($"Exception Hue Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                                await BlueToothService.SendHueSaturationToController(selectedAnimation.SelectedColor1);
                                Debug.WriteLine($"Exception Hue Command executed for {selectedAnimation.Title}.......{DateTime.Now.ToString("hh:mm:ss")}");
                            }
                            catch (Exception)
                            {
                                await Task.Delay(_delay);
                                Debug.WriteLine($"Exception2 Hue Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                                await BlueToothService.SendHueSaturationToController(selectedAnimation.SelectedColor1);
                                Debug.WriteLine($"Exception2 Hue Command executed for {selectedAnimation.Title}.......{DateTime.Now.ToString("hh:mm:ss")}");
                            }
                        }
                    }
                    #endregion

                    #region PATT Command
                    try
                    {
                        await Task.Delay(_delay);
                        Debug.WriteLine($"PATT Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                        await BlueToothService.SendCommandToController(selectedAnimation.Command, false);
                        Debug.WriteLine($"PATT Command executed for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                    }
                    catch (Exception)
                    {
                        try
                        {
                            await Task.Delay(_delay);
                            Debug.WriteLine($"Exception PATT Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                            await BlueToothService.SendCommandToController(selectedAnimation.Command, false);
                            Debug.WriteLine($"Exception PATT Command executed for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                        }
                        catch (Exception)
                        {
                            try
                            {
                                await Task.Delay(_delay);
                                Debug.WriteLine($"Exception2 PATT Command executing for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                                await BlueToothService.SendCommandToController(selectedAnimation.Command, false);
                                Debug.WriteLine($"Exception2 PATT Command executed for {selectedAnimation.Title}.........{DateTime.Now.ToString("hh:mm:ss")}");
                            }
                            catch (Exception)
                            {
                                selectedAnimation.IsSelected = false;
                                _lastSelectedItem = null;
                            }
                        }
                    } 
                    #endregion
                //} 
            }
            catch (Exception)
            {
                selectedAnimation.IsSelected = false;
                _lastSelectedItem = null;
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        /// <summary>
        /// Will be called to deselect selected animation from Favorite page list
        /// </summary>
        private void DeselectAnimationFromFavoritePageList()
        {
            var dashboard = App.Current.MainPage as Dashboard;
            if (dashboard != null)
            {
                var extendedTabbedPage = dashboard as ExtendedTabbedPage;
                if (extendedTabbedPage != null)
                {
                    var page = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(NavigationPage) && ((NavigationPage)x).RootPage.GetType() == typeof(FavoritesPage));
                    if (page != null)
                    {
                        var viewModel = ((NavigationPage)page).RootPage.BindingContext as FavoritesViewModel;
                        if (viewModel != null)
                        {
                            if (viewModel.Animations != null && viewModel.Animations.Any())
                            {
                                viewModel.Animations.ToList().ForEach(x => x.IsSelected = false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Will be called when star icon tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void StarImageTapped(object sender, EventArgs e)
        {
            try
            {
                UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                var tappedEventArgs = e as TappedEventArgs;
                var selectedAnimation = tappedEventArgs.Parameter as AnimationModel;
                selectedAnimation.IsFavorite = !selectedAnimation.IsFavorite;
                if (selectedAnimation.IsFavorite)
                {
                    await AddAnimationToFavariteList(selectedAnimation);
                }
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
        /// This will be called when star icon is tapped to add animation in favorite list
        /// </summary>
        /// <param name="selectedColor">Selected Animation</param>
        /// <returns>Task</returns>
        private async Task AddAnimationToFavariteList(AnimationModel selectedColor)
        {
            var favoriteAnimation = new FavoriteAnimation
            {
                Id = Guid.NewGuid().ToString(),
                Command = selectedColor.Command,
                AnimationType = selectedColor.AnimationType,
                BaseColorJson = selectedColor.BaseColorJson,
                Title = selectedColor.Title,
                Detail = selectedColor.Detail,
                IsFavorite = selectedColor.IsFavorite,
                IsSelected = false,
                Version = selectedColor.Version,
                ControllerVersion = selectedColor.ControllerVersion,
                IsShieldVisible = selectedColor.IsShieldVisible,
                CreatedAt = selectedColor.CreatedAt,
                UpdatedAt = selectedColor.UpdatedAt,
            };
            MessagingCenter.Send(this, StringResource.AddFavorite, favoriteAnimation);
            await App.Database.SaveFavoriteAnimationAsync(favoriteAnimation);
        }

        /// <summary>
        /// Will be called when device orientation change
        /// </summary>
        /// <param name="width">Width of device</param>
        /// <param name="height">Height of device</param>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                if (width > height)
                { 
                    //HorizontalListView.ColumnCount = 2;
                    //GridContainer.Style = (Style)App.Current.Resources["GridMarginStyle"];
                }
                else
                { 
                    //HorizontalListView.ColumnCount = 2;
                    //GridContainer.Style = (Style)App.Current.Resources["GridMarginStylePortrait"];
                }
            }
             
            //await _viewModel.OnPageAppering();
        }

        /// <summary>
        /// Will be called when info icon tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void InfoIconTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            var selectedColor = tappedEventArgs.Parameter as AnimationModel;
            Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new AnimationInfoPopupPage(selectedColor));
        }
    }
}