using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CoffeeJelly.tempadll.Extensions;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    class smNotifyViewModel : INotifyPropertyChanged
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
                        lock (_locker)
                        {
                            if (UIWindowInstance == null || UIWindowInstance.Dispatcher.HasShutdownFinished)
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
                            else if ((bool)UIWindowInstance.Dispatcher.Invoke(new Func<bool>(() => UIWindowInstance.WindowState == WindowState.Minimized)))
                                UIWindowInstance.Dispatcher.Invoke(new Action(() => UIWindowInstance.WindowState = WindowState.Normal));
                            else
                                UIWindowInstance.Dispatcher.BeginInvoke(new Action(() => UIWindowInstance.Close()));

                        }
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
                    CanExecuteFunc = () => UIWindowInstance != null,
                    CommandAction = () =>
                    {
                        UIWindowInstance.Dispatcher.Invoke(new Action(() =>
                        {
                            UIWindowInstance.Visibility = Visibility.Visible;
                            UIWindowInstance.WindowState = WindowState.Normal;
                            if (!UIWindowInstance.IsFolderBrowsTreeOnForm && !UIWindowInstance.IsSettingsGridOnForm && !UIWindowInstance.IsAboutOnForm)
                                UIWindowInstance.RaiseEvent(new RoutedEventArgs(UIwindow.SettingShowEvent, UIWindowInstance));
                        }));
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
                    CanExecuteFunc = () => UIWindowInstance != null,
                    CommandAction = () =>
                    {
                        UIWindowInstance.Dispatcher.Invoke(new Action(() =>
                        {
                            UIWindowInstance.Visibility = Visibility.Visible;
                        UIWindowInstance.WindowState = WindowState.Normal;
                        if (!UIWindowInstance.IsFolderBrowsTreeOnForm && !UIWindowInstance.IsSettingsGridOnForm && !UIWindowInstance.IsAboutOnForm)
                            UIWindowInstance.RaiseEvent(new RoutedEventArgs(UIwindow.AboutShowEvent, UIWindowInstance));
                        }));
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
                    LogMaker.InvokedLog(($"Не удалось создать {nameof(UIwindow)}."), true, UIWindowInstance.Dispatcher);
                    ExceptionHandler.Handle(ex, false);
                    UIWindowInstance = null;
                    IsUIWindowExist = false;
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
            UIWindowInstance = new UIwindow();

            try
            {
                UIWindowInstance.Show();
                UIWindowInstance.Closed += (sender, e) => CloseUICallback(sender, e);
                IsUIWindowExist = true;
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (ThreadAbortException ex)
            {
                UIWindowInstance.Close();
                UIWindowInstance.Dispatcher.InvokeShutdown();
                UIWindowInstance = null;
                IsUIWindowExist = false;
                throw new Exception("Abort thread Exception", ex);
            }

        }

        private void CloseUICallback(object sender, EventArgs e)
        {
            LogMaker.InvokedLog(($"Закрытие графического окна."), false, UIWindowInstance.Dispatcher);
            UIWindowInstance.Dispatcher.InvokeShutdown();
            UIWindowInstance = null;
            IsUIWindowExist = false;
        }


        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsUIWindowExist
        {
            get { return _isUIWindowExist; }
            set
            {
                _isUIWindowExist = value;
                NotifyPropertyChanged();
            }
        }

        public static UIwindow UIWindowInstance = null;

        private static readonly object _locker = new object();
        private bool _isUIWindowExist = false;
    }

}







