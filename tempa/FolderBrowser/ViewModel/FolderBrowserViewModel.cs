using CoffeeJelly.tempa.Exceptions;
using CoffeeJelly.tempadll.Extensions;
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
using CoffeeJelly.tempadll;

namespace CoffeeJelly.tempa.FolderBrowser.ViewModel
{
    class FolderBrowserViewModel : ViewModelBase
    {
        public FolderBrowserViewModel(FolderBrowserViewModelType type)
        {
            Type = type;
            base.PropertyChanged += FolderBrowserViewModel_PropertyChanged;
            CreateNewFolderCommand = new ActionCommand<string>(OnCreateNewFolder);
        }

        private void ExploreRootDrives()
        {
            Debug.Assert(TypeComparer(FolderBrowserViewModelType.AgrologReports, FolderBrowserViewModelType.GrainbarReports),
                    "This operation is possible only when FolderBrowserViewModel.Type defined as Agrolog or Grainbar.");

            Items.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                var folder = new Folder()
                {
                    Info = drive,
                    FolderName = drive.ToString(),
                    FullName = drive.RootDirectory.FullName,

                };
                try
                {
                    bool haveChildren = drive.RootDirectory.GetDirectories().Length != 0;
                    Items.Add(new FolderViewModel(folder, null, haveChildren));
                }
                catch { }
            }
        }

        private void ExploreFiles()
        {
            string directoryPath = string.Empty;

            try
            {
                Debug.Assert(TypeComparer(FolderBrowserViewModelType.AgrologData, FolderBrowserViewModelType.GrainbarData),
                    "This operation is possible only when FolderBrowserViewModel.Type defined as archive.");
                Debug.Assert(System.IO.Path.HasExtension(Path),
                    "Define FolderBrowserViewModel.Path with pattert file name which contains extension for defined Type.");

                string patternExtension = System.IO.Path.GetExtension(Path);

                Items.Clear();
                var currentDataFileName = Type == FolderBrowserViewModelType.AgrologData
                    ? Constants.AGROLOG_DATA_FILE
                    : Constants.GRAINBAR_DATA_FILE;
                var archiveDataFolderPath = Type == FolderBrowserViewModelType.AgrologData
                    ? Constants.APPLICATION_ARCHIVE_AGROLOG_DATA_FOLDER_PATH
                    : Constants.APPLICATION_ARCHIVE_GRAINBAR_DATA_FOLDER_PATH;

                Items.Add(new FileViewModel(new FileInfo(Constants.APPLICATION_DATA_FOLDER_PATH.PathFormatter() +
                    currentDataFileName), null)
                {
                    Name = Constants.ARCHIVE_DATA_CURRENT_INSCRIPTION
                });
                foreach (string fileName in Directory.GetFiles(archiveDataFolderPath, "*" + patternExtension))
                {
                    if (!CoffeeJTools.ArchiveDataFileNameValidation(Constants.ARCHIVE_DATA_FILE_NAME_DATE_FORMAT, fileName))
                        continue;
                    var info = new FileInfo(fileName);
                    Items.Add(new FileViewModel(info, null));
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                ExceptionHandler.Handle(ex, false);
                LogMaker.Log($"Системная директория {directoryPath} не обнаружена.", true);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, false);
                LogMaker.Log($"Невозможно отобразить файлы в каталоге {directoryPath}.", true);
            }
        }

        private bool TypeComparer(params FolderBrowserViewModelType[] patternType)
        {
            if (Enum.IsDefined(typeof(FolderBrowserViewModelType), Type) &&
                    Type.EqualsAny(patternType))
                return true;
            return false;
        }

