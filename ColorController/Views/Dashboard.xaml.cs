using ColorController.Controls;
using ColorController.Resources;
using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Xaml;

namespace ColorController.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard : ExtendedTabbedPage
    {
        /// <summary>
        /// Constructor icon_about
        /// </summary>
        public Dashboard()
        {
            InitializeComponent();
            App.DashboardInstance = this;
            NavigationPage FavoritesPage = new NavigationPage(new FavoritesPage());
            Children.Add(FavoritesPage);
            if (Device.Idiom == TargetIdiom.Phone)
            {
                NavigationPage colorsPage = new NavigationPage(new ColorsPage());
                Children.Add(colorsPage);
            }
            else
            {
                NavigationPage colorsTabPage = new NavigationPage(new ColorsTabPage());
                Children.Add(colorsTabPage);
            }
            NavigationPage AnimationPage = new NavigationPage(new AnimationPage());
            Children.Add(AnimationPage);
            NavigationPage navigationPage = new NavigationPage(new SettingsPage());
            Children.Add(navigationPage);

            if (Device.RuntimePlatform == Device.Android)
            {
                Children[0].IconImageSource = new FontImageSource
                {
                    Glyph = TabIconFont.IconFontStarFilled,
                    Size = 30,
                    FontFamily = "tabicons",
                    Color = (Color)App.Current.Resources["TabSelectedColor"]
                };

                Children[1].IconImageSource = new FontImageSource
                {
                    Glyph = TabIconFont.IconFontColerPalette,
                    Size = 30,
                    FontFamily = "tabicons",
                    Color = (Color)App.Current.Resources["TabUnselectedColor"]
                };

                Children[2].IconImageSource = new FontImageSource
                {
                    Glyph = TabIconFont.IconFontAnimations,
                    Size = 30,
                    FontFamily = "tabicons",
                    Color = (Color)App.Current.Resources["TabUnselectedColor"]
                };

                Children[3].IconImageSource = new FontImageSource
                {
                    Glyph = TabIconFont.IconFontSettings,
                    Size = 30,
                    FontFamily = "tabicons",
                    Color = (Color)App.Current.Resources["TabUnselectedColor"]
                };
            }
            else
            {
                Children[0].IconImageSource = "star.png";
                Children[0].BackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"]; 
                Children[1].IconImageSource = "color.png";
                Children[1].BackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"]; 
                Children[2].IconImageSource = "animation.png";
                Children[2].BackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"]; 
                Children[3].IconImageSource = "settings.png";
                Children[3].BackgroundColor = (Color)App.Current.Resources["BarBackgroundColor"];
            }
        }

        /// <summary>
        /// This will call on Tab changed
        /// </summary>
        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            try
            {
                if (CurrentPage.GetType() == typeof(NavigationPage) && ((NavigationPage)CurrentPage).RootPage.GetType() == typeof(FavoritesPage))
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[0].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontStarFilled,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabSelectedColor"]
                        };
                    }
                    else
                    {
                        Children[0].IconImageSource = "star.png";  
                    }
                }
                else
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[0].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontStarFilled,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabUnselectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[0].IconImageSource = "star.png"; 
                    }
                }
            }
            catch (Exception)
            {

            }

            try
            {
                if ((CurrentPage.GetType() == typeof(NavigationPage) && ((NavigationPage)CurrentPage).RootPage.GetType() == typeof(ColorsPage))|| 
                    (CurrentPage.GetType() == typeof(NavigationPage) && ((NavigationPage)CurrentPage).RootPage.GetType() == typeof(ColorsTabPage)))
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[1].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontColerPalette,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabSelectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[1].IconImageSource = "color.png"; 
                    }
                }
                else
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[1].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontColerPalette,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabUnselectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[1].IconImageSource = "color.png"; 
                    }
                }
            }
            catch (Exception)
            {

            }

            try
            {
                if (CurrentPage.GetType() == typeof(NavigationPage) && ((NavigationPage)CurrentPage).RootPage.GetType()== typeof(AnimationPage))
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[2].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontAnimations,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabSelectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[2].IconImageSource = "animation.png"; 
                    }
                }
                else
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[2].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontAnimations,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabUnselectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[2].IconImageSource = "animation.png"; 
                    }
                }
            }
            catch (Exception)
            {

            }

            try
            {
                if (CurrentPage.GetType() == typeof(NavigationPage) && ((NavigationPage)CurrentPage).RootPage.GetType()== typeof(SettingsPage))
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[3].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontSettings,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabSelectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[3].IconImageSource = "settings.png";
                    }
                }
                else
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        Children[3].IconImageSource = new FontImageSource
                        {
                            Glyph = TabIconFont.IconFontSettings,
                            Size = 30,
                            FontFamily = "tabicons",
                            Color = (Color)App.Current.Resources["TabUnselectedColor"]
                        }; 
                    }
                    else
                    {
                        Children[3].IconImageSource = "settings.png";
                    }
                }
            }
            catch (Exception)
            {

            }

            //if (CurrentPage.GetType() == typeof(ColorsPage))
            //{
            //    EnableDisableTabSwipeView(false);
            //}
            //else
            //{
            //    EnableDisableTabSwipeView(false);
            //}
        }

        public void EnableDisableTabSwipeView(bool isEnable = true)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                On<Xamarin.Forms.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(isEnable);
            }
        }
    }
}