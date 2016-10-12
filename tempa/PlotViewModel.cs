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
using System.Threading.Tasks;
using tempa.Extensions;

namespace tempa
{
    class PlotViewModel : INotifyPropertyChanged
    {
        public PlotViewModel()
        {
            PropertyChanged += PlotViewModel_PropertyChanged;
        }

        private void NewDate()
        {
            if (TermoData.Count < 2)
                return;

            DisplayDateStart = TermoData[1].MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;
            //SetUpModel();
            InitData();
        }
        private void SetUpAxes()
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
                //AbsoluteMaximum = 0.0,
                
            };

            Model.Axes.Add(valueAxis);
        }

        private Task SetUpSeriesAsync()
        {
            return Task.Factory.StartNew(() => SetUpSeries());
        }

        private void SetUpSeries()
        {
            for (int i = 0; i < SelectedCable.Sensor.Count(); i++)
            {
                var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = OxyColor.FromRgb(255, 255, 255),
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = $"Датчик №{i + 1}",
                    Smooth = false,
                };

                List<Termometer> termometers = TermoData.FindAll(t => t.Cable == SelectedCable.Cable);
                termometers.ForEach(t => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(t.MeasurementDate), t.Sensor[i].Value)));
                Model.Series.Add(lineSerie);
            }
        }

        private void InitData()
        {
            Siloses = TermoData.Unique(t => t.Silo).OrderBy(t => t.Silo, new SemiNumericComparer()).ToList();
        }

        private void NewSiloses()
        {
            SelectedSilo = Siloses.First();
        }

        private void NewCables()
        {
            SelectedCable = Cables.First();
        }

        private void SelectedSiloChanged()
        {
            Cables = TermoData.FindAll(t =>t.MeasurementDate == SelectedSilo.MeasurementDate && 
                                           t.Silo == SelectedSilo.Silo);
        }

        private void SelectedCableChanged()
        {
            CreateNewLineSeries();
        }

        private async void CreateNewLineSeries()
        {
            Model?.InvalidatePlot(true);
            Model = new PlotModel();
            SetUpAxes();
            await SetUpSeriesAsync();
            //Model.Series?.Clear();
            
        }
        private void DistributeData()
        {

        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void PlotViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TermoData))
                NewDate();
            else if (e.PropertyName == nameof(Siloses))
                NewSiloses();
            else if (e.PropertyName == nameof(SelectedSilo))
                SelectedSiloChanged();
            else if (e.PropertyName == nameof(Cables))
                NewCables();
            else if (e.PropertyName == nameof(SelectedCable))
                SelectedCableChanged();
        }



        private PlotModel _model;
        private DateTime _displayDateStart = DateTime.Now;
        private DateTime _displayDateEnd = DateTime.Now;
        private DateTime _initialDate = DateTime.Now;
        private DateTime _finalDate = DateTime.Now;
        private List<Termometer> _termoData;
        private List<Termometer> _siloses;
        private List<Termometer> _cables;
        private Termometer _selectedSilo;
        private Termometer _selectedCable;

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

        public List<Termometer> Siloses
        {
            get { return _siloses; }
            set { _siloses = value; NotifyPropertyChanged(); }
        }

        public List<Termometer> Cables
        {
            get { return _cables; }
            set { _cables = value; NotifyPropertyChanged(); }
        }

        public Termometer SelectedSilo
        {
            get { return _selectedSilo; }
            set { _selectedSilo = value; NotifyPropertyChanged(); }
        }

        public Termometer SelectedCable
        {
            get { return _selectedCable; }
            set { _selectedCable = value; NotifyPropertyChanged(); }
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
