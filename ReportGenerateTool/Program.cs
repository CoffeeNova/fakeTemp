using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Threading;

using CoffeeJelly.tempa;
using CoffeeJelly.tempa.Extensions;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CoffeeJelly.ReportGenerateTool
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            Settings();
            SetCulturePattern();
            WriteGreetings();
            var reportType = ReportTypeQuestion();
            ushort reporsCount = ReportsCountQuestion();
            bool anyDateRangePrefer = DateRangePreferQuestion();

            DateTime? startDate = null;
            DateTime? endDate = null;
            TimeRange? timeRange = null;
            if (anyDateRangePrefer)
            {
                startDate = StartDateQuestion();
                endDate = EndDateQuestion();
            }
            else
                timeRange = TimeRangeQuestion();
            string outputPath = ReportsOutputPathQuestion(reportType);

            if (BeginGenerateQuestion())
                LetsWork(reportType, reporsCount, startDate, endDate, timeRange, outputPath);
            Console.ReadLine();

        }

        private static async void LetsWork(ProgramType reportType, ushort reportsCount, DateTime? startDate, DateTime? endDate, TimeRange? timeRange, string outputPath)
        {
            string outputFileExtension = reportType == ProgramType.Agrolog ? AGROLOG_FAKE_REPORT_EXTENSION_NAME : GRAINBAR_FAKE_REPORT_EXTENSION_NAME;
            string datePattern = reportType == ProgramType.Agrolog ? AGROLOG_DATE_PATTERN : GRAINBAR_DATE_PATTERN;
            string content = RetrieveContentFromResource(reportType);

            List<int> reportNumbers = Enumerable.Range(1, reportsCount).ToList();
            var dividedReportsNumbers = reportNumbers.DivideByChunks(THREADS_COUNT);

            var checkList = new List<bool>();

            foreach (List<int> part in dividedReportsNumbers)
                checkList.Add(await GenerateReportsAsync(part, startDate, endDate, timeRange, content, outputPath, datePattern, outputFileExtension));

        }

        private static DateTime DateFunc1(int days, TimeRange? timeRange)
        {
            var currentDate = DateTime.Now;
            var d = currentDate;

            switch (timeRange)
            {
                case TimeRange.Week:
                    return d.AddDays(-days * 7);
                case TimeRange.Day:
                    return d.AddDays(-days);case TimeRange.Hour:
                    return d.AddHours(-days);
                default:
                    return d;
            }
        }

        private static DateTime DateFunc2(int days, DateTime? startDate, DateTime? endDate)
        {
            var random = new Random();
            Debug.Assert(endDate != null, "endDate != null");
            Debug.Assert(startDate != null, "startDate != null");

            double randomOADate = random.NextDouble() * (endDate.Value.ToOADate() - startDate.Value.ToOADate()) + startDate.Value.ToOADate();
            return DateTime.FromOADate(randomOADate);
        }

        private static Task<bool> GenerateReportsAsync(List<int> reportsCount, DateTime? startDate, DateTime? endDate,
                                            TimeRange? timeRange, string defaultContent, string outputPath, string datePattern, string outputFileExtension)
        {
            return Task.Factory.StartNew(() => GenerateReports(reportsCount, startDate, endDate, timeRange,
                                                                defaultContent, outputPath, datePattern, outputFileExtension));
        }

        private static bool GenerateReports(List<int> reportsCount, DateTime? startDate, DateTime? endDate,
                                            TimeRange? timeRange, string defaultContent, string outputPath, string datePattern, string outputFileExtension)
        {
            for (int i = reportsCount.First(); i <= reportsCount.Last(); i++)
            {
                var outputFullPath = outputPath.PathFormatter();

                DateTime reportDate = !startDate.HasValue ? DateFunc1(i, timeRange) : DateFunc2(i, startDate, endDate);
                string newContent = defaultContent.ReplaceFirst(REPLACE_DATE_PATTERN, reportDate.ToString(datePattern));
                outputFullPath += $"{reportDate:dd-MM-yyyy}_report.{outputFileExtension}";
                var fInfo = new FileInfo(outputFullPath);

                if (fInfo.Exists)
                {
                    i--;
                    continue;
                }

                using (Stream output = File.Create(outputFullPath))
                {
                    using (var outputWriter = new StreamWriter(output))
                    {
                        outputWriter.Write(newContent);
                    }
                }
            }
            return false;
        }

        private static string RetrieveContentFromResource(ProgramType reportType)
        {
            string resourceName = reportType == ProgramType.Agrolog ? AGROLOG_EXAMPLE_RESOURCE_NAME : GRAINBAR_EXAMPLE_RESOURCE_NAME;
            string content = string.Empty;

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resourceStream != null)
                    using (var resourceReader = new StreamReader(resourceStream))
                    {
                        content = resourceReader.ReadToEnd();
                    }
            }
            return content;
        }

        private static void CopyResource(string resourceName, string outputFileFullPath)
        {
            using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", nameof(resourceName));
                }
                using (Stream output = File.Create(outputFileFullPath))
                {
                    resource.CopyTo(output);
                }
            }
        }

        private static void CheckExit(ConsoleKeyInfo answerKey)
        {
            if (answerKey.Key == ConsoleKey.Q)
            {
                Console.WriteLine("Push any key to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        private static void CheckExit(string exitStr)
        {
            if (exitStr.EqualsAny(StringComparison.InvariantCultureIgnoreCase, "q", "quit", "exit"))
            {
                Console.WriteLine("Push any key to exit...");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }

        private static void WriteGreetings()
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║    AGROLOG/GREINBAR REPORTS GENERATE TOOL    ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║                  written by                  ║");
            Console.WriteLine("║       <Igor 'CoffeeJelly' Salzhenitsyn>      ║");
            Console.WriteLine("║             for JSC \"BELSOLOD\"               ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║               Copyright © 2016               ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝ ");
            Console.Write("\r\n\r\n\r\n");
            Console.WriteLine("Any time when you want to exit type q, exit, quit");
            Console.Write("\r\n\r\n");
        }

        private static void SetCulturePattern()
        {
            var ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            Thread.CurrentThread.CurrentCulture = ci;
        }

        private static void Settings()
        {
            Console.Title = "Generate reports tool";
            Console.SetWindowPosition(0, 0);   // sets window position to upper left
            Console.SetBufferSize(200, 100);   // make sure buffer is bigger than window
            Console.SetWindowSize(200, 54);   //set window size to almost full screen 

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
        }

        private const string FAKE_REPORTS_FOLDER_NAME = "Fake Reports";
        private const string AGROLOG_EXAMPLE_RESOURCE_NAME = "CoffeeJelly.ReportGenerateTool.Agrolog_example";
        private const string GRAINBAR_EXAMPLE_RESOURCE_NAME = "CoffeeJelly.ReportGenerateTool.Grainbar_example";
        private const string AGROLOG_FAKE_REPORT_EXTENSION_NAME = "csv";
        private const string GRAINBAR_FAKE_REPORT_EXTENSION_NAME = "txt";
        private const string REPLACE_DATE_PATTERN = "###";
        private const string AGROLOG_DATE_PATTERN = "yyyy-MM-dd hh:mm:ss";
        private const string GRAINBAR_DATE_PATTERN = "dd.MM.yy, hh:mm:ss";

        private const int THREADS_COUNT = 4;

        private enum TimeRange
        {
            Week,
            Day,
            Hour
        }
    }
}
