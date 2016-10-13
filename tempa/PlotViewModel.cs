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
            InitVisual();
            InitData();
        }

        private Task SetUpAxesAsync()
        {
            return Task.Factory.StartNew(() => SetUpAxes());
        }

        private void SetUpAxes()
        {
            int? sensorsCount = SelectedCable?.Sensor?.Count();
            if (!sensorsCount.HasValue)
                return;
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
                IntervalType = DateTimeIntervalType.Auto,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AbsoluteMinimum = DisplayDateStart.ToOADate(),
                AbsoluteMaximum = DisplayDateEnd.ToOADate(),
                IntervalLength = 120,
                TitlePosition = 0.1,
                Minimum = InitialDate.ToOADate(),
                Maximum = FinalDate.ToOADate()
            };
            dateAxis.AxisChanged += DateAxis_AxisChanged;
            //dateAxis.MouseUp += DateAxis_MouseUp;
            //dateAxis.KeyDown += DateAxis_KeyDown;
            //dateAxis.Zoom(InitialDate.ToOADate(), FinalDate.ToOADate());
            //dateAxis.IsZoomEnabled = false;
            Model.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Температура",
                //MinorStep = 1,
                IntervalLength = 30
            };
            List<Termometer> termometers = TermoData.FindAll(t => t.Cable == SelectedCable.Cable);
            float? maxValue = float.MinValue;
            float? minValue = float.MaxValue;
            termometers.ForEach((t) =>
            {//finding min and max sensor values
                maxValue = maxValue < t.Sensor.Max() ? t.Sensor.Max() : maxValue;
                minValue = minValue > t.Sensor.Min() ? t.Sensor.Min() : minValue;
            });
            valueAxis.AbsoluteMaximum = maxValue.Value + VERTICAL_AXE_ADDITIONAL_RANGE;
            valueAxis.AbsoluteMinimum = 0;
            Model.Axes.Add(valueAxis);
        }

        private void DateAxis_KeyDown(object sender, OxyKeyEventArgs e)
        {
            //if (e.Key == OxyKey.Left) || e.Key == OxyKey.Right)
            //{
            //    var axis = sender as DateTimeAxis;
            //    axis.ZoomAt()
            //    //ChangeDateRange(axis);
            //}
        }

        private void DateAxis_MouseUp(object sender, OxyMouseEventArgs e)
        {
            var axis = sender as DateTimeAxis;
            ChangeDateRange(axis);
        }

        private void DateAxis_AxisChanged(object sender, AxisChangedEventArgs e)
        {
            var axis = sender as DateTimeAxis;
            ChangeDateRange(axis);
        }

        private void ChangeDateRange(DateTimeAxis axis)
        {
            InitialDate = DateTime.FromOADate(axis.ActualMinimum);
            FinalDate = DateTime.FromOADate(axis.ActualMaximum);
            ZoomValue = (DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate()) / (axis.ActualMaximum - axis.ActualMinimum);
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

        private void InitVisual()
        {
            DisplayDateStart = TermoData[1].MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;
            FinalDate = DisplayDateEnd;
            InitialDate = FinalDate.AddDays(-INITITAL_DATE_RANGE_DAYS) > DisplayDateStart ? FinalDate.AddDays(-INITITAL_DATE_RANGE_DAYS) : DisplayDateStart;
            ActualDate = FinalDate;
            MaxZoomValue = (DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate()) / (FinalDate.ToOADate() - InitialDate.ToOADate());
            ZoomValue = MaxZoomValue;
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
            Cables = TermoData.FindAll(t => t.MeasurementDate == SelectedSilo.MeasurementDate &&
                                           t.Silo == SelectedSilo.Silo);
        }

        private void SelectedCableChanged()
        {
            CreateNewLineSeries();
        }

        private void FinalDateChanged()
        {
            //AxisZoomChange();
        }

        private void InitialDateChanged()
        {
            //AxisZoomChange();
        }

        private void ActualDateChanged()
        {
            //AxisZoomChange();
        }

        private void ZoomChanged()
        {
            var delta = DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate();

            //var initDate = (DisplayDateEnd.ToOADate() - delta / ZoomValue);
            //var finalDate = (delta / ZoomValue + initDate);
            //InitialDate = DateTime.FromOADate(initDate);
            //FinalDate = DateTime.FromOADate(finalDate);


            AxisZoomChange();
        }

        private void AxisZoomChange()
        {
            if (Model.Axes.Count == 0) return;
            Model.Axes[0].AxisChanged -= DateAxis_AxisChanged;
            Model.Axes[0].Reset();
            Model.Axes[0].Minimum = InitialDate.ToOADate();
            Model.Axes[0].Maximum = FinalDate.ToOADate();
            Model.Axes[0].AxisChanged += DateAxis_AxisChanged;
            Model?.InvalidatePlot(true);
        }

        private async void CreateNewLineSeries()
        {
            await SetUpAxesAsync();
            await SetUpSeriesAsync();
            Model?.InvalidatePlot(true);
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
            else if (e.PropertyName == nameof(ActualDate))
                ActualDateChanged();
            //else if (e.PropertyName == nameof(Zoom))
                //ZoomChanged();
        }

        #region private fields

        private PlotModel _model;
        private DateTime _displayDateStart = DateTime.MinValue;
        private DateTime _displayDateEnd = DateTime.Now;
        private DateTime _initialDate = DateTime.MinValue;
        private DateTime _actualDate;
        private DateTime _finalDate = DateTime.Now;
        private List<Termometer> _termoData;
        private List<Termometer> _siloses;
        private List<Termometer> _cables;
        //private List<LineSeries> _termometrSeries = new List<LineSeries>();
        private Termometer _selectedSilo;
        private Termometer _selectedCable;
        private readonly double _oneDayinOADate = DateTime.Now.AddDays(1).ToOADate() - DateTime.Now.ToOADate();
        private double _zoom = 1;
        private double _maxZoom;
        private double _minZoom =1;

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

        public DateTime ActualDate
        {
            get { return _actualDate; }
            set { _actualDate = value; NotifyPropertyChanged(); }
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

        public double ZoomValue
        {
            get { return _zoom; }
            set { _zoom = value;  NotifyPropertyChanged(); }
        }

        public double MaxZoomValue
        {
            get { return _maxZoom; }
            private set { _maxZoom = value; NotifyPropertyChanged(); }
        }

        public double MinZoomValue
        {
            get { return _minZoom; }
            private set { _minZoom = value; NotifyPropertyChanged(); }
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

        public ICommand UpdateZoomCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        ZoomChanged();
                    }
                };
            }
            set { }
        }
        #endregion

        private const int INITITAL_DATE_RANGE_DAYS = 7;
        private const int VERTICAL_AXE_ADDITIONAL_RANGE = 5;
    }
}
