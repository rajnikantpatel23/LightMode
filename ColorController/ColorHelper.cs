using System;
using Xamarin.Forms;

namespace ColorController
{
    public class ColorHelper
    {
        public static Color GetColorByAngle(Color color, int angle)
        {
            var selectedColorHue = GetHueFromRGB((float)color.R, (float)color.G, (float)color.B);
            var secondaryColorHue = GetSecondaryColor(selectedColorHue, angle);
            var selectedColor2 = Xamarin.Essentials.ColorExtensions.WithHue(color, (float)secondaryColorHue);
            var color2 = Color.FromHex(selectedColor2.Name);
            return color2;
        }

        private static double GetSecondaryColor(double selectedColorHue, int angle)
        {
            var secondaryColorHue = selectedColorHue + angle;
            var hueMod360 = secondaryColorHue % 360;
            return hueMod360;
        }

        public static double GetHueFromRGB(float red, float green, float blue)
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
    }
}
