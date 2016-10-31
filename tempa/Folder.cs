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
        bool Expanded { get; }
        bool Selected { get; }
        //ObservableCollection<FileInfo> Files { get; }
        ObservableCollection<IFolder> SubFolders { get; }
    }

    public class Folder : IFolder, INotifyPropertyChanged
    {
        public Folder()
        {
            PropertyChanged += Folder_PropertyChanged;
        }

        private void ExploreSubfolders()
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
                    newfolder.SubFolders.Add(new Folder());
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

        private void OnExpanded()
        {
            ExploreSubfolders();
        }

        private void Folder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Expanded))
                OnExpanded();
        }

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private bool _expanded;
        private string _folderName;

        public string FullPath { get; set;}

        public string FolderName
        {
            get { return _folderName; }
            set
            {
                if (_folderName == value)
                    return;
                _folderName = value;
                NotifyPropertyChanged();
            }
        }

        public object Info { get; set; }


        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded == value)
                    return;
                _expanded = value;
                NotifyPropertyChanged();
            }
        }

        public bool Selected { get; set; }

        public bool ExploreFile { get; set; }

        public ObservableCollection<IFolder> SubFolders { get; set; } = new ObservableCollection<IFolder>();


    }
}
