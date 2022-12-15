using Android.Content;
using ColorController.Droid.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Slider), typeof(CustomSliderRenderer))]
namespace ColorController.Droid.CustomRenderers
{
    public class CustomSliderRenderer : SliderRenderer
    {
        public CustomSliderRenderer(Context context) : base(context)
        {
        }

        private Slider view;
        protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
        {
            base.OnElementChanged(e);
            Control.Background = null;
            Control.SetBackgroundColor(Color.Transparent.ToAndroid());
        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);
             
        }
    }
}