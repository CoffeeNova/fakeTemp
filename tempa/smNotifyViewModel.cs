using System;
using System.Windows;
using System.Windows.Input;

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    class smNotifyViewModel
    {

        ///// <summary>
        //       /// Shows a window, if none is already open.
        //       /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        //Application.Current.MainWindow = new Magnet();
                        Application.Current.MainWindow.Visibility = Visibility.Visible;
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                    }
                };
            }
        }
        public ICommand ShowWindowSettings
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        Application.Current.MainWindow.Visibility = Visibility.Visible;
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        MainWindow _mainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);
                        if (!_mainWindow.IsFileBrowsTreeOnForm && !_mainWindow.IsSettingsGridOnForm)
                            _mainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.SettingShowEvent, _mainWindow));
                    }
                };
            }
        }

        //       /// <summary>
        //       /// Shuts down the application.
        //       /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

    }

}







