using ColorController.Controls;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Cell
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImageHueSaturationView : ContentView
    { 
        SKBitmap _srcBitmap;
        SKBitmap _dstBitmap;
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        public ImageHueSaturationView(string imageName)
        {
            InitializeComponent();

            _srcBitmap = BitmapExtensions.LoadBitmapResource(typeof(FillRectanglePage),$"ColorController.Resources.Images.{imageName}");
           // srcBitmap = AddTransparency(result, 1.0f);
            _dstBitmap = new SKBitmap(_srcBitmap.Width, _srcBitmap.Height);
            //OnSliderValueChanged(null, null);
        }

        SKBitmap AddTransparency(SKBitmap bitmapSource, float treshold)
        {
            var bitmapTarget = bitmapSource.Copy();

            try
            {
                // Calculate the treshold as a number between 0 and 255
                int value = (int)(255 * treshold);

                // loop trough every pixel
                int width = bitmapTarget.Width;
                int height = bitmapTarget.Height;

                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        var color = bitmapTarget.GetPixel(col, row);

                        if (color.Red > value && color.Green > value && color.Blue > value)
                        {
                            bitmapTarget.SetPixel(col, row, color.WithAlpha(0x00));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string> { { "AddTransparency()", $"{ex.StackTrace}" } });
            }

            return bitmapTarget;
        }

        public ImageHueSaturationView()
        {

        }
        
        protected override void OnBindingContextChanged()
        {
            string selectedAnimationVersion = null;

            try
            {
                base.OnBindingContextChanged();
                if (BindingContext != null)
                {
                    if (BindingContext.GetType() == typeof(AnimationModel))
                    {
                        var animation = BindingContext as AnimationModel;

                        if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                        {
                            selectedAnimationVersion = Constants.GetAnimations().FirstOrDefault(x => x.AnimationType == animation.AnimationType).ControllerVersion;

                            var isAnimationVersionParsed = Version.TryParse(selectedAnimationVersion, out Version animationControllerVersion);
                            var isControllerVersionParsed = Version.TryParse(App.ConnectedControllerVersion, out Version connectedControllerVersion);
                            
                            if (isAnimationVersionParsed && isControllerVersionParsed)
                            {
                                if (animationControllerVersion > connectedControllerVersion)
                                {
                                    animation.IsShieldVisible = true;
                                }
                                else
                                {
                                    animation.IsShieldVisible = false;
                                } 
                            }
                            else
                            {
                                Microsoft.AppCenter.Crashes.Crashes.TrackError(null, new Dictionary<string, string>
                                {
                                    { "ImageHueSaturationView: OnBindingContextChanged() Not able to parse version", null },
                                    { "Selected Animation Version", $"{selectedAnimationVersion}" },
                                    { "Connected Controller Version", $"{App.ConnectedControllerVersion}" },
                                });
                            }
                        }
                        else
                        {
                            animation.IsShieldVisible = false;
                        }
                    }
                    else
                    {
                        var animation = BindingContext as FavoriteAnimation;

                        if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                        {
                            selectedAnimationVersion = Constants.GetAnimations().FirstOrDefault(x => x.AnimationType == animation.AnimationType).ControllerVersion;

                            var isAnimationVersionParsed = Version.TryParse(selectedAnimationVersion, out Version animationControllerVersion);
                            var isControllerVersionParsed = Version.TryParse(App.ConnectedControllerVersion, out Version connectedControllerVersion);

                            if (isAnimationVersionParsed && isControllerVersionParsed)
                            {
                                if (animationControllerVersion > connectedControllerVersion)
                                {
                                    animation.IsShieldVisible = true;
                                }
                                else
                                {
                                    animation.IsShieldVisible = false;
                                }
                            }
                            else
                            {
                                Microsoft.AppCenter.Crashes.Crashes.TrackError(null, new Dictionary<string, string>
                                {
                                    { "ImageHueSaturationView: OnBindingContextChanged() Not able to parse version", null },
                                    { "Selected Animation Version", $"{selectedAnimationVersion}" },
                                    { "Connected Controller Version", $"{App.ConnectedControllerVersion}" },
                                });
                            }
                        }
                        else
                        {
                            animation.IsShieldVisible = false;
                        }
                    }
                }

                OnSliderValueChanged(null, null);
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "ImageHueSaturationView: OnBindingContextChanged()", $"{ex.StackTrace}" },
                    { "Selected Animation Version", $"{selectedAnimationVersion}" },
                    { "Connected Controller Version", $"{App.ConnectedControllerVersion}" },
                });
            }
        }

        private void DisplayShield(string controllerVersion)
        {
            //shield.IsVisible = true;
            //if (App.VERS == 0 || App.VERS == null)
            //{
            //    shield.IsVisible = false;
            //}
            //if (App.VERS != null && App.VERS != 0)
            //{ 
            //    var version = controllerVersion.Replace(".", "");
            //    var versionInt = int.Parse(version);
            //    if (App.VERS >= versionInt)
            //    {
            //        shield.IsVisible = false;
            //    }
            //}
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            try
            {
                if (BindingContext != null)
                {
                    Color color;
                    if (BindingContext.GetType() == typeof(AnimationModel))
                    {
                        color = (BindingContext as AnimationModel).SelectedColor1;
                    }
                    else
                    {
                        color = (BindingContext as FavoriteAnimation).SelectedColor1;
                    }

                    float hueAdjust = (float)GetHue(color.R, color.G, color.B);
                    //float saturationAdjust = (float)color.Saturation;

                    //Get saturation using luminosity
                    var saturationAdjust = BlueToothService.GetSaturationValue(color.Luminosity);

                    TransferPixels(hueAdjust, (float)saturationAdjust, 1);
                    canvasView.InvalidateSurface();
                }
            }
            catch (Exception ex)
            {
                Microsoft.AppCenter.Crashes.Crashes.TrackError(ex, new Dictionary<string, string> { { "OnSliderValueChanged()", $"{ex.StackTrace}" } });
            }
        }

        public double GetHue(double red, double green, double blue)
        {

            float min = (float)Math.Min(Math.Min(red, green), blue);
            float max = (float)Math.Max(Math.Max(red, green), blue);

            if (min == max)
            {
                return 0;
            }

            float hue = 0f;
            if (max == red)
            {
                hue = (float)(green - blue) / (max - min);
            }
            else if (max == green)
            {
                hue = ((float)((float)2f + (blue - red) / (max - min)));
            }
            else
            {
                hue = (float)((float)4f + (red - green) / (max - min));
            }

            hue = hue * 60;
            if (hue < 0) hue = hue + 360;

            return Math.Round(hue);
        }
        
        unsafe void TransferPixels(float hueAdjust, float saturationAdjust, float luminosityAdjust)
        {
            byte* srcPtr = (byte*)_srcBitmap.GetPixels().ToPointer();
            byte* dstPtr = (byte*)_dstBitmap.GetPixels().ToPointer();

            int width = _srcBitmap.Width;       // same for both bitmaps
            int height = _srcBitmap.Height;

            SKColorType typeOrg = _srcBitmap.ColorType;
            SKColorType typeAdj = _dstBitmap.ColorType;

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
            canvas.DrawBitmap(_dstBitmap, info.Rect, BitmapStretch.Uniform);
        }
    }
}