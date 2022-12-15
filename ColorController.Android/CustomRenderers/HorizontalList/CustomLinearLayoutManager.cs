using System;

using Android.Content;
using Android.Runtime;

#if __ANDROID_29__
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Sharpnado.HorizontalListView.Droid.Renderers.HorizontalList
{
    public class MyCustomLinearLayoutManager : LinearLayoutManager
    {
        public MyCustomLinearLayoutManager(IntPtr javaReference, JniHandleOwnership transfer)
            : base(
            javaReference,
            transfer)
        {
        }

        public MyCustomLinearLayoutManager(Context context, int orientation, bool reverseLayout)
            : base(
            context,
            orientation,
            reverseLayout)
        {
        }

        public bool CanScroll { get; set; }

        public override bool CanScrollHorizontally()
        {
            return CanScroll && base.CanScrollHorizontally();
        }
    }
}