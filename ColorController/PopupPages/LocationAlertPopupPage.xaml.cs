using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocationAlertPopupPage : BasePopupPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationAlertPopupPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OkClicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}