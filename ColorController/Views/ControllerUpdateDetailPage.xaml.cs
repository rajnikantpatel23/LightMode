using ColorController.Abstractions;
using ColorController.Models;
using ColorController.PopupPages;
using ColorController.ViewModels;
using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ControllerUpdateDetailPage : BaseContentPage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerUpdateDetailPage(Item item)
        {
            InitializeComponent();
            BindingContext = new ControllerUpdateDetailViewModel(item, this); 
        }

        /// <summary>
        /// Will be called when Update button will be click to update App
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void UpdateAppButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await Plugin.LatestVersion.CrossLatestVersion.Current.OpenAppInStore(AppInfo.PackageName);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Will be called when Update button will be click to update firmware
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void UpdateFirmwareButtonClicked(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    locationAlwaysPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                if (locationAlwaysPermissionStatus == PermissionStatus.Disabled)
                {
                    await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                }
                if (locationAlwaysPermissionStatus == PermissionStatus.Granted /*&& App.ConnectionState == ConnectionButtonState.ShowDisconnect*/)
                {
                    if (Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new AvailableWifiPopupPage(this));
                    }
                    else
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                    }
                }
            }
            else
            {
                await PopupNavigation.Instance.PushAsync(new EnterSSIDPasswordPopupPage(this));
            }
        }

        private async void BackButtonTapped(object sender, System.EventArgs e)
        {
            var viewModel = BindingContext as ControllerUpdateDetailViewModel;
            viewModel.UnubscribeBLEHelperToReceiveConnectionStatusNotifications();
            await Navigation.PopAsync();
        }
    } 
}