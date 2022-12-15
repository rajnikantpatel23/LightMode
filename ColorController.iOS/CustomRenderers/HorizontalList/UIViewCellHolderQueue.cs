using System;
using System.Collections.Generic;

namespace Sharpnado.HorizontalListView.iOS.Renderers.HorizontalList
{
    public class MyUIViewCellHolderQueue
    {
        private readonly Func<MyUIViewCellHolder> _viewFactory;
        private readonly Queue<MyUIViewCellHolder> _views;
        private readonly int _initialSize;

        public MyUIViewCellHolderQueue(int initialSize, Func<MyUIViewCellHolder> viewFactory)
        {
            _initialSize = initialSize;
            _viewFactory = viewFactory;
            _views = new Queue<MyUIViewCellHolder>(initialSize);
        }

        public void Build()
        {
            // System.Diagnostics.Debug.WriteLine($"Build: creating {_initialSize} views");
            for (int i = 0; i < _initialSize; i++)
            {
                var view = _viewFactory();
                lock (_views)
                {
                    _views.Enqueue(view);
                }
            }
        }

        public void Clear()
        {
            lock (_views)
            {
                foreach (var view in _views)
                {
                    view.CellContent.Dispose();
                }

                _views.Clear();
            }
        }

        public MyUIViewCellHolder Dequeue()
        {
            lock (_views)
            {
                if (_views.Count > 0)
                {
                    // System.Diagnostics.Debug.WriteLine($"Dequeue: dequeueing cached view ({_views.Count} remaining)");
                    return _views.Dequeue();
                }
            }

            return _viewFactory();
        }
    }
}