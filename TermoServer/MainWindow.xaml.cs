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
using CoffeeJelly.tempadll;

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
        }


        //private bool 
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private void Window_Initialized(object sender, EventArgs e)
        {
            GrainbarReportCreator.TestReportCreate();

        }
    }


}
