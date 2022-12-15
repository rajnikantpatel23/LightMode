using ColorController.Droid.Services;
using ColorController.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformSpecific))]
namespace ColorController.Droid.Services
{
    public class PlatformSpecific : IPlatformSpecific
    {
        public bool GetBluetoothStatus()
        {
            return true;
        }

        public string GetCurrentWiFi()
        {
            return null;
        }

        public bool IsActivityFinishing()
        {
            return Xamarin.Essentials.Platform.CurrentActivity.IsFinishing;
        }
    }
}