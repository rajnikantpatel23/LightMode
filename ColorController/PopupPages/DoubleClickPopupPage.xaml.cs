using ColorController.Controls;
using ColorController.Services;
using ColorController.ViewModels;
using ColorController.Views;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoubleClickPopupPage : BasePopupPage
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        private CancellationToken _cancellationToken;
        bool _runTimer = true;
        int _timeSpend = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public DoubleClickPopupPage(CancellationToken cancellationToken)
        {
            InitializeComponent();
            BindingContext = this;
            _cancellationToken = cancellationToken;
            _runTimer = true;
            StartSearchingTimer();
        }

        /// <summary>
        /// Will be called when Ok button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void OkTapped(object sender, EventArgs e)
        {
            try
            {
                var dashboard = App.Current.MainPage as Views.Dashboard;
                var extendedTabbedPage = (ExtendedTabbedPage)dashboard;
                var page = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(FavoritesPage));
                var favoritesViewModel = page.BindingContext as FavoritesViewModel;
                //favoritesViewModel.ScanAndConnectDevice();

            }
            catch (Exception)
            {

            }
            finally
            {
                await Navigation.PopPopupAsync();
            }
        }

        /// <summary>
        /// Will be called when Cancel button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void CancelTapped(object sender, EventArgs e)
        {
            _runTimer = false;
            await Navigation.PopPopupAsync();
            await BlueToothService.StopScanning();
        }

        protected override void OnDisappearing()
        {
            App.CancellationTokenSource.Cancel();
            base.OnDisappearing();
        }

        public void StartSearchingTimer()
        {
            Task.Run(() =>
            {
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    //Stoptimer if token cancellation requested
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    _timeSpend++;

                    //If 40 seconds over but still App is not connected then stop searching & close Button press popup page (if open)
                    if (_timeSpend == 40)
                    {
                        _timeSpend = 0;
                        _runTimer = false;

                        if (Navigation.NavigationStack.Count > 0)
                        {
                            Navigation.PopPopupAsync();
                        }

                        BlueToothService.StopScanning();

                        //Open WatchVideoLinkPopupPage page
                        PopupNavigation.Instance.PushAsync(new WatchVideoLinkPopupPage());
                    }

                    return _runTimer;
                });
            });
        }

        private void CachedImage_Success(object sender, FFImageLoading.Forms.CachedImageEvents.SuccessEventArgs e)
        {
            Debug.WriteLine(Enum.GetName(e.LoadingResult.GetType(), e.LoadingResult));
        }
    }
}