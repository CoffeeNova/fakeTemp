using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.TermoServer
{
    internal struct Settings
    {
        private static int _grainbarAutoCreateReportHour = 9;
        private static int _grainbarAutoCreateReportMinute = 0;

        public static int GrainbarAutoCreateReportHour
        {
            get { return _grainbarAutoCreateReportHour; }
            set
            {
                IsHourInRange(value);
                _grainbarAutoCreateReportHour = value;
            }
        }

        public static int GrainbarAutoCreateReportMinute
        {
            get { return _grainbarAutoCreateReportMinute; }
            set
            {
                IsMinuteInRange(value);
                _grainbarAutoCreateReportMinute = value;
            }
        }

        private static void IsHourInRange(int hour)
        {
            if (hour >= 0 && hour <= 24)
                return;

            throw new ArgumentOutOfRangeException(nameof(hour), "Hour value should be in range from 0 to 24");
        }

        private static void IsMinuteInRange(int minute)
        {
            if (minute >= 0 && minute <= 60)
                return;

            throw new ArgumentOutOfRangeException(nameof(minute), "Minute should be in range from 0 to 60");
        }
    }
}
