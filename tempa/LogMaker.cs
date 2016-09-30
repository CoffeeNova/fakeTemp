﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace tempa
{
    static class LogMaker
    {
        public static void Log(string message, bool isError)
        {
            DateTime currentDate = DateTime.Now;
            FormattedMessage = string.Format("{0}   {1}", currentDate.ToShortTimeString(), message);
            _log.Info(message);
            newMessage(FormattedMessage, isError);
        }

        public static string FormattedMessage
        {
            get;
            private set;
        }

        public delegate void MessageDelegate(string message, bool isError);
        public static event MessageDelegate newMessage;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }

}
