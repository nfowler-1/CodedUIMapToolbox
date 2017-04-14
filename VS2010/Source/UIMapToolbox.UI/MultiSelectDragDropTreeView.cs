using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using System.Collections.Generic;
using UIMapToolbox.Core;

// The MultiSelectDragDropTreeView class combines code from the following two projects:
// - http://www.codeproject.com/KB/tree/DragDropTreeview.aspx
// - http://www.codeproject.com/Articles/20581/Multiselect-Treeview-Implementation

namespace UIMapToolbox.UI
{
	// - Implements:
	//   + Auto scrolling
	//   + Target node highlighting when over a node
	//   + Custom cursor when dragging
	//   + Custom ghost icon + label when dragging
	//   + Escape key to cancel drag
	//	 + Blocks certain nodes from being dragged via cancel event
	//   + Sanity checks for dragging (no parent into children nodes, target isn't the source)

	// Gotchas:
	// - Explorer can tell if you have the treeview node selected or not
	// - The drag icon has to be dragged to the right, not in the center (or the form has 
	//  a fight with the treeview over focus)
    // - No auto opening of items

    #region DragDropTreeView class
    /// <summary>
    /// A treeview with multiselect, drag-drop support and custom cursor/icon dragging.
	/// </summary>
	[Description("A treeview with multiselect, drag-drop support and custom cursor/icon dragging.")]
	public class MultiSelectDragDropTreeView : System.Windows.Forms.TreeView
	{
		#region Win32 api import, events
		[DllImport("user32.dll")]
		private static extern int SendMessage (IntPtr hWnd, int wMsg, IntPtr wParam,int lParam);

		/// <summary>
		/// Occurs when an item is starting to be dragged. This
		/// event can be used to cancel dragging of particular items.
		/// </summary>
		[
		Description("Occurs when an item is starting to be dragged. This event can be used to cancel dragging of particular items."),
		]
		public event DragItemEventHandler DragStart;

		/// <summary>
		/// Occurs when an item is dragged and dropped onto another.
		/// </summary>
		[
		Description("Occurs when an item is dragged and dropped onto another."),
		]
		public event DragCompleteEventHandler DragComplete;
		
		/// <summary>
		/// Occurs when an item is dragged, and the drag is cancelled.
		/// </summary>
		[
		Description("Occurs when an item is dragged, and the drag is cancelled."),
		]
		public event DragItemEventHandler DragCancel;

		
		#endregion

		#region Public properties
		/// <summary>
		/// The imagelist control from which DragImage icons are taken.
		/// </summary>
		[
		Description("The imagelist control from which DragImage icons are taken."),
		Category("Drag and drop")
		]
		public ImageList DragImageList
		{
			get
			{
				return this._formDrag.imageList1;
			}
			set
			{
				if ( value == this._formDrag.imageList1 )
				{
					return;
				}

				this._formDrag.imageList1 = value;

				// Change the picture box to use this image
				if ( this._formDrag.imageList1.Images.Count > 0 && this._formDrag.imageList1.Images[this._dragImageIndex] != null )
				{
					this._formDrag.pictureBox1.Image = this._formDrag.imageList1.Images[this._dragImageIndex];
					this._formDrag.Height = this._formDrag.pictureBox1.Image.Height;
				}

				if ( !base.IsHandleCreated )
				{
					return;
				}
				SendMessage((IntPtr) 4361, 0, ((value == null) ? IntPtr.Zero : value.Handle),0);
			}

		}

        public UIMapFile UIMapFile { get; set; }

		/// <summary>
		/// The default image index for the DragImage icon.
		/// </summary>
		[
		Description("The default image index for the DragImage icon."),
		Category("Drag and drop"),
		TypeConverter(typeof(ImageIndexConverter)), 
		Editor("System.Windows.Forms.Design.ImageIndexEditor",typeof(System.Drawing.Design.UITypeEditor))
		]
		public int DragImageIndex
		{
			get
			{
				if ( this._formDrag.imageList1 == null)
				{
					return -1;
				}

				if ( this._dragImageIndex >= this._formDrag.imageList1.Images.Count)
				{
					return Math.Max(0, (this._formDrag.imageList1.Images.Count - 1));
				}
				else

				return this._dragImageIndex;
			}
			set
			{
				// Change the picture box to use this image
				if ( this._formDrag.imageList1.Images.Count > 0 && this._formDrag.imageList1.Images[value] != null )
				{
					this._formDrag.pictureBox1.Image = this._formDrag.imageList1.Images[value];
					this._formDrag.Size = new Size(this._formDrag.Width,this._formDrag.pictureBox1.Image.Height);
					this._formDrag.labelText.Size = new Size(this._formDrag.labelText.Width,this._formDrag.pictureBox1.Image.Height);
				}

				this._dragImageIndex = value;
			}
		}
		
