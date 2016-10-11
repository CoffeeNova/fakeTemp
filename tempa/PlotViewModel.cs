using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Series;
using System.ComponentModel;

namespace tempa
{
    class PlotViewModel : INotifyPropertyChanged
    {
        public PlotViewModel()
        {
            
            var tmp = new PlotModel { Title = "Simple example", Subtitle = "using OxyPlot" };

            // Create two line series (markers are hidden by default)
            var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
            series1.Points.Add(new DataPoint(0, 0));
            series1.Points.Add(new DataPoint(10, 18));
            series1.Points.Add(new DataPoint(20, 12));
            series1.Points.Add(new DataPoint(30, 8));
            series1.Points.Add(new DataPoint(40, 15));

            var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
            series2.Points.Add(new DataPoint(0, 4));
            series2.Points.Add(new DataPoint(10, 12));
            series2.Points.Add(new DataPoint(20, 16));
            series2.Points.Add(new DataPoint(30, 25));
            series2.Points.Add(new DataPoint(40, 5));

            // Add the series to the plot model
            tmp.Series.Add(series1);
            tmp.Series.Add(series2);

            // Axes are created automatically if they are not defined

            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content
            this.Model = tmp;

            PropertyChanged += PlotViewModel_PropertyChanged;
        }

        private void NewDate()
        {
            if (TermoData.Count < 2)
                return;
            DisplayDateStart = TermoData[1].MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;

        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void PlotViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is List<Termometer>)
                NewDate();
        }



        private PlotModel _model;
        private DateTime _displayDateStart = DateTime.Now;
        private DateTime _displayDateEnd = DateTime.Now;
        private DateTime _initialDate = DateTime.Now;
        private DateTime _finalDate = DateTime.Now;
        private List<Termometer> _termoData;

        public PlotModel Model
        {
            get { return _model; }
            private set { _model = value; NotifyPropertyChanged(); }
        }

        public DateTime InitialDate
        {
            get { return _initialDate; }
            set { _initialDate = value; NotifyPropertyChanged(); }
        }

        public DateTime FinalDate
        {
            get { return _finalDate; }
            set { _finalDate = value; NotifyPropertyChanged(); }
        }

        public DateTime DisplayDateStart
        {
            get { return _displayDateStart; }
            set { _displayDateStart = value; NotifyPropertyChanged(); }
        }

        public DateTime DisplayDateEnd
        {
            get { return _displayDateEnd; }
            set { _displayDateEnd = value; NotifyPropertyChanged(); }
        }

        public List<Termometer> TermoData
        {
            get { return _termoData; }
            set { _termoData = value; NotifyPropertyChanged(); }
        }
    }
}
