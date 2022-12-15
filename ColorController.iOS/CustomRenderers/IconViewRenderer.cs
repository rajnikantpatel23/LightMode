﻿using System.ComponentModel;
using Xamarin.Forms;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using CoreGraphics;
using ColorController.Controls;
using ColorController.iOS.CustomRenderers;

[assembly: ExportRendererAttribute(typeof(IconView), typeof(IconViewRenderer))]

namespace ColorController.iOS.CustomRenderers
{
    public class IconViewRenderer : ViewRenderer<IconView, UIImageView>
    {
        private bool _isDisposed;

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing && base.Control != null)
            {
                UIImage image = base.Control.Image;
                UIImage uIImage = image;
                if (image != null)
                {
                    uIImage.Dispose();
                    uIImage = null;
                }
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<IconView> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                UIImageView uIImageView = new UIImageView(CGRect.Empty)
                {
                    ContentMode = UIViewContentMode.ScaleAspectFit,
                    ClipsToBounds = true
                };
                SetNativeControl(uIImageView);
            }
            if (e.NewElement != null)
            {
                SetImage(e.OldElement);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == IconView.SourceProperty.PropertyName)
            {
                SetImage(null);
            }
            else if (e.PropertyName == IconView.ForegroundProperty.PropertyName)
            {
                SetImage(null);
            }
        }

        private void SetImage(IconView previous = null)
        {
            try
            {
                if (previous == null && !string.IsNullOrWhiteSpace(Element.Source))
                {
                    var uiImage = new UIImage(Element.Source);
                    uiImage = uiImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

                    //uiImage.ApplyTintColor(Element.Foreground.ToUIColor());

                    Control.TintColor = Element.Foreground.ToUIColor();
                    Control.Image = uiImage;
                    if (!_isDisposed)
                    {
                        ((IVisualElementController)Element).NativeSizeChanged();
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}