using Android.Content;
using Android.Views;
using ColorController.Controls;
using ColorController.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomScrollView), typeof(CustomScrollViewRenderer))]
[assembly: Dependency(typeof(CustomDependencyService))]

namespace ColorController.Droid.CustomRenderers
{
    public class CustomScrollViewRenderer : ScrollViewRenderer
    {
        public CustomScrollViewRenderer(Context context) : base(context)
        {
        }

        CustomScrollView scrollViewExt;
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            scrollViewExt = e.NewElement as CustomScrollView;
        }
        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            if (!scrollViewExt.IsScrolling)
                return false;

            return base.OnInterceptTouchEvent(ev);
        }
    }

    public class CustomDependencyService : IScrollViewDependencyService
    {

        public void Scrolling(bool isScrolling, object nativeObject)
        {

        }
    }
}