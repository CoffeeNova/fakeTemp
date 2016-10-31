using CoffeeJelly.tempa.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa
{
    public interface IFolder
    {
        string FullPath { get; }
        string FolderName { get; }
        object Info { get; }

        //ObservableCollection<FileInfo> Files { get; }
        ObservableCollection<IFolder> SubFolders { get; }
    }

    public class Folder : IFolder
    {
        public void ExploreSubfolders()
        {
            SubFolders.Clear();
            DirectoryInfo dir = GetDirectoryInfo(this);
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    var newfolder = new Folder()
                    {
                        Info = subDir,
                        FolderName = subDir.ToString(),
                        FullPath = subDir.FullName,
                    };
                    //newfolder.SubFolders.Add(new Folder());
                    SubFolders.Add(newfolder);
                }
            }
            catch(Exception ex)
            {
                throw new FolderException("Can't get subfolders", ex);
            }
        }

        private DirectoryInfo GetDirectoryInfo(Folder folder)
        {
            DirectoryInfo dir;
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

        public bool ExploreFile { get; set; }

        public ObservableCollection<IFolder> SubFolders { get;} = new ObservableCollection<IFolder>();


    }
}
