using ColorController.Models;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnimationInfoPopupPage : BasePopupPage
    {
        private AnimationModel _animationModel;
        public AnimationModel AnimationModel
        {
            get { return _animationModel; }
            set { _animationModel = value; OnPropertyChanged(nameof(AnimationModel)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationInfoPopupPage(AnimationModel animationModel)
        {
            InitializeComponent();
            BindingContext = this;
            AnimationModel = animationModel;
        }

        /// <summary>
        /// Will be called when Ok button tabbed
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private void OkTapped(object sender, EventArgs e)
        {
            Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
        }
    }
}