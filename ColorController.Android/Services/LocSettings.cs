using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ColorController.Droid.Services;
using ColorController.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocSettings))]
namespace ColorController.Droid.Services
{
    public class LocSettings : ILocSettings
    {
        public async Task<bool> IsGpsAvailable()
        {
            bool value;
            var locationManager = (Android.Locations.LocationManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.LocationService);
            if (!locationManager.IsProviderEnabled(Android.Locations.LocationManager.GpsProvider))
            {
                //Gps disable
                value = false;
            }
            else
            {
                //Gps enable
                value = true;
            }
            return value;
        }

        public void OpenSettings()
        {
            //Intent intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocat‌​ionSourceSettings);
            //intent.AddFlags(ActivityFlags.NewTask);
            //Android.App.Application.Context.StartActivity(intent);
            Xamarin.Essentials.AppInfo.ShowSettingsUI();
        }
    }
}

// https://social.msdn.microsoft.com/Forums/en-US/f185a20b-0f96-4a89-9bf2-14ae24b22738/xamarin-forms-how-to-check-if-gps-is-on-or-off-in-xamarin-ios-app?forum=xamarinforms