using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITest.Common;
using Microsoft.VisualStudio.TestTools.UITest.Common.UIMap;
using UIMapToolbox.Core;
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace UIMapToolbox.UI
{
    public partial class MainForm : Form
    {
        private bool _isExpanded = false;
        private FindElementForm _findElementForm = null;
        private UIMapTreeUserControl _currentActiveUIMap = null;
        private string _findText;

        public MainForm()
        {
            InitializeComponent();

            this.splitContainer1.Panel2Collapsed = true;
            _currentActiveUIMap = uiMapTreeUserControl1;
#if DEBUG
            //txtFileName.Text = String.Format(@"{0}\Samples\MainUIMap.uitest", System.IO.Directory.GetCurrentDirectory()); // will automatically trigger load of file
#endif
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox about = new AboutBox())
            {
                about.ShowDialog();
            }
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {
            ExpandCollapseForm();
        }

        private void ExpandCollapseForm()
        {
            if (!_isExpanded)
            {
                int panel1Width = splitContainer1.Panel1.Width;
                this.Width = this.Width * 2;
                splitContainer1.Panel2Collapsed = false;
                splitContainer1.SplitterDistance = panel1Width;
                btnExpandCollapse.Text = "<< Collapse";
                _isExpanded = true;
            }
            else
            {
                splitContainer1.Panel2Collapsed = true;
                this.Width = 482;
                btnExpandCollapse.Text = "Expand >>";
                _isExpanded = false;
            }
        }

        private void ExpandForm()
        {
            if (!_isExpanded)
                ExpandCollapseForm();
        }

        private void menuEditFind_Click(object sender, EventArgs e)
        {
            if ((_findElementForm == null) || (_findElementForm.IsDisposed))
            {
                _findElementForm = new FindElementForm();
                _findElementForm.FindNext += new EventHandler<FindNextEventArgs>(FindElementForm_FindNext);
            }

            if (!_findElementForm.Visible)
            {
                _findElementForm.Left = this.Left + _currentActiveUIMap.Parent.Right - _findElementForm.Width;
                _findElementForm.Top = this.Top + _currentActiveUIMap.Parent.Bottom - _findElementForm.Height;
                _findElementForm.Show(this);
            }

            _findElementForm.Focus();
            _findElementForm.SelectTextField();
        }

        void FindElementForm_FindNext(object sender, FindNextEventArgs e)
        {
            if (_currentActiveUIMap == null)
                return;

            _findText = e.Text;
            _currentActiveUIMap.FindNextNode(e.Text);
        }

        private void uiMapTreeUserControl1_Enter(object sender, EventArgs e)
        {
            if (_currentActiveUIMap == null)
                return;

            _currentActiveUIMap = uiMapTreeUserControl1;
        }

        private void uiMapTreeUserControl2_Enter(object sender, EventArgs e)
        {
            if (_currentActiveUIMap == null)
                return;

            _currentActiveUIMap = uiMapTreeUserControl2;
        }

        private void menuEditFindNext_Click(object sender, EventArgs e)
        {
            if (_currentActiveUIMap == null)
                return;

            _currentActiveUIMap.FindNextNode(_findText);
        }

        private void menuFileOpen_Click(object sender, EventArgs e)
        {
            if (_currentActiveUIMap == null)
                return;

            _currentActiveUIMap.ShowOpenFileDialog();
        }

        /// <summary>
        /// Loads the specified UIMap file into UI.
        /// </summary>
        /// <param name="pane">"Pane" number, i.e. 0 for left and 1 for right.</param>
        /// <param name="fileName">Name of the file.</param>
        public void LoadFile(int pane, string fileName)
        {
            switch (pane)
            {
                case 0:
                    uiMapTreeUserControl1.LoadUIMapFile(fileName);
                    break;

                case 1:
                    ExpandForm();
                    uiMapTreeUserControl2.LoadUIMapFile(fileName);
                    break;

                default:
                    MessageBox.Show("Pane number must be either 0 (left) or 1 (right)", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            if ((this.uiMapTreeUserControl1.UIMapFile != null) && (this.uiMapTreeUserControl1.UIMapFile.IsModified))
                sb.AppendFormat("UIMap '{0}' has been modified.\n", this.uiMapTreeUserControl1.UIMapFile.GetUIMapName());

            if ((this.uiMapTreeUserControl2.UIMapFile != null) && (this.uiMapTreeUserControl2.UIMapFile.IsModified))
                sb.AppendFormat("UIMap '{0}' has been modified.\n", this.uiMapTreeUserControl2.UIMapFile.GetUIMapName());

            if (sb.Length != 0)
            {
                sb.Append("\nClose anyway?");

                DialogResult answer = MessageBox.Show(sb.ToString(), Program.ApplicationName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                e.Cancel = (answer == DialogResult.No);
            }
        }
    }
}