using System; 
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomSwitchControl : Grid
    {
        public static readonly BindableProperty SwitchStatusProperty = BindableProperty.Create(nameof(SwitchStatus), typeof(bool), typeof(CustomSwitchControl), false);
        public bool SwitchStatus
        {
            get { return (bool)GetValue(SwitchStatusProperty); }
            set
            {
                SetValue(SwitchStatusProperty, value);
            }
        }

        public CustomSwitchControl()
        {
            InitializeComponent();
            if (SwitchStatus)
            {
                SwitchOn(); 
            }
            else
            {
                SwitchOff();
            }
        }

        public void SetSwitchStatus(bool value)
        {
            if (value)
            {
                SwitchOn();
            }
            else
            {
                SwitchOff();
            }
        }

        private void SliderControl_DragCompleted(object sender, EventArgs e)
        {
            var slider = sender as Slider;  
            if (slider.Value < 0.5)
            {
                SwitchOff();
            }
            else
            {
                SwitchOn();
            }
        }

        private void SwitchOn()
        {
            SwitchStatus = true;
            thumbOff.IsVisible = !SwitchStatus;
            thumbOn.IsVisible = SwitchStatus;

            sliderBack.BackgroundColor = Color.FromHex("#FF4DDDFF");
            sliderBack.Border.Color = Color.FromHex("#FF4DDDFF");
            sliderShadow.Color = Color.FromHex("#FF4DDDFF"); 
        }

        private void SwitchOff()
        {
            SwitchStatus = false;
            thumbOff.IsVisible = !SwitchStatus;
            thumbOn.IsVisible = SwitchStatus;

            sliderBack.BackgroundColor = Color.White;
            sliderBack.Border.Color = Color.FromHex("#FF262629");
            sliderShadow.Color = Color.Transparent; 
        }

        private void SwitchTapped(object sender, EventArgs e)
        {
            if (SwitchStatus)
            {
                SwitchOff();
            }
            else
            {
                SwitchOn();
            }
        }
    }
}