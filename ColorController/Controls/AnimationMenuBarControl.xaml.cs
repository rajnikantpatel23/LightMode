using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnimationMenuBarControl : Grid
    {
        public event EventHandler InfoIconTappedEvent;
        public event EventHandler StarIconTappedEvent;

        public AnimationMenuBarControl()
        {
            InitializeComponent();
        }

        private void InfoIconTapped(object sender, EventArgs e)
        {
            InfoIconTappedEvent?.Invoke(sender, e);
        }

        private void StarIconTapped(object sender, EventArgs e)
        {
            StarIconTappedEvent?.Invoke(sender, e);
        }
    }
}