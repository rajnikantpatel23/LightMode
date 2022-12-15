using Acr.UserDialogs;
using ColorController.Abstractions;
using ColorController.Controls;
using ColorController.Helpers;
using ColorController.Models;
using ColorController.StringResources;
using ColorController.Views;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ColorController.ViewModels
{
    public class ColorsViewModel : BaseViewModel
    {
        private MyColor _selectedTileColor;
        public ICommand DragAndDropEndedCommand { get; set; }
        public ICommand SelectedColorTappedCommand { get; set; }
        public ICommand RemoveTappedCommand { get; set; }
        public ICommand SaveTappedCommand { get; set; }

        private Intensity _selectedIntensityValue;
        public Intensity SelectedIntensityValue
        {
            get { return _selectedIntensityValue; }
            set { _selectedIntensityValue = value; OnPropertyChanged(nameof(SelectedIntensityValue)); }
        }

        private double _sliderWidth;
        public double SliderWidth
        {
            get { return _sliderWidth; }
            set { _sliderWidth = value; OnPropertyChanged(nameof(SliderWidth)); }
        }

        private ObservableCollection<MyColor> _savedColors;
        private bool _isClicked;

        public ObservableCollection<MyColor> MyColors
        {
            get { return _savedColors; }
            set { _savedColors = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="colorsPage">ColorsPage object</param>
        public ColorsViewModel()
        {
            DragAndDropEndedCommand = new Command(DragAndDropEnded);
            SelectedColorTappedCommand = new Command(SelectedColorTapped);
            RemoveTappedCommand = new Command(RemoveTapped);
            SaveTappedCommand = new Command(SaveTapped);
            SubscribeBLEHelperToReceiveConnectionStatusNotifications();
        }

        /// <summary>
        /// Will be called when Add button is clicked
        /// </summary>
        /// <param name="obj">Command Parameter</param>
        private async void SaveTapped(object obj)
        {
            if (!App.IsColorPickerTouchedManually)
            {
                App.CurrentSelectedColor = Color.Red;
            }

            if (MyColors == null)
            {
                MyColors = new ObservableCollection<MyColor>();
            }
            var color = new MyColor
            {
                Id = Guid.NewGuid().ToString(),
                BaseColorJson = JsonConvert.SerializeObject(App.CurrentSelectedColor),
                ColorText = App.CurrentSelectedColor.ToHex(),
                Index = MyColors.Count
            };
            MyColors.Add(color);
            App.CurrentIndex = MyColors.Count;

            //Add color to local DB
            await App.Database.SaveMyColorAsync(color);
        }

        /// <summary>
        /// Will be called when Remove button tapped
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event argument</param>
        private async void RemoveTapped(object obj)
        {
            try
            {
                if (_selectedTileColor != null)
                {
                    MyColors.Remove(_selectedTileColor);
                    await App.Database.DeleteMyColorAsync(_selectedTileColor);
                }
            }
            catch (Exception)
            {
 
            }
        }

        /// <summary>
        /// Will be called when a color is selected from color grid
        /// </summary>
        /// <param name="obj">Object parameter</param>
        private async void SelectedColorTapped(object obj)
        {
            if (!_isClicked)
            {
                _isClicked = true;
                try
                {
                    UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                    _selectedTileColor = obj as MyColor;

                    await CommandHelper.SendHueSaturationToController(_selectedTileColor.Color);

                    foreach (var item in MyColors.Where(x => x != _selectedTileColor))
                    {
                        item.IsSelected = false;
                    }
                    _selectedTileColor.IsSelected = !_selectedTileColor.IsSelected;
                    if (!_selectedTileColor.IsSelected)
                    {
                        _selectedTileColor = null;
                    }

                    App.CurrentSelectedColor = _selectedTileColor.Color;
                    ReloadAnimationPageList();
                }
                catch (Exception)
                {
                }
                finally
                {
                    UserDialogs.Instance.HideLoading();
                }
                _isClicked = false;
            }
        }

        /// <summary>
        /// Call this method to receive connection notifications from BLEHelper 
        /// </summary>
        private void SubscribeBLEHelperToReceiveConnectionStatusNotifications()
        {
            MessagingCenter.Unsubscribe<BLEHelper, string>(this, StringResource.Connection);
            MessagingCenter.Subscribe<BLEHelper, string>(this, StringResource.Connection, (sender, args) =>
            {
                switch (args)
                {
                    case "ShowSearching":
                        break;

                    case "ShowConnect":
                        break;

                    case "ShowConnecting":
                        break;

                    case "ShowDisconnect":
                        break;

                    case "QUIT":
                        break;

                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// Will be called on Page appearing
        /// </summary>
        /// <returns>Task</returns>
        public async override Task LoadData()
        {
            var myColors = await App.Database.GetMyColorsAsync();
            MyColors = new ObservableCollection<MyColor>(myColors.OrderBy(x => x.Index));
            App.CurrentIndex = MyColors.Count;
        }

        /// <summary>
        /// This will be call just after Drag-and-Drop completion to sort the Index of items & update in local DB
        /// </summary>
        /// <param name="obj"></param>
        private async void DragAndDropEnded(object obj)
        {
            var myColors = MyColors.Select((myColor, index) => new MyColor
            {
                Id = myColor.Id,
                Index = index,
                BaseColorJson = myColor.BaseColorJson,
                ColorText = myColor.ColorText,
                IsSelected = false
            }).ToList();

            await App.Database.UppdateMyColors(myColors);
        }

        /// <summary>
        /// Will be called when Color palette focused
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="color">Picked color</param>
        internal async void ColorPickerPickedColorChanged(object sender, Color color)
        {
            if (App.IsColorPickerTouchedManually)
            {
                try
                {
                    UserDialogs.Instance.ShowLoading(StringResource.PleaseWait);
                    await CommandHelper.SendHueSaturationToController(color);
                    App.CurrentSelectedColor = color;
                    ReloadAnimationPageList();
                }
                catch (Exception)
                {
                }
                finally
                {
                    UserDialogs.Instance.HideLoading();
                }
            }
        }

        private void ReloadAnimationPageList()
        {
            var dashboard = App.Current.MainPage as Dashboard;
            if (dashboard != null)
            {
                var extendedTabbedPage = dashboard as ExtendedTabbedPage;
                if (extendedTabbedPage != null)
                {
                    var page = extendedTabbedPage.Children.FirstOrDefault(x => x.GetType() == typeof(NavigationPage) && ((NavigationPage)x).RootPage.GetType() == typeof(AnimationPage));
                    if (page != null)
                    {
                        var animationViewModel = ((NavigationPage)page).RootPage.BindingContext as AnimationViewModel;
                        if (animationViewModel != null)
                        {
                            animationViewModel._refreshList = true;
                        }
                    }
                }
            }
        }
    }
}
