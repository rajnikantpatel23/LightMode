using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.DataTemplates
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FireballDataTemplate : DataTemplate
    {
        public event EventHandler AnimationTappedEvent;
        public event EventHandler StartIconTappedEvent;
        public event EventHandler InfoIconTappedEvent;
        public FireballDataTemplate()
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