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

        private void NewData()
        {
            if (TermoData.Count < 2)
                return;

            Model = new PlotModel();
            InitDatePickers();
            InitData();
        }

        private void SetUpAxes()
        {
            Model.Axes.Clear();
            Model.LegendTitle = "Legend";
            Model.LegendOrientation = LegendOrientation.Horizontal;
            Model.LegendPlacement = LegendPlacement.Outside;
            Model.LegendPosition = LegendPosition.TopRight;
            Model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            Model.LegendBorder = OxyColors.Black;

            var dateAxis = new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yy",
                Title = "Дата",
                Angle = 0,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                //IntervalLength = 1,
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
            int? sensorsCount = SelectedCable?.Sensor?.Count();
            if (!sensorsCount.HasValue)
                return;
            Model.Series.Clear();
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
                    
                };

                List<Termometer> termometers = TermoData.FindAll(t => t.Cable == SelectedCable.Cable);
                termometers.ForEach(t => { if (t.Sensor[i].HasValue) lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(t.MeasurementDate), t.Sensor[i].Value)); });
                //TermometrSeries.Add(lineSerie);
                
                Model.Series.Add(lineSerie);
                
            }
        }

        private void InitDatePickers()
        {
            DisplayDateStart = TermoData[1].MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;
            FinalDate = DisplayDateEnd;
            InitialDate = FinalDate.AddDays(-INITITAL_DATE_RANGE_DAYS) > DisplayDateStart ? FinalDate.AddDays(-INITITAL_DATE_RANGE_DAYS) : DisplayDateStart;
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

        private void FinalDateChanged()
        {
            CreateNewLineSeries();
        }

        private void InitialDateChanged()
        {
            CreateNewLineSeries();
        }

        private async void CreateNewLineSeries()
        {
            SetUpAxes();
            await SetUpSeriesAsync();
            Model?.InvalidatePlot(true);
        }

        private void CreateNewAxes()
        {
            SetUpAxes();
            Model?.InvalidatePlot(true);
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
                NewData();
            else if (e.PropertyName == nameof(Siloses))
                NewSiloses();
            else if (e.PropertyName == nameof(SelectedSilo))
                SelectedSiloChanged();
            else if (e.PropertyName == nameof(Cables))
                NewCables();
            else if (e.PropertyName == nameof(SelectedCable))
                SelectedCableChanged();
            else if (e.PropertyName == nameof(InitialDate))
                InitialDateChanged();
            else if (e.PropertyName == nameof(FinalDate))
                FinalDateChanged();
        }

        #region private fields

        private PlotModel _model;
        private DateTime _displayDateStart = DateTime.Now;
        private DateTime _displayDateEnd = DateTime.Now;
        private DateTime _initialDate = DateTime.Now;
        private DateTime _finalDate = DateTime.Now;
        private List<Termometer> _termoData;
        private List<Termometer> _siloses;
        private List<Termometer> _cables;
        //private List<LineSeries> _termometrSeries = new List<LineSeries>();
        private Termometer _selectedSilo;
        private Termometer _selectedCable;
        //private int _invalidateFlag;

        #endregion

        #region public properties
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

        #endregion

        private const int INITITAL_DATE_RANGE_DAYS = 7;
    }
}
