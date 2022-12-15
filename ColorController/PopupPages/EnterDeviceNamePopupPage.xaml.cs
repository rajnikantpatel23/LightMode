using Acr.UserDialogs;
using ColorController.Models;
using ColorController.StringResources;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterDeviceNamePopupPage : BasePopupPage
    {
        private bool _isDefault;
        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; OnPropertyChanged(nameof(IsDefault)); }
        }

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
            GetControllers();
        }

        private async void GetControllers()
        {
            await GetAllControllers();
        }

        private async Task GetAllControllers()
        {
            var controllers = await App.Database.GetControllers();
            if (controllers != null && controllers.Count > 0)
            {

            }
            else
            {
                IsDefault = true;
            }
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
                //string deviceId;
                //if (Device.RuntimePlatform == Device.Android)
                //{
                //    var deviceBase = _device as Plugin.BLE.Abstractions.DeviceBase;
                //    //Mac Address
                //    deviceId = deviceBase.NativeDevice.ToString(); 
                //}
                //else
                //{
                //    //UDID
                //    deviceId = _device.Id.ToString(); 
                //}

                //If New connected device is set as default then make previous default device as non-default
                if (IsDefault)
                {
                    var defaultController = await App.Database.GetDefaultController();
                    if (defaultController != null)
                    {
                        defaultController.IsDefault = false;
                        await App.Database.UpdateController(defaultController);
                    }
                }

                var newController = new Controller
                {
                    Id = _device.Id.ToString(),
                    Name = DeviceName,
                    IsDefault = IsDefault,
                };

                App.ConnectedController = newController;

                //Save new device in device list
                await App.Database.SaveController(newController);

                await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
            }
        }
    }
}