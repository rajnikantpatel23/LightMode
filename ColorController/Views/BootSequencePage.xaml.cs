using ColorController.Abstractions;
using ColorController.Models;
using ColorController.ViewModels;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BootSequencePage : BaseContentPage
    {
        private BootSequenceViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public BootSequencePage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new BootSequenceViewModel();
        }

        /// <summary>
        /// Will be called when Boot Sequence tapped
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void BootSequenceTapped(object sender, System.EventArgs e)
        {
            var tappedEventArgs = e as Xamarin.Forms.TappedEventArgs;
            var selectedItem = tappedEventArgs.Parameter as Item;
            foreach (var item in _viewModel.Items)
            {
                item.IsSelected = false;
            }
            selectedItem.IsSelected = true;
        }

        private async void BackButtonTapped(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as BootSequenceViewModel;
            viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }
    }
}