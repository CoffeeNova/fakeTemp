using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tempa.Extensions;

namespace tempa
{
    [Serializable]
    public class TermometerAgrolog : Termometer
    {
        public TermometerAgrolog(DateTime measurementDate, string silo, string cable, float?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        protected override int _sensorCount { get { return Sensors; } }
        public static readonly int Sensors = 7;
    }

    [Serializable]
    public class TermometerGrainbar : Termometer
    {
        public TermometerGrainbar(DateTime measurementDate, string silo, string cable, float?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        protected override int _sensorCount { get { return  Sensors;} }
        public static readonly int Sensors = 6;
    }

    [Serializable]
    public abstract class Termometer : ITermometer
    {
        public Termometer(DateTime measurementDate, string silo, string cable, float?[] sensor)
        {
            if (StringExtension.AnyNullOrEmpty(silo, cable))
                throw new ArgumentException("All strings should have not be empty or null values.");
            if (sensor.Count() != _sensorCount)
                throw new ArgumentOutOfRangeException("sensor", string.Format("Array size must equal {0} value", _sensorCount));
            MeasurementDate = measurementDate;
            Silo = silo;
            Cable = cable;
            Sensor = sensor;
        }

       // public void EditSensorValues;

        public DateTime MeasurementDate { get; private set; }
        public string Silo { get; private set; }
        public string Cable { get; private set; }
        public float?[] Sensor { get; private set; }

        protected abstract int _sensorCount { get; }
    }

    public interface ITermometer
    {
        DateTime MeasurementDate { get;  }
        string Silo { get;  }
        string Cable { get; }
        float?[] Sensor { get;  }
    }
}
