using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using NLog;
using System.Threading;
using System.ComponentModel;

namespace tempa
{
    /// <summary>
    /// Логика взаимодействия для Plot.xaml
    /// </summary>
    public partial class Plot : Window
    {
        public Plot(List<Termometer> data)
        {
            InitializeComponent();
            TermoData = data;
            FirstDate = TermoData.First().MeasurementDate;
            LastDate = TermoData.Last().MeasurementDate;
        }

        public DateTime FirstDate { get; private set; }

        public DateTime LastDate { get; private set; }

        public List<Termometer> TermoData { get; set; }
    }
}
