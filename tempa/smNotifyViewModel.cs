using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CoffeeJelly.tempa.Extensions;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

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

                        if (_uiWindow == null || _uiWindow.Dispatcher.HasShutdownFinished)
                        {
                            var hwndSources = HwndSource.CurrentSources;
                            foreach (PresentationSource hwnd in hwndSources)
                            {
                                var window = hwnd.RootVisual as Window;
                                if (window.GetType() == typeof(MainPlotWindow))
                                    return;
                            }
                            OpenUIAsync();

                        }
                        else if ((bool)_uiWindow.Dispatcher.Invoke(new Func<bool>(() => _uiWindow.WindowState == WindowState.Minimized)))
                            _uiWindow.Dispatcher.Invoke(new Action(() =>  _uiWindow.WindowState = WindowState.Normal));
                        else
                            _uiWindow.Dispatcher.BeginInvoke(new Action(() => _uiWindow.Close()));

                        //var mWindow = Application.Current.MainWindow;
                        //if (mWindow.Visibility != Visibility.Visible || mWindow.WindowState != WindowState.Normal)
                        //{
                        //    mWindow.Visibility = Visibility.Visible;
                        //    mWindow.WindowState = WindowState.Normal;
                        //}
                        //else if (mWindow.Visibility == Visibility.Visible || mWindow.WindowState == WindowState.Normal)
                        //{
                        //    mWindow.Hide();

                        //}
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

                        foreach (PresentationSource hwnd in hwndSources)
                        {
                            var window = hwnd.RootVisual as Window;
                            window?.Dispatcher.BeginInvoke(new Action(() => window.Close()));
                        }

                        Application.Current.Shutdown();
                    }
                };
            }
        }

        private Task OpenUIAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            var uiWindowThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    OpenUICallBack();
                    tcs.SetResult(null);
                }

                catch (Exception ex)
                {
                    LogMaker.InvokedLog(($"Не удалось создать {nameof(MainWindow)}."), true, _uiWindow.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    _uiWindow = null;
                    tcs.SetResult(null);
                }
            }));

            uiWindowThread.SetApartmentState(ApartmentState.STA);
            uiWindowThread.IsBackground = true;
            uiWindowThread.Name = "UI Thread";
            uiWindowThread.Start();
            return tcs.Task;
        }

        private void OpenUICallBack()
        {
            _uiWindow = new MainWindow();

            try
            {
                _uiWindow.Show();
                _uiWindow.Closed += (sender, e) => CloseUICallback(sender, e);
                System.Windows.Threading.Dispatcher.Run();

            }
            catch (ThreadAbortException ex)
            {
                _uiWindow.Close();
                _uiWindow.Dispatcher.InvokeShutdown();
                _uiWindow = null;
                throw new Exception("Abort thread Exception", ex);
            }

        }

        private void CloseUICallback(object sender, EventArgs e)
        {
            LogMaker.InvokedLog(($"Закрытие графического окна."), false, _uiWindow.Dispatcher);
            _uiWindow.Dispatcher.InvokeShutdown();
            _uiWindow = null;
        }

        public static MainWindow _uiWindow = null;
    }

}







