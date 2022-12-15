using Android.Views;
using ColorController.Droid.CustomRenderers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
 
//[assembly: ResolutionGroupName("Rajnikant")]
[assembly: ExportEffect(typeof(NoKeyboardEffect), "NoKeyboardEffect")]
namespace ColorController.Droid.CustomRenderers
{
    public class NoKeyboardEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Control is Android.Widget.EditText editText)
                {
                    // cursor shown, but keyboard will not be displayed
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                    {
                        editText.ShowSoftInputOnFocus = false;
                    }
                    else
                    {
                        editText.SetTextIsSelectable(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(NoKeyboardEffect)} failed to attached: {ex.Message}");
            }
        }

        protected override void OnDetached()
        {
        }
    }
}