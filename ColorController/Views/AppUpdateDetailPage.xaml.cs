using ColorController.Abstractions;
using ColorController.Models;
using ColorController.ViewModels;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppUpdateDetailPage : BaseContentPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AppUpdateDetailPage(Item item)
        {
            InitializeComponent();
            BindingContext = new AppUpdateDetailViewModel(item);
        }

        private async void BackButtonTapped(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as AppUpdateDetailViewModel;
            viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }
    }
}