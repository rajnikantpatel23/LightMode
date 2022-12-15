using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using ColorController.Services;
using ColorController.Models;
using Acr.UserDialogs;
using ColorController.Enums;

namespace ColorController.Abstractions
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();


        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public bool IsLoaded { get; internal set; }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public async virtual Task OnPageAppering()
        {
            
        }
         
        public async virtual Task LoadData()
        {
             
        }

        public static async Task OpenPopupPage(PopupPage page)
        {
            if (page != null)
            {
                if (PopupNavigation.Instance.PopupStack.Count == 0)
                {
                    await PopupNavigation.Instance.PushAsync(page);
                }
            }
        }

        public async Task PushToNextPage(Page page)
        {
            if (page != null)
            {
                var latestPage = App.Current.MainPage.Navigation.NavigationStack.LastOrDefault();
                if (latestPage==null || latestPage.GetType() != page.GetType())
                {
                    await App.Current.MainPage.Navigation.PushAsync(page);
                }
            }
        }

        public async Task PushModelNextPage(Page page)
        {
            if (page != null)
            {
                if (App.Current.MainPage.Navigation.ModalStack.Count > 0)
                {
                    var latestPage = App.Current.MainPage.Navigation.ModalStack.LastOrDefault();
                    if (latestPage?.GetType() != page.GetType())
                    {
                        await App.Current.MainPage.Navigation.PushModalAsync(page);
                    }
                }
                else
                {
                    await App.Current.MainPage.Navigation.PushModalAsync(page);
                }

            }
        }

        public async Task PopPageAsync()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        public bool IsInternetConnected()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                return true;
            }
            return false;
        }
        
        public Page FindPageByType(Type type)
        {
            return App.Current.MainPage.Navigation.NavigationStack.FirstOrDefault(x => x.GetType() == type);
        }

        public void ShowToast(string message, ToastPosition toastPosition = ToastPosition.Top)
        {
            var toastConfig = new ToastConfig(message)
            {
                Duration = new TimeSpan(0, 0, 1),
                Position = toastPosition,
                BackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"],
                MessageTextColor = Color.White
            };
            UserDialogs.Instance.Toast(toastConfig);
        }

        private string _textSearching;
        public string TextSearching
        {
            get { return _textSearching; }
            set { _textSearching = value; OnPropertyChanged(nameof(TextSearching)); }
        }

        private bool _ShowConnectingBtn;
        public bool ShowConnectingBtn
        {
            get { return _ShowConnectingBtn; }
            set { _ShowConnectingBtn = value; OnPropertyChanged(nameof(ShowConnectingBtn)); }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
