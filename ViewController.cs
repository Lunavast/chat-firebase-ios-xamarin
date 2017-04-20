using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Firebase.Database;
using Firebase.Database.Query;
using System.Reactive.Linq;

using Foundation;
using UIKit;

namespace Chat
{
	public partial class ViewController : UIViewController
	{
		static string KEYCHATCHILD = "messages";
		NSObject willShowToken;
		NSObject willHideToken;

		List<Message> messages;
		ChatSource chatSource;

		UITableView tableView;
		UIToolbar toolbar;

		NSLayoutConstraint toolbarBottomConstraint;
		NSLayoutConstraint toolbarHeightConstraint;

		ChatInputView chatInputView;

		String name = "cuong";
		String authSecret = "RxIsctLdNZeV110WJ6PP0QtOPIwj9R1Fm5Tn9N3G";
		String basePath = "https://chat-c642c.firebaseio.com/";
		FirebaseClient client;
		ChildQuery chatChild;
		IDisposable chatEventListener;

		void initFirebase()
		{
			this.client = new FirebaseClient(this.basePath,
				new FirebaseOptions
				{
					AuthTokenAsyncFactory = () => Task.FromResult(this.authSecret)
				});
			this.chatChild = this.client.Child(KEYCHATCHILD);

			var observable = this.chatChild.AsObservable<InboundMessage>();

			this.chatEventListener = observable
				.Where(f => !string.IsNullOrEmpty(f.Key)) // you get empty Key when there are no data on the server for specified node
				.Subscribe(f => this.showChat(f.Object));

		}

		void showChat(InboundMessage mess)
		{
			Console.WriteLine(mess.Content);

			var text = mess.Content;

			var msg = new Message
			{
				Type = mess.Author == this.name ? MessageType.Outgoing : MessageType.Incoming,
				Text = text
			};

			messages.Add(msg);

			InvokeOnMainThread(() => { 
				tableView.InsertRows(new NSIndexPath[] { 
					NSIndexPath.FromRowSection(messages.Count - 1, 0) }, UITableViewRowAnimation.None);
                ScrollToBottom(true);
			});
		}



		UIButton SendButton
		{
			get
			{
				return chatInputView.SendButton;
			}
		}

		UITextView TextView
		{
			get
			{
				return chatInputView.TextView;
			}
		}

		bool IsInputToolbarHasReachedMaximumHeight
		{
			get
			{
				return toolbar.Frame.GetMinY() == TopLayoutGuide.Length;
			}
		}

		protected ViewController(IntPtr handle) : base(handle)
		{
			messages = new List<Message>();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.
			SetUpTableView();
			SetUpToolbar();

			SendButton.TouchUpInside += OnSendClicked;
			TextView.Started += OnTextViewStarted;
			TextView.Changed += OnTextChanged;

			initFirebase();
		}


		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			willShowToken = UIKeyboard.Notifications.ObserveWillShow(KeyboardWillShowHandler);
			willHideToken = UIKeyboard.Notifications.ObserveWillHide(KeyboardWillHideHandler);

			UpdateTableInsets();
			UpdateButtonState();
			ScrollToBottom(false);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			AddObservers();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		void SetUpTableView()
		{
			tableView = new UITableView
			{
				TranslatesAutoresizingMaskIntoConstraints = false,
				AllowsSelection = false,
				SeparatorStyle = UITableViewCellSeparatorStyle.None
			};
			tableView.RegisterClassForCellReuse(typeof(IncomingCell), IncomingCell.CellId);
			tableView.RegisterClassForCellReuse(typeof(OutgoingCell), OutgoingCell.CellId);
			View.AddSubview(tableView);

			var pinLeft = NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
			View.AddConstraint(pinLeft);

			var pinRight = NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
			View.AddConstraint(pinRight);

			var pinTop = NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, TopLayoutGuide, NSLayoutAttribute.Bottom, 1f, 0f);
			View.AddConstraint(pinTop);

			var pinBottom = NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f);
			View.AddConstraint(pinBottom);

