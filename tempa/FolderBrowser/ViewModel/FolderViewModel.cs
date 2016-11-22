using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa.FolderBrowser.ViewModel
{
    public class FolderViewModel : TreeViewItemViewModel
    {
        public FolderViewModel(Folder folder, FolderViewModel parent, bool haveChildren)
            : base(parent, haveChildren)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            base.PropertyChanged += FolderViewModel_PropertyChanged;
            _folder = folder;
        }

        protected override void LoadChildren()
        {
            try
            {
                _folder.ExploreSubfolders();
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Нет доступа к данным каталога \"{_folder.FolderName}\"", true);
                ExceptionHandler.Handle(ex, false);
            }

            foreach (Folder folder in _folder.SubFolders)
            {
                bool haveChildren = true;
                try
                {
                    var directoryInfo = folder.Info as DirectoryInfo;
                    haveChildren = directoryInfo != null && directoryInfo.GetDirectories().Length != 0;
                }
                catch (UnauthorizedAccessException)
                { }
                base.Children.Add(new FolderViewModel(folder, this, haveChildren));
            }
        }

        private void FolderViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(base.IsSelected) && (sender as FolderViewModel).IsSelected)
                SelectedFolderViewPathChanged?.Invoke((sender as FolderViewModel).FullName);
        }

        public delegate void SelectedFolderViewPathChangedHandler(string selectedFolderViewPath);
        public static event SelectedFolderViewPathChangedHandler SelectedFolderViewPathChanged;

        private readonly Folder _folder;

        public Folder Folder => _folder;

        public string Name => _folder.FolderName;

        public string FullName => _folder.FullName;
    }
}
