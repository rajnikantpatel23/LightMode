using System;
using System.Linq;
using Xamarin.Forms;

namespace ColorController.Controls
{
    public class CustomCollectionView : CollectionView
    {
        private ScrollView _scrollView;
        private int _rowHeigt, _columns;
        private double _previousScrollViewPosition = 0;
        private int _rowCount => Convert.ToInt32(ItemsSource.Cast<object>().ToList().Count);

        [TypeConverter(typeof(ReferenceTypeConverter))]
        public ScrollView ScrollView
        {
            set
            {
                _scrollView = value;
                _scrollView.Scrolled += _scrollView_Scrolled;
            }
        }

        public int RowHeigt
        {
            set => _rowHeigt = Convert.ToInt32(value);
        }

        private void UpdateHeight()
        {
            if (_columns == 0)
            {
                if (ItemsLayout is GridItemsLayout layout)
                    _columns = layout.Span;
                else
                    _columns = 1;
            }

            if (_rowHeigt > 0)
                HeightRequest = (_rowHeigt * _rowCount) / _columns;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            UpdateHeight();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            UpdateHeight();
        }

        private void _scrollView_Scrolled(object sender, ScrolledEventArgs e)
        {
            if (_previousScrollViewPosition < e.ScrollY)
            {
                //scrolled down

            }
            else
            {
                //scrolled up

            }

            double scrollingSpace = _scrollView.ContentSize.Height - _scrollView.Height;
            if (scrollingSpace <= e.ScrollY)
            {
                // Touched bottom view
                RemainingItemsThresholdReachedCommand?.Execute(RemainingItemsThresholdReachedCommandParameter);
            }

            _previousScrollViewPosition = e.ScrollY;
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);
            UpdateHeight();
        }

        protected override void OnChildRemoved(Element child, int oldLogicalIndex)
        {
            base.OnChildRemoved(child, oldLogicalIndex);
            UpdateHeight();
        }
    }
}