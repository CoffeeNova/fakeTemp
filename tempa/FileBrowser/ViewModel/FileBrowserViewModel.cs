using CoffeeJelly.tempa.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CoffeeJelly.tempa.FileBrowser.ViewModel
{
    class FileBrowserViewModel : ViewModelBase
    {
        public FileBrowserViewModel()
        {
            base.PropertyChanged += FileBrowserViewModel_PropertyChanged;
            ExpandedCommand = new ActionCommand<RoutedEventArgs>(OnExpand);
        }


        //public FileBrowserViewModel(string path, ProgramType type)
        //{
        //this.SelectedChangedCommand = new ActionCommand<RoutedPropertyChangedEventArgs<object>>(OnSelectedChanged);

        //Path = path;
        //Type = type;

        //ExploreRootDrives();
        //if (Folders.Count == 0)
        //    return;

        //FolderExpandByPath(Path, Folders);

        //}

        private void ExploreRootDrives()
        {
            Folders.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                var folder = new Folder()
                {
                    Info = drive,
                    FolderName = drive.ToString(),
                    FullPath = drive.RootDirectory.FullName,
                };
                try
                {
                    bool haveChildren = drive.RootDirectory.GetDirectories().Length != 0;
                    Folders.Add(new FolderViewModel(folder, null, haveChildren));
                }
                catch { }
            }
        }


        //private void FolderExpandByPath(string path, ObservableCollection<IFolder> folders)
        //{
        //    foreach (Folder folder in folders)
        //    {
        //        var splittedPath = path.Split('\\').ToList();
        //        splittedPath.RemoveAll(string.IsNullOrEmpty);

        //        foreach (string dirName in splittedPath)
        //        {
        //            if (folder.FolderName.PathFormatter() != dirName.PathFormatter()) continue;

        //            folder.Expanded = false;
        //            folder.Expanded = true;
        //            folder.Selected = true;

        //            FolderExpandByPath(path.ReplaceFirst(dirName.PathFormatter(), string.Empty), folder.SubFolders);
        //            break;
        //        }
        //    }
        //}

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

        private void FileBrowserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Active))
                OnActive();

        }

        private void OnActive()
        {
            if (Active)
            {
                ExploreRootDrives();
                if (Folders.Count == 0)
                    return;

                //FolderExpandByPath(Path, Folders);
            }
            else
            {

            }

        }

        private void OnExpand(RoutedEventArgs e)
        {

        }

        #region private fields
        private bool _active;
        private string _path;
        private ObservableCollection<FolderViewModel> _folders = new ObservableCollection<FolderViewModel>();

        #endregion

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

        public bool ShowFiles { get; set; } = false;

        public ObservableCollection<FolderViewModel> Folders
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
            get; set;
        }

        public enum FileBrowserType
        {
            Agrolog,
            Grainbar,
            ArchiveAgrolog,
            ArchiveGrainbar
        }
    }
}
