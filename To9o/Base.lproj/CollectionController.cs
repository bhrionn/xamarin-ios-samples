// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using SafariServices;

namespace StoryboardTables
{
	public partial class CollectionController : UICollectionViewController, IUIViewControllerPreviewingDelegate
	{
		public static CollectionController Current;
		/// <summary>
		/// List of items
		/// </summary>
		List<TodoItem> todoItems;

		public CollectionController (IntPtr handle) : base (handle)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Todo", "");
			Current = this;
		}
		#region Lifecycle
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			AddButton.Clicked += (sender, e) => {
				CreateTodo ();
			};
			AboutButton.Clicked += (sender, e) => {
				// Safari View Controller
				var sfvc = new SFSafariViewController (new NSUrl("https://github.com/conceptdev/xamarin-ios-samples/blob/master/To9o/readme.md"),true);
				PresentViewController(sfvc, true, null);
			};
			// 3D Touch
			if (TraitCollection.ForceTouchCapability == UIForceTouchCapability.Available) {
				RegisterForPreviewingWithDelegate (this, CollectionView);
			}

		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			todoItems = AppDelegate.Current.TodoMgr.GetOrderedTodos ().ToList (); //ordered for CollectionView


			// bind every time, to reflect deletion in the Detail view
			Collection.Source = new TodoCollectionSource(todoItems.ToArray ());
			Collection.AllowsSelection = true;
			Collection.DelaysContentTouches = false;
		}

		/// <summary>
		/// Prepares for segue.
		/// </summary>
		/// <remarks>
		/// The prepareForSegue method is invoked whenever a segue is about to take place. 
		/// The new view controller has been loaded from the storyboard at this point but 
		/// it’s not visible yet, and we can use this opportunity to send data to it.
		/// </remarks>
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "todosegue") { // set in Storyboard
				var tvc = segue.DestinationViewController as DetailViewController;
				if (tvc != null) {
					var source = Collection.Source as TodoCollectionSource;
					var rowPath = Collection.IndexPathForCell (sender as UICollectionViewCell);
					var item = source.GetItem(rowPath.Row);
					tvc.Delegate = this;
					tvc.SetTodo(item);
				}
			}
		}
		#endregion

		#region CRUD
		public void CreateTodo ()
		{
			// StackView
			var detail = Storyboard.InstantiateViewController("detailvc") as DetailViewController;
			detail.Delegate = this;
			detail.SetTodo (new TodoItem());
			NavigationController.PushViewController (detail, true);

			// Could to this instead of the above, but need to create 'new TodoItem()' in PrepareForSegue()
			//this.PerformSegue ("TodoSegue", this);
		}
		public void SaveTodo (TodoItem todo) {
			AppDelegate.Current.TodoMgr.SaveTodo(todo);
			SpotlightHelper.Index (todo);

		}
		public void DeleteTodo (TodoItem todo) {
			Console.WriteLine("Delete "+todo.Name);
			if (todo.Id >= 0) {
				AppDelegate.Current.TodoMgr.DeleteTodo (todo.Id);
				SpotlightHelper.Delete (todo);
			}
		}
		#endregion

		#region 3DTouch Peek
		public UIViewController GetViewControllerForPreview (IUIViewControllerPreviewing previewingContext, CGPoint location)
		{
			// Obtain the index path and the cell that was pressed.
			var indexPath = CollectionView.IndexPathForItemAtPoint (location);

			Console.WriteLine ("ForPreview " + location.ToString() + " " + indexPath);

			if (indexPath == null)
				return null;

			var cell = CollectionView.CellForItem (indexPath);

			if (cell == null)
				return null;


			// Create a detail view controller and set its properties.
			var peekViewController = (PeekViewController)Storyboard.InstantiateViewController ("peekvc");
			if (peekViewController == null)
				return null;

			var peekAt = todoItems [indexPath.Row];
			peekViewController.SetTodo (peekAt);
			peekViewController.PreferredContentSize = new CGSize (0, 160);

			previewingContext.SourceRect = cell.Frame;

			return peekViewController;
		}

		public void CommitViewController (IUIViewControllerPreviewing previewingContext, UIViewController viewControllerToCommit)
		{
			Console.WriteLine ("CommitViewContoller");

			var sv = (UICollectionView)previewingContext.SourceView;

			var x = previewingContext.SourceRect.X + (previewingContext.SourceRect.Width / 2);
			var y = previewingContext.SourceRect.Y + (previewingContext.SourceRect.Height / 2);

			var indexPath = CollectionView.IndexPathForItemAtPoint (new CGPoint(x,y));
			var popAt = todoItems [indexPath.Row];


			var detailViewController = (DetailViewController)Storyboard.InstantiateViewController ("detailvc");
			if (detailViewController == null)
				return;

			detailViewController.SetTodo (popAt);

			//viewControllerToCommit = peekViewController;
			ShowViewController (detailViewController, this);
		}
		#endregion
	}
}
