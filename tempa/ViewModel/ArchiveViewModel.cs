using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CoffeeJelly.tempa.ViewModel
{
    class ArchiveViewModel : INotifyPropertyChanged
    {
        public ArchiveViewModel()
        {
            PropertyChanged += ArchiveViewModel_PropertyChanged;
            this.ExpandedCommand = new ActionCommand<RoutedEventArgs>(OnExpanded);
        }



        private void ArchiveViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public ActionCommand<RoutedEventArgs> ExpandedCommand { get; private set; }

        private void OnExpanded(RoutedEventArgs e)
        {
            //var item = (TreeViewItem)e.OriginalSource;
            //FillTreeViewItemWithDirectories(ref item);
            //ScrollViewer scroller = (ScrollViewer)Internal.FindVisualChildElement(this.FileBrowsTreeView, typeof(ScrollViewer));
            //scroller.ScrollToBottom();
            //item.BringIntoView();
        }

        //public ICommand SelectedItemChanged
        //{
        //    get
        //    {
        //        return new DelegateCommand
        //        {
        //            CanExecuteFunc = () => true,
        //            CommandAction = () =>
        //            {
        //                int i = 0;
        //                String path = "";
        //                Stack<TreeViewItem> pathstack = Internal.GetNodes(e.NewValue as UIElement);
        //                if (pathstack.Count == 0)
        //                    return;
        //                foreach (TreeViewItem item in pathstack)
        //                {
        //                    if (i > 0)
        //                        path += item.Header.ToString().PathFormatter();
        //                    else
        //                        path += item.Header.ToString();
        //                    i++;
        //                }
        //                var treeView = sender as TreeView;
        //                if (treeView != null)
        //                {
        //                    var tag = (ProgramType)treeView.Tag;
        //                    if (tag == ProgramType.Agrolog)
        //                        AgrologReportsPath = path;
        //                    else
        //                        GrainbarReportsPath = path;
        //                }
        //            }
        //        };
        //    }
        //    set { }
        //}

    }
}
