using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

using CoffeeJelly.tempa;
using CoffeeJelly.tempa.Extensions;
using System.IO;
using System.Reflection;

namespace CoffeeJelly.ReportGenerateTool
{
    class Program
    {
        static void Main(string[] args)
        {
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

            LetsWork();
        }

        private async static void LetsWork(ProgramType reportType, ushort reportsCount, DateTime? startDate, DateTime? endDate, TimeRange? timeRange, string outputPath)
        {
            DateTime currentDate = DateTime.Now;

            var dateFunc1 = new Func<int, DateTime>((i) =>
            {
                DateTime d = currentDate;
                switch (timeRange)
                {
                    case TimeRange.Week:
                        d.AddDays(-i * 7);
                        break;
                    case TimeRange.Day:
                        d.AddDays(-i);
                        break;
                    case TimeRange.Hour:
                        d.AddHours(-i);
                        break;
                    default:
                        d = DateTime.Now;
                        break;
                }
                return d;
            });

            var dateFunc2 = new Func<int, DateTime>((i) =>
            {
                var random = new Random();
                var randomOADate = random.NextDouble() * (endDate.Value.ToOADate() - startDate.Value.ToOADate()) + startDate.Value.ToOADate();
                return DateTime.FromOADate(randomOADate);
            });

            string resourceName = reportType == ProgramType.Agrolog ? AGROLOG_EXAMPLE_RESOURCE_NAME : GRAINBAR_EXAMPLE_RESOURCE_NAME;
            string outputFileExtension = reportType == ProgramType.Agrolog ? AGROLOG_FAKE_REPORT_EXTENSION_NAME : GRAINBAR_FAKE_REPORT_EXTENSION_NAME;
            string datePattern = reportType == ProgramType.Agrolog ? AGROLOG_DATE_PATTERN : GRAINBAR_DATE_PATTERN;

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    string content = resourceReader.ReadToEnd();

                    for (int i = 1; i <= reportsCount; i++)
                    {
                        DateTime reportDate;
                        string outputFullPath = outputPath.PathFormatter();
                        string newContent = string.Empty;

                        reportDate = !startDate.HasValue ? dateFunc1(i) : dateFunc2(i);
                        newContent = content.ReplaceFirst(REPLACE_DATE_PATTERN, reportDate.ToString(datePattern));
                        outputFullPath += $"{reportDate.ToString("dd-MM-yyyy")}_report.{outputFileExtension}";
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
                }

            }
        }

