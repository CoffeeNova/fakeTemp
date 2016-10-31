using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa.FileBrowser.ViewModel
{
    public class FolderViewModel : TreeViewItemViewModel
    {
        public FolderViewModel(Folder folder, FolderViewModel parent, bool haveChildren)
            : base(parent, haveChildren)
        {
            _folder = folder;
        }


        private readonly Folder _folder;

        public string FolderName => _folder.FolderName;

        protected override void LoadChildren()
        {
            try
            {
                _folder.ExploreSubfolders();
            }
            catch(Exception ex)
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
    }
}
