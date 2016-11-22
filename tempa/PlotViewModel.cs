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
using CoffeeJelly.tempa.Extensions;
using System.Reflection;

namespace CoffeeJelly.tempa
{
    class PlotViewModel : INotifyPropertyChanged
    {
        public PlotViewModel()
        {
            PropertyChanged += PlotViewModel_PropertyChanged;
        }

        private void NewData()
        {
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
            Model.LegendTitle = "Легенда";
            Model.Title = Caption;
            Model.LegendOrientation = LegendOrientation.Horizontal;
            Model.LegendPlacement = LegendPlacement.Outside;
            Model.LegendPosition = LegendPosition.TopRight;
            Model.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            Model.LegendBorder = OxyColors.Black;
            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM/yy",
                Title = "Дата",
                TitleFontSize = 16,
                Angle = 45,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Auto,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                AbsoluteMinimum = DisplayDateStart.ToOADate(),
                AbsoluteMaximum = DisplayDateEnd.ToOADate(),
                IntervalLength = 60,
                TitlePosition = 0.1,
                Minimum = InitialDate.ToOADate(),
                Maximum = FinalDate.ToOADate(),
                MinimumRange = MINIMUM_DATE_RANGE_DAYS >= DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate()
                ? DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate() : MINIMUM_DATE_RANGE_DAYS,
            };
            dateAxis.AxisChanged += DateAxis_AxisChanged;
            Model.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Температура",
                TitleFontSize = 16,
                //MinorStep = 1,
                IntervalLength = 30
            };
            List<Termometer> termometers = TermoData.FindAll(t => t.Cable == SelectedCable.Cable);
            double? maxValue = double.MinValue;
            double? minValue = double.MaxValue;
            termometers.ForEach((t) =>
            {//finding min and max sensor values
                maxValue = maxValue < t.Sensor.Max() ? t.Sensor.Max() : maxValue;
                minValue = minValue > t.Sensor.Min() ? t.Sensor.Min() : minValue;
            });
            valueAxis.AbsoluteMaximum = maxValue.Value + VERTICAL_AXE_ADDITIONAL_RANGE;
            valueAxis.AbsoluteMinimum = 0;
            valueAxis.IsZoomEnabled = false;
            Model.Axes.Add(valueAxis);
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
            ActualDate = FinalDate;

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
                if (!(bool)this.GetPropertyValue($"Line{i + 1}Enabled"))
                    continue;

                var lineSerie = new LineSeries
                {
                    StrokeThickness = 2,
                    MarkerSize = 3,
                    MarkerStroke = OxyColor.FromRgb(255, 255, 255),
                    MarkerType = MarkerType.Circle,
                    CanTrackerInterpolatePoints = false,
                    Title = $"Датчик №{i + 1}",
                    FontSize = 14
                };

                List<Termometer> termometers = TermoData.FindAll(t => t.Cable == SelectedCable.Cable);

                bool haveAnyValue = termometers.Any(t => t.Sensor[i].HasValue);
                if (!haveAnyValue)
                {
                    this.SetPropertyValue($"Sensor{i}HasValue", false);
                    continue;
                }

