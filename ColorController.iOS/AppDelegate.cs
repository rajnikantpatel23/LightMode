using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace ColorController.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Sharpnado.HorizontalListView.iOS.SharpnadoInitializer.Initialize();
            global::Xamarin.Forms.Forms.Init();
            Rg.Plugins.Popup.Popup.Init();
            Sharpnado.Shades.iOS.iOSShadowsRenderer.Initialize();
            //Used for GIF preloading
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            // Check for new data, and display it


            // Inform system of fetch results
            completionHandler(UIBackgroundFetchResult.NewData);
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var ex = e.Exception;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject;
        }
    }
}
