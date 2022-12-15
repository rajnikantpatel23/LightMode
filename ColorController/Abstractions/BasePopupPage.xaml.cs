using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace ColorController.Abstractions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BasePopupPage : PopupPage
    { 
        public BasePopupPage()
        {
            InitializeComponent();
        }          
    }
}