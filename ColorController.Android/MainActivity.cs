using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.Threading.Tasks;
using System.Linq;
using ColorController.PopupPages;
using Xamarin.Forms.Platform.Android;

namespace ColorController.Droid
{
    [Activity(Label = "LightMode", Icon = "@mipmap/icon", Theme = "@style/MyTheme.Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);
            Sharpnado.HorizontalListView.Droid.SharpnadoInitializer.Initialize();
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);
            Acr.UserDialogs.UserDialogs.Init(this);
            //Used for GIF preloading
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            LoadApplication(new App()); 
            SetStatusBarColor();
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception;
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == typeof(EnterDeviceNamePopupPage)))
            {
                //Do nothing...
            }
            if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Any(x => x.GetType() == typeof(UpdatingPopupPage)))
            {
                //Do nothing...
            }
            else
            {
                Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
            }
        }

        public void SetStatusBarColor()
        {
            // The SetStatusBarcolor is new since API 21
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var androidColor = Xamarin.Forms.Color.Black.ToAndroid();
                //Just use the plugin
                Window.SetStatusBarColor(androidColor);
            }
            else
            {
                // Here you will just have to set your 
                // color in styles.xml file as shown below.
            }
        }
    }
}