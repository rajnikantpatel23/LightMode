using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TitleArrowControl : Grid
    {
        public TitleArrowControl()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(TitleArrowControl), string.Empty);
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
    }
}