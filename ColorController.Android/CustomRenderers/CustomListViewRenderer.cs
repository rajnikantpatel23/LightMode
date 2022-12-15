using System;
using Android.Content;
using Android.Runtime;
using FormsEssentials.Controls;
using FormsEssentials.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidListView = Android.Widget.AbsListView;
using AndroidScrollState = Android.Widget.ScrollState;

[assembly: ExportRenderer(typeof(CustomListView), typeof(CustomListViewRenderer))]
namespace FormsEssentials.Droid
{
	public class CustomListViewRenderer : ListViewRenderer
	{
		CustomListView view;

		public CustomListViewRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);
			view = (CustomListView)Element;

			if (view == null)
			{
				return;
			}

			view.EventScrollToTop += View_EventScrollToTop;
			view.EventScrollToGroup += View_EventScrollToGroup;
			view.EventScrollToBottom += View_EventScrollToBottom;
			DisableSelectedItem();

			Control.NestedScrollingEnabled = true;
			Control.VerticalScrollBarEnabled = false;
			Control.SetOnScrollListener(new PixelScrollDetector(this));
		}

		private void View_EventScrollToBottom(object sender, EventArgs e)
		{
			Control.SmoothScrollToPosition(Control.MaxScrollAmount);
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CustomListView.DisableSelectionProperty.PropertyName)
			{
				DisableSelectedItem();
			}
		}

		void View_EventScrollToTop(object sender, EventArgs e)
		{
			Control.SmoothScrollToPositionFromTop(0, 0);
		}

		void View_EventScrollToGroup(int groupPosition)
		{
			Control.SmoothScrollToPositionFromTop(groupPosition == 0 ? groupPosition : (groupPosition + 1), 10);
		}

		private void DisableSelectedItem()
		{
			if (view.DisableSelection)
			{
				Control.SetSelector(Android.Resource.Color.Transparent);
				Control.CacheColorHint = Android.Graphics.Color.Transparent;
			}
		}

		private class PixelScrollDetector : Java.Lang.Object, AndroidListView.IOnScrollListener
		{
			private ListView _element;
			private float _density;
			private int _contentOffset = 0;

			private TrackElement[] _trackElements =
			{
				new TrackElement(0),    // Top view, bottom Y
                new TrackElement(1),    // Mid view, bottom Y
                new TrackElement(2),    // Mid view, top Y
                new TrackElement(3)     // Bottom view, top Y
            };

			public PixelScrollDetector(ListViewRenderer renderer)
			{
				_element = renderer.Element;
				_density = renderer.Context.Resources.DisplayMetrics.Density;
			}

			public void OnScrollStateChanged(AndroidListView view, [GeneratedEnum] AndroidScrollState scrollState)
			{
				// Initialize the values every time the list is moving.
				if (scrollState == AndroidScrollState.TouchScroll || scrollState == AndroidScrollState.Fling)
				{
					foreach (var t in _trackElements)
					{
						t.SyncState(view);
					}
				}
			}

			public void OnScroll(AndroidListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
			{
				var wasTracked = false;
				foreach (var t in _trackElements)
				{
					if (!wasTracked)
					{
						if (t.IsSafeToTrack(view))
						{
							wasTracked = true;
							_contentOffset += t.GetDeltaY();
							SendScrollEvent(_contentOffset, ((firstVisibleItem + visibleItemCount) >= totalItemCount - 1));
							t.SyncState(view);
						}
						else
						{
							t.Reset();
						}
					}
					else
					{
						t.SyncState(view);
					}
				}
			}

			private void SendScrollEvent(double y, bool isLastItemVisible)
			{
				var element = _element as CustomListView;

				// Calculate vertical offset in device-independent pixels (DIPs).
				element?.OnEventIsLastItemVisible(isLastItemVisible);
				var offset = Math.Abs(y) / _density;
				var args = new ScrolledEventArgs(0, offset);
				element?.OnScrolled(args);
			}

			private class TrackElement
			{
				private readonly int _position;

				private Android.Views.View _trackedView;
				private int _trackedViewPrevPosition;
				private int _trackedViewPrevTop;

				public TrackElement(int position)
				{
					_position = position;
				}

				public void SyncState(AndroidListView view)
				{
					if (view.ChildCount > 0)
					{
						_trackedView = GetChild(view);
						_trackedViewPrevTop = GetY();
						_trackedViewPrevPosition = view.GetPositionForView(_trackedView);
					}
				}

				public void Reset()
				{
					_trackedView = null;
				}

				public bool IsSafeToTrack(AndroidListView view)
				{
					return _trackedView != null
						&& _trackedView.Parent == view
						&& view.GetPositionForView(_trackedView) == _trackedViewPrevPosition;
				}

				public int GetDeltaY()
				{
					return GetY() - _trackedViewPrevTop;
				}

				private Android.Views.View GetChild(AndroidListView view)
				{
					switch (_position)
					{
						case 0:
							return view.GetChildAt(0);
						case 1:
						case 2:
							return view.GetChildAt(view.ChildCount / 2);
						case 3:
							return view.GetChildAt(view.ChildCount - 1);
						default:
							return null;
					}
				}

				private int GetY()
				{
					return _position <= 1
						? _trackedView.Bottom
						: _trackedView.Top;
				}
			}
		}
	}
}
