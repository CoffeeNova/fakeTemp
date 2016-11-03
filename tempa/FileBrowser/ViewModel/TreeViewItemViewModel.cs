using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CoffeeJelly.tempa.FileBrowser.ViewModel
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public abstract class TreeViewItemViewModel : ViewModelBase
    {

        #region Constructors

        protected TreeViewItemViewModel(TreeViewItemViewModel parent, bool lazyLoadChildren)
        {
            _parent = parent;

            _children = new ObservableCollection<TreeViewItemViewModel>();

            if (lazyLoadChildren)
                _children.Add(DummyChild);
        }

        // This is used to create the DummyChild instance.
        private TreeViewItemViewModel()
        {
        }

        #endregion // Constructors

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// </summary>
        protected abstract void LoadChildren();

        //protected abstract void OnSelected();

        #region private fields

        private static readonly TreeViewItemViewModel DummyChild = new DummyViewModel();

        readonly ObservableCollection<TreeViewItemViewModel> _children;
        private readonly TreeViewItemViewModel _parent;

        private bool _expanded;
        private bool _selected;

        #endregion // Data

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<TreeViewItemViewModel> Children => _children;

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        public TreeViewItemViewModel Parent => _parent;


        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _expanded; }
            set
            {
                if (this.HasDummyChild && value)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }

                if (value != _expanded)
                {
                    _expanded = value;
                    NotifyPropertyChanged();
                }
                
                // Expand all the way up to the root.
                if (_expanded && _parent != null && _parent.IsExpanded == false)
                    _parent.IsExpanded = true;

            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private class DummyViewModel : TreeViewItemViewModel
        {
            protected override void LoadChildren()
            {
            }
        }
    }
}