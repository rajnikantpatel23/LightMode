using ColorController;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ThomasPOC.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MaskingPage : ContentPage
    {
        private Color _selectedColor;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); }
        }

        public MaskingPage()
        {
            InitializeComponent();
            BindingContext = this;
            if (BackgroundColor == Color.White)
            {
                SetDarkMode();
            }
            else
            { 
                SetLightMode();
            }
        }

        private void SetDarkMode()
        {
            buttonMode.Text = "Light Mode";
            ImageRobocopBlack.IsVisible = false;
            ImageRobocopWhite.IsVisible = true;
        }

        private void SetLightMode()
        {
            buttonMode.Text = "Dark Mode";
            ImageRobocopWhite.IsVisible = false;
            ImageRobocopBlack.IsVisible = true;
        }

        private Color _color2;
        public Color Color2
        {
            get { return _color2; }
            set { _color2 = value; OnPropertyChanged(nameof(Color2)); }
        }

        private void ColorPicker_PickedColorChanged(object sender, Color color)
        {
            var selectedColor = color;
            //selectedColor.WithSaturation(0.75);
            //selectedColor.WithLuminosity(0.75);
            SelectedColor = selectedColor;
            var selectedColorHue = GetHueFromRGB((float)SelectedColor.R, (float)SelectedColor.G, (float)SelectedColor.B);
            var secondaryColorHue = GetSecondaryColor(selectedColorHue, 240);
            Color2 = Xamarin.Essentials.ColorExtensions.WithHue(SelectedColor, (float)secondaryColorHue);
        }

        private double GetSecondaryColor(double selectedColorHue, int angle)
        {
            var secondaryColorHue = selectedColorHue + angle;
            var hueMod360 = secondaryColorHue % 360;
            return hueMod360;
        }

        public double GetHueFromRGB(float red, float green, float blue)
        {
            float min = Math.Min(Math.Min(red, green), blue);
            float max = Math.Max(Math.Max(red, green), blue);

            if (min == max)
            {
                return 0;
            }

            float hue = 0f;
            if (max == red)
            {
                hue = (green - blue) / (max - min);

            }
            else if (max == green)
            {
                hue = 2f + (blue - red) / (max - min);
            }
            else
            {
                hue = 4f + (red - green) / (max - min);
            }

            hue = hue * 60;
            if (hue < 0) hue = hue + 360;

            return Math.Round(hue);
        }

        private void ChangeMode(object sender, EventArgs e)
        {
            if (BackgroundColor == Color.White)
            {
                BackgroundColor = Color.Black;
                //divider.Foreground = Color.Black;
                SetLightMode();
            }
            else
            {
                BackgroundColor = Color.White;
                //divider.Foreground = Color.White;
                SetDarkMode();
            }
        }

        private async void GoBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}