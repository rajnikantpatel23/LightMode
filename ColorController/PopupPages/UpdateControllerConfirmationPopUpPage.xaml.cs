using ColorController.Models;
using ColorController.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateControllerConfirmationPopUpPage : BasePopupPage
    {
        private INavigation _navigation;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateControllerConfirmationPopUpPage(Xamarin.Forms.INavigation navigation)
        {
            InitializeComponent();
            _navigation = navigation;
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
                await _navigation.PushAsync(new ControllerUpdateDetailPage(new Item { Text = "Update Controller"}));
                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
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