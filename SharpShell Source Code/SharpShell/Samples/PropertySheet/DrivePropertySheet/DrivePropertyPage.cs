using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpShell.SharpPropertySheet;

namespace DrivePropertySheet
{
    public partial class DrivePropertyPage : UserControl, ISharpPropertyPage
    {
        public DrivePropertyPage()
        {
            InitializeComponent();
        }

        private void DrivePropertyPage_Load(object sender, EventArgs e)
        {
            SharpShell.Diagnostics.Logging.Log("Drive Property Page Loaded");
        }

        public string Title
        {
            get { return "Extended Text File Properties"; }
        }

        public System.Drawing.Icon Icon
        {
            get { return null; }
        }

        public void OnPageInitialised(SharpPropertySheet parent)
        {
        }
    }
}
