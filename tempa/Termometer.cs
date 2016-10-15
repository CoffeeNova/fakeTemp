using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoffeeJelly.tempa.Extensions;

namespace CoffeeJelly.tempa
{
    [Serializable]
    public class TermometerAgrolog : Termometer
    {
        public TermometerAgrolog(DateTime measurementDate, string silo, string cable, float?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        public override int SensorsCount { get { return Sensors; } }
        public static readonly int Sensors = 7;
    }

    [Serializable]
    public class TermometerGrainbar : Termometer
    {
        public TermometerGrainbar(DateTime measurementDate, string silo, string cable, float?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        public override int SensorsCount { get { return  Sensors;} }
        public static readonly int Sensors = 6;
    }

    [Serializable]
    public abstract class Termometer : ITermometer
    {
        public Termometer(DateTime measurementDate, string silo, string cable, float?[] sensor)
        {
            if (StringExtension.AnyNullOrEmpty(silo, cable))
                throw new ArgumentException("All strings should have not be empty or null values.");
            if (sensor.Count() != SensorsCount)
                throw new ArgumentOutOfRangeException("sensor", string.Format("Array size must equal {0} value", SensorsCount));
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

        public abstract int SensorsCount { get; }
    }

    public interface ITermometer
    {
        DateTime MeasurementDate { get;  }
        string Silo { get;  }
        string Cable { get; }
        float?[] Sensor { get;  }
        int SensorsCount { get; }
    }
}
