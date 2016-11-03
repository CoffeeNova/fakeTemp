using CoffeeJelly.tempa.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CoffeeJelly.tempa.FileBrowser.ViewModel
{
    class FileBrowserViewModel : ViewModelBase
    {
        public FileBrowserViewModel(FileBrowserType type)
        {
            Type = type;
                base.PropertyChanged += FileBrowserViewModel_PropertyChanged;
        }

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

        private Task FolderExpandByPathAsync(string path, ObservableCollection<TreeViewItemViewModel> folderViewModels)
        {
            return Task.Factory.StartNew(new Action(() =>
            {
                 _continueExploreResetEvent.WaitOne();
                //await TaskEx.Delay(500);
                FolderExpandByPath(path, folderViewModels);
            }));
        }
        private void FolderExpandByPath(string path, ObservableCollection<TreeViewItemViewModel> folderViewModels)
        {
            foreach (var treeViewItemViewModel in folderViewModels)
            {
                var folderViewModel = treeViewItemViewModel as FolderViewModel;
                if (folderViewModel == null) continue;

                var splittedPath = path.Split('\\').ToList();
                splittedPath.RemoveAll(string.IsNullOrEmpty);

                foreach (string dirName in splittedPath)
                {
                    if (folderViewModel.FolderName.PathFormatter() != dirName.PathFormatter()) continue;

                    smNotifyViewModel.UIWindowInstance.Dispatcher.Invoke(new Action(() =>
                    {
                        folderViewModel.IsExpanded = false;
                        folderViewModel.IsExpanded = true;
                        folderViewModel.IsSelected = true;
                    }));
                    FolderExpandByPath(path.PathFormatter().ReplaceFirst(dirName.PathFormatter(), string.Empty), (folderViewModel.Children));
                    return;
                }
            }
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

        private void FileBrowserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Active))
                OnActive();
        }

        private async void OnActive()
        {
            if (Active)
            {
                FolderViewModel.SelectedFolderViewPathChanged += FolderViewModel_SelectedFolderViewPathChanged;

                ExploreRootDrives();
                if (Folders.Count == 0)
                    return;

                //await TaskEx.Delay(Timeout.Infinite, _continuationPermissionSource.Token);
                await FolderExpandByPathAsync(Path, Folders);
            }
            else
            {
                FolderViewModel.SelectedFolderViewPathChanged -= FolderViewModel_SelectedFolderViewPathChanged;
                _continueExploreResetEvent.Reset();
                Folders.Clear();

            }

        }

        private void FolderViewModel_SelectedFolderViewPathChanged(string selectedFolderViewPath)
        {
            Path = selectedFolderViewPath;
        }

        #region private fields
        private bool _active;
        private string _path;
        private ObservableCollection<TreeViewItemViewModel> _folders = new ObservableCollection<TreeViewItemViewModel>();
        private static readonly object _locker = new object();
        private readonly ManualResetEvent _continueExploreResetEvent = new ManualResetEvent(false);

        #endregion

        public FileBrowserType Type { get;}

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

        public ObservableCollection<TreeViewItemViewModel> Folders
        {
            get { return _folders; }
            set
            {
                _folders = value;
                NotifyPropertyChanged();
            }
        }

        //public ActionCommand<RoutedEventArgs> ExpandedCommand { get; private set; }

        //public ActionCommand<RoutedPropertyChangedEventArgs<object>> SelectedChangedCommand { get; private set; }


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

        public ICommand ContinueExploreCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        _continueExploreResetEvent.Set();
                    }
                };
            }
        }

        public ICommand BringIntoViewCommand
        {
            get; set;
        }
    }

    public enum FileBrowserType
    {
        Agrolog,
        Grainbar,
        ArchiveAgrolog,
        ArchiveGrainbar
    }
}
