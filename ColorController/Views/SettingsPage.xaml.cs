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
            var autoConnect = Preferences.Get("search_in_background", false);
            switchControl.SetSwitchStatus(autoConnect);
            _viewModel.SwitchStatus = autoConnect;
        }

        /// <summary>
        /// Will be called when auto-connect tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void AutoConnectTapped(object sender, System.EventArgs e)
        {
            switchControl.SetSwitchStatus(!switchControl.SwitchStatus);
            _viewModel.SwitchStatus = switchControl.SwitchStatus;
            Preferences.Set("search_in_background", _viewModel.SwitchStatus);
        }
    }
}