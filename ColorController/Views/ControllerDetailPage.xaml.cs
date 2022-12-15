using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Models;
using ColorController.ViewModels;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControllerDetailPage : BaseContentPage
    {
        private ControllerDetailViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerDetailPage(Controller controller, ControllerDevicesViewModel controllerDevicesViewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = new ControllerDetailViewModel(controller, controllerDevicesViewModel);
            //Set true to turn on the switch control
            switchControl.SetSwitchStatus(controller.IsDefault);
        }

        private async void BackButtonTapped(object sender, System.EventArgs e)
        { 
            _viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }

        private async void ToggleButtonTapped(object sender, System.EventArgs e)
        {
            switchControl.SetSwitchStatus(!switchControl.SwitchStatus);
            _viewModel.IsDefault = switchControl.SwitchStatus;
            await _viewModel.ChangeSwitchStatus();
        }
    }
}