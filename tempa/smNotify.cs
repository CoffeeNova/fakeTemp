﻿using System;
using System.Windows;
using System.Windows.Input;

namespace tempa
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    class smNotify
    {
        private MainWindow _mainWindow = ((MainWindow)System.Windows.Application.Current.MainWindow);

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
                        if (!_mainWindow.IsFileBrowsTreeOnForm && !_mainWindow.IsSettingsGridOnForm)
                            _mainWindow.RaiseEvent(new RoutedEventArgs(MainWindow.SettingShowEvent, _mainWindow));
                    }
                };
            }
        }
        //       public ICommand ShowWindowHome
        //       {
        //           get
        //           {
        //               return new DelegateCommand
        //               {
        //                   CanExecuteFunc = () => Application.Current.MainWindow != null,
        //                   CommandAction = () =>
        //                   {
        //                       //Application.Current.MainWindow = new Magnet();
        //                       Application.Current.MainWindow.Visibility = Visibility.Visible;
        //                       Application.Current.MainWindow.WindowState = WindowState.Normal;
        //                       Uri uri = new Uri("/Pages/Home.xaml", UriKind.Relative);
        //                       MainWindow.navigateLink = uri;

        //                   }
        //               };
        //           }
        //       }
        //       public ICommand ShowWindowTwo
        //       {
        //           get
        //           {
        //               return new DelegateCommand
        //               {
        //                   CanExecuteFunc = () => Application.Current.MainWindow != null,
        //                   CommandAction = () =>
        //                   {
        //                       //Application.Current.MainWindow = new Magnet();
        //                       Application.Current.MainWindow.Visibility = Visibility.Visible;
        //                       Application.Current.MainWindow.WindowState = WindowState.Normal;
        //                       Uri uri = new Uri("/Pages/Two.xaml", UriKind.Relative);
        //                       MainWindow.navigateLink = uri;

        //                   }
        //               };
        //           }
        //       }
        //       /// <summary>
        //       /// Hides the main window. This command is only enabled if a window is open.
        //       /// </summary>
        //       //public ICommand HideWindowCommand
        //       //{
        //       //    get
        //       //    {
        //       //        return new DelegateCommand
        //       //        {
        //       //            CommandAction = () => Application.Current.MainWindow.Close(),
        //       //            CanExecuteFunc = () => Application.Current.MainWindow != null
        //       //        };
        //       //    }
        //       //}


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
    //   /// <summary>
    //   /// Simplistic delegate command for the demo.
    //   /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}







