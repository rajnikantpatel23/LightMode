using Android.Content;
using Android.Runtime;
using Android.Widget;
using ColorController.Droid.Renderers;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(BorderLessEntryRenderer))]
namespace ColorController.Droid.Renderers
{
    public class BorderLessEntryRenderer : EntryRenderer
    {
        public BorderLessEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Background = null; 
            }
        }
    }
}