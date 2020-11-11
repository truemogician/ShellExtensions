using System;
using System.Linq;
using System.Windows.Forms;
using SharpShell.SharpPropertySheet;

namespace TextFilePropertySheet
{
    public partial class TextFilePropertyPage : SharpPropertyPage
    {
        public TextFilePropertyPage()
        {
            InitializeComponent();

            //  Set the title.
            PageTitle = "Text Properties";
        }

        private void DrivePropertyPage_Load(object sender, EventArgs e)
        {
            SharpShell.Diagnostics.Logging.Log("Text File Property Page Loaded");
        }

        public override void OnPageInitialised(SharpPropertySheet parent)
        {
            //  Set the name of the file in the text box.
            textBoxFileName.Text = parent.SelectedItemPaths.FirstOrDefault();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SetPageDataChanged(true);
        }
    }
}