		/// <summary>
		/// The custom cursor to use when dragging an item, if DragCursor is set to Custom.
		/// </summary>
		[
		Description("The custom cursor to use when dragging an item, if DragCursor is set to Custom."),
		Category("Drag and drop")
		]
		public Cursor DragCursor
		{
			get
			{
				return this._dragCursor;
			}
			set
			{
				if ( value == this._dragCursor)
				{
					return;
				}

				this._dragCursor = value;
				if ( !base.IsHandleCreated )
				{
					return;
				}
			}
		}

		/// <summary>
		/// The cursor type to use when dragging - None uses the default drag and drop cursor, DragIcon uses an icon and label, Custom uses a custom cursor.
		/// </summary>
		[
		Description("The cursor type to use when dragging - None uses the default drag and drop cursor, DragIcon uses an icon and label, Custom uses a custom cursor."),
		Category("Drag and drop")
		]
		public DragCursorType DragCursorType
		{
			get
			{
				return this._dragCursorType;
			}
			set
			{
				this._dragCursorType = value;
			}
		}

		/// <summary>
		/// Sets the font for the dragged node (shown as ghosted text/icon).
		/// </summary>
		[
		Description("Sets the font for the dragged node (shown as ghosted text/icon)."),
		Category("Drag and drop")
		]
		public Font DragNodeFont
		{
			get
			{
				return this._formDrag.labelText.Font ;
			}
			set
			{
				this._formDrag.labelText.Font = value;

				// Set the drag form height to the font height
				this._formDrag.Size = new Size(this._formDrag.Width,(int) this._formDrag.labelText.Font.GetHeight());
				this._formDrag.labelText.Size = new Size(this._formDrag.labelText.Width,(int) this._formDrag.labelText.Font.GetHeight());
				

			}
		}

		/// <summary>
		/// Sets the opacity for the dragged node (shown as ghosted text/icon).
		/// </summary>
		[
			Description("Sets the opacity for the dragged node (shown as ghosted text/icon)."),
			Category("Drag and drop"),
			TypeConverter(typeof(System.Windows.Forms.OpacityConverter))
		]
		public double DragNodeOpacity
		{ 
			get
			{
				return this._formDrag.Opacity;
			}
			set
			{
				this._formDrag.Opacity = value;
			}
		}

		/// <summary>
		/// The background colour of the node being dragged over.
		/// </summary>
		[
			Description("The background colour of the node being dragged over."),
			Category("Drag and drop")
		]
		public Color DragOverNodeBackColor
		{
			get
			{
				return this._dragOverNodeBackColor;
			}
			set
			{
				this._dragOverNodeBackColor = value;
			}
		}

		/// <summary>
		/// The foreground colour of the node being dragged over.
		/// </summary>
		[
			Description("The foreground colour of the node being dragged over."),
			Category("Drag and drop")
		]
		public Color DragOverNodeForeColor
		{
			get
			{
				return this._dragOverNodeForeColor;
			}
			set
			{
				this._dragOverNodeForeColor = value;
			}
		}

		/// <summary>
		/// The drag mode (move,copy etc.)
		/// </summary>
		[
			Description("The drag mode (move,copy etc.)"),
			Category("Drag and drop")
		]
		public DragDropEffects DragMode
		{
			get
			{
				return this._dragMode;
			}
			set
			{
				this._dragMode = value;
			}
		}
		#endregion
		
		#region Private members
		private int _dragImageIndex;
		private DragDropEffects _dragMode = DragDropEffects.Move;
		private Color _dragOverNodeForeColor = SystemColors.HighlightText;
		private Color _dragOverNodeBackColor = SystemColors.Highlight;
		private DragCursorType _dragCursorType;
		private Cursor _dragCursor = null;
		private TreeNode _previousNode;
		private TreeNode _selectedNode;
		private FormDrag _formDrag = new FormDrag();
		#endregion

