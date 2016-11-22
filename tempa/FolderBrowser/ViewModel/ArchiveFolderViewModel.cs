using CoffeeJelly.tempa.FolderBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CoffeeJelly.tempa.ViewModel
{
    class ArchiveFolderViewModel : TreeViewItemViewModel
    {
        public ArchiveFolderViewModel(Folder folder, FileViewModel parent) :
            base(parent, false)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            _folder = folder;
            PropertyChanged += ArchiveViewModel_PropertyChanged;
        }

        protected override void LoadChildren()
        {
            try
            {
                _folder.ExploreFiles();
            }
            catch (Exception ex)
            {
                LogMaker.Log($"Нет доступа к данным каталога \"{_folder.FolderName}\"", true);
                ExceptionHandler.Handle(ex, false);
            }

            foreach (var fileInfo in _folder.Files)
                base.Children.Add(new FileViewModel(fileInfo, this));
        }

        private void ArchiveViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        private readonly Folder _folder;

        public Folder Folder => _folder;

        public string Name => _folder.FolderName;

        public string Path => _folder.FullName;


    }
}
