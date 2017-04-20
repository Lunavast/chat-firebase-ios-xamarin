using System;
using Foundation;
using UIKit;

namespace Chat
{
	public class InAuthor : AuthorCell
	{
		public static readonly NSString CellId = new NSString("InAuthor");

		static readonly UIImage normalBubbleImage;
		static readonly UIImage highlightedBubbleImage;

		static InAuthor()
		{
			
		}

		public InAuthor(IntPtr handle)
			: base(handle)
		{
			Initialize();
		}

		public InAuthor()
		{
			Initialize();
		}

		[Export("initWithStyle:reuseIdentifier:")]
		public InAuthor(UITableViewCellStyle style, string reuseIdentifier)
			: base(style, reuseIdentifier)
		{
			Initialize();
		}

		void Initialize()
		{
			BubbleHighlightedImage = highlightedBubbleImage;
			BubbleImage = normalBubbleImage;

			ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[bubble]",
				0,
				"bubble", BubbleImageView));
			ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-2-[bubble]-2-|",
				0,
				"bubble", BubbleImageView
			));
			BubbleImageView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[bubble(>=48)]",
				0,
				"bubble", BubbleImageView
			));

			var vSpaceTop = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, BubbleImageView, NSLayoutAttribute.Top, 1, 5);
			ContentView.AddConstraint(vSpaceTop);

			var vSpaceBottom = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, BubbleImageView, NSLayoutAttribute.Bottom, 1, -5);
			ContentView.AddConstraint(vSpaceBottom);

			var msgLeading = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.Leading, NSLayoutRelation.GreaterThanOrEqual, BubbleImageView, NSLayoutAttribute.Leading, 1, 5);
			ContentView.AddConstraint(msgLeading);

			var msgCenter = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, BubbleImageView, NSLayoutAttribute.CenterX, 1, 3);
			ContentView.AddConstraint(msgCenter);
			MessageLabel.TextColor = UIColor.Gray;
		}
	}
}
