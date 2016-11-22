using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CoffeeJelly.tempa.Extensions;
using System.Globalization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CoffeeJelly.tempa
{
    public static partial class DataWorker
    {
        public static Task<List<T>> ReadPatternReportAsync<T>(string text) where T : ITermometer
        {
            return Task.Factory.StartNew(() => ReadPatternReport<T>(text));
        }

        public static List<T> ReadPatternReport<T>(string text) where T : ITermometer
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("text should have not be empty or null value.");

            List<String> lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            if (typeof(T) == typeof(TermometerAgrolog))
                return ParseAgrologReport(lines) as List<T>;
            return ParseGrainBarReport(lines) as List<T>;
        }

        public static Task<List<T>> ReadReportAsync<T>(string path, string fileName) where T : ITermometer
        {
            return Task.Factory.StartNew(() => ReadReport<T>(path, fileName));
        }

        public static List<T> ReadReport<T>(string path, string fileName) where T : ITermometer
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path should have not be empty or null value.");

            var lines = new List<string>();
            using (TextReader reader = new StreamReader(path.PathFormatter() + fileName, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            if (typeof(T) == typeof(TermometerAgrolog))
                return ParseAgrologReport(lines) as List<T>;
            return ParseGrainBarReport(lines) as List<T>;
        }

        public static Task<List<string>> GetReportsFileNamesAsync(ProgramType report, string path)
        {
            return Task.Factory.StartNew(() => GetReportsFileNames(report, path));
        }

        public static List<string> GetReportsFileNames(ProgramType report, string path)
        {
            var searchPattern = report == ProgramType.Agrolog ? "*.csv" : "*.txt";
            return Directory.GetFiles(path.PathFormatter(), searchPattern, SearchOption.TopDirectoryOnly).ToList();
        }

        private static ProgramType ReportFileNameChecker(string fileName)
        {
            string message = "";
            ProgramType rType = ProgramType.Agrolog;
            if (string.IsNullOrEmpty(fileName))
                message = "fileName should have not be empty or null value.";
            else if (!fileName.Contains('.'))
                message = "fileName should have an extension";
            else
            {
                string extension = fileName.Split('.').Last();
                if (extension.Equals("csv"))
                    rType = ProgramType.Agrolog;
                else if (extension.Equals("txt"))
                    rType = ProgramType.Grainbar;
                else
                    message = "Wrong fileName extension. Should be \".csv\" or \".txt\" ";
            }

            if (!string.IsNullOrEmpty(message))
                throw new ArgumentException(message);
            return rType;
        }

        private static void DatFileNameChecker(string fileName)
        {
            string message = "";
            if (string.IsNullOrEmpty(fileName))
                message = "fileName should have not be empty or null value.";
            else if (!fileName.Contains('.'))
                message = "fileName should have an extension";
            else
            {
                string extension = fileName.Split('.').Last();
                if (!extension.Equals("dat"))
                    message = "Wrong fileName extension. Should be \".dat\"";
            }

            if (!string.IsNullOrEmpty(message))
                throw new ArgumentException(message);
        }
        #region Agrolog methods
        private static List<TermometerAgrolog> ParseAgrologReport(List<string> lines)
        {
            DateTime date = ParseAgrologDate(lines);
            ParseAgrologTempData(ref lines);
            List<List<AgrologSensor>> dataList = ParseAgrologReportByTermometers(lines);

            return dataList.Select(termometerData =>
                {
                    string siloName = termometerData.First().SiloName;
                    string termometerName = termometerData.First().TermometerName;
                    termometerName = Convert.ToInt32(termometerName).ToString(); //Converting to int and back to remove strings like 001

                    var sensor = new double?[TermometerAgrolog.Sensors];
                    for (int i = 0; i < sensor.Count(); i++)
                    {
                        AgrologSensor requiredSensor = termometerData.FirstOrDefault(x => x.SensorName == (i + 1).ToString());
                        string sensorValue = requiredSensor != null ? requiredSensor.SensorValue : "";
                        if (string.IsNullOrEmpty(sensorValue))
                            sensor[i] = null;
                        else
                            sensor[i] = double.Parse(sensorValue, CultureInfo.InvariantCulture.NumberFormat);
                    }

                    return new TermometerAgrolog(date, siloName, termometerName, sensor);
                }).ToList();
        }

        private static DateTime ParseAgrologDate(List<string> lines)
        {
            try
            {
                char[] ch = new char[1] { ':' };
                string dateString = lines.Find(p => p.StartsWith("Date and time")).Split(ch, 2).Last();
                DateTime date;
                bool dateParseResult = DateTime.TryParse(dateString, out date);
                if (!dateParseResult)
                    throw new Exception();
                return date;
            }
            catch
            {
                throw new InvalidOperationException("Can't parse Date from report.");
            }

        }

        private static void ParseAgrologTempData(ref List<string> lines)
        {
            string initialMarker = "Silo,Cable,Sensor,Value";
            try
            {
                string markedLine = lines.Find(l => l.StartsWith(initialMarker));
                var markerIndex = lines.IndexOf(markedLine);
                lines = lines.Skip(markerIndex + 1).ToList();
                lines = lines.TakeWhile(s => !string.IsNullOrEmpty(s)).ToList();
            }
            catch
            {
                throw new InvalidOperationException("Can't parse temperature data.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of arrays which consists of 4 parts as strings (silo name, termometr number, sensor number, temperature value.</returns>
        private static List<List<AgrologSensor>> ParseAgrologReportByTermometers(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
                throw new InvalidOperationException("Report has no any temperature data.");

            var agrologTermometers = new List<List<AgrologSensor>>();
            try
            {
                var sensorsList = lines.ConvertAll(s =>
                {
                    string[] splitedLine = s.Split(',');
                    return new AgrologSensor(splitedLine[0], splitedLine[1], splitedLine[2], splitedLine[3]);
                });

                while (true)
                {
                    AgrologSensor sensor = sensorsList.FirstOrDefault();
                    List<AgrologSensor> dividedByTermometersList = sensorsList.FindAll(s => s.TermometerName == sensor.TermometerName);
                    sensorsList.RemoveAll(p => p.TermometerName == sensor.TermometerName);
                    agrologTermometers.Add(dividedByTermometersList);
                    if (sensorsList.Count == 0)
                        break;
                }
            }
            catch
            {
                throw new InvalidOperationException("Can't parse temperature data. Bad report file.");
            }
            return agrologTermometers;
        }
        #endregion

        #region GrainBar methods
        private static List<TermometerGrainbar> ParseGrainBarReport(List<string> lines)
        {
            DateTime date = ParseGrainbarDate(lines);
            ParseGrainbarTempData(ref lines);

            List<GrainbarSensor> dataList = ParseGrainbarData(lines);
            var listOfClassInstances = new List<TermometerGrainbar>();

             return dataList.Select(termometerData =>
            {
                string siloName = termometerData.SiloName;
                string termometerName = termometerData.TermometerName;
                var sensor = new double?[TermometerGrainbar.Sensors];

                for (int i = 0; i < sensor.Count(); i++)
                {
                    var sensorValue = termometerData.SensorsValues[i].RemoveGrainBarErrorValue();
                    if (string.IsNullOrEmpty(sensorValue))
                        sensor[i] = null;
                    else
                        sensor[i] = double.Parse(sensorValue, CultureInfo.InvariantCulture.NumberFormat);
                }

                return new TermometerGrainbar(date, siloName, termometerName, sensor);
            }).ToList();
        }

        private static DateTime ParseGrainbarDate(List<string> lines)
        {
            try
            {
                string lineWithDate = lines.Find(p => p.StartsWith("Измерение от"));
                string dateString = lineWithDate.Split(' ')[2];
                string timeString = lineWithDate.Split(' ')[3];
                string dateAsString = dateString.FormatToGrainBarDate() + " " + timeString.FormatToGrainBarDate();

                return GetGrainbarDateFromString(dateAsString);
            }
            catch
            {
                throw new InvalidOperationException("Can't parse Date from report.");
            }

        }

        private static DateTime GetGrainbarDateFromString(string dateString)
        {
            string date = dateString.Split(',')[0];
            string time = dateString.Split(',')[1];
            int year = Int32.Parse("20" + date.Split('.')[2]);
            int month = Int32.Parse(date.Split('.')[1]);
            int day = Int32.Parse(date.Split('.')[0]);
            int hour = Int32.Parse(time.Split(':')[0]);
            int minute = Int32.Parse(time.Split(':')[1]);
            int second = Int32.Parse(time.Split(':')[2]);

            return new DateTime(year, month,day, hour, minute, second);
        }

        private static void ParseGrainbarTempData(ref List<string> lines)
        {
            string initialMarker = "-------------------------------------";
            try
            {
                string markedLine = lines.Find(l => l.StartsWith(initialMarker));
                var markerIndex = lines.IndexOf(markedLine);
                lines = lines.Skip(markerIndex + 1).ToList();
                lines = lines.TakeWhile(s => !s.StartsWith("Конец")).ToList();
            }
            catch
            {
                throw new InvalidOperationException("Can't parse temperature data.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of arrays which consists of 3 parts as strings (silo name, termometr number, temperature values as 6s elements array.</returns>
        private static List<GrainbarSensor> ParseGrainbarData(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
                throw new InvalidOperationException("Report has no any temperature data.");

            List<GrainbarSensor> grainbarTermometers;
            try
            {
                grainbarTermometers = lines.ConvertAll(s =>
                {
                    string[] splitedLine = s.Split('|');
                    return new GrainbarSensor(splitedLine[2].RemoveWhiteSpaces(), splitedLine[1], splitedLine.Last().GrainbarParseValues());
                });
            }
            catch
            {
                throw new InvalidOperationException("Can't parse temperature data. Bad report file.");
            }
            return grainbarTermometers;
        }


        public static string[] GrainbarParseValues(this string str)
        {
            return Regex.Matches(str, @"-?(\d+\.\d+)|(E\d+)").OfType<Match‌>().Select(m => m.Value).ToArray();
        }
        #endregion

    }

    public enum ProgramType
    {
        Agrolog,
        Grainbar
    }

    internal sealed class AgrologSensor
    {
        public AgrologSensor(string siloName, string termometerName, string sensorName, string sensorValue)
        {
            SiloName = siloName;
            TermometerName = termometerName;
            SensorName = sensorName;
            SensorValue = sensorValue;
        }
        public string SiloName { get; set; }
        public string TermometerName { get; set; }
        public string SensorName { get; set; }
        public string SensorValue { get; set; }
    }

    internal sealed class GrainbarSensor
    {
        public GrainbarSensor(string siloName, string termometerName, string[] sensorsValues)
        {
            SiloName = siloName;
            TermometerName = termometerName;
            SensorsValues = sensorsValues;
        }
        public string SiloName { get; set; }
        public string TermometerName { get; set; }
        public string[] SensorsValues { get; set; }
    }
}
