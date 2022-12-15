using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertPopupPage : BasePopupPage
    {
        private string _textMessage;
        public string TextMessage
        {
            get { return _textMessage; }
            set { _textMessage = value; OnPropertyChanged(nameof(TextMessage)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AlertPopupPage(string messageText)
        {
            InitializeComponent();
            BindingContext = this;
            TextMessage = messageText;
        }

        private void OkClicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}