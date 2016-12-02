using CoffeeJelly.tempa.Exceptions;
using CoffeeJelly.tempadll.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa
{
    public interface IFolder
    {
        string FullName { get; }
        string FolderName { get; }
        object Info { get; }
        //FolderConfig Config { get; }
        string FilesPattern { get; }
        ObservableCollection<FileInfo> Files { get; }
        ObservableCollection<IFolder> SubFolders { get; }
    }

    public class Folder : IFolder
    {
        public void ExploreSubfolders()
        {
            SubFolders.Clear();
            DirectoryInfo dir = GetDirectoryInfo();
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    var newfolder = new Folder()
                    {
                        Info = subDir,
                        FolderName = subDir.ToString(),
                        FullName = subDir.FullName,
                    };
                    //newfolder.SubFolders.Add(new Folder());
                    SubFolders.Add(newfolder);
                }
            }
            catch (Exception ex)
            {
                throw new FolderException("Can't get subfolders", ex);
            }
        }

        public void ExploreFiles()
        {
            Debug.Assert(FilesPattern.Any((c) => c.EqualsAny(Path.GetInvalidFileNameChars())), "Invalid File Pattern!");
            Debug.Assert(string.IsNullOrEmpty(FilesPattern), "Set not empty FilesPattern value.");
            DirectoryInfo dir = GetDirectoryInfo();
            Files.Clear();
            foreach (var file in dir.GetFiles(FilesPattern))
            {
                Files.Add(file);
            }
        }

        public Folder CreateFolder(string folderName)
        {
            try
            {
                DirectoryInfo dir = GetDirectoryInfo();
                string path = dir.FullName.PathFormatter() + folderName;

                if (!Directory.Exists(path))
                {
                    DirectoryInfo newDir = Directory.CreateDirectory(path);
                    var newfolder = new Folder()
                    {
                        Info = newDir,
                        FolderName = newDir.ToString(),
                        FullName = newDir.FullName,
                    };
                    SubFolders.Add(newfolder);

                    return newfolder;
                }
                throw new InvalidOperationException("Directory already exists.");
            }
            catch (Exception ex)
            {
                throw new FolderException("Can't create a folder", ex);
            }
        }

        private DirectoryInfo GetDirectoryInfo()
        {
            DirectoryInfo dir;
            object info = this.Info;

            var driveInfo = info as DriveInfo;
            if (driveInfo != null)
            {
                DriveInfo drive = driveInfo;
                dir = drive.RootDirectory;
            }
            else dir = (DirectoryInfo)info;

            return dir;
        }


        public string FullName { get; set; }

        public string FolderName { get; set; }

        public object Info { get; set; }

        //public bool ExploreFile { get; set; }

        //public FolderConfig Config { get; set; }

        public string FilesPattern { get; set; } = "*.*";

        public ObservableCollection<IFolder> SubFolders { get; } = new ObservableCollection<IFolder>();

        public ObservableCollection<FileInfo> Files { get; } = new ObservableCollection<FileInfo>();
    }

    //public enum FolderConfig
    //{
    //    ContainsFoldersAndFiles = 0,
    //    ContainsFoldersOnly = 1,
    //    ContainsFilesOnly = 2
    //}
}
