using Acr.UserDialogs;
using BLESample1.Views;
using ColorController.Abstractions;
using ColorController.Helpers;
using ColorController.ViewModels;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : BaseContentPage
    {
        private SettingsViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SettingsViewModel(Navigation);
            SetSwitchControlStatus();
        }

        /// <summary>
        /// Set true/false to turn on/off the switch control
        /// </summary>
        private void SetSwitchControlStatus()
        {
            var autoConnect = Preferences.Get("auto_connect", false);
            switchControl.SetSwitchStatus(autoConnect);
            _viewModel.SwitchStatus = autoConnect;
        }

        /// <summary>
        /// Will be called when auto-connect tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void AutoConnectTapped(object sender, System.EventArgs e)
        {
            switchControl.SetSwitchStatus(!switchControl.SwitchStatus);
            _viewModel.SwitchStatus = switchControl.SwitchStatus;
            Preferences.Set("auto_connect", _viewModel.SwitchStatus);
            await StopAutoScanSearching();
        }

        /// <summary>
        /// Below method will be called to stop Auto Scanning process if user turn off the Auto-Connect toggle button
        /// </summary>
        /// <returns>Task</returns>
        private async Task StopAutoScanSearching()
        {
            if (!_viewModel.SwitchStatus && App.IsAutoScanningGoingOn)
            {
                App.IsAutoScanningGoingOn = false;

                var bleHelper = new BLEHelper(false);
                await bleHelper.StopScanning();
                bleHelper.SendMessageToDisplayConnectButton();
            }
        }

        private void OpenTestPage(object sender, System.EventArgs e)
        {
            if (App.Characteristic == null)
            {
                UserDialogs.Instance.Alert("Please Connect Controller!");
            }
            else
            {
                Navigation.PushAsync(new ControllerUpdatePage());
            }
        }

        private void OpenMaskingPage(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new ThomasPOC.Views.MaskingPage());

        }
    }
}