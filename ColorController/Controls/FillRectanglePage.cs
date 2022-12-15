using ColorController.Helpers;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ColorController.Controls
{
    public class FillRectanglePage : ContentView
    {
        SKBitmap bitmap =
            BitmapExtensions.LoadBitmapResource(typeof(FillRectanglePage),
                                                "ColorController.Resources.Images.Banana.jpg");
        public FillRectanglePage()
        {
            

            SKCanvasView canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnCanvasViewPaintSurface;
            Content = canvasView;
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.DrawBitmap(bitmap, info.Rect);
        }
    }
}
