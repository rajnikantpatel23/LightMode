using ColorController.Controls;
using ColorController.Helpers;
using ColorController.Models;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Cell
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SolidDataView : ContentView
    {
        public event EventHandler AnimationTappedEvent;
        public event EventHandler StartIconTappedEvent;
        public event EventHandler InfoIconTappedEvent;

        SKBitmap srcBitmap =
           BitmapExtensions.LoadBitmapResource(typeof(FillRectanglePage),
                                               "ColorController.Resources.Images.Solid.png");
        SKBitmap dstBitmap;
        public SolidDataView()
        {
            InitializeComponent();
            dstBitmap = new SKBitmap(srcBitmap.Width, srcBitmap.Height);
            //OnSliderValueChanged(null, null);
            
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var color = BindingContext as AnimationModel ;
            hueSlider.ValueChanged -= OnSliderValueChanged;
            saturationSlider.ValueChanged -= OnSliderValueChanged;
            luminositySlider.ValueChanged -= OnSliderValueChanged;

            hueSlider.Value = (App.CurrentSelectedColor.Hue *1000);

            saturationSlider.Value = App.CurrentSelectedColor.Saturation;
            luminositySlider.Value = App.CurrentSelectedColor.Luminosity;



            OnSliderValueChanged(null, null);
        }
        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            
            
             var color = (BindingContext as AnimationModel).SelectedColor1;
            var a = color.ToHex();
            var ab = (BindingContext as AnimationModel).SelectedColor2.ToHex();
            float hueAdjust = (float)getHue(color.R, color.G, color.B);
            //hueLabel.Text = $"Hue Adjustment: {hueAdjust:F0}";

            float saturationAdjust = (float)color.Saturation;
            //saturationLabel.Text = $"Saturation Adjustment: {saturationAdjust:F2}";

            float luminosityAdjust = (float)Math.Pow(2, luminositySlider.Value);
            //luminosityLabel.Text = $"Luminosity Adjustment: {luminosityAdjust:F2}";



            TransferPixels(hueAdjust, saturationAdjust, 1);
            canvasView.InvalidateSurface();
        }
        public double getHue(double red, double green, double blue)
        {

            float min =(float)  Math.Min(Math.Min(red, green), blue);
            float max = (float) Math.Max(Math.Max(red, green), blue);

            if (min == max)
            { 
                return 0;
            }

            float hue = 0f;
            if (max == red)
            {
                hue = (float) (green - blue) / (max - min);

            }
            else if (max == green)
            {
                hue = ((float)((float) 2f + (blue - red) / (max - min)));

            }
            else
            {
                hue = (float)((float) 4f + (red - green) / (max - min));
            }

            hue = hue * 60;
            if (hue < 0) hue = hue + 360;

            return Math.Round(hue);
        }
        unsafe void TransferPixels(float hueAdjust, float saturationAdjust , float luminosityAdjust)
        {
            byte* srcPtr = (byte*)srcBitmap.GetPixels().ToPointer();
            byte* dstPtr = (byte*)dstBitmap.GetPixels().ToPointer();

            int width = srcBitmap.Width;       // same for both bitmaps
            int height = srcBitmap.Height;

            SKColorType typeOrg = srcBitmap.ColorType;
            SKColorType typeAdj = dstBitmap.ColorType;

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    // Get color from original bitmap
                    byte byte1 = *srcPtr++;         // red or blue
                    byte byte2 = *srcPtr++;         // green
                    byte byte3 = *srcPtr++;         // blue or red
                    byte byte4 = *srcPtr++;         // alpha

                    SKColor color = new SKColor();

                    if (typeOrg == SKColorType.Rgba8888)
                    {
                        color = new SKColor(byte1, byte2, byte3, byte4);
                    }
                    else if (typeOrg == SKColorType.Bgra8888)
                    {
                        color = new SKColor(byte3, byte2, byte1, byte4);
                    }

                    // Get HSL components
                    color.ToHsl(out float hue, out float saturation, out float luminosity);

                    // Adjust HSL components based on adjustments
                    hue = (hue + hueAdjust) % 360;
                    saturation = Math.Max(0, Math.Min(100, saturationAdjust * saturation));
                    luminosity = Math.Max(0, Math.Min(100, luminosityAdjust * luminosity));

                    // Recreate color from HSL components
                    color = SKColor.FromHsl(hue, saturation, luminosity);

                    // Store the bytes in the adjusted bitmap
                    if (typeAdj == SKColorType.Rgba8888)
                    {
                        *dstPtr++ = color.Red;
                        *dstPtr++ = color.Green;
                        *dstPtr++ = color.Blue;
                        *dstPtr++ = color.Alpha;
                    }
                    else if (typeAdj == SKColorType.Bgra8888)
                    {
                        *dstPtr++ = color.Blue;
                        *dstPtr++ = color.Green;
                        *dstPtr++ = color.Red;
                        *dstPtr++ = color.Alpha;
                    }
                }
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            canvas.DrawBitmap(dstBitmap, info.Rect, BitmapStretch.Uniform);
        }

        private void StarIconTapped(object sender, EventArgs e)
        {
            StartIconTappedEvent?.Invoke(sender, e);
        }

        private void InfoIconTapped(object sender, EventArgs e)
        {
            InfoIconTappedEvent?.Invoke(sender, e);
        }

        private void AnimationTapped(object sender, EventArgs e)
        {
            AnimationTappedEvent?.Invoke(sender, e);
        }
    }
}