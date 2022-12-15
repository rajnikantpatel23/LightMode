using ColorController.Controls;
using ColorController.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//[assembly: ExportRenderer(typeof(ExtendedTabbedPage), typeof(SwipeTabbedRenderer))]
namespace ColorController.iOS.CustomRenderers
{
    class SwipeTabbedRenderer : TabbedRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NativeView.AddGestureRecognizer(new UISwipeGestureRecognizer(() => SelectNextTab(1)) { Direction = UISwipeGestureRecognizerDirection.Left, ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously });
            NativeView.AddGestureRecognizer(new UISwipeGestureRecognizer(() => SelectNextTab(-1)) { Direction = UISwipeGestureRecognizerDirection.Right, ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously });
        }

        void SelectNextTab(int direction)
        {
            //Return if user is selecting color from Palette
            if (App.ColorPaletteFocused)
                return;

            int nextIndex = TabbedPage.GetIndex(Tabbed.CurrentPage) + direction;
            if (nextIndex < 0 || nextIndex >= Tabbed.Children.Count) return;
            var nextPage = Tabbed.Children[nextIndex];
            UIView.Transition(Platform.GetRenderer(Tabbed.CurrentPage).NativeView, Platform.GetRenderer(nextPage).NativeView, 0.15, UIViewAnimationOptions.TransitionCrossDissolve, null);
            Tabbed.CurrentPage = nextPage;
        }

        static bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer) => gestureRecognizer != otherGestureRecognizer;
    }
}