		#region Constructor
		public MultiSelectDragDropTreeView()
		{
			base.SetStyle(ControlStyles.DoubleBuffer,true);
			this.AllowDrop = true;

			// Set the drag form to have ambient properties
			this._formDrag.labelText.Font = this.Font;
			this._formDrag.BackColor = this.BackColor;

			// Custom cursor handling
			if ( this._dragCursorType == DragCursorType.Custom && this._dragCursor != null )
			{
				this.DragCursor = this._dragCursor; 
			}

			this._formDrag.Show();
			this._formDrag.Visible = false;

            // MultiSelectTreeView:
            m_SelectedNodes = new List<TreeNode>();
            base.SelectedNode = null;
		}
		#endregion

		#region Over-ridden methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m)
		{
			//System.Diagnostics.Debug.WriteLine(m);
			// Stop erase background message
			if (m.Msg == (int)0x0014 )
			{
				m.Msg = (int) 0x0000; // Set to null
			} 
			
			base.WndProc(ref m);
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGiveFeedback(System.Windows.Forms.GiveFeedbackEventArgs e)
		{
			if ( e.Effect == this._dragMode )
			{
				e.UseDefaultCursors = false;

				if ( this._dragCursorType == DragCursorType.Custom && this._dragCursor != null )
				{
					// Custom cursor
					this.Cursor = this._dragCursor;
				}
				else if ( this._dragCursorType == DragCursorType.DragIcon )
				{
					// This removes the default drag + drop cursor
					this.Cursor = Cursors.Default;
				}
				else
				{
					e.UseDefaultCursors = true;
				}
			}
			else
			{
				e.UseDefaultCursors = true;
				this.Cursor = Cursors.Default;
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnItemDrag(System.Windows.Forms.ItemDragEventArgs e)
		{

            // If the user drags a node and the node being dragged is NOT
            // selected, then clear the active selection, select the
            // node being dragged and drag it. Otherwise if the node being
            // dragged is selected, drag the entire selection.
            try
            {
                TreeNode node = e.Item as TreeNode;

                if (node != null)
                {
                    if (!m_SelectedNodes.Contains(node))
                    {
                        SelectSingleNode(node);
                        ToggleNode(node, true);
                    }
                }

                base.OnItemDrag(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            // DragDropTreeView
			this._selectedNode = (TreeNode) e.Item;

			// Call dragstart event
			if ( this.DragStart != null )
			{
				CustomDragItemEventArgs ea = new CustomDragItemEventArgs();

                foreach (TreeNode node in this.SelectedNodes)
                {
                    ea.Nodes.Add(node);
                }
                ea.UIMapFile = this.UIMapFile;

				this.DragStart(this,ea);
			}
			// Change any previous node back 
			if ( this._previousNode != null )
			{
				this._previousNode.BackColor = SystemColors.HighlightText;
				this._previousNode.ForeColor = SystemColors.ControlText;
			}

			// Move the form with the icon/label on it
			// A better width measurement algo for the form is needed here

			int width = this._selectedNode.Text.Length * (int) this._formDrag.labelText.Font.Size;
			if ( this._selectedNode.Text.Length < 5 )
				width += 20;

			this._formDrag.Size = new Size(width,this._formDrag.Height);

			this._formDrag.labelText.Size = new Size(width,this._formDrag.labelText.Size.Height);
			this._formDrag.labelText.Text = this._selectedNode.Text;

			// Start drag drop
            DragData dragData = new DragData();
            dragData.UIMapFile = this.UIMapFile;
            foreach (TreeNode node in this.SelectedNodes)
                dragData.Nodes.Add(node);

            this.DoDragDrop(dragData, _dragMode);
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDragOver(System.Windows.Forms.DragEventArgs e)
		{
            // Get the node from the mouse position
            Point pt = ((TreeView)this).PointToClient(new Point(e.X, e.Y));
            TreeNode treeNode = this.GetNodeAt(pt);

            if (treeNode == null)
                return;
            
            // Change any previous node back
			if ( this._previousNode != null )
			{
				this._previousNode.BackColor = SystemColors.HighlightText;
				this._previousNode.ForeColor = SystemColors.ControlText;
			}

			// Colour tree node from the mouse position
			treeNode.BackColor = this._dragOverNodeBackColor;
			treeNode.ForeColor = this._dragOverNodeForeColor;

			// Move the icon form
			if ( this._dragCursorType == DragCursorType.DragIcon )
			{
				this._formDrag.Location = new Point(e.X+5,e.Y -5);
				this._formDrag.Visible = true;
			}
			
			// Scrolling down/up
			if ( pt.Y +10 > this.ClientSize.Height )
				SendMessage( this.Handle,277,(IntPtr) 1,0 );
			else if ( pt.Y < this.Top +10 )
				SendMessage( this.Handle,277,(IntPtr) 0,0 );

			// Remember the target node, so we can set it back
			this._previousNode = treeNode;
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDragLeave(EventArgs e)
		{
			if ( this._selectedNode != null )
			{
				this.SelectedNode = this._selectedNode;
			}

			if ( this._previousNode != null )
			{
				this._previousNode.BackColor = this._dragOverNodeBackColor;
				this._previousNode.ForeColor = this._dragOverNodeForeColor;
			}

			this._formDrag.Visible = false;
			this.Cursor = Cursors.Default;

			// Call cancel event
			if ( this.DragCancel != null )
			{
				CustomDragItemEventArgs ea = new CustomDragItemEventArgs();

                foreach (TreeNode node in this.SelectedNodes)
                    ea.Nodes.Add(node);

				this.DragCancel(this,ea);
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDragEnter(System.Windows.Forms.DragEventArgs e)
		{
			e.Effect = this._dragMode;

			// Reset the previous node var
			this._previousNode = null;
			this._selectedNode = null;
			System.Diagnostics.Debug.WriteLine(this._formDrag.labelText.Size);
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDragDrop(System.Windows.Forms.DragEventArgs e)
		{
			// Custom cursor handling
			if ( this._dragCursorType == DragCursorType.DragIcon )
			{
				this.Cursor = Cursors.Default;
			}

			this._formDrag.Visible = false;

			// Check it's a treenode being dragged
            if (e.Data.GetDataPresent("UIMapToolbox.UI.DragData", false))
			{
                DragData dragData = (DragData)e.Data.GetData("UIMapToolbox.UI.DragData");
				List<TreeNode> dragNodes = dragData.Nodes;

				// Get the target node from the mouse coords
				Point pt = ((TreeView)this).PointToClient(new Point(e.X, e.Y));
				TreeNode targetNode = this.GetNodeAt(pt);
			
				// De-color it
				targetNode.BackColor = SystemColors.HighlightText;
				targetNode.ForeColor = SystemColors.ControlText;

                CustomDragCompleteEventArgs ea = new CustomDragCompleteEventArgs();

                foreach (TreeNode dragNode in dragNodes)
                {
                    // 1) Check we're not dragging onto ourself
                    // 2) Check we're not dragging onto one of our children 
                    // (this is the lazy way, will break if there are nodes with the same name,
                    // but it's quicker than checking all nodes below is)
                    // 3) Check we're not dragging onto our parent
                    if (targetNode != dragNode && !targetNode.FullPath.StartsWith(dragNode.FullPath) && dragNode.Parent != targetNode)
                    {
                        // Copy the node, add as a child to the destination node
                        TreeNode newTreeNode = (TreeNode)dragNode.Clone();
                        targetNode.Nodes.Add(newTreeNode);
                        targetNode.Expand();

                        // Remove Original Node, set the dragged node as selected
                        dragNode.Remove();
                        this.SelectedNode = newTreeNode;

                        this.Cursor = Cursors.Default;

                        ea.SourceNodes.Add(dragNode);
                        ea.SourceUIMapFile = dragData.UIMapFile;
                        ea.TargetNode = targetNode;
                    }
                }

                // Call drag complete event
                if ((this.DragComplete != null) && (ea.SourceNodes.Count != 0))
                {
                    this.DragComplete(this, ea);
                }
			}	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if ( e.KeyCode == Keys.Escape )
			{
				if ( this._selectedNode != null )
				{
					this.SelectedNode = this._selectedNode;
				}

				if ( this._previousNode != null )
				{
					this._previousNode.BackColor = SystemColors.HighlightText;
					this._previousNode.ForeColor = SystemColors.ControlText;
				}

				this.Cursor = Cursors.Default;
				this._formDrag.Visible = false;

				// Call cancel event
				if ( this.DragCancel != null )
				{
					CustomDragItemEventArgs ea = new CustomDragItemEventArgs();
                    foreach (TreeNode node in this.SelectedNodes)
                        ea.Nodes.Add(node);

					this.DragCancel(this,ea);
				}
			}
		}

		// Custom double buffering courtesy of http://www.bobpowell.net/tipstricks.htm
		// (doesn't seem to work with this treeview, the wndproc method is the only solution)
		//	protected override void onpaint(painteventargs e) 
		//	{ 
		//		if(this._backbuffer==null) 
		//		{ 
		//			this._backbuffer=new bitmap(this.clientsize.width,this.clientsize.height); 
		//		} 
		//		graphics g=graphics.fromimage(this._backbuffer); 
		//    
		//		//paint your graphics on g here 
		//		g.dispose(); 
		//
		//		//copy the back buffer to the screen 
		//		e.graphics.drawimageunscaled(this._backbuffer,0,0); 
		//
		//		base.onpaint (e); //optional but not recommended 
		//	}
		//
		//	protected override void onpaintbackground(painteventargs e) 
		//	{ 
		//		//don't allow the background to paint
		//	} 
		//
		//	protected override void onsizechanged(eventargs e) 
		//	{ 
		//		if(this._backbuffer!=null) 
		//		{ 
		//			this._backbuffer.dispose(); 
		//			this._backbuffer=null; 
		//		} 
		//		base.onsizechanged (e); 
		//	} 
		#endregion

        #region Selected Node(s) Properties (MultiSelect)

        private List<TreeNode> m_SelectedNodes = null;
        public List<TreeNode> SelectedNodes
        {
            get
            {
                return m_SelectedNodes;
            }
            set
            {
                ClearSelectedNodes();
                if( value != null )
                {
                    foreach( TreeNode node in value )
                    {
                        ToggleNode( node, true );
                    }
                }
            }
        }

        // Note we use the new keyword to Hide the native treeview's 
        // SelectedNode property.
        private TreeNode m_SelectedNode;
        public new TreeNode SelectedNode
        {
            get
            {
                return m_SelectedNode;
            }
            set
            {
                ClearSelectedNodes();
                if( value != null )
                {
                    SelectNode( value );
                }
            }
        }

        #endregion

        #region Overridden Events (MultiSelect)

        protected override void OnGotFocus( EventArgs e )
        {
            // Make sure at least one node has a selection
            // this way we can tab to the ctrl and use the
            // keyboard to select nodes
            try
            {
                if( m_SelectedNode == null && this.TopNode != null )
                {
                    ToggleNode( this.TopNode, true );
                }

                base.OnGotFocus( e );
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            // If the user clicks on a node that was not
            // previously selected, select it now.
            try
            {
                base.SelectedNode = null;

                TreeNode node = this.GetNodeAt( e.Location );
                if( node != null )
                {
                    //Allow user to click on image
                    int leftBound = node.Bounds.X; // - 20; 
                    // Give a little extra room
                    int rightBound = node.Bounds.Right + 10; 
                    if( e.Location.X > leftBound && e.Location.X < rightBound )
                    {
                        if( ModifierKeys == 
                            Keys.None && ( m_SelectedNodes.Contains( node ) ) )
                        {
                            // Potential Drag Operation
                            // Let Mouse Up do select
                        }
                        else
                        {
                            SelectNode( node );
                        }
                    }
                }

                base.OnMouseDown( e );
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            // If you clicked on a node that WAS previously
            // selected then, reselect it now. This will clear
            // any other selected nodes. e.g. A B C D are selected
            // the user clicks on B, now A C & D are no longer selected.
            try
            {
                // Check to see if a node was clicked on
                TreeNode node = this.GetNodeAt( e.Location );
                if( node != null )
                {
                    if( ModifierKeys == Keys.None && m_SelectedNodes.Contains( node ) )
                    {
                        // Allow user to click on image
                        int leftBound = node.Bounds.X; // - 20; 
                        // Give a little extra room
                        int rightBound = node.Bounds.Right + 10; 
                        if( e.Location.X > leftBound && e.Location.X < rightBound )
                        {
                            SelectNode( node );
                        }
                    }
                }

                base.OnMouseUp( e );
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
        }

        protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
        {
            // Never allow base.SelectedNode to be set!
            try
            {
                base.SelectedNode = null;
                e.Cancel = true;

                base.OnBeforeSelect( e );
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
        }

        protected override void OnAfterSelect( TreeViewEventArgs e )
        {
            // Never allow base.SelectedNode to be set!
            try
            {
                base.OnAfterSelect( e );
                base.SelectedNode = null;
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            // Handle all possible key strokes for the control.
            // including navigation, selection, etc.

            base.OnKeyDown( e );

            if( e.KeyCode == Keys.ShiftKey ) return;

            //this.BeginUpdate();
            bool bShift = ( ModifierKeys == Keys.Shift );

            try
            {
                // Nothing is selected in the tree, this isn't a good state
                // select the top node
                if( m_SelectedNode == null && this.TopNode != null )
                {
                    ToggleNode( this.TopNode, true );
                }

                // Nothing is still selected in the tree, 
                // this isn't a good state, leave.
                if( m_SelectedNode == null ) return;

                if( e.KeyCode == Keys.Left )
                {
                    if( m_SelectedNode.IsExpanded && m_SelectedNode.Nodes.Count > 0 )
                    {
                        // Collapse an expanded node that has children
                        m_SelectedNode.Collapse();
                    }
                    else if( m_SelectedNode.Parent != null )
                    {
                        // Node is already collapsed, try to select its parent.
                        SelectSingleNode( m_SelectedNode.Parent );
                    }
                }
                else if( e.KeyCode == Keys.Right )
                {
                    if( !m_SelectedNode.IsExpanded )
                    {
                        // Expand a collapsed node's children
                        m_SelectedNode.Expand();
                    }
                    else
                    {
                        // Node was already expanded, select the first child
                        SelectSingleNode( m_SelectedNode.FirstNode );
                    }
                }
                else if( e.KeyCode == Keys.Up )
                {
                    // Select the previous node
                    if( m_SelectedNode.PrevVisibleNode != null )
                    {
                        SelectNode( m_SelectedNode.PrevVisibleNode );
                    }
                }
                else if( e.KeyCode == Keys.Down )
                {
                    // Select the next node
                    if( m_SelectedNode.NextVisibleNode != null )
                    {
                        SelectNode( m_SelectedNode.NextVisibleNode );
                    }
                }
                else if( e.KeyCode == Keys.Home )
                {
                    if( bShift )
                    {
                        if( m_SelectedNode.Parent == null )
                        {
                            // Select all of the root nodes up to this point
                            if( this.Nodes.Count > 0 )
                            {
                                SelectNode( this.Nodes[0] );
                            }
                        }
                        else
                        {
                            // Select all of the nodes up to this point under 
                            // this nodes parent
                            SelectNode( m_SelectedNode.Parent.FirstNode );
                        }
                    }
                    else
                    {
                        // Select this first node in the tree
                        if( this.Nodes.Count > 0 )
                        {
                            SelectSingleNode( this.Nodes[0] );
                        }
                    }
                }
                else if( e.KeyCode == Keys.End )
                {
                    if( bShift )
                    {
                        if( m_SelectedNode.Parent == null )
                        {
                            // Select the last ROOT node in the tree
                            if( this.Nodes.Count > 0 )
                            {
                                SelectNode( this.Nodes[this.Nodes.Count - 1] );
                            }
                        }
                        else
                        {
                            // Select the last node in this branch
                            SelectNode( m_SelectedNode.Parent.LastNode );
                        }
                    }
                    else
                    {
                        if( this.Nodes.Count > 0 )
                        {
                            // Select the last node visible node in the tree.
                            // Don't expand branches incase the tree is virtual
                            TreeNode ndLast = this.Nodes[0].LastNode;
                            while( ndLast.IsExpanded && ( ndLast.LastNode != null ) )
                            {
                                ndLast = ndLast.LastNode;
                            }
                            SelectSingleNode( ndLast );
                        }
                    }
                }
                else if( e.KeyCode == Keys.PageUp )
                {
                    // Select the highest node in the display
                    int nCount = this.VisibleCount;
                    TreeNode ndCurrent = m_SelectedNode;
                    while( ( nCount ) > 0 && ( ndCurrent.PrevVisibleNode != null ) )
                    {
                        ndCurrent = ndCurrent.PrevVisibleNode;
                        nCount--;
                    }
                    SelectSingleNode( ndCurrent );
                }
                else if( e.KeyCode == Keys.PageDown )
                {
                    // Select the lowest node in the display
                    int nCount = this.VisibleCount;
                    TreeNode ndCurrent = m_SelectedNode;
                    while( ( nCount ) > 0 && ( ndCurrent.NextVisibleNode != null ) )
                    {
                        ndCurrent = ndCurrent.NextVisibleNode;
                        nCount--;
                    }
                    SelectSingleNode( ndCurrent );
                }
                else
                {
                    // Assume this is a search character a-z, A-Z, 0-9, etc.
                    // Select the first node after the current node that
                    // starts with this character
                    string sSearch = ( (char) e.KeyValue ).ToString();

                    TreeNode ndCurrent = m_SelectedNode;
                    while( ( ndCurrent.NextVisibleNode != null ) )
                    {
                        ndCurrent = ndCurrent.NextVisibleNode;
                        if( ndCurrent.Text.StartsWith( sSearch ) )
                        {
                            SelectSingleNode( ndCurrent );
                            break;
                        }
                    }
                }
            }
            catch( Exception ex )
            {
                HandleException( ex );
            }
            finally
            {
                this.EndUpdate();
            }
        }

        #endregion

		#region FormDrag form
		internal class FormDrag : System.Windows.Forms.Form
		{
			#region Components
			public System.Windows.Forms.Label labelText;
			public System.Windows.Forms.PictureBox pictureBox1;
			public System.Windows.Forms.ImageList imageList1;
			private System.ComponentModel.Container components = null;
			#endregion

			#region Constructor, dispose
			public FormDrag()
			{
				InitializeComponent();
			}

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			protected override void Dispose( bool disposing )
			{
				if( disposing )
				{
					if(components != null)
					{
						components.Dispose();
					}
				}
				base.Dispose( disposing );
			}
			#endregion

			#region Windows Form Designer generated code
			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent()
			{
				this.components = new System.ComponentModel.Container();
				this.labelText = new System.Windows.Forms.Label();
				this.pictureBox1 = new System.Windows.Forms.PictureBox();
				this.imageList1 = new System.Windows.Forms.ImageList(this.components);
				this.SuspendLayout();
				// 
				// labelText
				// 
				this.labelText.BackColor = System.Drawing.Color.Transparent;
				this.labelText.Location = new System.Drawing.Point(16, 2);
				this.labelText.Name = "labelText";
				this.labelText.Size = new System.Drawing.Size(100, 16);
				this.labelText.TabIndex = 0;
				// 
				// pictureBox1
				// 
				this.pictureBox1.Location = new System.Drawing.Point(0, 0);
				this.pictureBox1.Name = "pictureBox1";
				this.pictureBox1.Size = new System.Drawing.Size(16, 16);
				this.pictureBox1.TabIndex = 1;
				this.pictureBox1.TabStop = false;
				// 
				// Form2
				// 
				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.BackColor = System.Drawing.SystemColors.Control;
				this.ClientSize = new System.Drawing.Size(100, 16);
				this.Controls.Add(this.pictureBox1);
				this.Controls.Add(this.labelText);
				this.Size = new Size(300,500);
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
				this.Opacity = 0.3;
				this.ShowInTaskbar = false;
				this.ResumeLayout(false);

			}
			#endregion
		}
		#endregion

        #region Multiselect Helper Methods

        private void SelectNode(TreeNode node)
        {
            try
            {
                this.BeginUpdate();

                if (m_SelectedNode == null || ModifierKeys == Keys.Control)
                {
                    // Ctrl+Click selects an unselected node, 
                    // or unselects a selected node.
                    bool bIsSelected = m_SelectedNodes.Contains(node);
                    ToggleNode(node, !bIsSelected);
                }
                else if (ModifierKeys == Keys.Shift)
                {
                    // Shift+Click selects nodes between the selected node and here.
                    TreeNode ndStart = m_SelectedNode;
                    TreeNode ndEnd = node;

                    if (ndStart.Parent == ndEnd.Parent)
                    {
                        // Selected node and clicked node have same parent, easy case.
                        if (ndStart.Index < ndEnd.Index)
                        {
                            // If the selected node is beneath 
                            // the clicked node walk down
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.NextVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                        else if (ndStart.Index == ndEnd.Index)
                        {
                            // Clicked same node, do nothing
                        }
                        else
                        {
                            // If the selected node is above the clicked node walk up
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.PrevVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                    }
                    else
                    {
                        // Selected node and clicked node have same parent, hard case.
                        // We need to find a common parent to determine if we need
                        // to walk down selecting, or walk up selecting.

                        TreeNode ndStartP = ndStart;
                        TreeNode ndEndP = ndEnd;
                        int startDepth = Math.Min(ndStartP.Level, ndEndP.Level);

                        // Bring lower node up to common depth
                        while (ndStartP.Level > startDepth)
                        {
                            ndStartP = ndStartP.Parent;
                        }

                        // Bring lower node up to common depth
                        while (ndEndP.Level > startDepth)
                        {
                            ndEndP = ndEndP.Parent;
                        }

                        // Walk up the tree until we find the common parent
                        while (ndStartP.Parent != ndEndP.Parent)
                        {
                            ndStartP = ndStartP.Parent;
                            ndEndP = ndEndP.Parent;
                        }

                        // Select the node
                        if (ndStartP.Index < ndEndP.Index)
                        {
                            // If the selected node is beneath 
                            // the clicked node walk down
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.NextVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                        else if (ndStartP.Index == ndEndP.Index)
                        {
                            if (ndStart.Level < ndEnd.Level)
                            {
                                while (ndStart != ndEnd)
                                {
                                    ndStart = ndStart.NextVisibleNode;
                                    if (ndStart == null) break;
                                    ToggleNode(ndStart, true);
                                }
                            }
                            else
                            {
                                while (ndStart != ndEnd)
                                {
                                    ndStart = ndStart.PrevVisibleNode;
                                    if (ndStart == null) break;
                                    ToggleNode(ndStart, true);
                                }
                            }
                        }
                        else
                        {
                            // If the selected node is above 
                            // the clicked node walk up
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.PrevVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                    }
                }
                else
                {
                    // Just clicked a node, select it
                    SelectSingleNode(node);
                }

                OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
            }
            finally
            {
                this.EndUpdate();
            }
        }

        private void ClearSelectedNodes()
        {
            try
            {
                foreach (TreeNode node in m_SelectedNodes)
                {
                    node.BackColor = this.BackColor;
                    node.ForeColor = this.ForeColor;
                }
            }
            finally
            {
                m_SelectedNodes.Clear();
                m_SelectedNode = null;
            }
        }

        private void SelectSingleNode(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            ClearSelectedNodes();
            ToggleNode(node, true);
            node.EnsureVisible();
        }

        private void ToggleNode(TreeNode node, bool bSelectNode)
        {
            if (bSelectNode)
            {
                m_SelectedNode = node;
                if (!m_SelectedNodes.Contains(node))
                {
                    m_SelectedNodes.Add(node);
                }
                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                m_SelectedNodes.Remove(node);
                node.BackColor = this.BackColor;
                node.ForeColor = this.ForeColor;
            }
        }

        private void HandleException(Exception ex)
        {
            // Perform some error handling here.
            // We don't want to bubble errors to the CLR.
            MessageBox.Show(ex.Message);
        }

        #endregion
	}
	#endregion

	#region DragCursorType enum
	[Serializable]
	public enum DragCursorType
	{
		None,
		DragIcon,
		Custom
	}
	#endregion

	#region Event classes/delegates
	public delegate void DragCompleteEventHandler(object sender, CustomDragCompleteEventArgs e);
	public delegate void DragItemEventHandler(object sender, CustomDragItemEventArgs e);

	public class CustomDragCompleteEventArgs : EventArgs
	{
		/// <summary>
		/// The node that was being dragged
		/// </summary>
		public List<TreeNode> SourceNodes
		{
			get
			{
				return this._sourceNodes;
			}
		}

        public UIMapFile SourceUIMapFile { get; set; }

		/// <summary>
		/// The node that the source node was dragged onto.
		/// </summary>
		public TreeNode TargetNode
		{
			get
			{
				return this._targetNode;
			}
			set
			{
				this._targetNode = value;
			}
		}
		
		private TreeNode _targetNode;		
		private List<TreeNode> _sourceNodes = new List<TreeNode>();
	}

	public class CustomDragItemEventArgs : EventArgs
	{
		/// <summary>
		/// The node(s) that was being dragged
		/// </summary>
		public List<TreeNode> Nodes
		{
			get { return _nodes; }
		}

        public UIMapFile UIMapFile { get; set; }
		private List<TreeNode> _nodes = new List<TreeNode>();
	}

    public class DragData
    {
        public List<TreeNode> Nodes
        {
            get { return _nodes; }
        }
        private List<TreeNode> _nodes = new List<TreeNode>();

        public UIMapFile UIMapFile { get; set; }
    }

	#endregion
}
