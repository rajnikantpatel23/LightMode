using Xamarin.Forms.Platform.Android;
using Android.Widget;
using System.ComponentModel;
using Xamarin.Forms;
using Android.Graphics;
using ColorController.Controls;
using ColorController.Droid.CustomRenderers;

[assembly: ExportRendererAttribute(typeof(IconView), typeof(IconViewRenderer))]
namespace ColorController.Droid.CustomRenderers
{
    public class IconViewRenderer : ViewRenderer<IconView, ImageView>
    {
        private bool _isDisposed;

        public IconViewRenderer()
        {
            base.AutoPackage = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<IconView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                SetNativeControl(new ImageView(Context));
            }
            UpdateBitmap(e.OldElement);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == IconView.SourceProperty.PropertyName)
            {
                UpdateBitmap(null);
            }
            else if (e.PropertyName == IconView.ForegroundProperty.PropertyName)
            {
                UpdateBitmap(null);
            }
        }

        private void UpdateBitmap(IconView previous = null)
        {
            if (!_isDisposed && !string.IsNullOrWhiteSpace(Element.Source))
            {
                //var d = Resources.GetDrawable(Element.Source).Mutate();
                var d = Context.GetDrawable(Element.Source).Mutate();
                d.SetColorFilter(new LightingColorFilter(Element.Foreground.ToAndroid(), Element.Foreground.ToAndroid()));
                d.Alpha = Element.Foreground.ToAndroid().A;
                Control.SetImageDrawable(d);
                ((IVisualElementController)Element).NativeSizeChanged();
            }
        }
    }
}