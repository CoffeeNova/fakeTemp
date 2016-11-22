using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoffeeJelly.tempa.Extensions;
using CoffeeJelly.tempa.Exceptions;
using System.Reflection;

namespace CoffeeJelly.tempa
{
    [Serializable]
    public class TermometerAgrolog : Termometer
    {
        public TermometerAgrolog(DateTime measurementDate, string silo, string cable, double?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        public override int SensorsCount => Sensors;
        public static readonly int Sensors = 7;
    }

    [Serializable]
    public class TermometerGrainbar : Termometer
    {
        public TermometerGrainbar(DateTime measurementDate, string silo, string cable, double?[] sensor) : base(measurementDate, silo, cable, sensor) { }

        public override int SensorsCount => Sensors;
        public static readonly int Sensors = 6;
    }

    [Serializable]
    public abstract class Termometer : ITermometer
    {
        protected Termometer(DateTime measurementDate, string silo, string cable, double?[] sensor)
        {
            if (StringExtension.AnyNullOrEmpty(silo, cable))
                throw new ArgumentException("All strings should have not be empty or null values.");
            if (sensor.Length != SensorsCount)
                throw new ArgumentOutOfRangeException(nameof(sensor), $"Array size must equal {SensorsCount} value");
            MeasurementDate = measurementDate;
            Silo = silo;
            Cable = cable;
            Sensor = sensor;
        }

        /// <summary>
        /// Создает новый экземпляр класса типа <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Тип создаваемого клаcса (должен наследовать интерфейс <see cref="ITermometer"/>).</typeparam>
        /// <returns>Возвращает экземляр созданного класса типа <typeparamref name="T"/>.</returns>
        /// <exception cref="TermometerBuildException">Исключение вызванное ошибкой создания экземпляра класса. Подробности во внутреннем исключении.</exception>
        public static T Create<T>(DateTime measurementDate, string silo, string cable, double?[] sensor) where T : ITermometer
        {
            System.Threading.Monitor.Enter(_locker);
            try
            {
                var newMessenger = (T)Activator.CreateInstance(typeof(T), measurementDate, silo, cable, sensor);
                return newMessenger;
            }
            catch (TargetInvocationException ex)
            {
                throw new TermometerBuildException("Cannot build a termometr", ex);
            }
            finally
            {
                System.Threading.Monitor.Exit(_locker);
            }
        }
        public DateTime MeasurementDate { get; }
        public string Silo { get; }
        public string Cable { get; }
        public double?[] Sensor { get; }
        public abstract int SensorsCount { get; }

        private static readonly object _locker = new object();
    }

    public interface ITermometer
    {
        DateTime MeasurementDate { get;  }
        string Silo { get;  }
        string Cable { get; }
        double?[] Sensor { get;  }
        int SensorsCount { get; }
    }
}
