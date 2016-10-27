using CoffeeJelly.tempa.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CoffeeJelly.tempa.ViewModel
{
    class FileBrowserViewModel : INotifyPropertyChanged
    {
        public FileBrowserViewModel(string path, ProgramType type)
        {
            this.ExpandedCommand = new ActionCommand<RoutedEventArgs>(OnExpanded);
            this.SelectedChangedCommand = new ActionCommand<RoutedPropertyChangedEventArgs<object>>(OnSelectedChanged);

            Path = path;
            Type = type;

            FillTreeViewWithRootDrives();
            if (Folders.Count == 0)
                return;

            FolderExpandByPath(Path, Folders);
            //if ((ProgramType)button.Tag == ProgramType.Agrolog)
            //    await FileBrowsTreeViewDirExpandAsync(AgrologReportsPath, FileBrowsTreeView.Items);
            //else
            //    await FileBrowsTreeViewDirExpandAsync(GrainbarReportsPath, FileBrowsTreeView.Items);
        }

        private void FillTreeViewWithRootDrives()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                var folder = new Folder()
                {
                    Info = drive,
                    FolderName = drive.ToString(),
                    FullPath = drive.RootDirectory.FullName,
                    //SubFolders = new List<IFolder>()
                };
                folder.SubFolders.Add(new Folder());
                Folders.Add(folder);
            }
        }

        //private Task FileBrowsTreeViewDirExpandAsync(string path, List<IFolder> folders)
        //{
        //    return Task.Factory.StartNew(() => FileBrowsTreeViewDirExpand(path, folders));
        //}


        private void FolderExpandByPath(string path, List<IFolder> folders)
        {
            foreach (Folder folder in folders)
            {
                DirectoryInfo dir = GetDirectoryInfo(folder, true);

                var splittedPath = path.Split('\\').ToList();
                splittedPath.RemoveAll(string.IsNullOrEmpty);

                foreach (string dirName in splittedPath)
                {
                    if (dir.Name.PathFormatter() != dirName.PathFormatter()) continue;
                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    //    folder.IsExpanded = false;
                    //    folder.IsExpanded = true;
                    //    folder.IsSelected = true;
                    //}));

                    folder.Expanded = false;
                    folder.Expanded = true;
                    folder.Selected = true;

                    FolderExpandByPath(path.ReplaceFirst(dirName.PathFormatter(), string.Empty), folder.SubFolders);
                    break;
                }
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

        private void FillTreeViewItemWithDirectories(ref Folder folder)
        {
            folder.SubFolders.Clear();
            DirectoryInfo dir = GetDirectoryInfo(folder, false);
            try
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    var newFolder = new Folder
                    {
                        Info = subDir,
                        FolderName = subDir.ToString(),
                        FullPath = subDir.FullName
                    };
                    newFolder.SubFolders.Add(new Folder());
                    folder.SubFolders.Add(newFolder);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void OnExpanded(RoutedEventArgs e)
        {
            var folder = (Folder)e.OriginalSource;
            FillTreeViewItemWithDirectories(ref folder);
            folder.Expanded = true;
            //ScrollViewer scroller = (ScrollViewer)Internal.FindVisualChildElement(this.FileBrowsTreeView, typeof(ScrollViewer));
            //scroller.ScrollToBottom();
            //folder.BringIntoView();
        }

        private void OnSelectedChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            //String path = "";
            //Stack<TreeViewItem> pathstack = Internal.GetNodes(e.NewValue as UIElement);
            //if (pathstack.Count == 0)
            //    return;

            //int i = 0;
            //foreach (TreeViewItem item in pathstack)
            //{
            //    if (i > 0)
            //        path += item.Header.ToString().PathFormatter();
            //    else
            //        path += item.Header.ToString();
            //    i++;
            //}
            //var treeView = sender as TreeView;
            //if (treeView != null)
            //{
            //    var tag = (ProgramType)treeView.Tag;
            //    if (tag == ProgramType.Agrolog)
            //        AgrologReportsPath = path;
            //    else
            //        GrainbarReportsPath = path;
            //}
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region private fields
        private bool _active;
        private string _path;
        private List<IFolder> _folders = new List<IFolder>();

        #endregion

        public ProgramType Type { get; set; }

        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                if (_path == value)
                    return;
                _path = value;
                NotifyPropertyChanged();
            }
        }

        public List<IFolder> Folders
        {
            get { return _folders; }
            set
            {
                _folders = value;
                NotifyPropertyChanged();
            }
        }


        public ActionCommand<RoutedEventArgs> ExpandedCommand { get; private set; }

        public ActionCommand<RoutedPropertyChangedEventArgs<object>> SelectedChangedCommand { get; private set; }


        public ICommand ActivateCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Active = true;
                    }
                };
            }
        }

        public ICommand DeactivateCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        Active = false;
                    }
                };
            }
        }

        public ICommand BringIntoViewCommand
        {
            get;set;
        }
    }
}
