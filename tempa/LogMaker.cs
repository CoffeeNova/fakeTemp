using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Windows.Threading;

namespace CoffeeJelly.tempa
{
    static class LogMaker
    {
        public static void Log(string message, bool isError)
        {
            DateTime currentDate = DateTime.Now;
            _log.Info(message);
            newMessage?.Invoke(message, currentDate, isError);
        }

        public static void InvokedLog(string message, bool isError, Dispatcher dispatcher)
        {
            dispatcher.BeginInvoke(new Action(() =>
                {
                    Log(message, isError);
                }));
        }
        //public static string FormattedMessage
        //{
        //    get;
        //    private set;
        //}

        public delegate void MessageDelegate(string message, DateTime time, bool isError);
        public static event MessageDelegate newMessage;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }

}
