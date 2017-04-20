using System;
using Foundation;
using UIKit;

namespace Chat
{
	public class OutAuthor : AuthorCell
	{
		public static readonly NSString CellId = new NSString("OutAuthor");

		static readonly UIImage normalBubbleImage;
		static readonly UIImage highlightedBubbleImage;

		static OutAuthor()
		{
			
		}

		public OutAuthor(IntPtr handle)
			: base(handle)
		{
			Initialize();
		}

		public OutAuthor()
		{
			Initialize();
		}

		[Export("initWithStyle:reuseIdentifier:")]
		public OutAuthor(UITableViewCellStyle style, string reuseIdentifier)
			: base(style, reuseIdentifier)
		{
			Initialize();
		}

		void Initialize()
		{
			BubbleHighlightedImage = highlightedBubbleImage;
			BubbleImage = normalBubbleImage;

			ContentView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[bubble]|",
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

			var msgTrailing = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.Trailing, NSLayoutRelation.LessThanOrEqual, BubbleImageView, NSLayoutAttribute.Trailing, 1, -5);
			ContentView.AddConstraint(msgTrailing);

			var msgCenter = NSLayoutConstraint.Create(MessageLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, BubbleImageView, NSLayoutAttribute.CenterX, 1, -3);
			ContentView.AddConstraint(msgCenter);

			MessageLabel.TextColor = UIColor.Gray;
		}
	}
}
