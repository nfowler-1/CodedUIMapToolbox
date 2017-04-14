using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UIMapToolbox.UI
{
    public partial class FindElementForm : Form
    {
        public FindElementForm()
        {
            InitializeComponent();
        }

        public event EventHandler<FindNextEventArgs> FindNext;

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (FindNext != null)
                FindNext(this, new FindNextEventArgs { Text = txtFindWhat.Text });
        }

        private void FindElementForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
                e.Handled = true;
            }
        }

        internal void SelectTextField()
        {
            this.txtFindWhat.Focus();
            this.txtFindWhat.SelectAll();
        }
    }

    public class FindNextEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}
