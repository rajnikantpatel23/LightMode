using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.DataTemplates
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LooperFavoriteDataTemplate : DataTemplate
    {
        public event EventHandler AnimationTappedEvent; 
        public event EventHandler InfoIconTappedEvent;
        public event EventHandler DeleteIconTappedEvent;
        public LooperFavoriteDataTemplate()
        {
            InitializeComponent();
        }
         
        private void InfoIconTapped(object sender, EventArgs e)
        {
            InfoIconTappedEvent?.Invoke(sender, e);
        }

        private void AnimationTapped(object sender, EventArgs e)
        {
            AnimationTappedEvent?.Invoke(sender, e);
        }

        private void DeleteIconTapped(object sender, EventArgs e)
        {
            DeleteIconTappedEvent?.Invoke(sender, e);
        }
    }
}