        private Task FolderExpandByPathAsync(string path, ObservableCollection<TreeViewItemViewModel> folderViewModels)
        {
            return Task.Factory.StartNew(new Action(() =>
            {
                _continueExploreResetEvent.WaitOne();
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
                    if (!folderViewModel.Name.PathFormatter().Equals(dirName.PathFormatter(), StringComparison.InvariantCultureIgnoreCase))
                        continue;

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

        private void FolderExpandByPathSecond(ObservableCollection<TreeViewItemViewModel> treeViewModelCollection)
        {
            var developedCollection = DevelopedCollection(treeViewModelCollection);

            var item = developedCollection.FirstOrDefault((t) =>
            {
                var f = t as FolderViewModel;
                if (f == null) return false;

                return f.FullName == Path;
            });

            item.IsExpanded = false;
            item.IsExpanded = true;
        }

        private List<TreeViewItemViewModel> DevelopedCollection(ObservableCollection<TreeViewItemViewModel> sourceCollection)
        {
            var developedCollection = new List<TreeViewItemViewModel>();
            if (sourceCollection == null) return developedCollection;

            foreach (var treeViewItemViewModel in sourceCollection)
            {
                if (treeViewItemViewModel == null) continue;
                var test = DevelopedCollection(treeViewItemViewModel.Children);
                developedCollection.AddRange(test);
                developedCollection.Add(treeViewItemViewModel);
            }
            return developedCollection;
        }

        private DirectoryInfo CreateNewFolder(string folderName)
        {
            var selectedTreeViewItemViewModell = FindSelectedTreeViewItemViewModel();

            var selectedFolderViewModel = selectedTreeViewItemViewModell as FolderViewModel;

            try
            {
                if (selectedFolderViewModel == null) throw new InvalidOperationException("Selected view model should be as FolderViewModel");

                var newFolder = selectedFolderViewModel.Folder.CreateFolder(folderName);
                selectedFolderViewModel.Children.Add(new FolderViewModel(newFolder, selectedFolderViewModel, false));

                LogMaker.Log($"Создана новая папка: \"{newFolder.FullName}\".", false);
                return (DirectoryInfo)newFolder.Info;
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
            catch (FolderException ex)
            {
                if (ex.InnerException.GetType() == typeof(ArgumentException))
                    LogMaker.Log("Имя папки содежит недопустимые символы, или содержит только пробелы.", true);
                else if (ex.InnerException.GetType() == typeof(InvalidOperationException))
                    LogMaker.Log($"Папка {folderName} уже существует в данной директории.", true);
                else
                    LogMaker.Log("Не удалось создать новую папку.", true);

                ExceptionHandler.Handle(ex, false);
            }
            return null;
        }

        private TreeViewItemViewModel FindSelectedTreeViewItemViewModel()
        {
            var developedCollection = DevelopedCollection(Items);
            return developedCollection.Single((t) => t.IsSelected);
        }

        private void FolderBrowserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Active))
            {
                if (!Enum.IsDefined(typeof(FolderBrowserViewModelType), Type))
                    return;
                if (Type == FolderBrowserViewModelType.AgrologReports || Type == FolderBrowserViewModelType.GrainbarReports)
                    OnActive();
                else
                    OnActiveArchive();
            }
        }

        private async void OnActive()
        {
            if (Active)
            {
                FolderViewModel.SelectedFolderViewPathChanged += SelectedTreeViewItemFullNameChanged;

                ExploreRootDrives();
                if (Items.Count == 0)
                    return;

                await FolderExpandByPathAsync(Path, Items);
            }
            else
            {
                FolderViewModel.SelectedFolderViewPathChanged -= SelectedTreeViewItemFullNameChanged;
                _continueExploreResetEvent.Reset();
                Items.Clear();

            }

        }

        private void OnActiveArchive()
        {
            if (Active)
            {
                FileViewModel.SelectedFolderViewFullNameChanged += SelectedTreeViewItemFullNameChanged;
                ExploreFiles();
            }
            else
            {
                FileViewModel.SelectedFolderViewFullNameChanged -= SelectedTreeViewItemFullNameChanged;
                Items.Clear();
            }
        }

        private void OnCreateNewFolder(string folderName)
        {
            _newFolderInfo = CreateNewFolder(folderName);
        }


        private void SelectedTreeViewItemFullNameChanged(string selectedViewFullName)
        {
            Path = selectedViewFullName;
        }

        #region private fields
        private bool _active;
        private string _path;
        private string _newFolderName = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
        private ObservableCollection<TreeViewItemViewModel> _items = new ObservableCollection<TreeViewItemViewModel>();
        private static readonly object _locker = new object();
        private readonly ManualResetEvent _continueExploreResetEvent = new ManualResetEvent(false);
        private DirectoryInfo _newFolderInfo;
        private bool _createNewFolderInvolve;

        #endregion

        public FolderBrowserViewModelType Type { get; }

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

        /// <summary>
        /// Поный путь к папке (если тип экземпляра класса <see cref="Type"/> 
        /// равен <see langword="FolderBrowserType.Agrolog"/> или <see langword="FolderBrowserType.Grainbar"/>),
        /// или поный путь к файлу (если тип экземпляра класса <see cref="Type"/> 
        /// равен <see langword="FolderBrowserType.ArchiveAgrolog"/> или <see langword="FolderBrowserType.ArchiveGrainbar"/>),
        /// </summary>
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

        public ObservableCollection<TreeViewItemViewModel> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyPropertyChanged();
            }
        }

        public string NewFolderName
        {
            get { return _newFolderName; }
            set
            {
                if (_newFolderName == value)
                    return;
                _newFolderName = value;
                NotifyPropertyChanged();
            }
        }

        public bool CreateNewFolderInvolve
        {
            get { return _createNewFolderInvolve; }
            set
            {
                if (_createNewFolderInvolve == value)
                    return;
                _createNewFolderInvolve = value;
                NotifyPropertyChanged();
            }
        }

        //public ActionCommand<RoutedEventArgs> ExpandedCommand { get; private set; }

        //public ActionCommand<RoutedPropertyChangedEventArgs<object>> SelectedChangedCommand { get; private set; }

        public DirectoryInfo NewFolderInfo => _newFolderInfo;


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

        public ICommand SavePathCommand
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

        //public ICommand NewFolderNameRegistrationCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand
        //        {
        //            CommandAction = () =>
        //            {
        //                NewFolderName = Constants.NEW_FOLDER_TEXT_BOX_INITIAL_TEXT;
        //            }
        //        };
        //    }
        //}

        public ActionCommand<string> CreateNewFolderCommand { get; private set; }

        public ICommand BringIntoViewCommand
        {
            get; set;
        }
    }

    public enum FolderBrowserViewModelType
    {
        AgrologReports,
        GrainbarReports,
        AgrologData,
        GrainbarData
    }
}
