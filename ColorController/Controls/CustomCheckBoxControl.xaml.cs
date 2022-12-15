using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomCheckBoxControl : Grid
    { 
        public static readonly BindableProperty CheckBoxStatusProperty = BindableProperty.Create(nameof(CheckBoxStatus), typeof(bool), typeof(CustomCheckBoxControl), false);
        public bool CheckBoxStatus
        {
            get
            {
                return (bool)GetValue(CheckBoxStatusProperty);
            }
            set
            {
                SetValue(CheckBoxStatusProperty, value); 
            }
        }
        public CustomCheckBoxControl()
        {
            InitializeComponent(); 
        } 

        private void CheckboxTapped(object sender, EventArgs e)
        {
            CheckBoxStatus = !CheckBoxStatus; 
        }
    }
}