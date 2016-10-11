using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.Windows.Input;

namespace tempa
{
    class PlotViewModel : INotifyPropertyChanged
    {
        public PlotViewModel()
        {

            Model = new PlotModel();

            //// Create two line series (markers are hidden by default)
            //var series1 = new LineSeries { Title = "Series 1", MarkerType = MarkerType.Circle };
            //series1.Points.Add(new DataPoint(0, 0));
            //series1.Points.Add(new DataPoint(10, 18));
            //series1.Points.Add(new DataPoint(20, 12));
            //series1.Points.Add(new DataPoint(30, 8));
            //series1.Points.Add(new DataPoint(40, 15));

            //var series2 = new LineSeries { Title = "Series 2", MarkerType = MarkerType.Square };
            //series2.Points.Add(new DataPoint(0, 4));
            //series2.Points.Add(new DataPoint(10, 12));
            //series2.Points.Add(new DataPoint(20, 16));
            //series2.Points.Add(new DataPoint(30, 25));
            //series2.Points.Add(new DataPoint(40, 5));

            //// Add the series to the plot model
            //tmp.Series.Add(series1);
            //tmp.Series.Add(series2);

            // Axes are created automatically if they are not defined

            // Set the Model property, the INotifyPropertyChanged event will make the WPF Plot control update its content

            PropertyChanged += PlotViewModel_PropertyChanged;
        }

        private void NewDate()
        {
            if (TermoData.Count < 2)
                return;

            DisplayDateStart = TermoData[1].MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;
            SetUpModel();
        }
        private void SetUpModel()
        {
            Model.LegendTitle = "Legend";
            Model.LegendOrientation = LegendOrientation.Horizontal;
            Model.LegendPlacement = LegendPlacement.Outside;
            Model.LegendPosition = LegendPosition.TopRight;
            Model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            Model.LegendBorder = OxyColors.Black;
            //(AxisPosition.Bottom, "Date", "dd/MM/yy HH:mm") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80 };
            var dateAxis = new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yy HH:mm",
                Title = "Дата",
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };
            Model.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Температура",
                AbsoluteMaximum = 0.0,
            };
            
            Model.Axes.Add(valueAxis);
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void PlotViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TermoData))
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

        public IView View { get; set; }

        public ICommand DragMoveWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        View.DragMove();
                    }
                };
            }
            set { }
        }

    }
}
