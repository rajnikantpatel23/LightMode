using Acr.UserDialogs;
using ColorController.Models;
using ColorController.StringResources;
using Plugin.BLE.Abstractions.Contracts;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterDeviceNamePopupPage : BasePopupPage
    {
        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; OnPropertyChanged(nameof(DeviceName)); }
        }

        private IDevice _device;

        /// <summary>
        /// Constructor
        /// </summary>
        public EnterDeviceNamePopupPage(IDevice device)
        {
            InitializeComponent();
            BindingContext = this;
            _device = device;
        }
 
        /// <summary>
        /// Will be called when Ok button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void OkTapped(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeviceName))
            {
                UserDialogs.Instance.Alert(StringResource.PleaseEnterDeviceName);
            }
            else
            {
                var exsitingController = await App.Database.GetController(_device.Id.ToString());
                if (exsitingController == null)
                {
                    var newController = new Controller
                    {
                        Id = _device.Id.ToString(),
                        Name = DeviceName,
                        IsDefault = false,
                    };
                    await App.Database.SaveController(newController);
                }
                else
                {
                    exsitingController = new Controller
                    {
                        Id = _device.Id.ToString(),
                        Name = DeviceName,
                        IsDefault = false,
                    };
                    await App.Database.UpdateController(exsitingController);
                }

                App.ConnectedController = exsitingController;

                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
            }
        }
    }
}