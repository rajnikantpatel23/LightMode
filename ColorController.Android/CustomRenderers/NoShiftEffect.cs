using Android.Views;
using ColorController.Droid.CustomRenderers;
using Google.Android.Material.BottomNavigation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Rajnikant")]
[assembly: ExportEffect(typeof(NoShiftEffect), "NoShiftEffect")]
namespace ColorController.Droid.CustomRenderers
{
    public class NoShiftEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (!(Container.GetChildAt(0) is ViewGroup layout))
                return;

            if (!(layout.GetChildAt(1) is BottomNavigationView bottomNavigationView))
                return;

            // This is what we set to adjust if the shifting happens
            bottomNavigationView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityUnlabeled; 
        }

        protected override void OnDetached()
        {
        }
    }
}