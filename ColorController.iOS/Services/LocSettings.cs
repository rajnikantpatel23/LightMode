using ColorController.iOS.Services;
using ColorController.PopupPages;
using ColorController.Services;
using CoreLocation;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocSettings))]
namespace ColorController.iOS.Services
{
    public class LocSettings : ILocSettings
    {
        public async Task<bool> IsGpsAvailable()
        {
            bool value = false;

            if (CLLocationManager.LocationServicesEnabled)
            {
                if (CLLocationManager.Status == CLAuthorizationStatus.Authorized || CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways || CLLocationManager.Status == CLAuthorizationStatus.AuthorizedWhenInUse)
                {//enable
                    value = true;
                }
                else if (CLLocationManager.Status == CLAuthorizationStatus.Denied)
                {
                    value = false;
                    await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new AlertPopupPageWithOkCancel("You must grant this App permission to access your location in order to pair.", (response) =>
                    { 
                        if(response)
                            OpenSettings();
                    }));
                }
                else
                {
                    value = false;
                    RequestRuntime();
                } 
            }
            else
            {
                //location service false
                value = false;
                //ask user to open system setting page to turn on it manually.
            }
            return value;
        }


        public void RequestRuntime()
        {
            CLLocationManager cLLocationManager = new CLLocationManager();
            cLLocationManager.RequestWhenInUseAuthorization();
        }

        public void OpenSettings()
        {
            //UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
            Xamarin.Essentials.AppInfo.ShowSettingsUI();
        }
    }
}

//https://stackoverflow.com/questions/64940855/xamarin-forms-how-to-check-if-gps-is-on-or-off-in-xamarin-ios-app