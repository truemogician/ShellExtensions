using System.Text;
using System.IO;
using System.Windows.Forms;
using SharpShell.SharpContextMenu;
using System.Runtime.InteropServices;
using SharpShell.Attributes;

namespace CountLinesExtension {
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    public class CountLinesExtension : SharpContextMenu {
        protected override bool CanShowMenu() {
            return true;
        }
        protected override ContextMenuStrip CreateMenu() {
            //  Create the menu strip
            var menu = new ContextMenuStrip();

            //  Create a 'count lines' item
            var itemCountLines = new ToolStripMenuItem {
                Text = "Count Lines"
            };

            //  When we click, we'll call the 'CountLines' function
            itemCountLines.Click += (sender, args) => CountLines();

            //  Add the item to the context menu
            menu.Items.Add(itemCountLines);

            //  Return the menu
            return menu;
        }

        private void CountLines() {
            //  Builder for the output
            var builder = new StringBuilder();

            //  Go through each file
            foreach (var filePath in SelectedItemPaths) {
                //  Count the lines
                builder.AppendLine(string.Format("{0} - {1} Lines",
                  Path.GetFileName(filePath), File.ReadAllLines(filePath).Length));
            }

            //  Show the output
            MessageBox.Show(builder.ToString());
        }
    }
}
