using Acr.UserDialogs;
using ColorController.Enums;
using ColorController.PopupPages;
using ColorController.StringResources;
using Plugin.BLE;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ColorController.Helpers
{
    public class CommonUtils
    {
        public static async Task<bool> GetLatestAppVersionNumber()
        {
            bool isLatestApp = true;
            try
            {
                UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                var currentVersion = Xamarin.Essentials.AppInfo.VersionString;
                var latestVersion = await Plugin.LatestVersion.CrossLatestVersion.Current.GetLatestVersionNumber();

                var installedAppVersion = new Version(currentVersion);
                var appStoreAppVersion = new Version(latestVersion);
                if (installedAppVersion < appStoreAppVersion)
                {
                    isLatestApp = false;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }

            return isLatestApp;
        }

        public static async Task ScanAndConnectDevice()
        {
            //Check Bluetooth ON/OFF status
            //iOS: If Bluetooth ON then Start Scanning
            //Android: Check Location Permission
            //Android: If Location Permission disabled then ask for permission
            //Android: If Location permission is denied then display confirmation alert and navigate to Settings 
            //Android: Check GPS ON/OFF status
            //Android: If GPS is OFF then display alert to turn ON GPS.
            //Android: If GPS is ON then start scanning.

            if (!CrossBluetoothLE.Current.IsOn)
            {
                await PopupNavigation.Instance.PushAsync(new AlertPopupPage(StringResource.PleaseTurnONBT));
                return;
            }

            if (Device.RuntimePlatform == Device.Android)
            {
                var locationAlwaysPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
             
                if (CrossGeolocator.IsSupported)
                {
                    if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                    {
                        locationAlwaysPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }
                    if (locationAlwaysPermissionStatus == PermissionStatus.Disabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                    }
                    if (locationAlwaysPermissionStatus == PermissionStatus.Denied)
                    {
                        await PopupNavigation.Instance.PushAsync(new AlertPopupPageWithOkCancel("You must grant this App permission to access your location in order to pair.", (response) =>
                        {
                            if (response)
                            {
                                var locSettings = DependencyService.Get<Services.ILocSettings>();
                                locSettings.OpenSettings();
                            }
                        }));
                        return;
                    }

                    if (!CrossGeolocator.Current.IsGeolocationEnabled)
                    {
                        await PopupNavigation.Instance.PushAsync(new LocationAlertPopupPage());
                        return;
                    }
                }

                if (locationAlwaysPermissionStatus != PermissionStatus.Granted)
                {
                    return;
                }
            }

            if (App.ConnectionState == ConnectionButtonState.ShowConnect || 
                App.ConnectionState == ConnectionButtonState.ShowSearchingText || 
                App.ConnectionState == ConnectionButtonState.ShowSearchingButton)
            {
                //Search for devices 
                //Find Light Mode Controller & Connect
                //If Controller is connected first time then Display Name Popup
                BLEHelper bLEHelper = new BLEHelper();

                await bLEHelper.StopScanning();
                var devices = await bLEHelper.ScanDevices(1000, true);

                //If Device is already connected then do notihing
                if (App.ConnectionState == ConnectionButtonState.ShowDisconnect)
                {
                    Debug.WriteLine("FavoriteViewModel: Already Connected....");
                    return;
                }

                if (devices != null && devices.Count == 0)
                {
                    Debug.WriteLine("FavoriteViewModel: Devices not found");
                }
                else
                {
                    var isConnected = await bLEHelper.ConnectController(devices);
                    if (!isConnected)
                    {
                        Debug.WriteLine("FavoriteViewModel: Not Connected....");
                    }
                    else
                    {
                        Debug.WriteLine("FavoriteViewModel: Connected....");
                    }
                }
            }
        }

        public static async Task PopToRootAsync()
        {
            try
            {
                if (App.Current.MainPage.Navigation.NavigationStack.Count > 0)
                {
                    await App.Current.MainPage.Navigation.PopToRootAsync();
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task OpenPopupPage(PopupPage popupPage)
        {
            if (popupPage != null && !PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == popupPage.GetType()))
            {
                await PopupNavigation.Instance.PushAsync(popupPage);
            }
        }

        public static async Task PushPage(Page page)
        {
            if (page != null && !App.Current.MainPage.Navigation.NavigationStack.Any(x => x.GetType() == page.GetType()))
            {
                await App.Current.MainPage.Navigation.PushAsync(page);
            }
        }

        public static async Task ClosePopup()
        {
            try
            {
                if(PopupNavigation.Instance.PopupStack.Count > 0)
                {
                    await PopupNavigation.Instance.PopAsync();
                }
            }
            catch (Exception)
            {

            }
        }

        public static void WriteLog(string message)
        {
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
            Debug.WriteLine($"{message}");
            Debug.WriteLine($"---------------------------------------------------------------------------------------");
        }
    }
}
