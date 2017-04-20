using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Chat
{
	public class ChatSource : UITableViewSource
	{
		static readonly NSString IncomingCellId = new NSString("Incoming");
		static readonly NSString OutgoingCellId = new NSString("Outgoing");
		static readonly NSString InAuthorCellId = new NSString("InAuthor");
		static readonly NSString OutAuthorCellId = new NSString("OutAuthor");

		IList<Object> messages;

		readonly BubbleCell[] sizingCells;
		readonly AuthorCell[] sizing2Cells;

		public ChatSource(IList<Object> messages)
		{
			if (messages == null)
				throw new ArgumentNullException(nameof(messages));

			this.messages = messages;
			sizingCells = new BubbleCell[2];
			sizing2Cells = new AuthorCell[2];
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return messages.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = null;

			Object msg = messages[indexPath.Row];
			if (msg.GetType() == typeof(Message))
			{
				cell = (BubbleCell)tableView.DequeueReusableCell(GetReuseId(((Message)msg).Type, msg));
				((BubbleCell)cell).Message = (Message)msg;

			}

			if (msg.GetType() == typeof(Author))
			{
				cell = (AuthorCell)tableView.DequeueReusableCell(GetReuseId(((Author)msg).Type, msg));
				((AuthorCell)cell).Message = new Message { Text = ((Author)msg).Text, Type =  ((Author)msg).Type};

			}

			return cell;
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			Object msg = messages[indexPath.Row];
			return CalculateHeightFor(msg, tableView);
		}

		public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
		{
			Object msg = messages[indexPath.Row];
			return CalculateHeightFor(msg, tableView);
		}

		nfloat CalculateHeightFor(Object msg, UITableView tableView)
		{
			var index = -1;
			UITableViewCell cell = null;
			if (msg.GetType() == typeof(Message))
			{
				index = (int)((Message)msg).Type;
				cell = (BubbleCell)sizingCells[index];
				if (cell == null)
					cell = sizingCells[index] = (BubbleCell)tableView.DequeueReusableCell(GetReuseId(((Message)msg).Type, msg));
				((BubbleCell)cell).Message = (Message)msg;

			}

			if (msg.GetType() == typeof(Author))
			{
				index = (int)((Author)msg).Type;
				cell = (AuthorCell)sizing2Cells[index];
				if (cell == null)
					cell = sizing2Cells[index] = (AuthorCell)tableView.DequeueReusableCell(GetReuseId(((Author)msg).Type, msg));
				((AuthorCell)cell).Message = new Message { Text = ((Author)msg).Text, Type =  ((Author)msg).Type};
			}

			cell.SetNeedsLayout();
			cell.LayoutIfNeeded();

			CGSize size = cell.ContentView.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
			return NMath.Ceiling(size.Height) + 1;
		}

		NSString GetReuseId(MessageType msgType, Object msg)
		{
			if (msg.GetType() == typeof(Message))
			{
				return msgType == MessageType.Incoming? IncomingCellId : OutgoingCellId;
			}

			if (msg.GetType() == typeof(Author))
			{
				return msgType == MessageType.Incoming? InAuthorCellId : OutAuthorCellId;;
			}
			return null;
		}
	}
}