        private static void CopyResource(string resourceName, string outputFileFullPath)
        {
            using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.Create(outputFileFullPath))
                {
                    resource.CopyTo(output);
                }
            }
        }

        private static TimeRange? TimeRangeQuestion()
        {
            Console.WriteLine("Choose time range then. [W] - week, [D] - day, [H] - hour. [Q] - quit.");

            var answerKey = new ConsoleKeyInfo();
            while (true)
            {
                answerKey = Console.ReadKey(false);
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.W, ConsoleKey.D, ConsoleKey.H, ConsoleKey.Q))
                    break;
                Console.WriteLine("Try again pls. Input should be only [W], [D], [H] or [Q].");
            }
            CheckExit(answerKey);

            if (answerKey.Key == ConsoleKey.W)
                return TimeRange.Week;
            if (answerKey.Key == ConsoleKey.D)
                return TimeRange.Day;
            if (answerKey.Key == ConsoleKey.H)
                return TimeRange.Hour;
            return null;
        }

        private static string ReportsOutputPathQuestion(ProgramType reportType)
        {
            string defaultPath = Constants.APPLICATION_DIRECTORY.PathFormatter() + FAKE_REPORTS_FOLDER_NAME.PathFormatter();
            defaultPath += reportType == ProgramType.Agrolog ? Constants.AGROLOG_FOLDER_NAME : Constants.GRAINBAR_FOLDER_NAME;
            string outpuPath = string.Empty;

            Console.WriteLine($"Type output reports path or [default] to set location {defaultPath}.");
            while (true)
            {
                var line = Console.ReadLine();

                CheckExit(line);

                if (line.Equals("default", StringComparison.InvariantCultureIgnoreCase))
                {
                    outpuPath = defaultPath;
                    break;
                }
                try
                {
                    var dirInfo = new DirectoryInfo(line);
                    if (dirInfo.Exists)
                    {
                        outpuPath = line;
                        break;
                    }
                    else
                        throw new Exception();
                }
                catch
                {
                    Console.WriteLine("Wrong directory or you have no permissions to this folder, try default it is quite good.");
                }
            }
            return outpuPath;
        }

        private static DateTime? StartDateQuestion()
        {
            string pattern = "dd.MM.yyyy";
            Console.WriteLine($"Type initial date in format {pattern}");

            DateTime answer;
            while (true)
            {
                var line = Console.ReadLine();
                CheckExit(line);
                try
                {
                    bool parseResult = DateTime.TryParseExact(line, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out answer);
                    if (parseResult)
                        break;
                }
                catch { }
                Console.WriteLine($"Wrong. Try again pls. Input patternt is {pattern} (example: 25.02.1987");
            }
            return answer;
        }

        private static DateTime? EndDateQuestion()
        {
            string pattern = "dd.MM.yyyy";
            Console.WriteLine($"Type final date in format {pattern}");

            DateTime answer;
            while (true)
            {
                var line = Console.ReadLine();
                CheckExit(line);
                try
                {
                    bool parseResult = DateTime.TryParseExact(line, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out answer);
                    if (parseResult)
                        break;
                }
                catch { }
                Console.WriteLine($"Wrong. Try again pls. Input patternt is {pattern} (example: 25.02.1987");
            }
            return answer;
        }

        private static bool DateRangePreferQuestion()
        {
            Console.WriteLine("Do you want to set date range for reports? [Y] ,[N] or [Q] to quit");

            var answerKey = new ConsoleKeyInfo();
            while (true)
            {
                answerKey = Console.ReadKey(false);
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.Y, ConsoleKey.N, ConsoleKey.Q))
                    break;
                Console.WriteLine("Try again pls. Input should be only [Y], [N] or [Q].");
            }
            CheckExit(answerKey);

            return answerKey.Key == ConsoleKey.Y ? true : false;
        }

        private static ushort ReportsCountQuestion()
        {
            Console.WriteLine($"Choose reports count. Only positive numbers (max:{UInt16.MaxValue}.");

            ushort answer = 0;
            while (true)
            {
                var line = Console.ReadLine();
                CheckExit(line);
                bool parseResult = UInt16.TryParse(line, out answer);
                if (parseResult)
                    break;

                Console.WriteLine("Wrong. Try again pls. Input should be POSITIVE NUMBERS or [Q].");
            }
            return answer;
        }

        private static ProgramType ReportTypeQuestion()
        {
            Console.WriteLine("Choose report type. [A] - Agrolog, [G] - Grainbar. [Q] - quit.");

            var answerKey = new ConsoleKeyInfo();
            while (true)
            {
                answerKey = Console.ReadKey(false);
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.A, ConsoleKey.G, ConsoleKey.Q))
                    break;
                Console.WriteLine("Try again pls. Input should be only [A], [G] or [Q].");
            }
            CheckExit(answerKey);

            return answerKey.Key == ConsoleKey.A ? ProgramType.Agrolog : ProgramType.Grainbar;
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


        private const string FAKE_REPORTS_FOLDER_NAME = "Fake Reports";
        private const string AGROLOG_EXAMPLE_RESOURCE_NAME = "CoffeeJelly.ReportGenerateTool.Agrolog_example";
        private const string GRAINBAR_EXAMPLE_RESOURCE_NAME = "CoffeeJelly.ReportGenerateTool.Grainbar_example";
        private const string AGROLOG_FAKE_REPORT_EXTENSION_NAME = "csv";
        private const string GRAINBAR_FAKE_REPORT_EXTENSION_NAME = "txt";
        private const string REPLACE_DATE_PATTERN = "###";
        private const string AGROLOG_DATE_PATTERN = "yyyy-MM-dd hh:mm:ss";
        private const string GRAINBAR_DATE_PATTERN = "dd.MM.yy, hh:mm:ss";

        private enum TimeRange
        {
            Week,
            Day,
            Hour
        }
    }
}
