using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ColorController.Controls
{
    public interface IScrollViewDependencyService
    {
        void Scrolling(bool isScrolling, object nativeObject);
    }

    public class CustomScrollView : ScrollView
    {
        public object NativeObject { get; set; }
        public bool IsScrolling { get; set; } = true;
    }
}