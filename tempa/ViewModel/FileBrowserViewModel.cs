using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CoffeeJelly.tempa.ViewModel
{
    class FileBrowserViewModel : INotifyPropertyChanged
    {
        public FileBrowserViewModel()
        {
            this.ActivateCommand = new ActionCommand<RoutedEventArgs>(OnActivate);
        }

        public ActionCommand<RoutedEventArgs> ActivateCommand { get; private set; }

        private void OnActivate(RoutedEventArgs e)
        {

        }


        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region private fields
        private bool _active;
        private string _path;
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

    }
}
