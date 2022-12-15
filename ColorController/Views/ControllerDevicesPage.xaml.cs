using ColorController.Abstractions;
using ColorController.Models;
using ColorController.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControllerDevicesPage : BaseContentPage
    {
        private ControllerDevicesViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerDevicesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel =  new ControllerDevicesViewModel(Navigation);
        }

        /// <summary>
        /// Will be called when Device tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void DeviceTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as Xamarin.Forms.TappedEventArgs;
            _viewModel.DeviceTappedCommand.Execute((Controller)tappedEventArgs.Parameter);
        }

        private async void BackButtonTapped(object sender, EventArgs e)
        {
            var viewModel = BindingContext as ControllerDevicesViewModel;
            viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }
    }
}