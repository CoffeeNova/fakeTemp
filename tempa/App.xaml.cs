﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NLog;
using CoffeeJelly.tempadll.Extensions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Markup;
using CoffeeJelly.tempadll;

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] startupArguments;
        public TaskbarIcon notifyIcon;
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private void Application_startup(object sender, StartupEventArgs e)
        {
            startupArguments = e.Args;

#if DEBUG
            _log.Debug("START DEBUGGING!");
#endif
            _log.Info($"{Constants.APPLICATION_NAME} starting.");
            Process[] pr = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (pr.Length > 1)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                _log.Info("Attempt to start another instance of the application. {0} closing.", Constants.APPLICATION_NAME);
            }

            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Ru");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-Ru");

            Thread.CurrentThread.CurrentCulture.GetType()
                .GetProperty("DefaultThreadCurrentCulture")
                .SetValue(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentCulture, null);
            Thread.CurrentThread.CurrentCulture.GetType()
                .GetProperty("DefaultThreadCurrentUICulture")
                .SetValue(Thread.CurrentThread.CurrentUICulture, Thread.CurrentThread.CurrentUICulture, null);

            //create the notifyicon (it's a resource declared in smNotify.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // Select the text in a TextBox when it receives focus.
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseLeftButtonDownEvent,
                new MouseButtonEventHandler(SelectivelyIgnoreMouseButton));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotKeyboardFocusEvent,
                new RoutedEventHandler(SelectAllText));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.MouseDoubleClickEvent,
                new RoutedEventHandler(SelectAllText));

            try
            {
                var win = new NewDataWatcherWindow();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, true);
            }

            _log.Info("{0} is started successfully.", Constants.APPLICATION_NAME);
        }

        

        void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focused, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NewDataWatcherWindow.ExitCancelTokenSource.CancelAfter(2000);
            CriticalTasks.Cleanup();
            CriticalTasks.WaitOnExit();

            var hwndSources = HwndSource.CurrentSources;

            foreach (PresentationSource hwnd in hwndSources)
            {
                var window = hwnd.RootVisual as Window;
                window?.Dispatcher.BeginInvoke(new Action(() => window.Close()));
            }


            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
            System.Threading.Timer closeTimer = new System.Threading.Timer((object state) => System.Diagnostics.Process.GetCurrentProcess().Kill(), null, 10000, System.Threading.Timeout.Infinite);
        }
    }
}
