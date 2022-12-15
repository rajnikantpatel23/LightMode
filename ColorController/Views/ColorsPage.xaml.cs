using ColorController.Abstractions;
using Xamarin.Forms.Xaml;
using ColorController.ViewModels;
using Xamarin.Forms;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorsPage : BaseContentPage
    {
        private ColorsViewModel _viewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ColorsViewModel();
            App.ColorsPage = this;
        }

        private void SelectedColorTapped(object sender, System.EventArgs e)
        {
            var tappedEventArgs = e as TappedEventArgs;
            _viewModel.SelectedColorTappedCommand.Execute(tappedEventArgs.Parameter);
        }
    }
}