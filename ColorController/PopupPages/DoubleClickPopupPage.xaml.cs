using ColorController.Controls;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.ViewModels;
using ColorController.Views;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DoubleClickPopupPage : BasePopupPage
    {
        bool _runTimer = true;
        int _timeSpend = 0;

        private string _dottedText;
        public string DottedText
        {
            get { return _dottedText; }
            set { _dottedText = value; OnPropertyChanged(nameof(DottedText)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DoubleClickPopupPage()
        {
            InitializeComponent();
            BindingContext = this;
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
           
            new BLEHelper(false).SendMessageToDisplayConnectButton();
        }

        public void StartSearchingTimer()
        {
            Task.Run(() =>
            {
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    //Stoptimer if any CONNECT button is visible
                    if (App.ConnectionState == ConnectionButtonState.ShowConnect)
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
                        
                        var bleHelper = new BLEHelper(false);
                        bleHelper.StopScanning();

                        if (App.ConnectionState != ConnectionButtonState.ShowDisconnect)
                        {
                            //Send Message to display 'Connect' button
                            bleHelper.SendMessageToDisplayConnectButton();
                            //Open WatchVideoLinkPopupPage page
                            PopupNavigation.Instance.PushAsync(new WatchVideoLinkPopupPage());
                        }
                    }

                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        switch (DottedText)
                        {
                            case "":
                                DottedText = ".";
                                break;

                            case ".":
                                DottedText = "..";
                                break;

                            case "..":
                                DottedText = "...";
                                break;

                            case "...":
                                DottedText = "";
                                break;

                            default:
                                DottedText = "";
                                break;
                        }
                    });

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