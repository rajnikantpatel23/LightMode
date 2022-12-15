using ColorController.Abstractions;
using ColorController.Models;
using ColorController.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdatesPage : BaseContentPage
    {
        private UpdatesViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdatesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new UpdatesViewModel(Navigation);
        }

        /// <summary>
        /// Will be called when list item tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void ItemTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as Xamarin.Forms.TappedEventArgs;
            _viewModel.ItemtappedCommad.Execute((Item)tappedEventArgs.Parameter);
        }

        private async void BackButtonTapped(object sender, EventArgs e)
        {
            var viewModel = BindingContext as UpdatesViewModel;
            viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }
    }
}