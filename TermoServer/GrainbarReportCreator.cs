using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using CoffeeJelly.tempadll;
using NLog;
using System.Windows.Automation;
using System.Threading;
using Gma.UserActivityMonitor;

namespace CoffeeJelly.TermoServer
{
    public static class GrainbarReportCreator
    {
        public static void InitializeGrainbarTimer()
        {
            _grainbarTimer = new System.Timers.Timer();
            _grainbarTimer.Interval = _grainBarTimerInterval;
            _grainbarTimer.Elapsed += _grainbarTimer_Elapsed;
            _grainbarTimer.Enabled = true;
            _grainbarTimer.AutoReset = true;
            _grainbarTimer.Start();
        }

        internal static void TestReportCreate()
        {
            _grainbarTimer = new System.Timers.Timer();
            _grainbarTimer.Interval = _grainBarTimerInterval;
            _grainbarTimer.Elapsed += _grainbarTimer_Elapsed;
            _grainbarTimer.Enabled = true;
            _grainbarTimer.AutoReset = true;
            TrySaveGrainbarReport();
        }
        internal static async void TestDisableGlobalControl()
        {
            _controlDisabled = DisableGlobalControl();
            await CoffeeJTools.Delay(10000);
            _controlDisabled = EnableGlobalControl();
        }

        private static void _grainbarTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour != Settings.GrainbarAutoCreateReportHour ||
                DateTime.Now.Minute != Settings.GrainbarAutoCreateReportMinute)
                return;

