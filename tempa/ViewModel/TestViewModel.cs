using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CoffeeJelly.tempa.ViewModel
{
    class TestViewModel
    {
        public TestViewModel()
        {
            this.TestCommand = new ActionCommand<RoutedEventArgs>(OnCLick);
        }

        public ActionCommand<RoutedEventArgs> TestCommand { get; private set; }

        private void OnCLick(RoutedEventArgs e)
        {
            // ...
        }
    }
}
