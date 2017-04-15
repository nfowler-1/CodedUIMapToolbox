namespace UIMapToolbox.UI
{
    partial class UIMapTreeUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UIMapTreeUserControl));
            this.btnSaveUIMapFile = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnReloadFile = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuRename = new System.Windows.Forms.ToolStripMenuItem();
            this.uiMapFileSystemWatcher = new System.IO.FileSystemWatcher();
            this.tvUIMap = new UIMapToolbox.UI.MultiSelectDragDropTreeView();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiMapFileSystemWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSaveUIMapFile
            // 
            this.btnSaveUIMapFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveUIMapFile.Location = new System.Drawing.Point(317, 409);
            this.btnSaveUIMapFile.Name = "btnSaveUIMapFile";
            this.btnSaveUIMapFile.Size = new System.Drawing.Size(102, 23);
            this.btnSaveUIMapFile.TabIndex = 9;
            this.btnSaveUIMapFile.Text = "Save UIMap file";
            this.btnSaveUIMapFile.UseVisualStyleBackColor = true;
            this.btnSaveUIMapFile.Click += new System.EventHandler(this.btnSaveUIMapFile_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(392, 1);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileName.Location = new System.Drawing.Point(65, 3);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(321, 20);
            this.txtFileName.TabIndex = 6;
            this.txtFileName.TextChanged += new System.EventHandler(this.txtFileName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "UIMap file:";
            // 
            // btnReloadFile
            // 
            this.btnReloadFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReloadFile.Location = new System.Drawing.Point(3, 409);
            this.btnReloadFile.Name = "btnReloadFile";
            this.btnReloadFile.Size = new System.Drawing.Size(102, 23);
            this.btnReloadFile.TabIndex = 10;
            this.btnReloadFile.Text = "Reload file";
            this.btnReloadFile.UseVisualStyleBackColor = true;
            this.btnReloadFile.Click += new System.EventHandler(this.btnReloadFile_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuCopyPath,
            this.toolStripMenuRename});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(130, 48);
            // 
            // toolStripMenuCopyPath
            // 
            this.toolStripMenuCopyPath.Name = "toolStripMenuCopyPath";
            this.toolStripMenuCopyPath.Size = new System.Drawing.Size(129, 22);
            this.toolStripMenuCopyPath.Text = "&Copy path";
            this.toolStripMenuCopyPath.Click += new System.EventHandler(this.toolStripMenuCopyPath_Click);
            // 
            // toolStripMenuRename
            // 
            this.toolStripMenuRename.Name = "toolStripMenuRename";
            this.toolStripMenuRename.Size = new System.Drawing.Size(129, 22);
            this.toolStripMenuRename.Text = "&Rename";
            this.toolStripMenuRename.Click += new System.EventHandler(this.toolStripMenuRename_Click);
            // 
            // uiMapFileSystemWatcher
            // 
            this.uiMapFileSystemWatcher.EnableRaisingEvents = true;
            this.uiMapFileSystemWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            this.uiMapFileSystemWatcher.SynchronizingObject = this;
            this.uiMapFileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.uiMapFileSystemWatcher_Changed);
            // 
            // tvUIMap
            // 
            this.tvUIMap.AllowDrop = true;
            this.tvUIMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvUIMap.ContextMenuStrip = this.contextMenuStrip1;
            this.tvUIMap.Cursor = System.Windows.Forms.Cursors.Default;
            this.tvUIMap.DragCursor = null;
            this.tvUIMap.DragCursorType = UIMapToolbox.UI.DragCursorType.None;
            this.tvUIMap.DragImageIndex = 0;
            this.tvUIMap.DragMode = System.Windows.Forms.DragDropEffects.Move;
            this.tvUIMap.DragNodeFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvUIMap.DragNodeOpacity = 0.3D;
            this.tvUIMap.DragOverNodeBackColor = System.Drawing.SystemColors.Highlight;
            this.tvUIMap.DragOverNodeForeColor = System.Drawing.SystemColors.HighlightText;
            this.tvUIMap.Location = new System.Drawing.Point(3, 37);
            this.tvUIMap.Name = "tvUIMap";
            this.tvUIMap.SelectedNodes = ((System.Collections.Generic.List<System.Windows.Forms.TreeNode>)(resources.GetObject("tvUIMap.SelectedNodes")));
            this.tvUIMap.Size = new System.Drawing.Size(417, 366);
            this.tvUIMap.TabIndex = 8;
            this.tvUIMap.UIMapFile = null;
            this.tvUIMap.DragStart += new UIMapToolbox.UI.DragItemEventHandler(this.tvUIMap_DragStart);
            this.tvUIMap.DragComplete += new UIMapToolbox.UI.DragCompleteEventHandler(this.tvUIMap_DragComplete);
            this.tvUIMap.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvUIMap_KeyDown);
            // 
            // UIMapTreeUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.btnReloadFile);
            this.Controls.Add(this.btnSaveUIMapFile);
            this.Controls.Add(this.tvUIMap);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.label1);
            this.Name = "UIMapTreeUserControl";
            this.Size = new System.Drawing.Size(422, 435);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiMapFileSystemWatcher)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSaveUIMapFile;
        private MultiSelectDragDropTreeView tvUIMap;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnReloadFile;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuCopyPath;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuRename;
        private System.IO.FileSystemWatcher uiMapFileSystemWatcher;
    }
}
