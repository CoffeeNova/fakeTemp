using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using NLog;

namespace tempa
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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            _log.Debug("START DEBUGGING!");
#endif
            _log.Info(string.Format("{0} starting.", Constants.APPLICATION_NAME));
            Process[] pr = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (pr.Length > 1)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                _log.Info(string.Format("Attempt to start another instance of the application. {0} closing.", Constants.APPLICATION_NAME));
            }
            base.OnStartup(e);
            //create the notifyicon (it's a resource declared in smNotify.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
            System.Threading.Timer closeTimer = new System.Threading.Timer((object state) => System.Diagnostics.Process.GetCurrentProcess().Kill(), null, 10000, System.Threading.Timeout.Infinite);
        }
    }
}
