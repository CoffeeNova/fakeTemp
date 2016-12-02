using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NLog;
using System.Threading;
using System.Globalization;
using System.Diagnostics;

namespace CoffeeJelly.TermoServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DefaulCultureInitializing();
            ManualInitializing();
        }

        private void DefaulCultureInitializing()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Ru");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-Ru");

            Thread.CurrentThread.CurrentCulture.GetType()
                .GetProperty("DefaultThreadCurrentCulture")
                .SetValue(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentCulture, null);
            Thread.CurrentThread.CurrentCulture.GetType()
                .GetProperty("DefaultThreadCurrentUICulture")
                .SetValue(Thread.CurrentThread.CurrentUICulture, Thread.CurrentThread.CurrentUICulture, null);
        }

        private void ManualInitializing()
        {
            InitializeGrainbarTimer();
        }

        private void InitializeGrainbarTimer()
        {
            _grainbarTimer = new System.Timers.Timer();
            _grainbarTimer.Interval = _grainBarTimerInterval;
            _grainbarTimer.Elapsed += _grainbarTimer_Elapsed;
            _grainbarTimer.Enabled = true;
            _grainbarTimer.AutoReset = true;
            _grainbarTimer.Start();
        }

        private void _grainbarTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour != Settings.GrainbarAutoCreateReportHour ||
                DateTime.Now.Minute != Settings.GrainbarAutoCreateReportMinute)
                return;

            TrySaveGrainbarReport();

        }

        private void TrySaveGrainbarReport()
        {

        }

        //private Process GrainbarProcess()
        //{
        //    var process = Process.GetProcessesByName()
        //}

        //private bool 
        private System.Timers.Timer _grainbarTimer;
        private static readonly double _grainBarTimerInterval = 1000; //ms
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
    }


}
