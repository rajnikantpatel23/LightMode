using Android.Graphics;
using Android.Widget;
using ColorController.Controls;
using ColorController.Droid.CustomRenderers;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(TintImageImpl), nameof(TintImage))]
namespace ColorController.Droid.CustomRenderers
{
    public class TintImageImpl : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var effect = (TintImage)Element.Effects.FirstOrDefault(e => e is TintImage);

                if (effect == null || !(Control is ImageView image))
                    return;

                var filter = new PorterDuffColorFilter(effect.TintColor.ToAndroid(), PorterDuff.Mode.SrcIn);
                image.SetColorFilter(filter);
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnDetached() { }
    }
}