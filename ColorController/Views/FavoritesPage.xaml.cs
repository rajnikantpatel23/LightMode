using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Controls;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.Services;
using ColorController.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoritesPage : BaseContentPage
    {
        private IPlatformSpecific _platformSpecific = DependencyService.Get<IPlatformSpecific>();
        private bool _isClicked;
        private FavoriteAnimation _lastSelectedItem;
        private FavoritesViewModel _viewModel;
        int _delay = 50;

        /// <summary>
        /// Constructor
        /// </summary>
        public FavoritesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new FavoritesViewModel();
            //HorizontalListView.ItemsSource = _viewModel.Animations;
            //if (Device.Idiom==TargetIdiom.Phone)
            //{
            //    HorizontalListView.ListLayout = Sharpnado.HorizontalListView.RenderedViews.HorizontalListViewLayout.Vertical;
            //}
            //else
            //{
            //    HorizontalListView.ListLayout = Sharpnado.HorizontalListView.RenderedViews.HorizontalListViewLayout.Grid;
            //}

            //_viewModel = BindingContext as FavoritesViewModel;
        }

        /// <summary>
        /// Will be called when animation tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void AnimationTapped(object sender, EventArgs e)
        {
            string selectedAnimationVersion = null;

            try
            {
                if (!_isClicked)
                {
                    _isClicked = true;
                    var tappedEventArgs = e as TappedEventArgs;
                    var selectedAnimation = tappedEventArgs.Parameter as FavoriteAnimation;
                    DisplayBlueBorderAroundSelectedAnimation(selectedAnimation);
                    DeselectAnimationFromAnimationPageList();
                    await Play(selectedAnimation);

                    //if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                    //{
                    //    selectedAnimationVersion = Constants.GetAnimations().FirstOrDefault(x => x.AnimationType == selectedAnimation.AnimationType).ControllerVersion;

                    //    var isAnimationVersionParsed = Version.TryParse(selectedAnimationVersion, out Version animationControllerVersion);
                    //    var isControllerVersionParsed = Version.TryParse(App.ConnectedControllerVersion, out Version connectedControllerVersion);

                    //    if (isAnimationVersionParsed && isControllerVersionParsed)
                    //    {
                    //        if (connectedControllerVersion >= animationControllerVersion)
                    //        {
                    //            await Play(selectedAnimation);
                    //        }
                    //        else
                    //        {
                    //            if (!_platformSpecific.IsActivityFinishing())
                    //            {
                    //                await PopupNavigation.Instance.PushAsync(new UpdateControllerConfirmationPopUpPage(Navigation));
                    //            }
                    //        }

                    //        await Task.Delay(500);
                    //    }
                    //    else
                    //    {
                    //        Microsoft.AppCenter.Crashes.Crashes.TrackError(null, new Dictionary<string, string>
                    //        {
                    //            { "FavoritePage: AnimationTapped() Not able to parse version", null },
                    //            { "Selected Animation Version", $"{selectedAnimationVersion}" },
                    //            { "Connected Controller Version", $"{App.ConnectedControllerVersion}" },
                    //        });
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "FavoritePage: AnimationTapped()", $"{ex.StackTrace}" },
                    { "Selected Animation Version", $"{selectedAnimationVersion}" },
                    { "Connected Controller Version", $"{App.ConnectedControllerVersion}" },
                });
            }
            finally
            {
                _isClicked = false;
            }
        }

        private void DisplayBlueBorderAroundSelectedAnimation(FavoriteAnimation selectedAnimation)
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

        private async Task Play(FavoriteAnimation selectedAnimation)
        {
            try
            {
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
        /// Will be called to deselect selected animation from Animation page list
        /// </summary>
        private void DeselectAnimationFromAnimationPageList()
        {
            var dashboard = App.Current.MainPage as Dashboard;
            if (dashboard != null)
            {
                var extendedTabbedPage = dashboard as ExtendedTabbedPage;
                if (extendedTabbedPage != null)
                {
                    var page = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(NavigationPage) && ((NavigationPage)x).RootPage.GetType() == typeof(AnimationPage));
                    if (page != null)
                    {
                        var viewModel = ((NavigationPage)page).RootPage.BindingContext as AnimationViewModel;
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
        /// Will be called when delete icon tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void DeleteIconTapped(object sender, EventArgs e)
        {
            try
            {
                var tappedEventArgs = e as Xamarin.Forms.TappedEventArgs;
                var selectedFavoriteAnimation = tappedEventArgs.Parameter as FavoriteAnimation;
                _viewModel.Animations.Remove(selectedFavoriteAnimation);
                HorizontalListView.ItemsSource = _viewModel.Animations;
                _viewModel.DisplayBottomMessage();
                await App.Database.DeleteFavoriteAnimationAsync(selectedFavoriteAnimation);
            }
            catch (Exception)
            {

            }
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
                    //colectionViewLayout.Span = 2;
                    //colectionViewLayout.HorizontalItemSpacing = 0;
                    HorizontalListView.ColumnCount = 2;
                    GridContainer.Style = (Style)App.Current.Resources["GridMarginStyle"];
                }
                else
                {
                    //colectionViewLayout.Span = 1;
                    HorizontalListView.ColumnCount = 2;
                    GridContainer.Style = (Style)App.Current.Resources["GridMarginStylePortrait"];
                }
            }
        }
    }
}