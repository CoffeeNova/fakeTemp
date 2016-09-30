using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Windows;

namespace tempa
{
    internal static class ExceptionHandler
    {
        public static void Handle(Exception ex, bool closeApplication)
        {
            _log.Error(ex);
            Close(closeApplication);
        }

        public static void Handle(ArgumentException ex, bool closeApplication)
        {
            _log.Error(ex);
            Close(closeApplication);
        }

        private static void Close(bool close)
        {
            if (!close)
                return;
            _log.Info(string.Format("{0} is closed with error.", Constants.APPLICATION_NAME));
            Application.Current.Shutdown();
        }

        private static Logger _log = LogManager.GetCurrentClassLogger();
    }
}
