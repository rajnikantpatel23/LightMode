using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BasePopupPage : PopupPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BasePopupPage()
        {
            InitializeComponent();
        }  
    }
}