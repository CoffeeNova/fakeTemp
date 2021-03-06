﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CoffeeJelly.tempa.Exceptions;
using System.Globalization;
using System.Threading;
using CoffeeJelly.tempadll;

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

            var ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            Thread.CurrentThread.CurrentCulture = ci;

            this.DataContext = new PlotViewModel();
            (DataContext as PlotViewModel).View = this as IView;

            var patternDate = data.First().MeasurementDate;
            data.RemoveAll(t => t.MeasurementDate == patternDate); 

            if (data.Count < 2)
                throw new PlotDataException("Have no data for plotting.");
            (DataContext as PlotViewModel).TermoData = data;
            (DataContext as PlotViewModel).Caption = data.First().MeasurementDate.ToString("dd.MM.yy") 
                + " - " + data.Last().MeasurementDate.ToString("dd.MM.yy");
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
