using System;
using System.Globalization;
using Xamarin.Forms;

namespace ColorController.Converters
{
    public class SelectedAnimationBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var boolValue = (bool)value;
                if (boolValue)
                {
                    return Color.FromHex("#62f1ff");
                }
                else
                {
                    return Color.FromHex("#E609090B");
                }
            }

            return Color.FromHex("#E609090B");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Color.FromHex("#E609090B");
        }
    }

    public class SelectedItemBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var boolValue = (bool)value;
                if (boolValue)
                {
                    return Color.Black;
                }
                else
                {
                    return Color.Transparent;
                }
            }

            return Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ButtonGlowEffectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
            {
                var boolValue = (Color)value;
                if (boolValue == (Color)App.Current.Resources["ButtonTextColor"])
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    public class InverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is bool)
                {
                    return !(bool)value;
                }
                else if (value is int)
                {
                    return (int)value > 0 ? false : (object)true;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is bool)
                {
                    return !(bool)value;
                }
                else if (value is int)
                {
                    return (int)value > 0 ? false : (object)true;
                }
            }
            return false;
        }
    }

    public class StringCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((parameter as string).ToUpper()[0])
            {
                case 'U':
                    return ((string)value).ToUpper();
                case 'L':
                    return ((string)value).ToLower();
                default:
                    return ((string)value);
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value);
        }
    }

    public class VersionToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value is string)
                {
                    if (string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                    {
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(App.ConnectedControllerVersion))
                    {
                        var animationControllerVersion = new Version(value.ToString());
                        var connectedControllerVersion = new Version(App.ConnectedControllerVersion);
                        if (connectedControllerVersion >= animationControllerVersion)
                        {
                            return false;
                        }
                    } 
                }
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