			chatSource = new ChatSource(messages);
			tableView.Source = chatSource;
		}

		void SetUpToolbar()
		{
			toolbar = new UIToolbar
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};
			chatInputView = new ChatInputView
			{
				TranslatesAutoresizingMaskIntoConstraints = false
			};

			View.AddSubview(toolbar);

			var pinLeft = NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1f, 0f);
			View.AddConstraint(pinLeft);

			var pinRight = NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, View, NSLayoutAttribute.Trailing, 1f, 0f);
			View.AddConstraint(pinRight);

			toolbarBottomConstraint = NSLayoutConstraint.Create(View, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, toolbar, NSLayoutAttribute.Bottom, 1f, 0f);
			View.AddConstraint(toolbarBottomConstraint);

			toolbarHeightConstraint = NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 0f, 44f);
			View.AddConstraint(toolbarHeightConstraint);

			toolbar.AddSubview(chatInputView);

			var c1 = NSLayoutConstraint.FromVisualFormat("H:|[chat_container_view]|",
				0,
				"chat_container_view", chatInputView
			);
			var c2 = NSLayoutConstraint.FromVisualFormat("V:|[chat_container_view]|",
				0,
				"chat_container_view", chatInputView
			);
			toolbar.AddConstraints(c1);
			toolbar.AddConstraints(c2);
		}



		void AddObservers()
		{
			TextView.AddObserver(this, "contentSize", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
		}

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath == "contentSize")
				OnSizeChanged(new NSObservedChange(change));
			else
				base.ObserveValue(keyPath, ofObject, change, context);
		}

		void OnSizeChanged(NSObservedChange change)
		{
			CGSize oldValue = ((NSValue)change.OldValue).CGSizeValue;
			CGSize newValue = ((NSValue)change.NewValue).CGSizeValue;

			var dy = newValue.Height - oldValue.Height;
			AdjustInputToolbarOnTextViewSizeChanged(dy);
		}

		void AdjustInputToolbarOnTextViewSizeChanged(nfloat dy)
		{
			bool isIncreasing = dy > 0;
			if (IsInputToolbarHasReachedMaximumHeight && isIncreasing)
			{
				// TODO: scroll to bottom
				return;
			}

			nfloat oldY = toolbar.Frame.GetMinY();
			nfloat newY = oldY - dy;
			if (newY < TopLayoutGuide.Length)
				dy = oldY - TopLayoutGuide.Length;

			AdjustInputToolbar(dy);
		}

		void AdjustInputToolbar(nfloat change)
		{
			toolbarHeightConstraint.Constant += change;

			if (toolbarHeightConstraint.Constant < ChatInputView.ToolbarMinHeight)
				toolbarHeightConstraint.Constant = ChatInputView.ToolbarMinHeight;

			View.SetNeedsUpdateConstraints();
			View.LayoutIfNeeded();
		}

		void KeyboardWillShowHandler(object sender, UIKeyboardEventArgs e)
		{
			UpdateButtomLayoutConstraint(e);
		}

		void KeyboardWillHideHandler(object sender, UIKeyboardEventArgs e)
		{
			UpdateButtomLayoutConstraint(e);
		}

		void UpdateButtomLayoutConstraint(UIKeyboardEventArgs e)
		{
			UIViewAnimationCurve curve = e.AnimationCurve;
			UIView.Animate(e.AnimationDuration, 0, ConvertToAnimationOptions(e.AnimationCurve), () =>
			{
				nfloat offsetFromBottom = tableView.Frame.GetMaxY() - e.FrameEnd.GetMinY();
				offsetFromBottom = NMath.Max(0, offsetFromBottom);
				SetToolbarContstraint(offsetFromBottom);
			}, null);
		}

		void SetToolbarContstraint(nfloat constant)
		{
			toolbarBottomConstraint.Constant = constant;
			View.SetNeedsUpdateConstraints();
			View.LayoutIfNeeded();

			UpdateTableInsets();
		}

		void UpdateTableInsets()
		{
			nfloat bottom = tableView.Frame.GetMaxY() - toolbar.Frame.GetMinY();
			var insets = new UIEdgeInsets(0f, 0f, bottom, 0f);
			tableView.ContentInset = insets;
			tableView.ScrollIndicatorInsets = insets;
		}

		UIViewAnimationOptions ConvertToAnimationOptions(UIViewAnimationCurve curve)
		{
			// Looks like a hack. But it is correct.
			// UIViewAnimationCurve and UIViewAnimationOptions are shifted by 16 bits
			// http://stackoverflow.com/questions/18870447/how-to-use-the-default-ios7-uianimation-curve/18873820#18873820
			return (UIViewAnimationOptions)((int)curve << 16);
		}

		async void OnSendClicked(object sender, EventArgs e)
		{
			var text = TextView.Text;
			TextView.Text = string.Empty; // this will not generate change text event
			UpdateButtonState();

			if (string.IsNullOrWhiteSpace(text))
				return;

			await this.chatChild.PostAsync(new OutboundMessage { Author = name, Content = text });
		}

		void OnTextViewStarted(object sender, EventArgs e)
		{
			ScrollToBottom(true);
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		void UpdateButtonState()
		{
			SendButton.Enabled = !string.IsNullOrWhiteSpace(TextView.Text);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			willShowToken.Dispose();
			willHideToken.Dispose();
		}

		void ScrollToBottom(bool animated)
		{
			if (tableView.NumberOfSections() == 0)
				return;

			var items = (int)tableView.NumberOfRowsInSection(0);
			if (items == 0)
				return;

			var finalRow = (int)NMath.Max(0, tableView.NumberOfRowsInSection(0) - 1);
			NSIndexPath finalIndexPath = NSIndexPath.FromRowSection(finalRow, 0);
			tableView.ScrollToRow(finalIndexPath, UITableViewScrollPosition.Top, animated);
		}
	}
}
