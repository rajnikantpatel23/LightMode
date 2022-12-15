using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.PopupPages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectedPopupPage : BasePopupPage
    {
        private bool _runTimer;

        private string _textMessage;
        public string TextMessage
        {
            get { return _textMessage; }
            set { _textMessage = value; OnPropertyChanged(nameof(TextMessage)); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectedPopupPage()
        {
            InitializeComponent();
            BindingContext = this;
            TextMessage = Preferences.Get("defaultControllerName", string.Empty);
            _runTimer = true;
            StartSearchingTimer();
        }

        public void StartSearchingTimer()
        {
            Task.Run(() =>
            {
                Device.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    try
                    {
                        _runTimer = false;
                        PopupNavigation.Instance.PopAsync();
                    }
                    catch (Exception)
                    {
                         
                    }
                    return _runTimer;
                });
            });
        }
    }
}