using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using CoffeeJelly.tempa.FileBrowser.ViewModel;

namespace CoffeeJelly.tempa.Behaviors
{
    public class TextBoxEnterKeyFocusNextBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                base.OnAttached();
                this.AssociatedObject.KeyDown += AssociatedObject_KeyDown;
            }
        }

        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
                base.OnDetaching();
            }
        }

        private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
                if (e.Key == Key.Return)
                    if (e.Key == Key.Enter)
                    {
                        var viewModel = textBox.DataContext as FileBrowserViewModel;
                        if (viewModel == null) return;

                        if (viewModel.CreateNewFolderCommand.CanExecute(null))
                            viewModel.CreateNewFolderCommand.Execute(null);
                        textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        
                    }

        }
    }
}
