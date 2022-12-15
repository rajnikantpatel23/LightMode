using ColorController.StringResources;
using ColorController.Views;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterWifiPasswordPopupPage : BasePopupPage
    { 
        private bool _isCanceButtonlVisible;
        public bool IsCanceButtonlVisible
        {
            get { return _isCanceButtonlVisible; }
            set { _isCanceButtonlVisible = value; OnPropertyChanged(nameof(IsCanceButtonlVisible)); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        private string _messageText;
        public string MessageText
        {
            get { return _messageText; }
            set { _messageText = value; OnPropertyChanged(nameof(MessageText)); }
        }

        private ControllerUpdateDetailPage _controllerUpdateDetailPage;
        private string _ssid;
        private bool _isClicked;

        public EnterWifiPasswordPopupPage(ControllerUpdateDetailPage controllerUpdateDetailPage, string ssid)
        {
            InitializeComponent();
            BindingContext = this;
            _ssid = ssid;            
            MessageText = "Enter wifi password.";
            _controllerUpdateDetailPage = controllerUpdateDetailPage;
        }

        private async void CancelTapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async void OkTapped(object sender, EventArgs e)
        {
            if (!_isClicked)
            {
                _isClicked = true; 
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    MessageText = StringResource.EnteWifPassword;

                    if (_controllerUpdateDetailPage != null)
                    {
                        await PopupNavigation.Instance.PopAsync();
                        //_controllerUpdateDetailPage.UpdateFirmware(_ssid, Password);
                        await PopupNavigation.Instance.PushAsync(new UpdatingPopupPage(_ssid, Password, _controllerUpdateDetailPage.Navigation));
                    }
                }
                else
                {
                    await PopupNavigation.Instance.PushAsync(new AlertPopupPage(StringResource.EnterWiFiPassword));
                } 
                
                _isClicked = false;
            }
        }
    }
}