            if (DisableControl)
                _controlDisabled = DisableGlobalControl();
            TrySaveGrainbarReport();
            _controlDisabled = EnableGlobalControl();

        }

        private static bool DisableGlobalControl()
        {
            if (!_controlDisabled)
            {
                HookManager.KeyBlock += HookManager_Block;
                HookManager.MouseClickBlock += HookManager_Block;
                _log.Info("Control disabled");
                return true;
            }
            return false;
        }

        private static bool EnableGlobalControl()
        {
            if (_controlDisabled)
            {
                HookManager.KeyBlock -= HookManager_Block;
                HookManager.MouseClickBlock -= HookManager_Block;
                _log.Info("Control enabled");
                return true;
            }
            return false;
        }

        private static void HookManager_Block(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
        }

        private static bool TrySaveGrainbarReport()
        {
            try
            {
                var grainbarProcess = GrainbarProcess();
                var windowAe = GetWindowAeByClass(grainbarProcess.Id, Constants.GRAINBAR_MAIN_WINDOW_CLASS_NAME);
                IntPtr hWnd = (IntPtr)windowAe.Current.NativeWindowHandle;

                if (hWnd == IntPtr.Zero)
                    throw new GrainbarMainWindowNotExistException();
                bool windowRestored = CoffeeJTools.RestoreWindow(grainbarProcess.MainWindowHandle);
                var workPlaceAe = WorkPlaceAe(windowAe);

                //1. Должна быть открыта вкладка "Измерения", если нет, то нажмем на кнопку "Измерения.
                // Найдем панель "измерения".
                AutomationElement measurementPanelAe;
                try
                {
                    measurementPanelAe = MeasurementsPanelAe(workPlaceAe);
                }
                catch
                {
                    measurementPanelAe = null;
                }

                if (measurementPanelAe == null) //try to switch to measurement panel
                {
                    var headerPanelAe = HeaderBarPanelAe(windowAe);
                    var measurementButtonAe = MeasurementButtonAe(headerPanelAe);

                    CoffeeJTools.SimulateClickUIAutomation(measurementButtonAe, false);

                    measurementPanelAe = MeasurementsPanelAe(workPlaceAe);
                }

                //2. Найдем кнопку выбора архива, если сейчас не текущие данные, 
                //нажмем на кнопку и в открытом окне нажмем кнопку выбора текущих данных
                var currentArchiveButtonAe = CurrentArchiveButtonAe(measurementPanelAe);
                AutomationElement workWithArchiveWindowAe;
                if (currentArchiveButtonAe.Current.Name != "Текущий")
                {
                    CoffeeJTools.SimulateClickUIAutomation(currentArchiveButtonAe, false);
                    workWithArchiveWindowAe = GetWindowAeByClass(grainbarProcess.Id, Constants.GRAINBAR_WORK_WITH_ARCHIVE_WINDOW_CLASS_NAME);
                    var openCurrentDataButtonAe = OpenCurrentDataButtonAe(workWithArchiveWindowAe);
                    CoffeeJTools.SimulateClickUIAutomation(openCurrentDataButtonAe, false);
                }

                //3.0 В списоке измерений, кликнем мышкой в верхнюю область, чтобы выделить последнее измерение.
                var measurementListAe = MeasurementListAe(measurementPanelAe);
                //3.1 Полосе прокрутки зададим значение 0, чтобы перемотать вначало.
                var scrollBarAe = ScrollBarAe(measurementListAe);
                if (scrollBarAe != null)
                {
                    var patternId = scrollBarAe.GetSupportedPatterns()[0];
                    object pattern = null;
                    scrollBarAe.TryGetCurrentPattern(patternId, out pattern);
                    ((RangeValuePattern)pattern).SetValue(0);
                    Thread.Sleep(100);
                }
                CoffeeJTools.SimulateClickUIAutomation(measurementListAe, false, new System.Windows.Vector(5, 5));

                //4. Нажмем на кнопку выбора текущего архива.
                CoffeeJTools.SimulateClickUIAutomation(currentArchiveButtonAe, false);

                //5. Найдем открытое окно "Работа с архивом" и нажмем в нем кнопку экспорта данных.
                workWithArchiveWindowAe = GetWindowAeByClass(grainbarProcess.Id, Constants.GRAINBAR_WORK_WITH_ARCHIVE_WINDOW_CLASS_NAME);
                var exportdDataButtonAe = ExportDataButtonAe(workWithArchiveWindowAe);
                CoffeeJTools.SimulateClickUIAutomation(exportdDataButtonAe, false);

                //6. Найдем открытое диалоговое окно открытия(почему не сохранения я хз), укажем имя файла и кликнем по кнопке "Открыть"
                var openDialogWindowAe = GetWindowAeByClass(grainbarProcess.Id, Constants.DIALOG_WINDOW_CLASS_NAME);
                var fileNameTextBoxAe = FileNameEditAe(openDialogWindowAe);
                var fntbvp = fileNameTextBoxAe.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                fileNameTextBoxAe.SetFocus();
                Thread.Sleep(100);
                fntbvp?.SetValue(FullReportName);
                Thread.Sleep(100);
                var openButtonAe = OpenButtonAe(openDialogWindowAe);
                var obip = openButtonAe.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                obip?.Invoke();
                return true;
            }
            catch (GrainbarProcessNotExistException ex)
            {
                _log.Info("Grainbar process isn't found.", ex);
            }
            catch (GrainbarMainWindowNotExistException ex)
            {
                _log.Info("Grainbar program has no active window.", ex);
            }
            catch (Exception ex)
            {
                _log.Info(ex, "Saving Grainbar report failed");
            }
            return false;
        }

        private static Process GrainbarProcess()
        {
            var process = Process.GetProcessesByName(Constants.GRAINBAR_PROGRAM_PROCESS_NAME);
            if (process.Length == 0)
                throw new GrainbarProcessNotExistException();
            return process.First();
        }

        private static AutomationElement MainWindowAe(IntPtr mainWindowHandle)
        {
            Func<AutomationElement> func = () => AutomationElement.FromHandle(mainWindowHandle);

            var mainWindowAe = CoffeeJTools.AttemptExecuteFunction(5000, 100, func);
            if (mainWindowAe == null)
                throw new ElementNotAvailableException("UI of Grainbars main window not available.");
            if (!TreeWalker.RawViewWalker.GetParent(mainWindowAe).Current.ClassName.Equals("#32769"))
                throw new ArgumentException($"{nameof(mainWindowHandle)} is not main window handle");
            return mainWindowAe;
        }

        private static AutomationElement WorkPlaceAe(AutomationElement parentAe)
        {
            var workPlaceAe = parentAe.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TPageControl"));
            if (workPlaceAe == null)
                throw new ElementNotAvailableException("UI of work place not available.");
            return workPlaceAe;
        }

        private static AutomationElement MeasurementsPanelAe(AutomationElement parentAe)
        {
            Func<AutomationElement> func = () => parentAe.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "ts2"));

            var measuremenstPanelAe = CoffeeJTools.AttemptExecuteFunction(5000, 100, func);
            if (measuremenstPanelAe == null)
                throw new ElementNotAvailableException("UI of measurements panel not available.");
            return measuremenstPanelAe;
            //var grandChild = child.FindFirst(TreeScope.Children,
            //    new PropertyCondition(AutomationElement.ClassNameProperty, "TfrArhiv"));
            //var grandx2Child = grandChild.FindFirst(TreeScope.Children,
            //    new PropertyCondition(AutomationElement.ClassNameProperty, "TPanel"));
        }

        private static AutomationElement HeaderBarPanelAe(AutomationElement parentAe)
        {
            var headerBarPanelAe = parentAe.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TPanel"));
            if (headerBarPanelAe == null)
                throw new ElementNotAvailableException("UI of header panel not available.");
            return headerBarPanelAe;
        }

        private static AutomationElement MeasurementButtonAe(AutomationElement parentAe)
        {
            var measurementButtonAe = parentAe.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TPanelButton"));
            if (measurementButtonAe == null)
                throw new ElementNotAvailableException("UI of measurement button not available.");
            return measurementButtonAe;
        }

        private static AutomationElement CurrentArchiveButtonAe(AutomationElement parentAe)
        {
            try
            {
                var childPanelAe = parentAe.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TfrArhiv"));

                var grandChildPanelAe = childPanelAe.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TPanel"));

                var currentArchiveButtonAe = grandChildPanelAe.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TPanelButton"))[3];

                return currentArchiveButtonAe;
            }
            catch (Exception ex)
            {
                throw new ElementNotAvailableException("UI of active archive button not available.", ex);
            }
        }

        private static AutomationElement GetWindowAeByClass(int processId, string className)
        {
            Func<AutomationElement> func = () =>
            {
                IntPtr handle = CoffeeJTools.GetWidgetWindowHandles(processId, className).First();
                return AutomationElement.FromHandle(handle);
            };
            var measuremenstPanelAe = CoffeeJTools.AttemptExecuteFunction(5000, 1000, func);
            if (measuremenstPanelAe == null)
                throw new ElementNotAvailableException($"UI of{nameof(className)} window not available.");
            return measuremenstPanelAe;
        }

        private static AutomationElement OpenCurrentDataButtonAe(AutomationElement parentAe)
        {
            var openCurrentDataButtonAe = parentAe.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TPanelButton"));
            if (openCurrentDataButtonAe == null)
                throw new ElementNotAvailableException("UI of open currend data button not available.");
            return openCurrentDataButtonAe;
        }

        private static AutomationElement ScrollBarAe(AutomationElement parentAe)
        {
            var scrollBarAe = parentAe.FindFirst(TreeScope.Children, Condition.TrueCondition);
            if (scrollBarAe == null)
                throw new ElementNotAvailableException("UI of scrollbar not available.");
            return scrollBarAe;
        }

        private static AutomationElement MeasurementListAe(AutomationElement parentAe)
        {
            try
            {
                var childPanelAe = parentAe.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TfrArhiv"));
                var grandChildPanelAe = childPanelAe.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TPanel"));
                var grandChildx2PanelAe = grandChildPanelAe.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "TPanel"))[1];
                var measurementListAe = grandChildx2PanelAe.FindAll(TreeScope.Children, Condition.TrueCondition)[2];
                //var measurementListAe = childPanelAe.FindFirst(TreeScope.Children,
                //new PropertyCondition(AutomationElement.ClassNameProperty, "TDBGridSlon"));

                return measurementListAe;
            }
            catch (Exception ex)
            {
                throw new ElementNotAvailableException("UI of measurement list not available.", ex);
            }
        }

        private static AutomationElement ExportDataButtonAe(AutomationElement parentAe)
        {
            try
            {
                var exportDataButtonAe = parentAe.FindAll(TreeScope.Children,
                   new PropertyCondition(AutomationElement.ClassNameProperty, "TPanelButton"))[2];
                return exportDataButtonAe;
            }
            catch
            {
                throw new ElementNotAvailableException("UI of export data button not available.");
            }
        }

        private static AutomationElement FileNameEditAe(AutomationElement parentAe)
        {
            try
            {
                //var comboBoxEx32Ae = parentAe.FindAll(TreeScope.Children,
                //    Condition.TrueCondition);
                //foreach (var t in comboBoxEx32Ae)
                //    Console.WriteLine((t as AutomationElement).Current.Name);
                var comboBoxAe = parentAe.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "ComboBox"))[1];
                var editAe = comboBoxAe.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ClassNameProperty, "Edit"));
                return editAe;
            }
            catch (Exception ex)
            {
                throw new ElementNotAvailableException("UI of file name edit not available.", ex);
            }
        }

        private static AutomationElement OpenButtonAe(AutomationElement parentAe)
        {
            var openButtonAe = parentAe.FindFirst(TreeScope.Children,
               new PropertyCondition(AutomationElement.ClassNameProperty, "Button"));
            if (openButtonAe == null)
                throw new ElementNotAvailableException("UI of open button dialog window not available.");
            return openButtonAe;
        }

        private static System.Timers.Timer _grainbarTimer;
        private static readonly double _grainBarTimerInterval = 1000; //ms
        private static Logger _log = LogManager.GetCurrentClassLogger();
        private static string _fullReportName = Constants.DEFAULT_GRAINBAR_AUTOCREATED_REPORT_FULL_NAME;
        private static bool _controlDisabled = false;

        public static string FullReportName => _fullReportName;

        public static bool DisableControl { get; set; }
    }
}
