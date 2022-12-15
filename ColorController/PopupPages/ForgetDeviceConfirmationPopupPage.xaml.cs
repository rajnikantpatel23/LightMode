using ColorController.Models;
using ColorController.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgetDeviceConfirmationPopupPage : BasePopupPage
    {
        private Controller _controller;
        private ControllerDevicesViewModel _controllerDevicesViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ForgetDeviceConfirmationPopupPage(Controller controller, ControllerDevicesViewModel controllerDevicesViewModel)
        {
            InitializeComponent();
            _controller = controller;
            _controllerDevicesViewModel = controllerDevicesViewModel;
        }

        /// <summary>
        /// Will be called when Yes button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void YesTapped(object sender, EventArgs e)
        {
            try
            {
                _controllerDevicesViewModel.Controllers.Remove(_controller);
                var result = await App.Database.DeleteController(_controller);
                if (result == 0)
                {
                    _controllerDevicesViewModel.Controllers.Add(_controller);
                }
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
                await Navigation.PopAsync();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Will be called when No button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void NoTapped(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }
    }
}