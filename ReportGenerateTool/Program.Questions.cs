using CoffeeJelly.tempadll.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CoffeeJelly.tempadll;

namespace CoffeeJelly.ReportGenerateTool
{
    public partial class Program
    {

        private static bool BeginGenerateQuestion()
        {
            Console.Write("Press [Enter] to start generate reports. [Q] - quit: ");
            ConsoleKeyInfo answerKey;
            while (true)
            {
                answerKey = Console.ReadKey(false);
                Console.WriteLine();
                if (answerKey.Key.EqualsAny(ConsoleKey.Enter, ConsoleKey.Q))
                    break;
                Console.Write("Fail. Press [Enter] to start generate reports. [Q] - quit:   ");
            }
            CheckExit(answerKey);
            return answerKey.Key == ConsoleKey.Enter;
        }

        private static TimeRange? TimeRangeQuestion()
        {
            Console.Write("Choose time range then. [W] - week, [D] - day, [H] - hour. [Q] - quit:   ");

            ConsoleKeyInfo answerKey;
            while (true)
            {
                answerKey = Console.ReadKey(false);
                Console.WriteLine();
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.W, ConsoleKey.D, ConsoleKey.H, ConsoleKey.Q))
                    break;
                Console.Write("Try again pls. Input should be only [W], [D], [H] or [Q]:    ");
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
            string outpuPath;

            Console.Write($"Type output reports path or [default] of just press [Enter] to set location {defaultPath}:    ");
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine();
                CheckExit(line);

                Debug.Assert(line != null, "line != null");
                if (line.EqualsAny(StringComparison.InvariantCultureIgnoreCase, "", "default"))
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
                    Console.Write("Wrong directory or you have no permissions to this folder, try default it is quite good:    ");
                }
            }
            return outpuPath;
        }

        private static DateTime? StartDateQuestion()
        {
            string pattern = "dd.MM.yyyy";
            Console.Write($"Type initial date in format {pattern}:    ");

            DateTime answer;
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine();
                CheckExit(line);
                try
                {
                    bool parseResult = DateTime.TryParseExact(line, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out answer);
                    if (parseResult)
                        break;
                }
                catch
                {
                    // ignored
                }
                Console.Write($"Wrong. Try again pls. Input patternt is {pattern} (example: 25.02.1987):    ");
            }
            return answer;
        }

        private static DateTime? EndDateQuestion()
        {
            string pattern = "dd.MM.yyyy";
            Console.Write($"Type final date in format {pattern}:    ");

            DateTime answer;
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine();
                CheckExit(line);
                try
                {
                    bool parseResult = DateTime.TryParseExact(line, pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out answer);
                    if (parseResult)
                        break;
                }
                catch
                {
                    // ignored
                }
                Console.Write($"Wrong. Try again pls. Input patternt is {pattern} (example: 25.02.1987):    ");
            }
            return answer;
        }

        private static bool DateRangePreferQuestion()
        {
            Console.Write("Do you want to set date range for reports? [Y] ,[N] or [Q] to quit:    ");

            ConsoleKeyInfo answerKey;
            while (true)
            {
                answerKey = Console.ReadKey(false);
                Console.WriteLine();
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.Y, ConsoleKey.N, ConsoleKey.Q))
                    break;
                Console.Write("Try again pls. Input should be only [Y], [N] or [Q]:    ");
            }
            CheckExit(answerKey);

            return answerKey.Key == ConsoleKey.Y ? true : false;
        }

        private static ushort ReportsCountQuestion()
        {
            Console.Write($"Choose reports count. Only positive numbers (min:1, max:{UInt16.MaxValue}):    ");

            ushort answer = 0;
            while (true)
            {
                var line = Console.ReadLine();
                Console.WriteLine();
                CheckExit(line);
                bool parseResult = UInt16.TryParse(line, out answer);
                if (parseResult && answer > 0)
                    break;

                Console.Write("Wrong. Try again pls. Input should be POSITIVE NUMBERS or [Q]:    ");
            }
            return answer;
        }

        private static ProgramType ReportTypeQuestion()
        {
            Console.Write("Choose report type. [A] - Agrolog, [G] - Grainbar. [Q] - quit:    ");

            ConsoleKeyInfo answerKey;
            while (true)
            {
                answerKey = Console.ReadKey(false);
                Console.WriteLine();
                if (answerKey.Key.EqualsAny<ConsoleKey>(ConsoleKey.A, ConsoleKey.G, ConsoleKey.Q))
                    break;
                Console.Write("Try again pls. Input should be only [A], [G] or [Q]:    ");
            }
            CheckExit(answerKey);

            return answerKey.Key == ConsoleKey.A ? ProgramType.Agrolog : ProgramType.Grainbar;
        }

    }
}
