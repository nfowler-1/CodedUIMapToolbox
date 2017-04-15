using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UIMapToolbox.UI
{
    static class Program
    {
        public const string ApplicationName = "UIMap Toolbox";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string msg = String.Format("An error occured. Please report to http://uimaptoolbox.codeplex.com with information on how to reproduce issue.\n\n{0}\n\nStack trace:\n{1}", e.Exception.Message, e.Exception.StackTrace);
            
            MessageBox.Show(msg, ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
