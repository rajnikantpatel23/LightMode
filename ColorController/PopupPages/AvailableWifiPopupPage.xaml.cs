using ColorController.Models;
using ColorController.Services;
using ColorController.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AvailableWifiPopupPage : BasePopupPage
    {
        private bool _continueWifiScanning = true;
        private IWifiConnector _wifiConnector;

        private ObservableCollection<WifiNetwork> _wifiNetworks;
        public ObservableCollection<WifiNetwork> WifiNetworks
        {
            get { return _wifiNetworks; }
            set { _wifiNetworks = value; OnPropertyChanged(nameof(WifiNetworks)); }
        }

        public string SelectedSSID { get; set; }

        private ControllerUpdateDetailPage _controllerUpdateDetailPage;

        public AvailableWifiPopupPage(ControllerUpdateDetailPage controllerUpdateDetailPage)
        {
            InitializeComponent();
            BindingContext = this;
            _controllerUpdateDetailPage = controllerUpdateDetailPage;
            _wifiConnector = DependencyService.Get<IWifiConnector>();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await GetListOfWifiNetworksNearby();
        }

        private async Task GetListOfWifiNetworksNearby()
        {
            var wifiList = await _wifiConnector.WifiList();
            WifiNetworks = new ObservableCollection<WifiNetwork>(wifiList);

            _ = Task.Run(() =>
            {
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    Device.InvokeOnMainThreadAsync(async () =>
                    {
                        wifiList = await _wifiConnector.WifiList();
                        WifiNetworks = new ObservableCollection<WifiNetwork>(wifiList);
                    });

                    return _continueWifiScanning;
                });
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopWifiScanning();
        }

        private void StopWifiScanning()
        {
            _continueWifiScanning = false;
        }

        private async void CancelTapped(object sender, System.EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }

        private async void ListViewItemTapped(object sender, System.EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            var wifiNetwork = tappedEventArgs.Parameter as WifiNetwork;
            SelectedSSID = wifiNetwork.Name;
            if (SelectedSSID.Contains("5G"))
            {
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new FiveGNetworkAlertPopupPage(this));
            }
            else
            {
                await NavigateToNextPage(SelectedSSID);
            }
        }

        public async Task NavigateToNextPage(string ssid)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new EnterWifiPasswordPopupPage(_controllerUpdateDetailPage, ssid));
        }
    }
}