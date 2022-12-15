using Xamarin.Forms;

namespace ColorController.Controls
{
    public class TintImage : RoutingEffect
    {
        public Color TintColor { get; private set; }
        public TintImage(Color color) : base($"Rajnikant.{nameof(TintImage)}")
        {
            TintColor = color;
        }
    }
}
