using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoriteMenuBarControl : Grid
    {
        public event EventHandler InfoIconTappedEvent;
        public event EventHandler DeleteIconTappedEvent;
        public FavoriteMenuBarControl()
        {
            InitializeComponent();
        }

        private void InfoIconTapped(object sender, EventArgs e)
        {
            InfoIconTappedEvent?.Invoke(sender, e);
        }

        private void DeleteIconTapped(object sender, EventArgs e)
        {
            DeleteIconTappedEvent?.Invoke(sender, e);
        }
    }
}