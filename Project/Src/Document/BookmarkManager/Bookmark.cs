﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Drawing;
using SWF = System.Windows.Forms;

namespace ICSharpCode.TextEditor.Document
{
	/// <summary>
	///     Description of Bookmark.
	/// </summary>
	public class Bookmark
	{
		private IDocument _document;
		private bool _isEnabled = true;
		private TextLocation _location;

		public Bookmark(IDocument document, TextLocation location) : this(document, location, true)
		{
		}

		public Bookmark(IDocument document, TextLocation location, bool isEnabled)
		{
			_document = document;
			_isEnabled = isEnabled;
			Location = location;
		}

		public IDocument Document
		{
			get { return _document; }
			set
			{
				if (_document != value)
				{
					if (Anchor != null)
					{
						_location = Anchor.Location;
						Anchor = null;
					}
					_document = value;
					CreateAnchor();
					OnDocumentChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		///     Gets the TextAnchor used for this bookmark.
		///     Is null if the bookmark is not connected to a document.
		/// </summary>
		public TextAnchor Anchor { get; private set; }

		public TextLocation Location
		{
			get
			{
				if (Anchor != null)
					return Anchor.Location;
				return _location;
			}
			set
			{
				_location = value;
				CreateAnchor();
			}
		}

		public bool IsEnabled
		{
			get { return _isEnabled; }
			set
			{
				if (_isEnabled != value)
				{
					_isEnabled = value;
					if (_document != null)
					{
						_document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, LineNumber));
						_document.CommitUpdate();
					}
					OnIsEnabledChanged(EventArgs.Empty);
				}
			}
		}

		public int LineNumber
		{
			get
			{
				if (Anchor != null)
					return Anchor.LineNumber;
				return _location.Line;
			}
		}

		public int ColumnNumber
		{
			get
			{
				if (Anchor != null)
					return Anchor.ColumnNumber;
				return _location.Column;
			}
		}

		/// <summary>
		///     Gets if the bookmark can be toggled off using the 'set/unset bookmark' command.
		/// </summary>
		public virtual bool CanToggle => true;

		private void CreateAnchor()
		{
			if (_document != null)
			{
				var line = _document.GetLineSegment(Math.Max(0, Math.Min(_location.Line, _document.TotalNumberOfLines - 1)));
				Anchor = line.CreateAnchor(Math.Max(0, Math.Min(_location.Column, line.Length)));
				// after insertion: keep bookmarks after the initial whitespace (see DefaultFormattingStrategy.SmartReplaceLine)
				Anchor.MovementType = AnchorMovementType.AfterInsertion;
				Anchor.Deleted += AnchorDeleted;
			}
		}

		private void AnchorDeleted(object sender, EventArgs e)
		{
			_document.BookmarkManager.RemoveMark(this);
		}

		public event EventHandler DocumentChanged;

		protected virtual void OnDocumentChanged(EventArgs e)
		{
			if (DocumentChanged != null)
				DocumentChanged(this, e);
		}

		public event EventHandler IsEnabledChanged;

		protected virtual void OnIsEnabledChanged(EventArgs e)
		{
			if (IsEnabledChanged != null)
				IsEnabledChanged(this, e);
		}

		public virtual bool Click(SWF.Control parent, SWF.MouseEventArgs e)
		{
			if (e.Button == SWF.MouseButtons.Left && CanToggle)
			{
				_document.BookmarkManager.RemoveMark(this);
				return true;
			}
			return false;
		}

		public virtual void Draw(IconBarMargin margin, Graphics g, Point p)
		{
			margin.DrawBookmark(g, p.Y, _isEnabled);
		}
	}
}