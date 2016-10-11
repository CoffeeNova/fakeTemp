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
using System.ComponentModel;

namespace tempa
{
    /// <summary>
    /// Логика взаимодействия для Plot.xaml
    /// </summary>
    public partial class MainPlotWindow : Window, IView
    {
        public MainPlotWindow(List<Termometer> data)
        {
            InitializeComponent();
            this.DataContext = new PlotViewModel();
            (DataContext as PlotViewModel).TermoData = data;
            (DataContext as PlotViewModel).View = this as IView;
        }

        private void PlotWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

    }
    public interface IView
    {
        void DragMove();
    }
}
