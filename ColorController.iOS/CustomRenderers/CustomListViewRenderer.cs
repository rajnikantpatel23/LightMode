using System;
using UIKit;
using CoreGraphics;
using System.ComponentModel;
using FormsEssentials.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;

[assembly: ExportRenderer(typeof(CustomListView), typeof(FormsEssentials.iOS.CustomListViewRenderer))]
namespace FormsEssentials.iOS
{
    public class CustomListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            try
            {
                base.OnElementChanged(e);
            }
            catch (Exception ex)
            {
                //ex.LogException();
            }

            if (e.OldElement != null || e.NewElement == null)
            {
                return;
            }

            var element = Element as CustomListView;

            if (element != null)
            {
                Control.TableFooterView = new UIView(CGRect.Empty);

                Control.Delegate = new ListViewTableViewDelegate(this);
                Control.AllowsSelection = !element.DisableSelection;
                Control.ShowsVerticalScrollIndicator = false;
                Control.Bounces = !element.DisableBounce;
                element.ReloadRow += Element_ReloadRow;
                element.EventScrollToTop += View_EventScrollToTop;
				element.EventScrollToBottom += Element_EventScrollToBottom;

                Control.ScrollEnabled = !element.IsScrollDisabled;
            }
        }

		private void Element_EventScrollToBottom(object sender, EventArgs e)
		{
			try {
                Control.ScrollRectToVisible(Control.TableFooterView.Frame, animated: false);
            } catch (Exception ex) {
				 
			}
		}

		void View_EventScrollToTop(object sender, EventArgs e)
        {
            Control.ScrollRectToVisible(new CGRect(x: 0, y: 0, width: 1, height: 1), animated: true);
        }

        void Element_ReloadRow(int index)
        {
            var paths = new NSIndexPath[] {
                NSIndexPath.FromItemSection(index, 0)
            };

            PerformWithoutAnimation(() => {
                Control.ReloadRows(paths, UITableViewRowAnimation.None);
            });
        }
    }

    public class ListViewTableViewDelegate : UITableViewDelegate
    {
        private ListView _element;
        private UITableViewSource _source;

        public ListViewTableViewDelegate(ListViewRenderer renderer)
        {
            _element = renderer.Element;
            _source = renderer.Control.Source;
        }

        public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
        {
            _source.DraggingEnded(scrollView, willDecelerate);
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            _source.DraggingStarted(scrollView);
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            return _source.GetHeightForHeader(tableView, section);
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            return _source.GetViewForHeader(tableView, section);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            _source.RowDeselected(tableView, indexPath);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            _source.RowSelected(tableView, indexPath);
        }

        public override void Scrolled(UIScrollView scrollView)
        {
            _source.Scrolled(scrollView);
            SendScrollEvent(scrollView.ContentOffset.Y);

            var element = _element as CustomListView;
            var contentOffsetMaxY = scrollView.ContentOffset.Y + scrollView.Bounds.Size.Height;
            var contentHeight = scrollView.ContentSize.Height;
            if (contentOffsetMaxY > contentHeight - 100)
            {
                element?.OnEventIsLastItemVisible(true);
            }
            else
            {
                element?.OnEventIsLastItemVisible(false);
            }
        }

        private void SendScrollEvent(double y)
        {
            var element = _element as CustomListView;
            var args = new ScrolledEventArgs(0, y);
            element?.OnScrolled(args);

        }
    }
}