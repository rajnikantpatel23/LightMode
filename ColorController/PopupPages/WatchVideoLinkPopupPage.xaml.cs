using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WatchVideoLinkPopupPage : BasePopupPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WatchVideoLinkPopupPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OkClicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void ClickHereLinkTapped(object sender, System.EventArgs e)
        {
            Xamarin.Essentials.Browser.OpenAsync("https://lightmodehelmets.com/quick-connect");
        }
    }
}