using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomButtonControl : Grid
    {
        public CustomButtonControl()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty ButtonWidthProperty = BindableProperty.Create(nameof(ButtonWidth), typeof(double), typeof(CustomButtonControl), 100.0);
        public double ButtonWidth
        {
            get { return (double)GetValue(ButtonWidthProperty); }
            set { SetValue(ButtonWidthProperty, value); }
        }

        public static readonly BindableProperty FontFamilyValueProperty = BindableProperty.Create(nameof(FontFamilyValue), typeof(string), typeof(CustomButtonControl), "LatoRegular");
        public string FontFamilyValue
        {
            get { return (string)GetValue(FontFamilyValueProperty); }
            set { SetValue(FontFamilyValueProperty, value); }
        }

        public static readonly BindableProperty ButtonTextColorProperty = BindableProperty.Create(nameof(ButtonTextColor), typeof(Color), typeof(CustomButtonControl), (Color)App.Current.Resources["ButtonTextColor"]);
        public Color ButtonTextColor
        {
            get { return (Color)GetValue(ButtonTextColorProperty); }
            set { SetValue(ButtonTextColorProperty, value); }
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomButtonControl), string.Empty);
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CustomButtonControl), 14.0);
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static new readonly BindableProperty ScaleXProperty = BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(CustomButtonControl), 1.5);
        public new double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        public static new readonly BindableProperty ScaleYProperty = BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(CustomButtonControl), 0.9);
        public new double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }
    }
}