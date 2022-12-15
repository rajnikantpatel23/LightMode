using System;
using System.ComponentModel;
using System.Diagnostics;
using ColorController;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Spillman.Xamarin.Forms.ColorPicker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPickerView : ContentView
    {
        public ColorPickerView()
        {
            InitializeComponent();
            App.IsColorPickerTouchedManually = false;
        }

        public ColorPickerViewModel ViewModel
        {
            get => BindingContext as ColorPickerViewModel;
            set => BindingContext = value;
        }

        private bool _changingAlphaText;
        private string _alphaText = "";
        public string AlphaText
        {
            get => _alphaText;
            set
            {
                _changingAlphaText = true;

                if (_alphaText != value)
                {
                    var cleanedText = "";
                    foreach (var c in value)
                    {
                        if (c >= '0' && c <= '9')
                        {
                            cleanedText += c;
                        }
                    }

                    if (cleanedText == "")
                    {
                        cleanedText = "0";
                    }

                    if (byte.TryParse(cleanedText, out var alpha))
                    {
                        ViewModel.A = alpha;
                    }

                    _alphaText = value;
                    OnPropertyChanged();
                }

                _changingAlphaText = false;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (ViewModel != null)
            {
                AlphaText = ViewModel.A.ToString();
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ColorPickerViewModel.H):
                    SelectedHueRainbowCanvasView.InvalidateSurface();
                    SelectedSaturationValueCanvasView.InvalidateSurface();
                    break;
                case nameof(ColorPickerViewModel.S):
                case nameof(ColorPickerViewModel.V):
                    SelectedSaturationValueCanvasView.InvalidateSurface();
                    break;
                case nameof(ColorPickerViewModel.A):
                    if (!_changingAlphaText)
                    {
                        AlphaText = ViewModel.A.ToString();
                    }
                    break;
            }
        }

        public event EventHandler<Color> PickedColorChanged;

        private void OnHexUnfocused(object sender, FocusEventArgs e)
        {
            ViewModel.Hex = ViewModel.Color.ToRgbHex();
        }

        private void OnAlphaEntryUnfocused(object sender, FocusEventArgs e)
        {
            AlphaText = ViewModel.A.ToString();
        }

        private SKSizeI _selectedSaturationValuePixelSize;
        private void OnSelectedSaturationValuePaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (App.IsColorPickerTouchedManually)
            {
                var info = e.Info;
                var surface = e.Surface;
                var canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);

                using (var saturationPaint = new SKPaint())
                {
                    using (
                        saturationPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(0, 0),
                            new SKPoint(info.Width, 0),
                            new[] { SKColors.White, SKColors.Transparent },
                            null,
                            SKShaderTileMode.Clamp
                        )
                    )
                    {
                        canvas.DrawRect(0, 0, info.Width, info.Height, saturationPaint);
                    }
                }

                using (var valuePaint = new SKPaint())
                {
                    using (
                        valuePaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(0, 0),
                            new SKPoint(0, info.Height),
                            new[] { SKColors.Empty, SKColors.Transparent },
                            null,
                            SKShaderTileMode.Clamp
                        )
                    )
                    {
                        canvas.DrawRect(0, 0, info.Width, info.Height, valuePaint);
                    }
                }

                _selectedSaturationValuePixelSize = info.Size;

                //canvas.Clear(SKColors.Transparent);

                var center = new SKPoint(
                    ViewModel.S / 100 * info.Width,
                    info.Height - ViewModel.V / 100 * info.Height
                );

                var pixelsPerXamarinUnit = info.Width / SelectedSaturationValueCanvasView.Width;

                var innerRadius = (float)(16 * pixelsPerXamarinUnit);
                var strokeWidth = (float)(1.5f * pixelsPerXamarinUnit);

                //Create Color selector Circle 
                using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = strokeWidth })
                {
                    paint.Style = SKPaintStyle.Fill;
                    //paint.Color = SKColorUtil.FromHsv(ViewModel.H, ViewModel.S, ViewModel.V);
                    // Represent the color of the current Touch point
                    paint.Style = SKPaintStyle.Fill;
                    SKColor touchPointColor;
                    using (SKBitmap bitmap = new SKBitmap(info))
                    {
                        // get the pixel buffer for the bitmap
                        IntPtr dstpixels = bitmap.GetPixels();

                        // read the surface into the bitmap
                        surface.ReadPixels(info,
                            dstpixels,
                            info.RowBytes,
                            (int)_lastTouchPoint.X, (int)_lastTouchPoint.Y);

                        // access the color
                        touchPointColor = bitmap.GetPixel(0, 0);
                    }

                    paint.Color = /*touchPointColor;*/ SKColorUtil.ToSKColor(ViewModel.Color);
                    ViewModel.SelectedColor = touchPointColor.ToFormsColor();
                    canvas.DrawCircle(center, innerRadius, paint);

                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.White;
                    canvas.DrawCircle(center, innerRadius, paint);

                    paint.Color = SKColors.Black;
                    canvas.DrawCircle(center, innerRadius + strokeWidth, paint);
                }

                canvas.Flush();

                ColorController.App.ColorPaletteFocused = false;
            }
            else
            {
                Debug.WriteLine($"OnSelectedSaturationValuePaintSurface()");
                ViewModel.S = 100;
                ViewModel.V = 100;
                ViewModel.H = 0;

                var info = e.Info;
                var surface = e.Surface;
                var canvas = surface.Canvas;
                _lastTouchPoint = new SKPoint(info.Width, 0);

                canvas.Clear(SKColors.Transparent);

                using (var saturationPaint = new SKPaint())
                {
                    using (
                        saturationPaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(0, 0),
                            new SKPoint(info.Width, 0),
                            new[] { SKColors.White, SKColors.Transparent },
                            null,
                            SKShaderTileMode.Clamp
                        )
                    )
                    {
                        canvas.DrawRect(0, 0, info.Width, info.Height, saturationPaint);
                    }
                }

                using (var valuePaint = new SKPaint())
                {
                    using (
                        valuePaint.Shader = SKShader.CreateLinearGradient(
                            new SKPoint(0, 0),
                            new SKPoint(0, info.Height),
                            new[] { SKColors.Empty, SKColors.Transparent },
                            null,
                            SKShaderTileMode.Clamp
                        )
                    )
                    {
                        canvas.DrawRect(0, 0, info.Width, info.Height, valuePaint);
                    }
                }

                _selectedSaturationValuePixelSize = info.Size;

                //canvas.Clear(SKColors.Transparent);
                var center = new SKPoint(
                                100 / 100 * info.Width,
                                info.Height - 100 / 100 * info.Height
                                );

                Debug.WriteLine($"Center X: {center.X}");
                Debug.WriteLine($"Center Y: {center.Y}");

                var pixelsPerXamarinUnit = info.Width / SelectedSaturationValueCanvasView.Width;

                Debug.WriteLine($"PixelsPerXamarinUnit: {pixelsPerXamarinUnit}");

                var innerRadius = (float)(16 * pixelsPerXamarinUnit);
                var strokeWidth = (float)(1.5f * pixelsPerXamarinUnit);

                //Create Color selector Circle 
                using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = strokeWidth })
                {
                    paint.Style = SKPaintStyle.Fill;
                    //paint.Color = SKColorUtil.FromHsv(ViewModel.H, ViewModel.S, ViewModel.V);
                    // Represent the color of the current Touch point
                    paint.Style = SKPaintStyle.Fill;
                    SKColor touchPointColor;
                    using (SKBitmap bitmap = new SKBitmap(info))
                    {
                        // get the pixel buffer for the bitmap
                        IntPtr dstpixels = bitmap.GetPixels();

                        // read the surface into the bitmap
                        surface.ReadPixels(info,
                            dstpixels,
                            info.RowBytes,
                            (int)_lastTouchPoint.X, (int)_lastTouchPoint.Y);

                        // access the color
                        touchPointColor = bitmap.GetPixel(0, 0);
                    }

                    //paint.Color = /*touchPointColor;*/ SKColorUtil.ToSKColor(Color.Red);

                    touchPointColor = Color.Red.ToSKColor();
                    paint.Color = /*touchPointColor;*/ SKColorUtil.ToSKColor(Color.Red);
                    ViewModel.SelectedColor = touchPointColor.ToFormsColor();
                    canvas.DrawCircle(center, innerRadius, paint);

                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.White;
                    canvas.DrawCircle(center, innerRadius, paint);

                    paint.Color = SKColors.Black;
                    canvas.DrawCircle(center, innerRadius + strokeWidth, paint);
                }

                canvas.Flush();

                ColorController.App.ColorPaletteFocused = false;
            }
        }

        private void OnSelectedSaturationValueTouch(object sender, SKTouchEventArgs e)
        {
            Debug.WriteLine($"OnSelectedSaturationValueTouch()");
           
            App.IsColorPickerTouchedManually = true;

            ColorController.App.ColorPaletteFocused = true;
            _lastTouchPoint = e.Location;
            
            e.Handled = true;

            if (!e.InContact)
            {
                if (e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled)
                {
                    ViewModel.UpdateHex();
                }
                ColorController.App.ColorPaletteFocused = false;
                return;
            }

            var x = e.Location.X;
            var y = e.Location.Y;

            if (x < 0)
            {
                x = 0;
            }
            else if (x > _selectedSaturationValuePixelSize.Width)
            {
                x = _selectedSaturationValuePixelSize.Width;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y > _selectedSaturationValuePixelSize.Height)
            {
                y = _selectedSaturationValuePixelSize.Height;
            }

            var saturation = x / _selectedSaturationValuePixelSize.Width * 100;
            var value = 100 - y / _selectedSaturationValuePixelSize.Height * 100;

            ViewModel.S = saturation;
            ViewModel.V = value;
        }

        private void OnHueRainbowPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            Debug.WriteLine($"OnHueRainbowPaintSurface()");
          
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            using (var paint = new SKPaint { Style = SKPaintStyle.Fill })
            {
                var colors = new SKColor[7];

                for (var i = 0; i < colors.Length; i++)
                {
                    colors[i] = SKColor.FromHsl(i * 360f / (colors.Length - 1), 100, 50);
                }

                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(info.Width, 0),
                    colors,
                    null,
                    SKShaderTileMode.Clamp
                );

                canvas.DrawRect(0, 0, info.Width, info.Height, paint);
            }

            canvas.Flush();
        }

        private SKSizeI _selectedHueRainbowPixelSize;
        private void OnSelectedHueRainbowPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            Debug.WriteLine($"OnSelectedHueRainbowPaintSurface()");
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            _selectedHueRainbowPixelSize = info.Size;

            canvas.Clear(SKColors.Transparent);

            var center = new SKPoint(
                ViewModel.H / 360 * info.Width,
                info.Height / 2f
            );

            var pixelsPerXamarinUnit = info.Width / SelectedHueRainbowCanvasView.Width;

            var strokeWidth = (float)(1.5 * pixelsPerXamarinUnit);
            var innerRadius = info.Height / 2f - 2 * strokeWidth;

            using (var paint = new SKPaint { IsAntialias = true, StrokeWidth = strokeWidth })
            {
                paint.Style = SKPaintStyle.Fill;
                paint.Color = SKColorUtil.FromHsv(ViewModel.H, 100, 100);
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Style = SKPaintStyle.Stroke;
                paint.Color = SKColors.White;
                canvas.DrawCircle(center, innerRadius, paint);

                paint.Color = SKColors.Black;
                canvas.DrawCircle(center, innerRadius + strokeWidth, paint);
            }

            canvas.Flush();
            ColorController.App.ColorPaletteFocused = false;
        }

        private void OnSelectedHueRainbowTouch(object sender, SKTouchEventArgs e)
        {
            Debug.WriteLine($"OnSelectedHueRainbowTouch()");
           
            App.IsColorPickerTouchedManually = true;

            ColorController.App.ColorPaletteFocused = !ColorController.App.ColorPaletteFocused;

            e.Handled = true;

            if (!e.InContact)
            {
                if (e.ActionType == SKTouchAction.Released || e.ActionType == SKTouchAction.Cancelled)
                {
                    ViewModel.UpdateHex();
                    ColorController.App.ColorPaletteFocused = false;
                }

                return;
            }

            var x = e.Location.X;
            var width = _selectedHueRainbowPixelSize.Width;
            if (x < 0)
            {
                x = 0;
            }
            else if (x > width)
            {
                x = width;
            }

            ViewModel.H = x / width * 360;
        }

        #region Color Palette 2

        /// <summary>
        /// Occurs when the Picked Color changes
        /// </summary>

        public static readonly BindableProperty PickedColorProperty
            = BindableProperty.Create(
                nameof(PickedColor),
                typeof(Color),
                typeof(ColorPickerView));

        /// <summary>
        /// Get the current Picked Color
        /// </summary>
        public Color PickedColor
        {
            get { return (Color)GetValue(PickedColorProperty); }
            private set { SetValue(PickedColorProperty, value); }
        }


        public static readonly BindableProperty GradientColorStyleProperty
            = BindableProperty.Create(
                nameof(GradientColorStyle),
                typeof(GradientColorStyle),
                typeof(ColorPickerView),
                GradientColorStyle.ColorsToDarkStyle,
                BindingMode.OneTime, null);

        /// <summary>
        /// Set the Color Spectrum Gradient Style
        /// </summary>
        public GradientColorStyle GradientColorStyle
        {
            get { return (GradientColorStyle)GetValue(GradientColorStyleProperty); }
            set { SetValue(GradientColorStyleProperty, value); }
        }


        public static readonly BindableProperty ColorListProperty
            = BindableProperty.Create(
                nameof(ColorList),
                typeof(string[]),
                typeof(ColorPickerView),
                new string[]
                {
                        new Color(255, 0, 0).ToHex(), // Red
        	new Color(255, 255, 0).ToHex(), // Yellow
        	new Color(0, 255, 0).ToHex(), // Green (Lime)
        	new Color(0, 255, 255).ToHex(), // Aqua
        	new Color(0, 0, 255).ToHex(), // Blue
        	new Color(255, 0, 255).ToHex(), // Fuchsia
        	new Color(255, 0, 0).ToHex(), // Red
                },
                BindingMode.OneTime, null);

        /// <summary>
        /// Sets the Color List
        /// </summary>
        public string[] ColorList
        {
            get { return (string[])GetValue(ColorListProperty); }
            set { SetValue(ColorListProperty, value); }
        }


        public static readonly BindableProperty ColorListDirectionProperty
            = BindableProperty.Create(
                nameof(ColorListDirection),
                typeof(ColorListDirection),
                typeof(ColorPickerView),
                ColorListDirection.Horizontal,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Color List flow Direction
        /// </summary>
        public ColorListDirection ColorListDirection
        {
            get { return (ColorListDirection)GetValue(ColorListDirectionProperty); }
            set { SetValue(ColorListDirectionProperty, value); }
        }


        public static readonly BindableProperty PointerCircleDiameterUnitsProperty
            = BindableProperty.Create(
                nameof(PointerCircleDiameterUnits),
                typeof(double),
                typeof(ColorPickerView),
                0.6,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Picker Pointer Size
        /// Value must be between 0-1
        /// Calculated against the View Canvas size
        /// </summary>
        public double PointerCircleDiameterUnits
        {
            get { return (double)GetValue(PointerCircleDiameterUnitsProperty); }
            set { SetValue(PointerCircleDiameterUnitsProperty, value); }
        }


        public static readonly BindableProperty PointerCircleBorderUnitsProperty
            = BindableProperty.Create(
                nameof(PointerCircleBorderUnits),
                typeof(double),
                typeof(ColorPickerView),
                0.3,
                BindingMode.OneTime);

        /// <summary>
        /// Sets the Picker Pointer Border Size
        /// Value must be between 0-1
        /// Calculated against pixel size of Picker Pointer
        /// </summary>
        public double PointerCircleBorderUnits
        {
            get { return (double)GetValue(PointerCircleBorderUnitsProperty); }
            set { SetValue(PointerCircleBorderUnitsProperty, value); }
        }


        private SKPoint _lastTouchPoint = new SKPoint();

        private void SkCanvasView_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var skImageInfo = e.Info;
            var skSurface = e.Surface;
            var skCanvas = skSurface.Canvas;

            var skCanvasWidth = skImageInfo.Width;
            var skCanvasHeight = skImageInfo.Height;

            skCanvas.Clear(SKColors.White);

            // Draw gradient rainbow Color spectrum
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;

                System.Collections.Generic.List<SKColor> colors = new System.Collections.Generic.List<SKColor>();
                ColorList.ForEach((color) => { colors.Add(Color.FromHex(color).ToSKColor()); });

                // create the gradient shader between Colors
                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    ColorListDirection == ColorListDirection.Horizontal ?
                        new SKPoint(skCanvasWidth, 0) : new SKPoint(0, skCanvasHeight),
                    colors.ToArray(),
                    null,
                    SKShaderTileMode.Clamp))
                {
                    paint.Shader = shader;
                    skCanvas.DrawPaint(paint);
                }
            }

            // Draw darker gradient spectrum
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;

                // Initiate the darkened primary color list
                var colors = GetGradientOrder();

                // create the gradient shader 
                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    ColorListDirection == ColorListDirection.Horizontal ?
                        new SKPoint(0, skCanvasHeight) : new SKPoint(skCanvasWidth, 0),
                    colors,
                    null,
                    SKShaderTileMode.Clamp))
                {
                    paint.Shader = shader;
                    skCanvas.DrawPaint(paint);
                }
            }

            // Picking the Pixel Color values on the Touch Point

            // Represent the color of the current Touch point
            SKColor touchPointColor;

            // Efficient and fast
            // https://forums.xamarin.com/discussion/92899/read-a-pixel-info-from-a-canvas
            // create the 1x1 bitmap (auto allocates the pixel buffer)
            using (SKBitmap bitmap = new SKBitmap(skImageInfo))
            {
                // get the pixel buffer for the bitmap
                IntPtr dstpixels = bitmap.GetPixels();

                // read the surface into the bitmap
                skSurface.ReadPixels(skImageInfo,
                    dstpixels,
                    skImageInfo.RowBytes,
                    (int)_lastTouchPoint.X, (int)_lastTouchPoint.Y);

                // access the color
                touchPointColor = bitmap.GetPixel(0, 0);
            }

            // Painting the Touch point
            using (SKPaint paintTouchPoint = new SKPaint())
            {
                //Add shadow on circle
                SKColor shadowColor = new SKColor(0, 0, 0, 70); // black with alpha transparency set to 70
                                                                // set drop shadow with position x/y of 2, and blur x/y of 4
                paintTouchPoint.ImageFilter = SKImageFilter.CreateDropShadow(2, 2, 8, 8, shadowColor,
                        SKDropShadowImageFilterShadowMode.DrawShadowAndForeground);

                // now draw something on the surface, and the drop shadow will automatically appear
                //surface.Canvas.DrawBitmap(..., 0, 0, paint);


                paintTouchPoint.Style = SKPaintStyle.Fill;
                paintTouchPoint.Color = SKColors.White;
                paintTouchPoint.IsAntialias = true;

                var valueToCalcAgainst = (skCanvasWidth > skCanvasHeight) ? skCanvasWidth : skCanvasHeight;

                var pointerCircleDiameterUnits = PointerCircleDiameterUnits; // 0.6 (Default)
                pointerCircleDiameterUnits = (float)pointerCircleDiameterUnits / 10f; //  calculate 1/10th of that value
                var pointerCircleDiameter = (float)(valueToCalcAgainst * pointerCircleDiameterUnits);

                // Outer circle of the Pointer (Ring)
                skCanvas.DrawCircle(
                    _lastTouchPoint.X,
                    _lastTouchPoint.Y,
                    pointerCircleDiameter / 2, paintTouchPoint);

                // Draw another circle with picked color
                paintTouchPoint.Color = touchPointColor;

                var pointerCircleBorderWidthUnits = PointerCircleBorderUnits; // 0.3 (Default)
                var pointerCircleBorderWidth = (float)pointerCircleDiameter *
                                                        (float)pointerCircleBorderWidthUnits; // Calculate against Pointer Circle

                // Inner circle of the Pointer (Ring)
                skCanvas.DrawCircle(
                    _lastTouchPoint.X,
                    _lastTouchPoint.Y,
                    ((pointerCircleDiameter - pointerCircleBorderWidth) / 2), paintTouchPoint);
            }

            // Set selected color
            PickedColor = touchPointColor.ToFormsColor();
            PickedColorChanged?.Invoke(this, PickedColor);

            //if (Device.RuntimePlatform == Device.Android)
            //{
            //    await System.Threading.Tasks.Task.Delay(2000); 
            //}
            ColorController.App.ColorPaletteFocused = false;
            //App.ColorsPage?.EnableScrollView();
        }

        private void SkCanvasView_OnTouch(object sender, SKTouchEventArgs e)
        {
            ColorController.App.ColorPaletteFocused = true;
            _lastTouchPoint = e.Location;

            var canvasSize = SkCanvasView.CanvasSize;

            // Check for each touch point XY position to be inside Canvas
            // Ignore any Touch event ocurred outside the Canvas region 
            if ((e.Location.X > 0 && e.Location.X < canvasSize.Width) &&
                (e.Location.Y > 0 && e.Location.Y < canvasSize.Height))
            {
                e.Handled = true;

                // update the Canvas as you wish
                SkCanvasView.InvalidateSurface();
            }
        }

        private SKColor[] GetGradientOrder()
        {
            if (GradientColorStyle == GradientColorStyle.ColorsOnlyStyle)
            {
                return new SKColor[]
                {
                            SKColors.Transparent
                };
            }
            else if (GradientColorStyle == GradientColorStyle.ColorsToDarkStyle)
            {
                return new SKColor[]
                {
                            SKColors.Transparent,
                            SKColors.Black
                };
            }
            else if (GradientColorStyle == GradientColorStyle.DarkToColorsStyle)
            {
                return new SKColor[]
                {
                            SKColors.Black,
                            SKColors.Transparent
                };
            }
            else if (GradientColorStyle == GradientColorStyle.ColorsToLightStyle)
            {
                return new SKColor[]
                {
                            SKColors.Transparent,
                            SKColors.White
                };
            }
            else if (GradientColorStyle == GradientColorStyle.LightToColorsStyle)
            {
                return new SKColor[]
                {
                            SKColors.White,
                            SKColors.Transparent
                };
            }
            else if (GradientColorStyle == GradientColorStyle.LightToColorsToDarkStyle)
            {
                return new SKColor[]
                {
                            SKColors.White,
                            SKColors.Transparent,
                            SKColors.Black
                };
            }
            else if (GradientColorStyle == GradientColorStyle.DarkToColorsToLightStyle)
            {
                return new SKColor[]
                {
                            SKColors.Black,
                            SKColors.Transparent,
                            SKColors.White
                };
            }
            else
            {
                return new SKColor[]
                {
                        SKColors.Transparent,
                        SKColors.Black
                };
            }
        }
    }

    public enum GradientColorStyle
    {
        ColorsOnlyStyle,
        ColorsToDarkStyle,
        DarkToColorsStyle,
        ColorsToLightStyle,
        LightToColorsStyle,
        LightToColorsToDarkStyle,
        DarkToColorsToLightStyle
    }

    public enum ColorListDirection
    {
        Horizontal,
        Vertical
    }
    #endregion
} 