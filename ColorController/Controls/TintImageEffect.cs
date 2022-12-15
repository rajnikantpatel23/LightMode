using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ColorController.Controls
{
    public static class TintImageEffect
    {
        public static BindableProperty TintColorProperty =
            BindableProperty.CreateAttached("TintColor",
                                            typeof(Color),
                                            typeof(TintImageEffect),
                                            Color.Default,
                                            propertyChanged: OnTintColorPropertyPropertyChanged);

        public static Color GetTintColor(BindableObject element)
        {
            return (Color)element.GetValue(TintColorProperty);
        }

        public static void SetTintColor(BindableObject element, Color value)
        {
            element.SetValue(TintColorProperty, value);
        }


        static void OnTintColorPropertyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AttachEffect(bindable as Image, (Color)newValue);
        }

        static void AttachEffect(Image element, Color color)
        {
            var effect = element.Effects.FirstOrDefault(x => x is TintImage) as TintImage;

            if (effect != null)
            {
                element.Effects.Remove(effect);
            }

            element.Effects.Add(new TintImage(color));
        }
    }
}
