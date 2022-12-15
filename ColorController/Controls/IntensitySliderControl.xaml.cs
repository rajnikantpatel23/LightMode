using Acr.UserDialogs;
using ColorController.Enums;
using ColorController.Helpers;
using ColorController.Services;
using ColorController.StringResources;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IntensitySliderControl : Grid
    {
        public IBlueToothService BlueToothService => DependencyService.Get<IBlueToothService>();

        private bool _isClicked;

        private double _sliderValuer;
        public double SliderValue
        {
            get { return _sliderValuer; }
            set
            {
                _sliderValuer = value;
                OnPropertyChanged(nameof(SliderValue));
                try
                {
                    if (SliderValue > 1 && SliderValue < 2)
                    {
                        StandardTapped(null, null);  
                    }
                    else if (SliderValue > 2)
                    {
                        HighTapped(null, null);
                    }
                    else
                    {
                        LowTapped(null, null);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public static readonly BindableProperty IntensityValueProperty = BindableProperty.Create("IntensityValue", typeof(Intensity), typeof(IntensitySliderControl), Intensity.Low);

        public Intensity IntensityValue
        {
            get { return (Intensity)GetValue(IntensityValueProperty); }
            set { SetValue(IntensityValueProperty, value); }
        }


        private double _sliderWidth;
        public double SliderWidth
        {
            get { return _sliderWidth; }
            set { _sliderWidth = value; OnPropertyChanged(nameof(SliderWidth)); }
        }

        public IntensitySliderControl()
        {
            InitializeComponent();
            SetIntensityControlValue();
        }

        private void SetIntensityControlValue()
        {
            var britCommand = Preferences.Get("LastPlayedBritCommand", null);
            if (!string.IsNullOrWhiteSpace(britCommand))
            {
                switch (britCommand)
                {
                    case "BRIT 015":
                        LowTapped(null, null);
                        break;
                    case "BRIT 040":
                        StandardTapped(null, null);
                        break;
                    case "BRIT 080":
                        HighTapped(null, null);
                        break;
                    default:
                        StandardTapped(null, null);
                        break;
                }
            }
            else
            {
                StandardTapped(null, null);
            }
        }

        private async void HighTapped(object sender, EventArgs e)
        {
            ONHighIntensity();
            ONStandardIntensity();
            ONLowIntensity();

            LabelLow.Opacity = 0.2;
            LabelStandard.Opacity = 0.2;
            LabelHigh.Opacity = 1;
            slider.Value = 2.5;
            await SendBRITCommandToController("BRIT 080");
        }

        private async void StandardTapped(object sender, EventArgs e)
        {
            ONStandardIntensity();
            ONLowIntensity();
            OffHighIntensity();

            LabelLow.Opacity = 0.2;
            LabelStandard.Opacity = 1;
            LabelHigh.Opacity = 1;

            slider.Value = 1.5;
            await SendBRITCommandToController("BRIT 040");
        }

        private async void LowTapped(object sender, EventArgs e)
        {
            ONLowIntensity();
            OffHighIntensity();
            OffStandardIntensity();

            LabelLow.Opacity = 1;
            LabelStandard.Opacity = 1;
            LabelHigh.Opacity = 1;
            slider.Value = 0.5;
            await SendBRITCommandToController("BRIT 015");
        }

        private void OffHighIntensity()
        {
            TabHighSelected.IsVisible = false;
            TabHighUnselected.IsVisible = true;
            LabelHigh.TextColor = Color.White;
        }

        private void OffLowIntensity()
        {
            TabLowSelected.IsVisible = false;
            TabLowUnselected.IsVisible = true;
            LabelLow.TextColor = Color.White;
        }

        private void OffStandardIntensity()
        {
            TabStandardSelected.IsVisible = false;
            TabStandardUnselected.IsVisible = true;
            LabelStandard.TextColor = Color.White;
        }

        private void ONHighIntensity()
        {
            TabHighSelected.IsVisible = true;
            TabHighUnselected.IsVisible = false;
            LabelHigh.TextColor = Color.Black;
        }

        private void ONStandardIntensity()
        {
            TabStandardSelected.IsVisible = true;
            TabStandardUnselected.IsVisible = false;
            LabelStandard.TextColor = Color.Black;
        }

        private void ONLowIntensity()
        {
            TabLowSelected.IsVisible = true;
            TabLowUnselected.IsVisible = false;
            LabelLow.TextColor = Color.Black;
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            StandardTapped(null, null);
        }

        private async Task SendBRITCommandToController(string command)
        {
            try
            {
                if (_isClicked)
                    return;
                _isClicked = true;

                UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                await BlueToothService.SendCommandToController(command);
            }
            catch (Exception)
            {
            }
            finally
            {
                _isClicked = false;
                UserDialogs.Instance.HideLoading();
                Preferences.Set("LastPlayedBritCommand", command);
            }
        }
    }

    public enum Intensity
    {
        Low,
        Standard,
        High
    }
}