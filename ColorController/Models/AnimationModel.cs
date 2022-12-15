using ColorController.Enums;
using ProdKart.Resources;
using SQLite;
using System;
using Xamarin.Forms;

namespace ColorController.Models
{
    public class AnimationModel : BaseModel
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; OnPropertyChanged(nameof(Index)); }
        }

        public string Title { get; set; }

        public string Detail { get; set; }
        public string Command { get; set; }
        public string Code { get; set; }

        public AnimationType AnimationType { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
                OnPropertyChanged(nameof(StarFillColor));
                OnPropertyChanged(nameof(StarImage));
            }
        }

        public string BaseColorJson { get; set; }
        public string FileName { get; set; }

        public string ControllerVersion { get; set; }
      
        private bool _isShieldVisible;
        public bool IsShieldVisible
        {
            get { return _isShieldVisible; }
            set
            {
                _isShieldVisible = value;
                OnPropertyChanged(nameof(IsShieldVisible)); 
            }
        }

        [Ignore]
        public Color SelectedColor1
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BaseColorJson))
                {
                    var colorModel =  Newtonsoft.Json.JsonConvert.DeserializeObject<ColorModel>(BaseColorJson);
                    var a = Color.FromRgba(colorModel.R, colorModel.G, colorModel.B, colorModel.A);
                    //var b = a.WithHue(a.Hue);
                    //var c = b.WithSaturation(b.Saturation);
                    //var d = c.WithSaturation(c.Luminosity);
                    return a;
                }

                return Color.Red;
            }
        }

        [Ignore]
        public Color SelectedColor2
        {
            get
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(BaseColorJson))
                    {
                        var colorModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ColorModel>(BaseColorJson);
                        //Get base color
                        var baseColor = Color.FromRgba(colorModel.R, colorModel.G, colorModel.B, colorModel.A);
                        //Get 2nd color 240 degree away
                        var color2 = ColorHelper.GetColorByAngle(baseColor, 240);
                        return color2;
                    }
                }
                catch (Exception)
                {

                }

                return Color.Blue;
            }
        }

        [Ignore]
        public Color StarFillColor => IsFavorite == true ? Color.FromHex("#FFFFFF") : Color.FromHex("#FF979797");

        [Ignore]
        public string StarImage => IsFavorite == true ? IconFont.IconFontStar : IconFont.IconFontStarEmpty;
    }
}
