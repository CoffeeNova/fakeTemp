using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

using CoffeeJelly.tempa;
using CoffeeJelly.tempa.Extensions;
using System.IO;

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
            if (anyDateRangePrefer)
            {
                startDate = StartDateQuestion();
                endDate = EndDateQuestion();
            }
            string outputPath = ReportsOutputPathQuestion(reportType);

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

                if(line.Equals("default", StringComparison.InvariantCultureIgnoreCase))
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
            while(true)
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


        private static readonly string FAKE_REPORTS_FOLDER_NAME = "Fake Reports";
    }
}
