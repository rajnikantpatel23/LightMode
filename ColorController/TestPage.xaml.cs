using ColorController.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        private ObservableCollection<FavoriteAnimation> _animations;
        public ObservableCollection<FavoriteAnimation> Animations
        {
            get { return _animations; }
            set { _animations = value; OnPropertyChanged(nameof(Animations)); }
        }

        public TestPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected async override void OnAppearing()
        {
            var favoriteAnimations = await App.Database.GetFavoriteAnimationsAsync();
            Animations = new ObservableCollection<FavoriteAnimation>(favoriteAnimations);
        }
    }
}