﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Input;

using Foundation;

using Sharpnado.HorizontalListView.iOS.Helpers;
using Sharpnado.Tasks;

using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Sharpnado.HorizontalListView.iOS.Renderers.HorizontalList
{
    public struct MyUIViewCellHolder
    {
        public static MyUIViewCellHolder Empty = new MyUIViewCellHolder(null, null, null);

        private readonly ICommand _tapCommand;

        public MyUIViewCellHolder(Xamarin.Forms.ViewCell formsCell, UIView view, ICommand tapCommand)
        {
            FormsCell = formsCell;
            CellContent = view;
            _tapCommand = tapCommand;

            if (tapCommand != null)
            {
                view.AddGestureRecognizer(new UITapGestureRecognizer(OnTap));
            }
        }

        public Xamarin.Forms.ViewCell FormsCell { get; }

        public UIView CellContent { get; }

        public static bool operator ==(MyUIViewCellHolder c1, MyUIViewCellHolder c2)
        {
            return ReferenceEquals(c1.FormsCell, c2.FormsCell) && ReferenceEquals(c1.CellContent, c2.CellContent);
        }

        public static bool operator !=(MyUIViewCellHolder c1, MyUIViewCellHolder c2)
        {
            return !ReferenceEquals(c1.FormsCell, c2.FormsCell) || !ReferenceEquals(c1.CellContent, c2.CellContent);
        }

        public bool Equals(MyUIViewCellHolder other)
        {
            return Equals(FormsCell, other.FormsCell) && Equals(CellContent, other.CellContent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MyUIViewCellHolder holder && Equals(holder);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FormsCell != null ? FormsCell.GetHashCode() : 0) * 397) ^ (CellContent != null ? CellContent.GetHashCode() : 0);
            }
        }

        private void OnTap()
        {
            if (_tapCommand.CanExecute(null))
            {
                _tapCommand.Execute(FormsCell.BindingContext);
            }
        }
    }

    public class MyViewCell : UICollectionViewCell
    {
        private const int InnerViewTag = 99;

        public MyViewCell(IntPtr p)
            : base(p)
        {
        }

        public bool IsInitialized => FormsCell != null;

        public Xamarin.Forms.ViewCell FormsCell { get; private set; }

        public void Reset()
        {
            if (!IsInitialized)
            {
                return;
            }

            FormsCell = null;
        }

        public void Initialize(Xamarin.Forms.ViewCell formsCell, UIView view, HorizontalListView.RenderedViews.HorizontalListView parent)
        {
            FormsCell = formsCell;

            // A previous view may have been added in a different data source
            ContentView.ViewWithTag(InnerViewTag)?.RemoveFromSuperview();

            view.Tag = InnerViewTag;
            ContentView.AddSubview(view);
        }

        public void Bind(object dataContext, HorizontalListView.RenderedViews.HorizontalListView parent)
        {
            FormsCell.BindingContext = dataContext;
            FormsCell.Parent = parent;
        }
    }

    public class MyViewSource : UICollectionViewDataSource
    {
        private readonly WeakReference<HorizontalListView.RenderedViews.HorizontalListView> _weakElement;

        private readonly List<object> _dataSource;
        private readonly MyUIViewCellHolderQueue _viewCellHolderCellHolderQueue;
        private readonly Dictionary<long, WeakReference<MyViewCell>> _createdCells;
        private readonly List<DataTemplate> _dataTemplates;
        private readonly bool _multipleCellTemplates;

        private CancellationTokenSource _enableDragAndDropCts;

        private int _currentMaxPosition = -1;

        public MyViewSource(HorizontalListView.RenderedViews.HorizontalListView element, List<DataTemplate> dataTemplates)
        {
            _weakElement = new WeakReference<HorizontalListView.RenderedViews.HorizontalListView>(element);
            _createdCells = new Dictionary<long, WeakReference<MyViewCell>>();
            _dataTemplates = dataTemplates;

            var elementItemsSource = element.ItemsSource;
            _dataSource = elementItemsSource?.Cast<object>().ToList();

            if (_dataSource == null)
            {
                return;
            }

            _multipleCellTemplates = element.ItemTemplate is DataTemplateSelector;

            if (!_multipleCellTemplates)
            {
                // Cache only support single DataTemplate
                _viewCellHolderCellHolderQueue = new MyUIViewCellHolderQueue(
                    element.ViewCacheSize,
                    () => CreateViewCellHolder());
                _viewCellHolderCellHolderQueue.Build();
            }
        }

        public void HandleNotifyCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _dataSource.InsertRange(eventArgs.NewStartingIndex, eventArgs.NewItems.Cast<object>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _dataSource.RemoveRange(eventArgs.OldStartingIndex, eventArgs.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Handled in the iOSHorizontalListViewRenderer class, will just create a new adapter
                    break;
                case NotifyCollectionChangedAction.Move:
                    var item = _dataSource[eventArgs.OldStartingIndex];
                    _dataSource.RemoveAt(eventArgs.OldStartingIndex);
                    _dataSource.Insert(eventArgs.NewStartingIndex, item);
                    break;
            }
        }

        public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath) =>
            _weakElement.TryGetTarget(out var element) && element.EnableDragAndDrop;

        public override void MoveItem(UICollectionView collectionView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
        {
            var item = _dataSource[(int)sourceIndexPath.Item];

            _dataSource.RemoveAt((int)sourceIndexPath.Item);
            _dataSource.Insert((int)destinationIndexPath.Item, item);
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override nint GetItemsCount(UICollectionView collectionView, nint section)
        {
            return _dataSource?.Count ?? 0;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            MyViewCell nativeCell;
            if (!_multipleCellTemplates)
            {
                nativeCell = (MyViewCell)collectionView.DequeueReusableCell(nameof(MyViewCell), indexPath);
            }
            else
            {
                var cellType = GetItemTemplateIdentifier(collectionView, indexPath);
                nativeCell = (MyViewCell)collectionView.DequeueReusableCell(cellType, indexPath);
            }

            if (!_createdCells.ContainsKey(nativeCell.GetHashCode()))
            {
                _createdCells.Add(nativeCell.GetHashCode(), new WeakReference<MyViewCell>(nativeCell));
            }

            if (!nativeCell.IsInitialized)
            {
                MyUIViewCellHolder holder;
                if (_weakElement.TryGetTarget(out var element) && _multipleCellTemplates)
                {
                    holder = CreateViewCellHolder(indexPath.Row);
                }
                else
                {
                    holder = _viewCellHolderCellHolderQueue.Dequeue();
                }

                if (holder == MyUIViewCellHolder.Empty)
                {
                    return nativeCell;
                }

                if (_weakElement.TryGetTarget(out var p))
                {
                    nativeCell.Initialize(holder.FormsCell, holder.CellContent, p);
                }
            }

            if (_weakElement.TryGetTarget(out var parent))
            {
                nativeCell.Bind(_dataSource[indexPath.Row], parent);

                if (indexPath.Row > _currentMaxPosition)
                {
                    _currentMaxPosition = indexPath.Row;
                    AnimateCell(nativeCell.FormsCell, parent);
                }
            }

            return nativeCell;
        }

        public void OnEnableDragAndDropUpdated(bool isEnabled)
        {
            _enableDragAndDropCts?.Cancel();
            _enableDragAndDropCts = new CancellationTokenSource();

            if (!isEnabled)
            {
                return;
            }

            foreach (var weakViewCell in _createdCells.Values)
            {
                if (!weakViewCell.TryGetTarget(out var nativeViewCell))
                {
                    continue;
                }

                HandleDragAndDropEnabledAnimation(nativeViewCell.FormsCell);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewCellHolderCellHolderQueue?.Clear();

                foreach (var weakCreatedCell in _createdCells.Values)
                {
                    if (weakCreatedCell.TryGetTarget(out var createdCell))
                    {
                        createdCell.Reset();
                    }
                }

                _createdCells.Clear();
            }

            base.Dispose(disposing);
        }

        private void HandleDragAndDropEnabledAnimation(Xamarin.Forms.ViewCell viewCell)
        {
            if (!_weakElement.TryGetTarget(out var element))
            {
                return;
            }

            if (_enableDragAndDropCts != null
                && !_enableDragAndDropCts.IsCancellationRequested
                && element.EnableDragAndDrop
                && element.DragAndDropEnabledAnimationAsync != null)
            {
                InternalLogger.Debug("HandleDragAndDropEnabledAnimation");

                TaskMonitor.Create(element.DragAndDropEnabledAnimationAsync(viewCell, _enableDragAndDropCts.Token));
            }
        }

        private void AnimateCell(Xamarin.Forms.ViewCell cell, RenderedViews.HorizontalListView element)
        {
            TaskMonitor.Create(
                async () =>
                    {
                        if (element.PreRevealAnimationAsync != null)
                        {
                            await element.PreRevealAnimationAsync(cell);
                        }

                        if (element.RevealAnimationAsync != null)
                        {
                            await element.RevealAnimationAsync(cell);
                        }

                        if (element.PostRevealAnimationAsync != null)
                        {
                            await element.PostRevealAnimationAsync(cell);
                        }
                    });
        }

        private string GetItemTemplateIdentifier(UICollectionView collectionView, NSIndexPath indexPath)
        {
            if (_weakElement.TryGetTarget(out var element) && element.ItemTemplate is DataTemplateSelector dataTemplateSelector)
            {
                var item = _dataSource[(int)indexPath.Item];
                var template = dataTemplateSelector.SelectTemplate(item, element);
                var templateIndex = _dataTemplates.IndexOf(template);
                if (templateIndex == -1)
                {
                    // Rare cases where DataTemplate of the DataTemplateSelectors are not public properties
                    _dataTemplates.Add(template);
                    templateIndex = _dataTemplates.Count - 1;
                    collectionView.RegisterClassForCell(
                        typeof(MyViewCell),
                        IdentifierFormatter.FormatDataTemplateCellIdentifier(templateIndex));
                }

                return IdentifierFormatter.FormatDataTemplateCellIdentifier(templateIndex);
            }

            return nameof(MyViewCell);
        }

        private MyUIViewCellHolder CreateViewCellHolder(int itemIndex = -1)
        {
            Xamarin.Forms.ViewCell formsCell = null;

            if (!_weakElement.TryGetTarget(out var element))
            {
                return MyUIViewCellHolder.Empty;
            }

            if (element.ItemTemplate is DataTemplateSelector selector)
            {
                if (itemIndex == -1)
                {
                    throw new InvalidOperationException("Cannot select a DataTemplate without an itemIndex");
                }

                var template = selector.SelectTemplate(_dataSource[itemIndex], element);
                formsCell = (Xamarin.Forms.ViewCell)template.CreateContent();
            }
            else
            {
                formsCell = (Xamarin.Forms.ViewCell)element.ItemTemplate.CreateContent();
            }

            // formsCell.Parent = element;
            formsCell.View.Layout(new Rectangle(0, 0, element.ItemWidth, element.ItemHeight));

            if (Platform.GetRenderer(formsCell.View) == null)
            {
                IVisualElementRenderer renderer = Platform.CreateRenderer(formsCell.View);
                Platform.SetRenderer(formsCell.View, renderer);
            }

            var nativeView = Platform.GetRenderer(formsCell.View).NativeView;
            nativeView.ContentMode = UIViewContentMode.ScaleAspectFit;

            HandleDragAndDropEnabledAnimation(formsCell);

            return new MyUIViewCellHolder(formsCell, nativeView, element.TapCommand);
        }
    }
}