using Rg.Plugins.Popup.Services;
using System;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertPopupPageWithOkCancel : BasePopupPage
    {
        private string _textMessage;
        public string TextMessage
        {
            get { return _textMessage; }
            set { _textMessage = value; OnPropertyChanged(nameof(TextMessage)); }
        }

        private Action<bool> _callBack;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public AlertPopupPageWithOkCancel(string messageText, Action<bool> callBack)
        {
            InitializeComponent();
            BindingContext = this;
            TextMessage = messageText;
            _callBack = callBack;
        }

        private void OkClicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            _callBack(true);
        }

        private void CancelClicked(object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            _callBack(false);
        }
    }
}