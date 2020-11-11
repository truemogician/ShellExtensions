using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpShell.SharpPropertySheet;

namespace ResourcesPropertySheet
{
    public partial class UnmanagedResourcesPropertyPage : SharpPropertyPage
    {
        public UnmanagedResourcesPropertyPage()
        {
            InitializeComponent();

            PageTitle = "Unmanaged Resources";
        }
    }
}
