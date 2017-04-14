using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITest.Common.UIMap;
using System.Collections.ObjectModel;
using UIMapToolbox.Core;
using System.Collections;

namespace UIMapToolbox.UI
{
    public partial class UIMapTreeUserControl : UserControl
    {
        private UIMapFile _uiMapFile;

        public UIMapFile UIMapFile
        {
            get { return _uiMapFile; }
        }

        public UIMapTreeUserControl()
        {
            InitializeComponent();

            tvUIMap.TreeViewNodeSorter = new TreeViewNodeSorter();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ShowOpenFileDialog();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            this.ReloadUIMapFile(txtFileName.Text);
        }

        private void ReloadUIMapFile(string fileName)
        {
            this.uiMapFileSystemWatcher.EnableRaisingEvents = false;

            if (!File.Exists(fileName))
            {
                tvUIMap.Nodes.Clear();
                return;
            }

            if ((this.UIMapFile != null) && (this.UIMapFile.IsModified))
            {
                string msg = String.Format("UIMap file '{0}' has been modified.\n\nReload anyway?", this.UIMapFile.GetUIMapName());

                DialogResult answer = MessageBox.Show(msg, Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (answer != DialogResult.Yes)
                    return;
            }

            FileInfo fi = new FileInfo(fileName);
            this.uiMapFileSystemWatcher.Path = fi.DirectoryName;
            this.uiMapFileSystemWatcher.Filter = fi.Name;
            this.uiMapFileSystemWatcher.EnableRaisingEvents = true;

            _uiMapFile = UIMapFile.Create(fileName);
            tvUIMap.UIMapFile = _uiMapFile;

            RedrawTree();
        }

        internal void LoadUIMapFile(string fileName)
        {
            // setting filename textbox will trigger a reload of file
            txtFileName.Text = fileName;
        }

        private void RedrawTree(string expandNodePath = null)
        {
            tvUIMap.Nodes.Clear();
            if (_uiMapFile == null)
                return;

            foreach (UIMap uiMap in _uiMapFile.Maps)
            {
                TreeNode uiMapNode = tvUIMap.Nodes.Add(uiMap.Id, uiMap.Id);
                string path = uiMap.Id;
                uiMapNode.Tag = path;

                foreach (TopLevelElement topLevelElement in uiMap.TopLevelWindows)
                {
                    TreeNode topLevelElementNode = uiMapNode.Nodes.Add(topLevelElement.Id, topLevelElement.Id);
                    path = String.Format("{0}.{1}", uiMap.Id, topLevelElement.Id);
                    topLevelElementNode.Tag = path;

                    RecursivelyPopulateDescendants(topLevelElementNode, topLevelElement.Descendants, path);
                }
            }

            if (tvUIMap.Nodes.Count != 0)
            {
                TreeNode selectedNode = tvUIMap.TopNode;

                if (!String.IsNullOrEmpty(expandNodePath))
                {
                    // expand to the specified path
                    string[] path = expandNodePath.Split('.');

                    for (int i = 1; i < path.Length; i++)
                    {
                        string part = path[i];
                        selectedNode = selectedNode.Nodes[part];
                    }
                }

                if (selectedNode != null)
                {
                    selectedNode.Expand();
                    selectedNode.EnsureVisible();
                    tvUIMap.SelectedNode = selectedNode;
                }

                tvUIMap.Select();
            }
        }

        private void RecursivelyPopulateDescendants(TreeNode parentTreeNode, Collection<UIObject> descendants, string parentPath)
        {
            if (descendants == null)
                return;

            foreach (UIObject uiObject in descendants)
            {
                string path = String.Format("{0}.{1}", parentPath, uiObject.Id);
                TreeNode treeNode = parentTreeNode.Nodes.Add(uiObject.Id, uiObject.Id);
                treeNode.Tag = path;

                RecursivelyPopulateDescendants(treeNode, uiObject.Descendants, path);
            }
        }

        private void tvUIMap_DragStart(object sender, CustomDragItemEventArgs e)
        {
            if ((e == null) || (e.Nodes == null))
                return;

            // cannot drag root node
            foreach (TreeNode node in e.Nodes)
            {
                if (node == tvUIMap.TopNode)
                {
                    tvUIMap.AllowDrop = false;
                    return;
                }
            }

            tvUIMap.AllowDrop = true;
        }

        private void tvUIMap_DragComplete(object sender, CustomDragCompleteEventArgs e)
        {
            if ((e == null) || (e.TargetNode == null))
                return;

            string destElementParentPath = (string)e.TargetNode.Tag;
            UIMapFile sourceUIMapFile = e.SourceUIMapFile;
            if (sourceUIMapFile == null)
                return;

            try
            {
                // move selected nodes
                foreach (TreeNode sourceNode in e.SourceNodes)
                {
                    string srcElementPath = (string)sourceNode.Tag;
                    if (sourceUIMapFile == null)
                        // same UIMap
                        _uiMapFile.MoveUIObject(srcElementPath, destElementParentPath);
                    else
                        // from other UIMap
                        _uiMapFile.MoveUIObject(sourceUIMapFile, srcElementPath, destElementParentPath);
                }

                RedrawTree(destElementParentPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSaveUIMapFile_Click(object sender, EventArgs e)
        {
            SaveUIMapFile();
        }

        private void SaveUIMapFile()
        {
            if (String.IsNullOrWhiteSpace(txtFileName.Text))
                return;

            // save backup
            string uiMapFileName = txtFileName.Text;
            string backupFileName = String.Empty;
            try
            {
                FileInfo fi = new FileInfo(txtFileName.Text);
                backupFileName = String.Format(@"{0}\UIMapEditorBackups\{1}_{2:yyyyMMdd_hhmmss}{3}", fi.DirectoryName, fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length), DateTime.Now, fi.Extension);
                Directory.CreateDirectory(Path.GetDirectoryName(backupFileName));
                File.Copy(uiMapFileName, backupFileName);
            }
            catch (Exception ex)
            {
                DialogResult answer = MessageBox.Show(
                    String.Format("An error occured while trying to save backup: {0}\n\nFile name: {1}\n\nSave file anyway (without backup file)?", ex.Message, backupFileName),
                    Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (answer != DialogResult.Yes)
                    return;
            }

            if (this.uiMapFileSystemWatcher != null)
                this.uiMapFileSystemWatcher.EnableRaisingEvents = false;

            try
            {
                _uiMapFile.Save(uiMapFileName);

                MessageBox.Show("File saved.\n\nRemember to load file in Visual Studio and save it, in order to regenerate C# code.", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occured while trying to save UIMap file: {0}\n\nFile name: {1}\n\nA possible cause might be that the file is not checked out from source control.", ex.Message, uiMapFileName),
                    this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                if (this.uiMapFileSystemWatcher != null)
                    this.uiMapFileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Deletes the selected UI objects from UIMap.
        /// </summary>
        private void DeleteSelectedUIObjects()
        {
            if (tvUIMap.SelectedNodes.Count == 0)
                return;

            try
            {
                if (tvUIMap.SelectedNodes.Count == 1)
                {
                    string path = (string)tvUIMap.SelectedNode.Tag;

                    if (MessageBox.Show(String.Format("Delete '{0}'?", path), Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _uiMapFile.DeleteUIObject(path);
                        tvUIMap.SelectedNode.Remove();
                    }
                }
                else
                {
                    if (MessageBox.Show(String.Format("Delete selected {0} elements?", tvUIMap.SelectedNodes.Count), Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        foreach (TreeNode node in tvUIMap.SelectedNodes)
                        {
                            string path = (string)node.Tag;

                            _uiMapFile.DeleteUIObject(path);
                            node.Remove();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tvUIMap_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    DeleteSelectedUIObjects();
                    break;

                case Keys.F2:
                    RenameSelectedElement();
                    break;
            }
        }

        private void btnReloadFile_Click(object sender, EventArgs e)
        {
            this.ReloadUIMapFile(txtFileName.Text);
        }

        private void toolStripMenuCopyPath_Click(object sender, EventArgs e)
        {
            if (tvUIMap.SelectedNode == null)
                return;

            string path = tvUIMap.SelectedNode.Tag as string;
            if (String.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("No path for selected node in tree.", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(path);

                MessageBox.Show(
                    String.Format("Succesfully copied path '{0}' to clipboard.", path),
                    Program.ApplicationName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("An error occured while copying path '{0}' to clipboard. Error: {1}", path, ex.Message),
                    Program.ApplicationName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        internal void FindNextNode(string text)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                TreeNode node = tvUIMap.SelectedNode;
                do
                {
                    node = GetNextNode(node);
                    if (node != null)
                    {
                        if (node.Text.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            tvUIMap.SelectedNode = node;
                            node.EnsureVisible();
                            tvUIMap.Focus();
                            return;
                        }
                    }
                } while (node != null);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            MessageBox.Show(String.Format("No more elements found containing text '{0}'", text));
        }


        /// <summary>
        /// Returns the next node in tree from selected node. 
        /// If current node have children, then first child is returned.
        /// If current node is last child, then it will return next sibling of parent.
        /// </summary>
        /// <returns></returns>
        private TreeNode GetNextNode(TreeNode currentNode)
        {
            if (currentNode == null)
                return null;

            // return first child (if exists)
            if ((currentNode.Nodes != null) && (currentNode.Nodes.Count != 0))
                return currentNode.Nodes[0];

            // no children -> try with next sibling
            TreeNode nextNode = currentNode.NextNode;
            if (nextNode != null)
                return nextNode;

            // no sibling -> return next sibling of first ancestor with sibling
            TreeNode ancestor = currentNode;
            do
            {
                ancestor = ancestor.Parent;
                if (ancestor == null)
                    return null;

                nextNode = ancestor.NextNode;

                if (nextNode != null)
                    return nextNode;
            } while (ancestor.Level != 0);

            return null;
        }

        internal void ShowOpenFileDialog()
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.FileName = txtFileName.Text;

                if (!String.IsNullOrWhiteSpace(txtFileName.Text))
                    dlg.InitialDirectory = Path.GetDirectoryName(txtFileName.Text);

                dlg.Filter = "UIMap Files (*.uitest)|*.uitest|All Files (*.*)|*.*";
                dlg.FilterIndex = 0;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtFileName.Text = dlg.FileName;
                }
            }
        }

        private void toolStripMenuRename_Click(object sender, EventArgs e)
        {
            RenameSelectedElement();
        }

        private void RenameSelectedElement()
        {
            if (tvUIMap.SelectedNode == null)
                return;

            try
            {
                string path = tvUIMap.SelectedNode.Tag as string;
                if (String.IsNullOrWhiteSpace(path))
                {
                    MessageBox.Show("No path for selected node in tree.", Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string id = tvUIMap.SelectedNode.Text;
                if (InputBox.Show("Rename UI element", "Enter new id:", ref id) == DialogResult.OK)
                {
                    string newPath = _uiMapFile.RenameUIObject(path, id);
 
                    RedrawTree(newPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiMapFileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((!this.uiMapFileSystemWatcher.EnableRaisingEvents) || (this.IsDisposed))
                return;

            try
            {
                this.uiMapFileSystemWatcher.EnableRaisingEvents = false; // avoid event being fired twice - see http://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice

                string msg = String.Format("{0}\n\nThis file has been modified outside {1}.\nDo you wan't to reload it?", e.FullPath, Program.ApplicationName);

                if (MessageBox.Show(msg, Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ReloadUIMapFile(e.FullPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Program.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.uiMapFileSystemWatcher.EnableRaisingEvents = true;
            }
        }
    }

    // Create a node sorter that implements the IComparer interface.
    public class TreeViewNodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;

            if ((tx == null) && (ty == null))
                return 0;
            
            if (tx == null)
                return 1;
            
            if (ty == null)
                return -1;

            return string.Compare(tx.Text, ty.Text);
        }
    }
}