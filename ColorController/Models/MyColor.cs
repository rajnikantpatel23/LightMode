using SQLite;
using Xamarin.Forms;

namespace ColorController.Models
{
    public class MyColor : BaseModel
    {
        public int Index { get; set; }
        
        public string BaseColorJson { get; set; }
       
        public string ColorText { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        [Ignore]
        public Color Color
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BaseColorJson))
                {
                    var colorModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ColorModel>(BaseColorJson);
                    var color = Color.FromRgba(colorModel.R, colorModel.G, colorModel.B, colorModel.A);
                    return color;
                }

                return Color.Transparent;
            }
        }
    }
}
