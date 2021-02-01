using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.Interop;
using SharpShell.SharpContextMenu;
using SharpShell.SharpDropHandler;
using SharpShell.SharpIconHandler;
using SharpShell.SharpIconOverlayHandler;
using Extension;

namespace DateStatusCopier {
    static internal class DateStatusIcons {
        static internal Image Main {
            get;
        }
        static internal Image Document {
            get;
        }
        static internal Image Advanced {
            get;
        }
        static internal Image Configuration {
            get;
        }
        static internal Image Universal {
            get;
        }
        static internal Image Special {
            get;
        }
        static internal Image SpecialWithDefault {
            get;
        }
        static internal Image Recursive {
            get;
        }
    }
    static internal class ContextMenuStripItems {
        static readonly ToolStripSeparator seperator = new ToolStripSeparator();
        static readonly ToolStripMenuItem document = new ToolStripMenuItem("帮助文档", DateStatusIcons.Document, (sender, e) => {
            throw new NotImplementedException();
        });
        static ToolStripMenuItem Advanced(string[] paths) => new ToolStripMenuItem("自定义", DateStatusIcons.Advanced, (sender, e) => {
            AdvancedGenerator advancedGenerator = new AdvancedGenerator(paths);
            advancedGenerator.ShowDialog();
        });

        static internal ToolStripMenuItem SingleMenu(string path) {
            var result = new ToolStripMenuItem {
                Text = "生成日期文件",
                DropDownItems = {
                    new ToolStripMenuItem("通用（仅应用于拖放文件）", DateStatusIcons.Universal, (sender, e) => {
                        var content = Generator.Generate(path);
                        Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path),"universal.date-status"),content);
                    }),
                    new ToolStripMenuItem("通用（应用于子结构）", DateStatusIcons.Universal, (sender, e) => {
                        var content = Generator.Generate(path, recursive:true);
                        Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path),"universal.date-status"),content);
                    }),
                    new ToolStripMenuItem(Directory.Exists(path)?"专用（仅文件夹）":"专用", DateStatusIcons.Special, (sender, e) => {
                        var content = Generator.Generate(path, true);
                        Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path),"special.date-status"),content);
                    })
                }
            };
            if (Directory.Exists(path))
                result.DropDownItems.Add(new ToolStripMenuItem("专用（包括子结构）", DateStatusIcons.Recursive, (sender, e) => {
                    var content = Generator.Generate(path, true, true);
                    Generator.WriteFile(Path.Combine(Path.GetDirectoryName(path), "special.date-status"), content);
                }));
            result.DropDownItems.Add(Advanced(new string[] { path }));
            result.DropDownItems.Add(seperator);
            result.DropDownItems.Add(document);
            return result;
        }
        static internal ToolStripMenuItem MultipleMenu(string[] paths) {
            ToolStripMenuItem result = new ToolStripMenuItem("生成日期文件");
            result.DropDownItems.Add(new ToolStripMenuItem("专用（仅文件夹本身）", DateStatusIcons.Universal, (sender, e) => {
                var content = Generator.Generate(paths);
                Generator.WriteFile(Path.Combine(PathTool.GetCommonPath(paths), "special.date-status"), content);
            }));
            if (paths.Any(path => Directory.Exists(path)))
                result.DropDownItems.Add(new ToolStripMenuItem("专用（包括文件夹子结构）", DateStatusIcons.Universal, (sender, e) => {
                    var content = Generator.Generate(paths, true);
                    Generator.WriteFile(Path.Combine(PathTool.GetCommonPath(paths), "special.date-status"), content);
                }));
            result.DropDownItems.Add(Advanced(paths));
            result.DropDownItems.Add(seperator);
            result.DropDownItems.Add(document);
            return result;
        }
        static internal ToolStripMenuItem DateStatusMenu(string path) => new ToolStripMenuItem("更改日期配置", DateStatusIcons.Configuration, (sender, e) => {
            var modifier = new DateStatusModifier(path);
            modifier.ShowDialog();
        });
        static internal ToolStripMenuItem DirectoryMenu(string path) {
            string[] paths = Directory.GetFiles(path).Concat(Directory.GetDirectories(path)).ToArray();
            return MultipleMenu(paths);
        }
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFilesAndFolders)]
    class FileFolderContextMenu : SharpContextMenu {
        protected override bool CanShowMenu() => true;
        protected override ContextMenuStrip CreateMenu() {
            string[] selectedPaths = SelectedItemPaths.ToArray();
            var menu = new ContextMenuStrip();
            if (selectedPaths.Length == 1) {
                menu.Items.Add(ContextMenuStripItems.SingleMenu(selectedPaths[0]));
                if (Path.GetExtension(selectedPaths[0]) == ".date-status")
                    menu.Items.Add(ContextMenuStripItems.DateStatusMenu(selectedPaths[0]));
            }
            else
                menu.Items.Add(ContextMenuStripItems.MultipleMenu(selectedPaths));
            return menu;
        }
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.DirectoryBackground)]
    class BackgroundContextMenu : SharpContextMenu {
        protected override bool CanShowMenu() => true;
        protected override ContextMenuStrip CreateMenu() => new ContextMenuStrip {
            Items = { ContextMenuStripItems.DirectoryMenu(FolderPath) }
        };
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".date-status")]
    class DropHandler : SharpDropHandler {
        protected override void DragEnter(DragEventArgs dragEventArgs) => dragEventArgs.Effect = DragDropEffects.Link;
        protected override void Drop(DragEventArgs dragEventArgs) => Applier.Apply(DragItems.ToArray(), SelectedItemPath);
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".date-status")]
    class IconHandler : SharpIconHandler {
        protected override Icon GetIcon(bool smallIcon, uint iconSize) => throw new NotImplementedException();
    }
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".date-status")]
    class IconOverlayHandler : SharpIconOverlayHandler {
        protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes) => throw new NotImplementedException();
        protected override Icon GetOverlayIcon() => throw new NotImplementedException();
        protected override int GetPriority() => throw new NotImplementedException();
    }
}