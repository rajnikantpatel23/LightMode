using ColorController.Controls;
using ColorController.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms.Platform.iOS;

[assembly: Xamarin.Forms.ExportRenderer(typeof(CustomScrollView), typeof(CustomRenderer))]
[assembly: Xamarin.Forms.Dependency(typeof(CustomDependencyService))]
namespace ColorController.iOS.CustomRenderers
{
    public class CustomRenderer : ScrollViewRenderer
    {
        CustomScrollView scrollViewExt;
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            scrollViewExt = e.NewElement as CustomScrollView;
            scrollViewExt.NativeObject = NativeView as UIScrollView;
        }
    }
    public class CustomDependencyService : IScrollViewDependencyService
    {
        public void Scrolling(bool isScrolling, object nativeObject)
        {
            var native = nativeObject as UIScrollView;
            (nativeObject as UIScrollView).ScrollEnabled = isScrolling;
        }
    }
}