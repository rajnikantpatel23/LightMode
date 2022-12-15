using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.DataTemplates
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WaveTemplate : DataTemplate
    {
        public event EventHandler AnimationTappedEvent;
        public event EventHandler StartIconTappedEvent;
        public event EventHandler InfoIconTappedEvent;
        public WaveTemplate()
        {
            InitializeComponent();
        }

        private void StarIconTapped(object sender, EventArgs e)
        {
            StartIconTappedEvent?.Invoke(sender, e);
        }

        private void InfoIconTapped(object sender, EventArgs e)
        {
            InfoIconTappedEvent?.Invoke(sender, e);
        }

        private void AnimationTapped(object sender, EventArgs e)
        {
            AnimationTappedEvent?.Invoke(sender, e);
        }
    }
}