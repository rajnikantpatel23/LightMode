using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using ColorController;
using ColorController.ViewModels;
using ColorController.Views;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace Spillman.Xamarin.Forms.ColorPicker
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isAlphaEnabled = true;
        public bool IsAlphaEnabled
        {
            get => _isAlphaEnabled;
            set => SetProperty(ref _isAlphaEnabled, value);
        }

        private Color _selectedColor;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); }
        }
         
        public Color Color
        {
            get => SKColor.ToFormsColor();
            set
            {
                var skColor = value.ToSKColor();
                skColor.ToHsv(out var h, out var s, out var v);

                A = skColor.Alpha;
                H = h;
                S = s;
                V = v;
            }
        }

        // ReSharper disable once InconsistentNaming
        public SKColor SKColor => SKColorUtil.FromHsv(H, S, 100, A);

        public Color HueColor => SKColorUtil.FromHsv(H, 100, 100).ToFormsColor();

        private string _hex = "000000";
        public string Hex
        {
            get => _hex;
            set
            {
                var stringBuilder = new StringBuilder();
                foreach (var c in value)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                    {
                        stringBuilder.Append(c);
                    }
                }

                value = stringBuilder.ToString();

                if (SetProperty(ref _hex, value) && value.Length >= 3)
                {
                    var color = Color.FromHex(value);
                    if (color == Color.Default)
                    {
                        color = Color.White;
                    }

                    var skColor = color.ToSKColor();

                    skColor.ToHsv(out var h, out var s, out var v);

                    var hChanged = SetProperty(ref _h, h, nameof(H));
                    var sChanged = SetProperty(ref _s, s, nameof(S));
                    var vChanged = SetProperty(ref _v, v, nameof(V));

                    if (hChanged || sChanged || vChanged)
                    {
                        OnPropertyChanged(nameof(Color));
                    }

                    if (hChanged)
                    {
                        OnPropertyChanged(nameof(HueColor));
                    }
                }
            }
        }

        private byte _a = byte.MaxValue;
        public byte A
        {
            get => _a;
            set
            {
                if (SetProperty(ref _a, value))
                {
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private float _h;
        public float H
        {
            get => _h;
            set
            {
                if (SetProperty(ref _h, value))
                {
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(HueColor));
                }
            }
        }

        private float _s;
        public float S
        {
            get => _s;
            set
            {
                if (SetProperty(ref _s, value))
                {
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        private float _v;
        public float V
        {
            get => _v;
            set
            {
                if (SetProperty(ref _v, value))
                {
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        public void UpdateHex()
        {
            SetProperty(ref _hex, Color.ToRgbHex(), nameof(Hex));
            //ColorController.App.ColorsPage.ColorPickerPickedColorChanged(null, Color.FromHex($"#{_hex}"));
            if (App.ColorsPage.GetType()==typeof(ColorsPage))
            {
                var viewModel = ((ColorsPage)App.ColorsPage).BindingContext as ColorsViewModel;
                viewModel.ColorPickerPickedColorChanged(null, Color);
            }
            if (App.ColorsPage.GetType() == typeof(ColorsTabPage))
            {
                var viewModel = ((ColorsTabPage)App.ColorsPage).BindingContext as ColorsViewModel;
                viewModel.ColorPickerPickedColorChanged(null, Color);
            }
        }

        private bool SetProperty<T>(ref T obj, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(obj, value))
            {
                return false;
            }

            obj = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
