using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CoffeeJelly.tempa.FolderBrowser.ViewModel
{
    public class FileViewModel : TreeViewItemViewModel
    {
        public FileViewModel(FileInfo info, TreeViewItemViewModel parent): base(parent, false)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            _info = info;
            _name = System.IO.Path.GetFileNameWithoutExtension(Info.FullName);
            base.PropertyChanged += FileViewModel_PropertyChanged;
        }

        protected override void LoadChildren()
        {
        }

        private void FileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(base.IsSelected) && (sender as FileViewModel).IsSelected)
                SelectedFolderViewFullNameChanged?.Invoke((sender as FileViewModel).FullName);
        }

        public delegate void SelectedFileViewPathChangedHandler(string selectedFolderViewFullName);
        public static event SelectedFileViewPathChangedHandler SelectedFolderViewFullNameChanged;

        private FileInfo _info;
        private string _name;

        public FileInfo Info => _info;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public string FullName => Info.FullName;
    }
}
