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
using System.Windows.Interactivity;

namespace CoffeeJelly.tempa
{
    /// <summary>
    /// Логика взаимодействия для Plot.xaml
    /// </summary>
    public partial class MainPlotWindow : Window, IView
    {
        public MainPlotWindow(List<Termometer> data)
        {
            InitializeComponent();
            DwmDropShadow.DropShadowToWindow(this);
            this.DataContext = new PlotViewModel();
            (DataContext as PlotViewModel).View = this as IView;
            data.RemoveAll(t => t.MeasurementDate == data.First().MeasurementDate);
            (DataContext as PlotViewModel).TermoData = data;
            GenerateCheckBoxes(data.First().SensorsCount);
        }

        public void Minimize()
        {
            this.WindowState = WindowState.Minimized;
        }
        private void GenerateCheckBoxes(int sensors)
        {
            var style = this.FindResource("CheckBoxMetroStyle") as Style;
            for (int i = 0; i < sensors; i++)
            {

                var checkBox = new CheckBox()
                {
                    Content = $"{ i + 1}",
                    FlowDirection = FlowDirection.RightToLeft,
                    Style = style,
                    Background = new BrushConverter().ConvertFrom("#FF110F0F") as Brush,
                    Foreground = new BrushConverter().ConvertFrom("#FFF1EBEB") as Brush,
                    FontSize = 14,
                    IsChecked = true,
                    ToolTip = $"Датчик №{i+1}"
            };

                var isCheckedBinding = new Binding($"Line{i + 1}Enabled");
                isCheckedBinding.Mode = BindingMode.TwoWay;
                isCheckedBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(checkBox, CheckBox.IsCheckedProperty, isCheckedBinding);

                var isEnabledBinding = new Binding($"Sensor{i}HasValue");
                isEnabledBinding.Mode = BindingMode.OneWay;
                isEnabledBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(checkBox, CheckBox.IsEnabledProperty, isEnabledBinding);

                CheckBoxStackPanel.Children.Add(checkBox);
            }
        }

    }


    public interface IView
    {
        void DragMove();
        void Close();
        void Minimize();
    }
}
