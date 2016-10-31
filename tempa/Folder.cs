using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa
{
    public interface IFolder
    {
        string FullPath { get; }
        string FolderName { get; }
        object Info { get; }
        bool Expanded { get; }
        bool Selected { get; }
        List<IFolder> SubFolders { get; }
    }

    public class Folder : IFolder
    {
        public Folder(string fullPath)
        {
        }

        private void FillTreeViewItemWithDirectories(ref TreeViewItem treeViewItem)
        {
            treeViewItem.Items.Clear();
            DirectoryInfo dir = GetDirectoryInfo(treeViewItem, false);
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    var newItem = new TreeViewItem
                    {
                        Tag = subDir,
                        Header = subDir.ToString()
                    };
                    newItem.Items.Add("*");
                    treeViewItem.Items.Add(newItem);
                }
            }
            catch
            {
                // ignored
            }
        }

        private DirectoryInfo GetDirectoryInfo(Folder folder, bool anotherThread)
        {
            DirectoryInfo dir;
            //object tag = anotherThread ? Dispatcher.Invoke(new Func<object>(() => folder.Tag)) : folder.Tag;
            object info = folder.Info;

            var driveInfo = info as DriveInfo;
            if (driveInfo != null)
            {
                DriveInfo drive = driveInfo;
                dir = drive.RootDirectory;
            }
            else dir = (DirectoryInfo)info;

            return dir;
        }

        public string FullPath { get; set;}

        public string FolderName { get; set; }

        public object Info { get; set; }

        public bool Expanded { get; set; }

        public bool Selected { get; set; }

        public List<IFolder> SubFolders { get; set; } = new List<IFolder>();

    }
}
