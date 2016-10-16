using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

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
                        var mWindow = Application.Current.MainWindow;
                        if (mWindow.Visibility != Visibility.Visible || mWindow.WindowState != WindowState.Normal)
                        {
                            mWindow.Visibility = Visibility.Visible;
                            mWindow.WindowState = WindowState.Normal;
                        }
                        else if (mWindow.Visibility == Visibility.Visible || mWindow.WindowState == WindowState.Normal)
                        {
                            mWindow.Hide();

                        }
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
                        var mWindow = Application.Current.MainWindow;
                        mWindow.Visibility = Visibility.Visible;
                        mWindow.WindowState = WindowState.Normal;
                        MainWindow mainWindow = ((MainWindow)mWindow);
                        if (!mainWindow.IsFileBrowsTreeOnForm && !mainWindow.IsSettingsGridOnForm && !mainWindow.IsAboutOnForm)
                            mainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.SettingShowEvent, mainWindow));
                    }
                };
            }
        }

        public ICommand ShowAbout
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        var mWindow = Application.Current.MainWindow;
                        mWindow.Visibility = Visibility.Visible;
                        mWindow.WindowState = WindowState.Normal;
                        MainWindow mainWindow = ((MainWindow)mWindow);
                        if (!mainWindow.IsFileBrowsTreeOnForm && !mainWindow.IsSettingsGridOnForm && !mainWindow.IsAboutOnForm)
                            mainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.AboutShowEvent, mainWindow));
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
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        var hwndSources = HwndSource.CurrentSources;

                        foreach(PresentationSource hwnd in hwndSources)
                        {
                            var window = hwnd.RootVisual as Window;
                            window?.Dispatcher.BeginInvoke(new Action(() => window.Close()));
                        }

                        Application.Current.Shutdown();
                    }
                };
            }
        }

    }

}







