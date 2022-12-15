using ColorController.Services;
using ColorController.Views;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterSSIDPasswordPopupPage : BasePopupPage
    { 
        private bool _isCanceButtonlVisible;
        public bool IsCanceButtonlVisible
        {
            get { return _isCanceButtonlVisible; }
            set { _isCanceButtonlVisible = value; OnPropertyChanged(nameof(IsCanceButtonlVisible)); }
        }

        private string _ssid;
        public string SSID
        {
            get { return _ssid; }
            set { _ssid = value; OnPropertyChanged(nameof(SSID)); }
        }
        
        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        } 

        private ControllerUpdateDetailPage _controllerUpdateDetailPage;

        public EnterSSIDPasswordPopupPage(ControllerUpdateDetailPage controllerUpdateDetailPage)
        {
            InitializeComponent();
            BindingContext = this;
            _controllerUpdateDetailPage = controllerUpdateDetailPage;
            try
            {
                var ssid = Xamarin.Forms.DependencyService.Get<IPlatformSpecific>().GetCurrentWiFi();
                if (!string.IsNullOrWhiteSpace(ssid))
                {
                    SSID = ssid;
                }
            }
            catch (Exception)
            {
                 
            }
        }

        private async void CancelTapped(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }

        private async void OkTapped(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SSID) || string.IsNullOrWhiteSpace(Password))
            {
                await PopupNavigation.Instance.PushAsync(new AlertPopupPage("Please enter wifi SSID/Password."));
                return;
            }

            if (SSID.Contains("5G"))
            {
                await PopupNavigation.Instance.PushAsync(new FiveGNetworkAlertPopupPage(this));
            }
            else
            {
                await UpdateFirmfare();
            }
        }

        public async Task UpdateFirmfare()
        {
            if (!string.IsNullOrWhiteSpace(SSID) && !string.IsNullOrWhiteSpace(Password))
            {
                if (_controllerUpdateDetailPage != null)
                {
                    await PopupNavigation.Instance.PopAsync();
                    //_controllerUpdateDetailPage.UpdateFirmware(SSID, Password);
                    await PopupNavigation.Instance.PushAsync(new UpdatingPopupPage(SSID, Password, _controllerUpdateDetailPage.Navigation));
                }
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new AlertPopupPage("Please enter wifi SSID/Password."));
            }
        }
    }
}