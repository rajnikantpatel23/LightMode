using ColorController.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ColorController.ViewModels;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorsTabPage : BaseContentPage
    {
        private ColorsViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorsTabPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ColorsViewModel();
            App.ColorsPage = this; 
        }
   
        private void SelectedColorTapped(object sender, EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            _viewModel.SelectedColorTappedCommand.Execute(tappedEventArgs.Parameter);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > height)
            {
                //Landscape 
                StackLayoutMainTabLandscape.IsVisible = true;
                StackLayoutMainTabProtrait.IsVisible = false;
            }
            else
            {
                //Portrait 
                StackLayoutMainTabLandscape.IsVisible = false;
                StackLayoutMainTabProtrait.IsVisible = true;
            }
        }
    }
}