                termometers.ForEach(t =>
                {
                    if (t.Sensor[i].HasValue)
                        lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(t.MeasurementDate),  t.Sensor[i].Value));
                });
                this.SetPropertyValue($"Sensor{i}HasValue", true);

                Model.Series.Add(lineSerie);

            }
        }

        private void InitVisual()
        {
            DisplayDateStart = TermoData.First().MeasurementDate;
            DisplayDateEnd = TermoData.Last().MeasurementDate;

            double minimumDateRange = MINIMUM_DATE_RANGE_DAYS >=
                                   DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate() ?
                                   DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate() :
                                   MINIMUM_DATE_RANGE_DAYS;
            FinalDate = DisplayDateEnd;

            InitialDate = FinalDate.AddDays(-Convert.ToInt32(minimumDateRange)) > DisplayDateStart ?
                          FinalDate.AddDays(-Convert.ToInt32(minimumDateRange)) :
                          DisplayDateStart;
            ActualDate = FinalDate;
            MaxZoomValue = (DisplayDateEnd.ToOADate() - DisplayDateStart.ToOADate()) / (FinalDate.ToOADate() - InitialDate.ToOADate());
            ZoomValue = MaxZoomValue;
        }

        private void InitData()
        {
            if (TermoData.First() is TermometerAgrolog)
                Siloses = TermoData.Unique(t => t.Silo).OrderBy(t => t.Silo, new SemiNumericComparer()).ToList();
            else if (TermoData.First() is TermometerGrainbar)
                Siloses = TermoData.Unique(t => t.Silo).ToList();
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

        private void ZoomChanged()
        {
            var I = InitialDate.ToOADate();
            var F = FinalDate.ToOADate();
            var A = ActualDate.ToOADate();
            var K = ZoomValue;
            var De = DisplayDateEnd.ToOADate();
            var Ds = DisplayDateStart.ToOADate();

            var initDate = A - (De - Ds) / (2 * K);
            var finalDate = A + (De - Ds) / (2 * K);

            if (initDate < Ds)
            {
                finalDate += Ds - initDate;
                initDate = Ds;
            }
            if (finalDate > De)
            {
                initDate -= finalDate - De;
                finalDate = De;
            }

            InitialDate = DateTime.FromOADate(initDate);
            FinalDate = DateTime.FromOADate(finalDate);

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

        private void DisplayDate()
        {
            if (ActualDate >= InitialDate && ActualDate <= FinalDate)
                return;
            var dateRange = FinalDate.ToOADate() - InitialDate.ToOADate();
            var actualDate = ActualDate.ToOADate();

            InitialDate = DateTime.FromOADate(actualDate).AddDays(-dateRange / 2);
            FinalDate = DateTime.FromOADate(actualDate).AddDays(dateRange / 2);
            AxisZoomChange();
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
            else if (e.PropertyName.EqualsAny(nameof(Line1Enabled), nameof(Line2Enabled), nameof(Line3Enabled),
                                              nameof(Line4Enabled), nameof(Line5Enabled), nameof(Line6Enabled), nameof(Line7Enabled)))
                CreateNewLineSeries();


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
        private double _minZoom = 1;
        private bool _line1Enabled = true;
        private bool _line2Enabled = true;
        private bool _line3Enabled = true;
        private bool _line4Enabled = true;
        private bool _line5Enabled = true;
        private bool _line6Enabled = true;
        private bool _line7Enabled = true;
        private bool _sensor0HasValue = true;
        private bool _sensor1HasValue = true;
        private bool _sensor2HasValue = true;
        private bool _sensor3HasValue = true;
        private bool _sensor4HasValue = true;
        private bool _sensor5HasValue = true;
        private bool _sensor6HasValue = true;
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

        public string Caption { get; set; }

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
            set { _zoom = value; NotifyPropertyChanged("ScaleInPercent"); NotifyPropertyChanged(); }
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

        public double ScaleInPercent
        {
            get
            {
                return (ZoomValue - MinZoomValue) / (MaxZoomValue - MinZoomValue) * 100;
            }

        }

        public bool Line1Enabled
        {
            get { return _line1Enabled; }
            set { _line1Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line2Enabled
        {
            get { return _line2Enabled; }
            set { _line2Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line3Enabled
        {
            get { return _line3Enabled; }
            set { _line3Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line4Enabled
        {
            get { return _line4Enabled; }
            set { _line4Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line5Enabled
        {
            get { return _line5Enabled; }
            set { _line5Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line6Enabled
        {
            get { return _line6Enabled; }
            set { _line6Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Line7Enabled
        {
            get { return _line7Enabled; }
            set { _line7Enabled = value; NotifyPropertyChanged(); }
        }

        public bool Sensor0HasValue
        {
            get { return _sensor0HasValue; }
            private set { _sensor0HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor1HasValue
        {
            get { return _sensor1HasValue; }
            private set { _sensor1HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor2HasValue
        {
            get { return _sensor2HasValue; }
            private set { _sensor2HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor3HasValue
        {
            get { return _sensor3HasValue; }
            private set { _sensor3HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor4HasValue
        {
            get { return _sensor4HasValue; }
            private set { _sensor4HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor5HasValue
        {
            get { return _sensor5HasValue; }
            private set { _sensor5HasValue = value; NotifyPropertyChanged(); }
        }

        public bool Sensor6HasValue
        {
            get { return _sensor6HasValue; }
            private set { _sensor6HasValue = value; NotifyPropertyChanged(); }
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

        public ICommand DisplaySelectedDateCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        DisplayDate();
                    }
                };
            }
            set { }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        View.Close();
                    }
                };
            }
            set { }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => true,
                    CommandAction = () =>
                    {
                        View.Minimize();
                    }
                };
            }
            set { }
        }
        #endregion

        private const int MINIMUM_DATE_RANGE_DAYS = 7;
        private const int VERTICAL_AXE_ADDITIONAL_RANGE = 5;
    }
}
