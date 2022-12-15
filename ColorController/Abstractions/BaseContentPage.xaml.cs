using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ColorController.Abstractions
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BaseContentPage : ContentPage
    {
        public BaseContentPage()
        {
            try
            {
                //BackgroundColor = (Color)App.Current.Resources["PageBackgroundColor"];
                //SetBinding(IconImageSourceProperty, new Binding("Icon"));
                //SetBinding(TitleProperty, new Binding("Title"));
                NavigationPage.SetHasNavigationBar(this, false);
            }
            catch (Exception)
            {

            }
        }

        public void PopThisPage()
        {
            Navigation.PopAsync();
        }

        public void RemoveThisPage()
        {
            Navigation.RemovePage(this);
        }

        protected async override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                var viewModel = (BaseViewModel)BindingContext;

                if (viewModel != null && !viewModel.IsLoaded)
                {
                    viewModel.IsLoaded = true;
                    viewModel.IsBusy = true;
                    await viewModel.LoadData();
                    viewModel.IsBusy = false;
                }

                if (viewModel != null)
                {
                    await viewModel.OnPageAppering();
                }
            }
            catch (Exception)
            {
                 
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.Count > 0)
            {
                foreach (var popupPage in Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack)
                {
                    Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
                }
            }
            return base.OnBackButtonPressed();
        }
    }
}