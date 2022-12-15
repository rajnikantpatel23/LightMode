using ColorController.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Entry), typeof(BorderLessEntryRenderer))]

namespace ColorController.iOS.Renderers
{
    public class BorderLessEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.BorderStyle = UITextBorderStyle.None;
                Control.TintColor = UIColor.White;
            }
        }
    }
}