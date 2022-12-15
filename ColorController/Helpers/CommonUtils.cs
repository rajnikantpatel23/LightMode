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
