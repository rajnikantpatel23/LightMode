using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FiveGNetworkAlertPopupPage : BasePopupPage
    {
        private BasePopupPage _basePopupPage;

        /// <summary>
        /// Constructor EnterWifiPasswordPopupPage
        /// </summary>
        public FiveGNetworkAlertPopupPage(BasePopupPage basePopupPage)
        {
            InitializeComponent();
            BindingContext = this;
            _basePopupPage = basePopupPage;
        }

        private async void OkClicked(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            if (_basePopupPage.GetType()==typeof(AvailableWifiPopupPage))
            {
                var availableWifiPopupPage = (AvailableWifiPopupPage)_basePopupPage;
                await availableWifiPopupPage.NavigateToNextPage(availableWifiPopupPage.SelectedSSID);
            }
            if (_basePopupPage.GetType() == typeof(EnterSSIDPasswordPopupPage))
            {
                ((EnterSSIDPasswordPopupPage)_basePopupPage).UpdateFirmfare();
            }
        }

        private async void CancelTapped(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}