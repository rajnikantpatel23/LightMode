using ColorController.Controls;
using ColorController.iOS.CustomRenderers;
using System;
using System.Linq;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("Rajnikant")]
[assembly: ExportEffect(typeof(TintImageImpl), nameof(TintImage))]
namespace ColorController.iOS.CustomRenderers
{
    public class TintImageImpl : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var effect = (TintImage)Element.Effects.FirstOrDefault(e => e is TintImage);

                if (effect == null || !(Control is UIImageView image))
                    return;

                image.Image = image.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                image.TintColor = effect.TintColor.ToUIColor();
            }
            catch (Exception ex)
            {
            }
        }

        protected override void OnDetached() { }
    }
}