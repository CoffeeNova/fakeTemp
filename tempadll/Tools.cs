using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NWTweak;
using System.Threading.Tasks;
using System.Globalization;
using CoffeeJelly.tempadll;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Automation;
using System.Threading;

namespace CoffeeJelly
{
    public class CoffeeJTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        /// <exception cref="InvalidOperationException">Ошибка записи в реестр. Подробности во внутреннем исключении.</exception>
        public static void SaveRegistrySettings(ref bool lastValue, string valueName, string keyLocation, bool saveValue)
        {
            if (lastValue == saveValue)
                return;
            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue == true ? "true" : "false");
                lastValue = saveValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        /// <exception cref="InvalidOperationException">Ошибка записи в реестр. Подробности во внутреннем исключении.</exception>
        public static void SaveRegistrySettings(string valueName, string keyLocation, bool saveValue)
        {
            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue == true ? "true" : "false");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        public static void SaveRegistrySettings(ref string lastValue, string valueName, string keyLocation, string saveValue)
        {
            if (lastValue == saveValue)
                return;
            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue);
                lastValue = saveValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
        }

        public static string SaveRegistrySettings(string valueName, string keyLocation, string saveValue)
        {
            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, saveValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
            return saveValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        public static void CheckRegistrySettings(ref string keyValue, string valueName, string keyLocation, string keyDefaultValue)
        {
            string getkey = null;
            try
            {
                getkey = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            }
            catch (System.IO.IOException) { }

            if (getkey != null) { keyValue = getkey; return; }

            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue);
                keyValue = keyDefaultValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        public static void CheckRegistrySettings(ref bool keyValue, string valueName, string keyLocation, bool keyDefaultValue)
        {
            string getkey = null;
            try
            {
                getkey = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            }
            catch (System.IO.IOException) { }

            if (getkey != null) { keyValue = getkey == "true" ? true : false; return; }

            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue == true ? "true" : "false");
                keyValue = keyDefaultValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        /// <returns></returns>
        public static bool CheckRegistrySettings(string valueName, string keyLocation, bool keyDefaultValue)
        {
            bool keyValue;
            string getkey = null;
            try
            {
                getkey = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            }
            catch (System.IO.IOException) { }

            if (getkey != null)
                return keyValue = getkey == "true" ? true : false;

            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue == true ? "true" : "false");
                keyValue = keyDefaultValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
            return keyValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="keyDefaultValue"></param>
        public static string CheckRegistrySettings(string valueName, string keyLocation, string keyDefaultValue)
        {
            string keyValue;
            string getkey = null;
            try
            {
                getkey = RegistryWorker.GetKeyValue<string>(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, valueName);
            }
            catch (System.IO.IOException) { }

            if (getkey != null) { return getkey; }

            try
            {
                RegistryWorker.WriteKeyValue(Microsoft.Win32.RegistryHive.LocalMachine, keyLocation, Microsoft.Win32.RegistryValueKind.String, valueName, keyDefaultValue);
                keyValue = keyDefaultValue;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to write a value to HKEY_LOCAL_MACHINE\\" + keyLocation + "\\" + valueName, ex);
            }
            return keyValue;
        }

        public static Stack<TreeViewItem> GetNodes(UIElement element)
        {

            var tempNodePath = new Stack<TreeViewItem>();
            // Walk up the element tree to the nearest tree view item. 

            while ((element != null))
            {
                var container = element as TreeViewItem;
                if (container != null)
                    tempNodePath.Push(container);
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
            return tempNodePath;
        }

        public static FrameworkElement FindVisualChildElement(DependencyObject element, Type childType)
        {
            int count = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < count; i++)
            {
                var dependencyObject = VisualTreeHelper.GetChild(element, i);
                var fe = (FrameworkElement)dependencyObject;

                if (fe.GetType() == childType)
                    return fe;

                FrameworkElement ret = null;

                if (fe.GetType().Equals(typeof(ScrollViewer)))
                    ret = FindVisualChildElement((fe as ScrollViewer).Content as FrameworkElement, childType);
                else
                    ret = FindVisualChildElement(fe, childType);

                if (ret != null)
                    return ret;
            }

            return null;
        }

        public static FrameworkElement FindVisualParentElement(DependencyObject element, Type parentType)
        {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(element);

            if (ic == null) return null;

            if (ic.GetType() == parentType)
                return ic;
            else
                return FindVisualParentElement(ic, parentType);

        }

        /// <summary>
        /// создает список всех видимых заданных дочерних элементов в родительском элементе 
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="childElement"></param>
        /// <returns></returns>
        public static List<FrameworkElement> GetChildElementsByType(FrameworkElement parentElement, Type childElementType)
        {
            List<FrameworkElement> childElemList = new List<FrameworkElement>();
            List<FrameworkElement> allElem = new List<FrameworkElement>();
            //создадим список всех элементов в родительском элементе
            ChildControls(parentElement, allElem);
            //выберем из списка только видимые и нужные по типу
            foreach (FrameworkElement elem in allElem)
            {
                if (elem.GetType() == childElementType && elem.Visibility != Visibility.Hidden)
                {
                    childElemList.Add(elem);
                }
            }
            return childElemList;
        }

        /// <summary>
        /// список всех wpf элементов в заданном родительском элементе 
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="Controls"></param>
        public static void ChildControls(FrameworkElement parentElement, List<FrameworkElement> Controls)
        {
            foreach (FrameworkElement child in LogicalTreeHelper.GetChildren(parentElement))
            {
                try
                {
                    Controls.Add(child);
                    if (child is ContentControl)
                    {
                        if (!((child as ContentControl).Content is string))
                            ChildControls((FrameworkElement)(child as ContentControl).Content, Controls);
                    }
                    else ChildControls(child, Controls);
                }
                catch { }
            }
        }

        /// <summary>
        /// создает список всех видимых заданных дочерних элементов в родительском элементе 
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="childElement"></param>
        /// <returns></returns>
        public static FrameworkElement GetChildElementByName(FrameworkElement parentElement, string childElementName)
        {
            var allElem = new List<FrameworkElement>();
            //создадим список всех элементов в родительском элементе
            ChildControls(parentElement, allElem);
            foreach (FrameworkElement elem in allElem)
                if (elem.Name == childElementName && elem.Visibility != Visibility.Hidden)
                    return elem;
            return null;
        }

        public static void CopyResource(string resourceName, string outputFileFullPath)
        {
            using (Stream resource = Assembly.GetCallingAssembly().GetManifestResourceStream(resourceName))
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

        public static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                uint lpdwProcessId;
                WinApi.GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new WinApi.Win32Callback(EnumWindow);
                WinApi.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;

        }

        public static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        public static string GetProgramName<T>()
        {
            return typeof(T) == typeof(TermometerAgrolog) ? Constants.AGROLOG_PROGRAM_NAME : Constants.GRAINBAR_PROGRAM_NAME;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        /// <remarks>http://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding</remarks>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        public static Encoding GetEncoding(Stream stream)
        {
            // Read the BOM
            var bom = new byte[4];
            stream.Read(bom, 0, 4);

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        public static string ArchiveDataFileName(string dateFormat, DateTime initDate, DateTime finalDate, string restName)
        {
            return $"{initDate.Date.ToString(dateFormat)}-{finalDate.Date.ToString(dateFormat)} {restName}";
        }

        public static bool ArchiveDataFileNameValidation(string dateFormat, string fileName)
        {
            try
            {
                string shortFileName = fileName.Split('\\').Last();
                string[] splittedByEmpty = shortFileName.Split(' ');
                string dateRange = splittedByEmpty.First();
                string[] splittedToDates = dateRange.Split('-');
                string initDate = splittedToDates.First();
                string finalDate = splittedToDates.Last();

                if (!DateValidation(initDate, dateFormat) ||
                    !DateValidation(finalDate, dateFormat) ||
                    !Path.HasExtension(splittedByEmpty.Last()))
                    throw new Exception("ArchiveDataFileNameValidation fails");

                return true;
            }
            catch
            {
                return false;
            }

        }

        public static bool DateValidation(string dateString, string dateFormat)
        {
            DateTime dt;
            return DateTime.TryParseExact(
                    dateString,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt);
        }

        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.TrySetResult(true);
            };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }

        public static string MakeExcelReportFileNameFromDataFilePath(string dataFilePath)
        {
            string dataFileName = System.IO.Path.GetFileNameWithoutExtension(dataFilePath);
            return dataFileName + " report.xlsm";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static bool RestoreWindow(IntPtr hWnd)
        {
            var placement = GetPlacement(hWnd);
            if (placement.showCmd == ShowWindowCommands.Minimized || placement.showCmd == ShowWindowCommands.Hide)
            {
                WinApi.ShowWindow(hWnd, ShowWindowEnum.Restore);
                return true;
            }
            return false;
        }


        public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            WinApi.GetWindowPlacement(hwnd, ref placement);
            return placement;
        }

        /// <summary>
        /// Simulate a mouse click to center UIAutomation element.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="handle"></param>
        public static void SimulateClickUIAutomation(AutomationElement element, bool isDoubleClick)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            var clickPoint = new System.Windows.Point(element.Current.BoundingRectangle.Width/2, element.Current.BoundingRectangle.Height/2);

            //create lParam
            int point = (int)clickPoint.Y << 16 | (int)clickPoint.X;
            if (isDoubleClick)
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONDBLCLK, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
            else
            {
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONDOWN, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONUP, IntPtr.Zero, (IntPtr)point);
            }
        }

        /// <summary>
        /// Simulate a mouse click to custom zone UIAutomation element.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="handle"></param>
        /// <param name="offset">Positive offset with initial point of counting based in top left corner.</param>
        public static void SimulateClickUIAutomation(AutomationElement element, bool isDoubleClick, Vector offset)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            Debug.Assert(offset.Length >= 0, $"{nameof(offset)} shouldn't be negative.");

            System.Windows.Rect clickZone = element.Current.BoundingRectangle;
            Debug.Assert(offset.Length <= new Vector(clickZone.Width, clickZone.Height).Length,
                $"{nameof(offset)} doesn't get to borders of area of an element");

            var clickPoint = new System.Windows.Point(offset.X, offset.Y);

            //create lParam
            int point = (int)clickPoint.Y << 16 | (int)clickPoint.X;
            if (isDoubleClick)
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONDBLCLK, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
            else
            {
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONDOWN, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
                WinApi.PostMessage((IntPtr)element.Current.NativeWindowHandle, (uint)WindowMessage.WM_LBUTTONUP, IntPtr.Zero, (IntPtr)point);
            }
        }

        /// <summary>
        /// Simulate a mouse click to center UIAutomation element.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="handle"></param>
        public static void SimulateClickUIAutomation(AutomationElement child, AutomationElement parent, IntPtr handle, bool isDoubleClick)
        {
            if (child == null) throw new ArgumentNullException("child");
            if (parent == null) throw new ArgumentNullException("parent");
            if (handle == IntPtr.Zero) throw new ArgumentException("IntPtr.Zero value",
                 "handle");
            //get new rectangle relatively parent window
            System.Windows.Rect clickZone = BoundingRectangleUIElement(child, parent);

            System.Windows.Point clickPoint = CountRectangleCenter(clickZone);

            //create lParam
            int point = (int)clickPoint.Y << 16 | (int)clickPoint.X;
            if (isDoubleClick)
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONDBLCLK, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
            else
            {
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONDOWN, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONUP, IntPtr.Zero, (IntPtr)point);
            }
        }

        /// <summary>
        /// Simulate a mouse click to custom zone in UIAutomation element.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="handle"></param>
        ///  <param name="isDoubleClick"></param>
        /// <param name="offset">Positive offset with initial point of counting based in top left corner.</param>
        public static void SimulateClickUIAutomation(AutomationElement child, AutomationElement parent, IntPtr handle, bool isDoubleClick, Vector offset)
        {
            if (child == null) throw new ArgumentNullException("child");
            if (parent == null) throw new ArgumentNullException("parent");
            if (handle == IntPtr.Zero) throw new ArgumentException("IntPtr.Zero value",
                 "handle");
            Debug.Assert(offset.Length >= 0, $"{nameof(offset)} shouldn't be negative.");

            //get new rectangle relatively parent window
            System.Windows.Rect clickZone = BoundingRectangleUIElement(child, parent);
            Debug.Assert(offset.Length <= new Vector(clickZone.Right - clickZone.Left, clickZone.Bottom - clickZone.Top).Length, 
                $"{nameof(offset)} doesn't get to borders of area of an element");
            
            var clickPoint = new System.Windows.Point(clickZone.Left + offset.X, clickZone.Top + offset.Y);

            //create lParam
            int point = (int)clickPoint.Y << 16 | (int)clickPoint.X;
            if (isDoubleClick)
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONDBLCLK, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
            else
            {
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONDOWN, (IntPtr)MouseButtonWParams.MK_LBUTTON, (IntPtr)point);
                WinApi.PostMessage(handle, (uint)WindowMessage.WM_LBUTTONUP, IntPtr.Zero, (IntPtr)point);
            }
        }

        /// <summary>
        ///  Возвращает координаты прямоугольника, который полностью охватывает элемент, относительно главного окна
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static System.Windows.Rect BoundingRectangleUIElement(AutomationElement child, AutomationElement parent)
        {
            if (child == null || parent == null)
                return new System.Windows.Rect();
            System.Windows.Rect parentRect = parent.Current.BoundingRectangle; //.ConvertToRect()
            System.Windows.Rect childRect = child.Current.BoundingRectangle;    //.ConvertToRect()
            System.Windows.Point relativePoint = new System.Windows.Point(childRect.X - parentRect.X, childRect.Y - parentRect.Y);
            return new System.Windows.Rect(relativePoint, childRect.Size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private static System.Windows.Point CountRectangleCenter(System.Windows.Rect rect)
        {
            return new System.Windows.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        /// <summary>
        /// Пытается выполнить функцию <paramref name="func"/>, до тех пор пока не выйдет время выполнения <paramref name="attemptTime"/>.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения, должен быть nullable</typeparam>
        /// <param name="attemptTime">Время, которое будет ожидаться положительного результата выполнения функции.</param>
        /// <param name="delayBetweenAttempts">Задержка между попытками выполнения.</param>
        /// <param name="func">Исполняемая функция.</param>
        /// <returns></returns>
        public static T AttemptExecuteFunction<T>(int attemptTime, int delayBetweenAttempts, Func<T> func) where T : class
        {
            T result = null;
            var waitingTimer = Delay(attemptTime);
            while (result == null && waitingTimer.Status != TaskStatus.RanToCompletion)
            {
                try
                {
                    result = func();
                }
                catch
                {
                    result = null;
                }
                if (result == null)
                    Thread.Sleep(delayBetweenAttempts);
            }
            return result;
        }

        /// <summary>
        /// Возвращает список дескрипторов окон, имя класса которых className
        /// </summary>
        /// <param name="processID">id процесса</param>
        /// <param name="className">имя класса по которому производится выборка</param>
        /// <returns></returns>
        public static List<IntPtr> GetWidgetWindowHandles(int processID, string className)
        {
            //get all windows handles
            List<IntPtr> rootWindows = GetRootWindowsOfProcess(processID);
            // find the handles witch contains widget window
            AutomationElement rootWindowAE;
            List<IntPtr> widgetHandles = new List<IntPtr>();
            foreach (IntPtr handle in rootWindows)
            {
                rootWindowAE = AutomationElement.FromHandle(handle);
                if (rootWindowAE == null)
                    continue;
                if (rootWindowAE.Current.ClassName == className)
                {
                    widgetHandles.Add(handle);
                }
            }
            return widgetHandles;
        }
    }

    #region structs and enums
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public ShowWindowCommands showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public Rectangle rcNormalPosition;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Width;
        public int Height;
    }
    public enum ShowWindowCommands : int
    {
        Hide = 0,
        Normal = 1,
        Minimized = 2,
        Maximized = 3,
    }
    public struct KeyboardIndicatorParameters
    {
        public ushort UnitId;
        public Locks LedFlags;
    }
    [Flags]
    public enum Locks : ushort
    {
        None = 0,
        KeyboardScrollLockOn = 1,
        KeyboardNumLockOn = 2,
        KeyboardCapsLockOn = 4
    }
    public enum GetWindow_Cmd : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    public enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };
    //public enum SwitchTo
    //{
    //    Tab = 0,
    //    Window = 1
    //}

    public enum InternetBrowser
    {
        GoogleChrome,
        Firefox,
        Opera,
        TorBrowser,
        InternetExplorer
    }

    //public enum Messenger
    //{
    //    Skype,
    //    WhatsApp,
    //    Telegram,
    //    Viber
    //}
    public enum GaFlags
    {
        /// <summary>
        /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function. 
        /// </summary>
        GA_PARENT = 1,
        /// <summary>
        /// Retrieves the root window by walking the chain of parent windows.
        /// </summary>
        GA_ROOT = 2,
        /// <summary>
        /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent. 
        /// </summary>
        GA_ROOTOWNER = 3
    }

    public struct KBDLLHOOKSTRUCT
    {
        public Keys key;
        public int scanCode;
        public int flags;
        public int time;
        public IntPtr extra;
    }

    /// <summary>
    /// From http://wiki.winehq.org/List_Of_Windows_Messages
    /// </summary>
    [Flags]
    public enum WindowMessage : uint
    {
        WM_NULL = 0x0,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000a,
        WM_SETREDRAW = 0x000b,
        WM_SETTEXT = 0x000c,
        WM_GETTEXT = 0x000d,
        WM_GETTEXTLENGTH = 0x000e,
        WM_PAINT = 0x000f,
        WM_CLOSE = 0x0010,
        WM_QUERYENDSESSION = 0x0011,
        WM_QUIT = 0x0012,
        WM_QUERYOPEN = 0x0013,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_ENDSESSION = 0x0016,
        WM_SHOWWINDOW = 0x0018,
        WM_CTLCOLOR = 0x0019,
        WM_WININICHANGE = 0x001a,
        WM_DEVMODECHANGE = 0x001b,
        WM_ACTIVATEAPP = 0x001c,
        WM_FONTCHANGE = 0x001d,
        WM_TIMECHANGE = 0x001e,
        WM_CANCELMODE = 0x001f,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,
        WM_GETMINMAXINFO = 0x0024,
        WM_PAINTICON = 0x0026,
        WM_ICONERASEBKGND = 0x0027,
        WM_NEXTDLGCTL = 0x0028,
        WM_SPOOLERSTATUS = 0x002a,
        WM_DRAWITEM = 0x002b,
        WM_MEASUREITEM = 0x002c,
        WM_DELETEITEM = 0x002d,
        WM_VKEYTOITEM = 0x002e,
        WM_CHARTOITEM = 0x002f,
        WM_SETFONT = 0x0030,
        WM_GETFONT = 0x0031,
        WM_SETHOTKEY = 0x0032,
        WM_GETHOTKEY = 0x0033,
        WM_QUERYDRAGICON = 0x0037,
        WM_COMPAREITEM = 0x0039,
        WM_GETOBJECT = 0x003d,
        WM_COMPACTING = 0x0041,
        WM_COMMNOTIFY = 0x0044,
        WM_WINDOWPOSCHANGING = 0x0046,
        WM_WINDOWPOSCHANGED = 0x0047,
        WM_POWER = 0x0048,
        WM_COPYGLOBALDATA = 0x0049,
        WM_COPYDATA = 0x004a,
        WM_CANCELJOURNAL = 0x004b,
        WM_NOTIFY = 0x004e,
        WM_INPUTLANGCHANGEREQUEST = 0x0050,
        WM_INPUTLANGCHANGE = 0x0051,
        WM_TCARD = 0x0052,
        WM_HELP = 0x0053,
        WM_USERCHANGED = 0x0054,
        WM_NOTIFYFORMAT = 0x0055,
        WM_CONTEXTMENU = 0x007b,
        WM_STYLECHANGING = 0x007c,
        WM_STYLECHANGED = 0x007d,
        WM_DISPLAYCHANGE = 0x007e,
        WM_GETICON = 0x007f,
        WM_SETICON = 0x0080,
        WM_NCCREATE = 0x0081,
        WM_NCDESTROY = 0x0082,
        WM_NCCALCSIZE = 0x0083,
        WM_NCHITTEST = 0x0084,
        WM_NCPAINT = 0x0085,
        WM_NCACTIVATE = 0x0086,
        WM_GETDLGCODE = 0x0087,
        WM_SYNCPAINT = 0x0088,
        WM_NCMOUSEMOVE = 0x00a0,
        WM_NCLBUTTONDOWN = 0x00a1,
        WM_NCLBUTTONUP = 0x00a2,
        WM_NCLBUTTONDBLCLK = 0x00a3,
        WM_NCRBUTTONDOWN = 0x00a4,
        WM_NCRBUTTONUP = 0x00a5,
        WM_NCRBUTTONDBLCLK = 0x00a6,
        WM_NCMBUTTONDOWN = 0x00a7,
        WM_NCMBUTTONUP = 0x00a8,
        WM_NCMBUTTONDBLCLK = 0x00a9,
        WM_NCXBUTTONDOWN = 0x00ab,
        WM_NCXBUTTONUP = 0x00ac,
        WM_NCXBUTTONDBLCLK = 0x00ad,
        SBM_SETPOS = 0x00e0,
        SBM_GETPOS = 0x00e1,
        SBM_SETRANGE = 0x00e2,
        SBM_GETRANGE = 0x00e3,
        SBM_ENABLE_ARROWS = 0x00e4,
        SBM_SETRANGEREDRAW = 0x00e6,
        SBM_SETSCROLLINFO = 0x00e9,
        SBM_GETSCROLLINFO = 0x00ea,
        SBM_GETSCROLLBARINFO = 0x00eb,
        WM_INPUT = 0x00ff,
        WM_KEYDOWN = 0x0100,
        WM_KEYFIRST = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,
        WM_KEYLAST = 0x0108,
        WM_WNT_CONVERTREQUESTEX = 0x0109,
        WM_CONVERTREQUEST = 0x010a,
        WM_CONVERTRESULT = 0x010b,
        WM_INTERIM = 0x010c,
        WM_IME_STARTCOMPOSITION = 0x010d,
        WM_IME_ENDCOMPOSITION = 0x010e,
        WM_IME_COMPOSITION = 0x010f,
        WM_IME_KEYLAST = 0x010f,
        WM_INITDIALOG = 0x0110,
        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_TIMER = 0x0113,
        WM_HSCROLL = 0x0114,
        WM_VSCROLL = 0x0115,
        WM_INITMENU = 0x0116,
        WM_INITMENUPOPUP = 0x0117,
        WM_SYSTIMER = 0x0118,
        WM_MENUSELECT = 0x011f,
        WM_MENUCHAR = 0x0120,
        WM_ENTERIDLE = 0x0121,
        WM_MENURBUTTONUP = 0x0122,
        WM_MENUDRAG = 0x0123,
        WM_MENUGETOBJECT = 0x0124,
        WM_UNINITMENUPOPUP = 0x0125,
        WM_MENUCOMMAND = 0x0126,
        WM_CHANGEUISTATE = 0x0127,
        WM_UPDATEUISTATE = 0x0128,
        WM_QUERYUISTATE = 0x0129,
        WM_CTLCOLORMSGBOX = 0x0132,
        WM_CTLCOLOREDIT = 0x0133,
        WM_CTLCOLORLISTBOX = 0x0134,
        WM_CTLCOLORBTN = 0x0135,
        WM_CTLCOLORDLG = 0x0136,
        WM_CTLCOLORSCROLLBAR = 0x0137,
        WM_CTLCOLORSTATIC = 0x0138,
        WM_MOUSEFIRST = 0x0200,
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSELAST = 0x0209,
        WM_MOUSEWHEEL = 0x020a,
        WM_XBUTTONDOWN = 0x020b,
        WM_XBUTTONUP = 0x020c,
        WM_XBUTTONDBLCLK = 0x020d,
        WM_PARENTNOTIFY = 0x0210,
        WM_ENTERMENULOOP = 0x0211,
        WM_EXITMENULOOP = 0x0212,
        WM_NEXTMENU = 0x0213,
        WM_SIZING = 0x0214,
        WM_CAPTURECHANGED = 0x0215,
        WM_MOVING = 0x0216,
        WM_POWERBROADCAST = 0x0218,
        WM_DEVICECHANGE = 0x0219,
        WM_MDICREATE = 0x0220,
        WM_MDIDESTROY = 0x0221,
        WM_MDIACTIVATE = 0x0222,
        WM_MDIRESTORE = 0x0223,
        WM_MDINEXT = 0x0224,
        WM_MDIMAXIMIZE = 0x0225,
        WM_MDITILE = 0x0226,
        WM_MDICASCADE = 0x0227,
        WM_MDIICONARRANGE = 0x0228,
        WM_MDIGETACTIVE = 0x0229,
        WM_MDISETMENU = 0x0230,
        WM_ENTERSIZEMOVE = 0x0231,
        WM_EXITSIZEMOVE = 0x0232,
        WM_DROPFILES = 0x0233,
        WM_MDIREFRESHMENU = 0x0234,
        WM_IME_REPORT = 0x0280,
        WM_IME_SETCONTEXT = 0x0281,
        WM_IME_NOTIFY = 0x0282,
        WM_IME_CONTROL = 0x0283,
        WM_IME_COMPOSITIONFULL = 0x0284,
        WM_IME_SELECT = 0x0285,
        WM_IME_CHAR = 0x0286,
        WM_IME_REQUEST = 0x0288,
        WM_IMEKEYDOWN = 0x0290,
        WM_IME_KEYDOWN = 0x0290,
        WM_IMEKEYUP = 0x0291,
        WM_IME_KEYUP = 0x0291,
        WM_NCMOUSEHOVER = 0x02a0,
        WM_MOUSEHOVER = 0x02a1,
        WM_NCMOUSELEAVE = 0x02a2,
        WM_MOUSELEAVE = 0x02a3,
        WM_CUT = 0x0300,
        WM_COPY = 0x0301,
        WM_PASTE = 0x0302,
        WM_CLEAR = 0x0303,
        WM_UNDO = 0x0304,
        WM_RENDERFORMAT = 0x0305,
        WM_RENDERALLFORMATS = 0x0306,
        WM_DESTROYCLIPBOARD = 0x0307,
        WM_DRAWCLIPBOARD = 0x0308,
        WM_PAINTCLIPBOARD = 0x0309,
        WM_VSCROLLCLIPBOARD = 0x030a,
        WM_SIZECLIPBOARD = 0x030b,
        WM_ASKCBFORMATNAME = 0x030c,
        WM_CHANGECBCHAIN = 0x030d,
        WM_HSCROLLCLIPBOARD = 0x030e,
        WM_QUERYNEWPALETTE = 0x030f,
        WM_PALETTEISCHANGING = 0x0310,
        WM_PALETTECHANGED = 0x0311,
        WM_HOTKEY = 0x0312,
        WM_PRINT = 0x0317,
        WM_PRINTCLIENT = 0x0318,
        WM_APPCOMMAND = 0x0319,
        WM_HANDHELDFIRST = 0x0358,
        WM_HANDHELDLAST = 0x035f,
        WM_AFXFIRST = 0x0360,
        WM_AFXLAST = 0x037f,
        WM_PENWINFIRST = 0x0380,
        WM_RCRESULT = 0x0381,
        WM_HOOKRCRESULT = 0x0382,
        WM_GLOBALRCCHANGE = 0x0383,
        WM_PENMISCINFO = 0x0383,
        WM_SKB = 0x0384,
        WM_HEDITCTL = 0x0385,
        WM_PENCTL = 0x0385,
        WM_PENMISC = 0x0386,
        WM_CTLINIT = 0x0387,
        WM_PENEVENT = 0x0388,
        WM_PENWINLAST = 0x038f,
        DDM_SETFMT = 0x0400,
        DM_GETDEFID = 0x0400,
        NIN_SELECT = 0x0400,
        TBM_GETPOS = 0x0400,
        WM_PSD_PAGESETUPDLG = 0x0400,
        WM_USER = 0x0400,
        CBEM_INSERTITEMA = 0x0401,
        DDM_DRAW = 0x0401,
        DM_SETDEFID = 0x0401,
        HKM_SETHOTKEY = 0x0401,
        PBM_SETRANGE = 0x0401,
        RB_INSERTBANDA = 0x0401,
        SB_SETTEXTA = 0x0401,
        TB_ENABLEBUTTON = 0x0401,
        TBM_GETRANGEMIN = 0x0401,
        TTM_ACTIVATE = 0x0401,
        WM_CHOOSEFONT_GETLOGFONT = 0x0401,
        WM_PSD_FULLPAGERECT = 0x0401,
        CBEM_SETIMAGELIST = 0x0402,
        DDM_CLOSE = 0x0402,
        DM_REPOSITION = 0x0402,
        HKM_GETHOTKEY = 0x0402,
        PBM_SETPOS = 0x0402,
        RB_DELETEBAND = 0x0402,
        SB_GETTEXTA = 0x0402,
        TB_CHECKBUTTON = 0x0402,
        TBM_GETRANGEMAX = 0x0402,
        WM_PSD_MINMARGINRECT = 0x0402,
        CBEM_GETIMAGELIST = 0x0403,
        DDM_BEGIN = 0x0403,
        HKM_SETRULES = 0x0403,
        PBM_DELTAPOS = 0x0403,
        RB_GETBARINFO = 0x0403,
        SB_GETTEXTLENGTHA = 0x0403,
        TBM_GETTIC = 0x0403,
        TB_PRESSBUTTON = 0x0403,
        TTM_SETDELAYTIME = 0x0403,
        WM_PSD_MARGINRECT = 0x0403,
        CBEM_GETITEMA = 0x0404,
        DDM_END = 0x0404,
        PBM_SETSTEP = 0x0404,
        RB_SETBARINFO = 0x0404,
        SB_SETPARTS = 0x0404,
        TB_HIDEBUTTON = 0x0404,
        TBM_SETTIC = 0x0404,
        TTM_ADDTOOLA = 0x0404,
        WM_PSD_GREEKTEXTRECT = 0x0404,
        CBEM_SETITEMA = 0x0405,
        PBM_STEPIT = 0x0405,
        TB_INDETERMINATE = 0x0405,
        TBM_SETPOS = 0x0405,
        TTM_DELTOOLA = 0x0405,
        WM_PSD_ENVSTAMPRECT = 0x0405,
        CBEM_GETCOMBOCONTROL = 0x0406,
        PBM_SETRANGE32 = 0x0406,
        RB_SETBANDINFOA = 0x0406,
        SB_GETPARTS = 0x0406,
        TB_MARKBUTTON = 0x0406,
        TBM_SETRANGE = 0x0406,
        TTM_NEWTOOLRECTA = 0x0406,
        WM_PSD_YAFULLPAGERECT = 0x0406,
        CBEM_GETEDITCONTROL = 0x0407,
        PBM_GETRANGE = 0x0407,
        RB_SETPARENT = 0x0407,
        SB_GETBORDERS = 0x0407,
        TBM_SETRANGEMIN = 0x0407,
        TTM_RELAYEVENT = 0x0407,
        CBEM_SETEXSTYLE = 0x0408,
        PBM_GETPOS = 0x0408,
        RB_HITTEST = 0x0408,
        SB_SETMINHEIGHT = 0x0408,
        TBM_SETRANGEMAX = 0x0408,
        TTM_GETTOOLINFOA = 0x0408,
        CBEM_GETEXSTYLE = 0x0409,
        CBEM_GETEXTENDEDSTYLE = 0x0409,
        PBM_SETBARCOLOR = 0x0409,
        RB_GETRECT = 0x0409,
        SB_SIMPLE = 0x0409,
        TB_ISBUTTONENABLED = 0x0409,
        TBM_CLEARTICS = 0x0409,
        TTM_SETTOOLINFOA = 0x0409,
        CBEM_HASEDITCHANGED = 0x040a,
        RB_INSERTBANDW = 0x040a,
        SB_GETRECT = 0x040a,
        TB_ISBUTTONCHECKED = 0x040a,
        TBM_SETSEL = 0x040a,
        TTM_HITTESTA = 0x040a,
        WIZ_QUERYNUMPAGES = 0x040a,
        CBEM_INSERTITEMW = 0x040b,
        RB_SETBANDINFOW = 0x040b,
        SB_SETTEXTW = 0x040b,
        TB_ISBUTTONPRESSED = 0x040b,
        TBM_SETSELSTART = 0x040b,
        TTM_GETTEXTA = 0x040b,
        WIZ_NEXT = 0x040b,
        CBEM_SETITEMW = 0x040c,
        RB_GETBANDCOUNT = 0x040c,
        SB_GETTEXTLENGTHW = 0x040c,
        TB_ISBUTTONHIDDEN = 0x040c,
        TBM_SETSELEND = 0x040c,
        TTM_UPDATETIPTEXTA = 0x040c,
        WIZ_PREV = 0x040c,
        CBEM_GETITEMW = 0x040d,
        RB_GETROWCOUNT = 0x040d,
        SB_GETTEXTW = 0x040d,
        TB_ISBUTTONINDETERMINATE = 0x040d,
        TTM_GETTOOLCOUNT = 0x040d,
        CBEM_SETEXTENDEDSTYLE = 0x040e,
        RB_GETROWHEIGHT = 0x040e,
        SB_ISSIMPLE = 0x040e,
        TB_ISBUTTONHIGHLIGHTED = 0x040e,
        TBM_GETPTICS = 0x040e,
        TTM_ENUMTOOLSA = 0x040e,
        SB_SETICON = 0x040f,
        TBM_GETTICPOS = 0x040f,
        TTM_GETCURRENTTOOLA = 0x040f,
        RB_IDTOINDEX = 0x0410,
        SB_SETTIPTEXTA = 0x0410,
        TBM_GETNUMTICS = 0x0410,
        TTM_WINDOWFROMPOINT = 0x0410,
        RB_GETTOOLTIPS = 0x0411,
        SB_SETTIPTEXTW = 0x0411,
        TBM_GETSELSTART = 0x0411,
        TB_SETSTATE = 0x0411,
        TTM_TRACKACTIVATE = 0x0411,
        RB_SETTOOLTIPS = 0x0412,
        SB_GETTIPTEXTA = 0x0412,
        TB_GETSTATE = 0x0412,
        TBM_GETSELEND = 0x0412,
        TTM_TRACKPOSITION = 0x0412,
        RB_SETBKCOLOR = 0x0413,
        SB_GETTIPTEXTW = 0x0413,
        TB_ADDBITMAP = 0x0413,
        TBM_CLEARSEL = 0x0413,
        TTM_SETTIPBKCOLOR = 0x0413,
        RB_GETBKCOLOR = 0x0414,
        SB_GETICON = 0x0414,
        TB_ADDBUTTONSA = 0x0414,
        TBM_SETTICFREQ = 0x0414,
        TTM_SETTIPTEXTCOLOR = 0x0414,
        RB_SETTEXTCOLOR = 0x0415,
        TB_INSERTBUTTONA = 0x0415,
        TBM_SETPAGESIZE = 0x0415,
        TTM_GETDELAYTIME = 0x0415,
        RB_GETTEXTCOLOR = 0x0416,
        TB_DELETEBUTTON = 0x0416,
        TBM_GETPAGESIZE = 0x0416,
        TTM_GETTIPBKCOLOR = 0x0416,
        RB_SIZETORECT = 0x0417,
        TB_GETBUTTON = 0x0417,
        TBM_SETLINESIZE = 0x0417,
        TTM_GETTIPTEXTCOLOR = 0x0417,
        RB_BEGINDRAG = 0x0418,
        TB_BUTTONCOUNT = 0x0418,
        TBM_GETLINESIZE = 0x0418,
        TTM_SETMAXTIPWIDTH = 0x0418,
        RB_ENDDRAG = 0x0419,
        TB_COMMANDTOINDEX = 0x0419,
        TBM_GETTHUMBRECT = 0x0419,
        TTM_GETMAXTIPWIDTH = 0x0419,
        RB_DRAGMOVE = 0x041a,
        TBM_GETCHANNELRECT = 0x041a,
        TB_SAVERESTOREA = 0x041a,
        TTM_SETMARGIN = 0x041a,
        RB_GETBARHEIGHT = 0x041b,
        TB_CUSTOMIZE = 0x041b,
        TBM_SETTHUMBLENGTH = 0x041b,
        TTM_GETMARGIN = 0x041b,
        RB_GETBANDINFOW = 0x041c,
        TB_ADDSTRINGA = 0x041c,
        TBM_GETTHUMBLENGTH = 0x041c,
        TTM_POP = 0x041c,
        RB_GETBANDINFOA = 0x041d,
        TB_GETITEMRECT = 0x041d,
        TBM_SETTOOLTIPS = 0x041d,
        TTM_UPDATE = 0x041d,
        RB_MINIMIZEBAND = 0x041e,
        TB_BUTTONSTRUCTSIZE = 0x041e,
        TBM_GETTOOLTIPS = 0x041e,
        TTM_GETBUBBLESIZE = 0x041e,
        RB_MAXIMIZEBAND = 0x041f,
        TBM_SETTIPSIDE = 0x041f,
        TB_SETBUTTONSIZE = 0x041f,
        TTM_ADJUSTRECT = 0x041f,
        TBM_SETBUDDY = 0x0420,
        TB_SETBITMAPSIZE = 0x0420,
        TTM_SETTITLEA = 0x0420,
        MSG_FTS_JUMP_VA = 0x0421,
        TB_AUTOSIZE = 0x0421,
        TBM_GETBUDDY = 0x0421,
        TTM_SETTITLEW = 0x0421,
        RB_GETBANDBORDERS = 0x0422,
        MSG_FTS_JUMP_QWORD = 0x0423,
        RB_SHOWBAND = 0x0423,
        TB_GETTOOLTIPS = 0x0423,
        MSG_REINDEX_REQUEST = 0x0424,
        TB_SETTOOLTIPS = 0x0424,
        MSG_FTS_WHERE_IS_IT = 0x0425,
        RB_SETPALETTE = 0x0425,
        TB_SETPARENT = 0x0425,
        RB_GETPALETTE = 0x0426,
        RB_MOVEBAND = 0x0427,
        TB_SETROWS = 0x0427,
        TB_GETROWS = 0x0428,
        TB_GETBITMAPFLAGS = 0x0429,
        TB_SETCMDID = 0x042a,
        RB_PUSHCHEVRON = 0x042b,
        TB_CHANGEBITMAP = 0x042b,
        TB_GETBITMAP = 0x042c,
        MSG_GET_DEFFONT = 0x042d,
        TB_GETBUTTONTEXTA = 0x042d,
        TB_REPLACEBITMAP = 0x042e,
        TB_SETINDENT = 0x042f,
        TB_SETIMAGELIST = 0x0430,
        TB_GETIMAGELIST = 0x0431,
        TB_LOADIMAGES = 0x0432,
        TTM_ADDTOOLW = 0x0432,
        TB_GETRECT = 0x0433,
        TTM_DELTOOLW = 0x0433,
        TB_SETHOTIMAGELIST = 0x0434,
        TTM_NEWTOOLRECTW = 0x0434,
        TB_GETHOTIMAGELIST = 0x0435,
        TTM_GETTOOLINFOW = 0x0435,
        TB_SETDISABLEDIMAGELIST = 0x0436,
        TTM_SETTOOLINFOW = 0x0436,
        TB_GETDISABLEDIMAGELIST = 0x0437,
        TTM_HITTESTW = 0x0437,
        TB_SETSTYLE = 0x0438,
        TTM_GETTEXTW = 0x0438,
        TB_GETSTYLE = 0x0439,
        TTM_UPDATETIPTEXTW = 0x0439,
        TB_GETBUTTONSIZE = 0x043a,
        TTM_ENUMTOOLSW = 0x043a,
        TB_SETBUTTONWIDTH = 0x043b,
        TTM_GETCURRENTTOOLW = 0x043b,
        TB_SETMAXTEXTROWS = 0x043c,
        TB_GETTEXTROWS = 0x043d,
        TB_GETOBJECT = 0x043e,
        TB_GETBUTTONINFOW = 0x043f,
        TB_SETBUTTONINFOW = 0x0440,
        TB_GETBUTTONINFOA = 0x0441,
        TB_SETBUTTONINFOA = 0x0442,
        TB_INSERTBUTTONW = 0x0443,
        TB_ADDBUTTONSW = 0x0444,
        TB_HITTEST = 0x0445,
        TB_SETDRAWTEXTFLAGS = 0x0446,
        TB_GETHOTITEM = 0x0447,
        TB_SETHOTITEM = 0x0448,
        TB_SETANCHORHIGHLIGHT = 0x0449,
        TB_GETANCHORHIGHLIGHT = 0x044a,
        TB_GETBUTTONTEXTW = 0x044b,
        TB_SAVERESTOREW = 0x044c,
        TB_ADDSTRINGW = 0x044d,
        TB_MAPACCELERATORA = 0x044e,
        TB_GETINSERTMARK = 0x044f,
        TB_SETINSERTMARK = 0x0450,
        TB_INSERTMARKHITTEST = 0x0451,
        TB_MOVEBUTTON = 0x0452,
        TB_GETMAXSIZE = 0x0453,
        TB_SETEXTENDEDSTYLE = 0x0454,
        TB_GETEXTENDEDSTYLE = 0x0455,
        TB_GETPADDING = 0x0456,
        TB_SETPADDING = 0x0457,
        TB_SETINSERTMARKCOLOR = 0x0458,
        TB_GETINSERTMARKCOLOR = 0x0459,
        TB_MAPACCELERATORW = 0x045a,
        TB_GETSTRINGW = 0x045b,
        TB_GETSTRINGA = 0x045c,
        TAPI_REPLY = 0x0463,
        ACM_OPENA = 0x0464,
        BFFM_SETSTATUSTEXTA = 0x0464,
        CDM_FIRST = 0x0464,
        CDM_GETSPEC = 0x0464,
        IPM_CLEARADDRESS = 0x0464,
        WM_CAP_UNICODE_START = 0x0464,
        ACM_PLAY = 0x0465,
        BFFM_ENABLEOK = 0x0465,
        CDM_GETFILEPATH = 0x0465,
        IPM_SETADDRESS = 0x0465,
        PSM_SETCURSEL = 0x0465,
        UDM_SETRANGE = 0x0465,
        WM_CHOOSEFONT_SETLOGFONT = 0x0465,
        ACM_STOP = 0x0466,
        BFFM_SETSELECTIONA = 0x0466,
        CDM_GETFOLDERPATH = 0x0466,
        IPM_GETADDRESS = 0x0466,
        PSM_REMOVEPAGE = 0x0466,
        UDM_GETRANGE = 0x0466,
        WM_CAP_SET_CALLBACK_ERRORW = 0x0466,
        WM_CHOOSEFONT_SETFLAGS = 0x0466,
        ACM_OPENW = 0x0467,
        BFFM_SETSELECTIONW = 0x0467,
        CDM_GETFOLDERIDLIST = 0x0467,
        IPM_SETRANGE = 0x0467,
        PSM_ADDPAGE = 0x0467,
        UDM_SETPOS = 0x0467,
        WM_CAP_SET_CALLBACK_STATUSW = 0x0467,
        BFFM_SETSTATUSTEXTW = 0x0468,
        CDM_SETCONTROLTEXT = 0x0468,
        IPM_SETFOCUS = 0x0468,
        PSM_CHANGED = 0x0468,
        UDM_GETPOS = 0x0468,
        CDM_HIDECONTROL = 0x0469,
        IPM_ISBLANK = 0x0469,
        PSM_RESTARTWINDOWS = 0x0469,
        UDM_SETBUDDY = 0x0469,
        CDM_SETDEFEXT = 0x046a,
        PSM_REBOOTSYSTEM = 0x046a,
        UDM_GETBUDDY = 0x046a,
        PSM_CANCELTOCLOSE = 0x046b,
        UDM_SETACCEL = 0x046b,
        EM_CONVPOSITION = 0x046c,
        PSM_QUERYSIBLINGS = 0x046c,
        UDM_GETACCEL = 0x046c,
        MCIWNDM_GETZOOM = 0x046d,
        PSM_UNCHANGED = 0x046d,
        UDM_SETBASE = 0x046d,
        PSM_APPLY = 0x046e,
        UDM_GETBASE = 0x046e,
        PSM_SETTITLEA = 0x046f,
        UDM_SETRANGE32 = 0x046f,
        PSM_SETWIZBUTTONS = 0x0470,
        UDM_GETRANGE32 = 0x0470,
        WM_CAP_DRIVER_GET_NAMEW = 0x0470,
        PSM_PRESSBUTTON = 0x0471,
        UDM_SETPOS32 = 0x0471,
        WM_CAP_DRIVER_GET_VERSIONW = 0x0471,
        PSM_SETCURSELID = 0x0472,
        UDM_GETPOS32 = 0x0472,
        PSM_SETFINISHTEXTA = 0x0473,
        PSM_GETTABCONTROL = 0x0474,
        PSM_ISDIALOGMESSAGE = 0x0475,
        MCIWNDM_REALIZE = 0x0476,
        PSM_GETCURRENTPAGEHWND = 0x0476,
        MCIWNDM_SETTIMEFORMATA = 0x0477,
        PSM_INSERTPAGE = 0x0477,
        MCIWNDM_GETTIMEFORMATA = 0x0478,
        PSM_SETTITLEW = 0x0478,
        WM_CAP_FILE_SET_CAPTURE_FILEW = 0x0478,
        MCIWNDM_VALIDATEMEDIA = 0x0479,
        PSM_SETFINISHTEXTW = 0x0479,
        WM_CAP_FILE_GET_CAPTURE_FILEW = 0x0479,
        MCIWNDM_PLAYTO = 0x047b,
        WM_CAP_FILE_SAVEASW = 0x047b,
        MCIWNDM_GETFILENAMEA = 0x047c,
        MCIWNDM_GETDEVICEA = 0x047d,
        PSM_SETHEADERTITLEA = 0x047d,
        WM_CAP_FILE_SAVEDIBW = 0x047d,
        MCIWNDM_GETPALETTE = 0x047e,
        PSM_SETHEADERTITLEW = 0x047e,
        MCIWNDM_SETPALETTE = 0x047f,
        PSM_SETHEADERSUBTITLEA = 0x047f,
        MCIWNDM_GETERRORA = 0x0480,
        PSM_SETHEADERSUBTITLEW = 0x0480,
        PSM_HWNDTOINDEX = 0x0481,
        PSM_INDEXTOHWND = 0x0482,
        MCIWNDM_SETINACTIVETIMER = 0x0483,
        PSM_PAGETOINDEX = 0x0483,
        PSM_INDEXTOPAGE = 0x0484,
        DL_BEGINDRAG = 0x0485,
        MCIWNDM_GETINACTIVETIMER = 0x0485,
        PSM_IDTOINDEX = 0x0485,
        DL_DRAGGING = 0x0486,
        PSM_INDEXTOID = 0x0486,
        DL_DROPPED = 0x0487,
        PSM_GETRESULT = 0x0487,
        DL_CANCELDRAG = 0x0488,
        PSM_RECALCPAGESIZES = 0x0488,
        MCIWNDM_GET_SOURCE = 0x048c,
        MCIWNDM_PUT_SOURCE = 0x048d,
        MCIWNDM_GET_DEST = 0x048e,
        MCIWNDM_PUT_DEST = 0x048f,
        MCIWNDM_CAN_PLAY = 0x0490,
        MCIWNDM_CAN_WINDOW = 0x0491,
        MCIWNDM_CAN_RECORD = 0x0492,
        MCIWNDM_CAN_SAVE = 0x0493,
        MCIWNDM_CAN_EJECT = 0x0494,
        MCIWNDM_CAN_CONFIG = 0x0495,
        IE_GETINK = 0x0496,
        IE_MSGFIRST = 0x0496,
        MCIWNDM_PALETTEKICK = 0x0496,
        IE_SETINK = 0x0497,
        IE_GETPENTIP = 0x0498,
        IE_SETPENTIP = 0x0499,
        IE_GETERASERTIP = 0x049a,
        IE_SETERASERTIP = 0x049b,
        IE_GETBKGND = 0x049c,
        IE_SETBKGND = 0x049d,
        IE_GETGRIDORIGIN = 0x049e,
        IE_SETGRIDORIGIN = 0x049f,
        IE_GETGRIDPEN = 0x04a0,
        IE_SETGRIDPEN = 0x04a1,
        IE_GETGRIDSIZE = 0x04a2,
        IE_SETGRIDSIZE = 0x04a3,
        IE_GETMODE = 0x04a4,
        IE_SETMODE = 0x04a5,
        IE_GETINKRECT = 0x04a6,
        WM_CAP_SET_MCI_DEVICEW = 0x04a6,
        WM_CAP_GET_MCI_DEVICEW = 0x04a7,
        WM_CAP_PAL_OPENW = 0x04b4,
        WM_CAP_PAL_SAVEW = 0x04b5,
        IE_GETAPPDATA = 0x04b8,
        IE_SETAPPDATA = 0x04b9,
        IE_GETDRAWOPTS = 0x04ba,
        IE_SETDRAWOPTS = 0x04bb,
        IE_GETFORMAT = 0x04bc,
        IE_SETFORMAT = 0x04bd,
        IE_GETINKINPUT = 0x04be,
        IE_SETINKINPUT = 0x04bf,
        IE_GETNOTIFY = 0x04c0,
        IE_SETNOTIFY = 0x04c1,
        IE_GETRECOG = 0x04c2,
        IE_SETRECOG = 0x04c3,
        IE_GETSECURITY = 0x04c4,
        IE_SETSECURITY = 0x04c5,
        IE_GETSEL = 0x04c6,
        IE_SETSEL = 0x04c7,
        CDM_LAST = 0x04c8,
        IE_DOCOMMAND = 0x04c8,
        MCIWNDM_NOTIFYMODE = 0x04c8,
        IE_GETCOMMAND = 0x04c9,
        IE_GETCOUNT = 0x04ca,
        IE_GETGESTURE = 0x04cb,
        MCIWNDM_NOTIFYMEDIA = 0x04cb,
        IE_GETMENU = 0x04cc,
        IE_GETPAINTDC = 0x04cd,
        MCIWNDM_NOTIFYERROR = 0x04cd,
        IE_GETPDEVENT = 0x04ce,
        IE_GETSELCOUNT = 0x04cf,
        IE_GETSELITEMS = 0x04d0,
        IE_GETSTYLE = 0x04d1,
        MCIWNDM_SETTIMEFORMATW = 0x04db,
        EM_OUTLINE = 0x04dc,
        MCIWNDM_GETTIMEFORMATW = 0x04dc,
        EM_GETSCROLLPOS = 0x04dd,
        EM_SETSCROLLPOS = 0x04de,
        EM_SETFONTSIZE = 0x04df,
        MCIWNDM_GETFILENAMEW = 0x04e0,
        MCIWNDM_GETDEVICEW = 0x04e1,
        MCIWNDM_GETERRORW = 0x04e4,
        FM_GETFOCUS = 0x0600,
        FM_GETDRIVEINFOA = 0x0601,
        FM_GETSELCOUNT = 0x0602,
        FM_GETSELCOUNTLFN = 0x0603,
        FM_GETFILESELA = 0x0604,
        FM_GETFILESELLFNA = 0x0605,
        FM_REFRESH_WINDOWS = 0x0606,
        FM_RELOAD_EXTENSIONS = 0x0607,
        FM_GETDRIVEINFOW = 0x0611,
        FM_GETFILESELW = 0x0614,
        FM_GETFILESELLFNW = 0x0615,
        WLX_WM_SAS = 0x0659,
        SM_GETSELCOUNT = 0x07e8,
        UM_GETSELCOUNT = 0x07e8,
        WM_CPL_LAUNCH = 0x07e8,
        SM_GETSERVERSELA = 0x07e9,
        UM_GETUSERSELA = 0x07e9,
        WM_CPL_LAUNCHED = 0x07e9,
        SM_GETSERVERSELW = 0x07ea,
        UM_GETUSERSELW = 0x07ea,
        SM_GETCURFOCUSA = 0x07eb,
        UM_GETGROUPSELA = 0x07eb,
        SM_GETCURFOCUSW = 0x07ec,
        UM_GETGROUPSELW = 0x07ec,
        SM_GETOPTIONS = 0x07ed,
        UM_GETCURFOCUSA = 0x07ed,
        UM_GETCURFOCUSW = 0x07ee,
        UM_GETOPTIONS = 0x07ef,
        UM_GETOPTIONS2 = 0x07f0,
        OCMBASE = 0x2000,
        OCM_CTLCOLOR = 0x2019,
        OCM_DRAWITEM = 0x202b,
        OCM_MEASUREITEM = 0x202c,
        OCM_DELETEITEM = 0x202d,
        OCM_VKEYTOITEM = 0x202e,
        OCM_CHARTOITEM = 0x202f,
        OCM_COMPAREITEM = 0x2039,
        OCM_NOTIFY = 0x204e,
        OCM_COMMAND = 0x2111,
        OCM_HSCROLL = 0x2114,
        OCM_VSCROLL = 0x2115,
        OCM_CTLCOLORMSGBOX = 0x2132,
        OCM_CTLCOLOREDIT = 0x2133,
        OCM_CTLCOLORLISTBOX = 0x2134,
        OCM_CTLCOLORBTN = 0x2135,
        OCM_CTLCOLORDLG = 0x2136,
        OCM_CTLCOLORSCROLLBAR = 0x2137,
        OCM_CTLCOLORSTATIC = 0x2138,
        OCM_PARENTNOTIFY = 0x2210,
        WM_APP = 0x8000,
        WM_RASDIALEVENT = 0xcccd
    }

    public enum MouseButtonWParams : uint
    {
        MK_CONTROL = 0x0008,  //The CTRL key is down.
        MK_LBUTTON = 0x0001, //The left mouse button is down.
        MK_MBUTTON = 0x0010, //The middle mouse button is down.
        MK_RBUTTON = 0x0002, //The right mouse button is down.
        MK_SHIFT = 0x0004,   //The SHIFT key is down.
        MK_XBUTTON1 = 0x0020,//The first X button is down.
        MK_XBUTTON2 = 0x0040 //The second X button is down.
    }

    /// <summary>
    /// From https://msdn.microsoft.com/en-us/library/windows/desktop/aa372716(v=vs.85).aspx
    /// </summary>
    [Flags]
    public enum WindowMessageParameter : uint
    {
        PBT_APMQUERYSUSPEND = 0x0,
        PBT_APMBATTERYLOW = 0x9, // Notifies applications that the battery power is low.
        PBT_APMOEMEVENT = 0xb, // Notifies applications that the APM BIOS has signalled  an APM OEM event.
        PBT_APMQUERYSTANDBY = 0x0001, // 
        PBT_APMPOWERSTATUSCHANGE = 0xa, // Notifies applications of a change in the power status of the computer, such as a switch from battery power to A/C. The system also broadcasts this event when remaining battery power slips below the threshold specified by the user or if the battery power changes by a specified percentage.
        PBT_APMQUERYSUSPENDFAILED = 0x218, // Notifies applications that permission to suspend the computer was denied.
        PBT_APMRESUMEAUTOMATIC = 0x12, // Notifies applications that the system is resuming from sleep or hibernation. If the system detects any user activity after broadcasting PBT_APMRESUMEAUTOMATIC, it will broadcast a PBT_APMRESUMESUSPEND event to let applications know they can resume full interaction with the user.
        PBT_APMRESUMECRITICAL = 0x6, // Notifies applications that the system has resumed operation.
        PBT_APMRESUMESUSPEND = 0x7, // Notifies applications that the system has resumed operation after being suspended.
        PBT_APMSUSPEND = 0x4, // Notifies applications that the computer is about to enter a suspended state. 
        PBT_POWERSETTINGCHANGE = 0x8013, // Notifies applications that a power setting change event occurred.
        WM_POWER = 0x48, // Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.
        WM_POWERBROADCAST = 0x218, // Notifies applications that a power-management event has occurred.
        BROADCAST_QUERY_DENY = 0x424D5144 //
    }

    public enum ShellEvents : int
    {
        HSHELL_WINDOWCREATED = 1,
        HSHELL_WINDOWDESTROYED = 2,
        HSHELL_ACTIVATESHELLWINDOW = 3,
        HSHELL_WINDOWACTIVATED = 4,
        HSHELL_GETMINRECT = 5,
        HSHELL_REDRAW = 6,
        HSHELL_TASKMAN = 7,
        HSHELL_LANGUAGE = 8,
        HSHELL_ACCESSIBILITYSTATE = 11,
        HSHELL_APPCOMMAND = 12
    }

    public enum ProcessSecurityAndAccessRights : uint
    {
        PROCESS_ALL_ACCESS = 0x001F0FF, //All possible access rights for a process object.
        PROCESS_CREATE_PROCESS = 0x0080,	//Required to create a process.
        PROCESS_CREATE_THREAD = 0x0002,	//Required to create a thread.
        PROCESS_DUP_HANDLE = 0x0040,	//Required to duplicate a handle using DuplicateHandle.
        PROCESS_QUERY_INFORMATION = 0x0400,	//Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken).
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,	//Required to retrieve certain information about a process (see GetExitCodeProcess, GetPriorityClass, IsProcessInJob, QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP:  This access right is not supported.
        PROCESS_SET_INFORMATION = 0x0200,	//Required to set certain information about a process, such as its priority class (see SetPriorityClass).
        PROCESS_SET_QUOTA = 0x0100,	//Required to set memory limits using SetProcessWorkingSetSize.
        PROCESS_SUSPEND_RESUME = 0x0800,	//Required to suspend or resume a process.
        PROCESS_TERMINATE = 0x0001,	//Required to terminate a process using TerminateProcess.
        PROCESS_VM_OPERATION = 0x0008,	//Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
        PROCESS_VM_READ = 0x0010,	//Required to read memory in a process using ReadProcessMemory.
        PROCESS_VM_WRITE = 0x0020,	//Required to write to memory in a process using WriteProcessMemory.
    }

    #region WindowsVirtualKey
    public enum WindowsVirtualKey
    {
        [Description("Left mouse button")]
        VK_LBUTTON = 0x01,

        [Description("Right mouse button")]
        VK_RBUTTON = 0x02,

        [Description("Control-break processing")]
        VK_CANCEL = 0x03,

        [Description("Middle mouse button (three-button mouse)")]
        VK_MBUTTON = 0x04,

        [Description("X1 mouse button")]
        VK_XBUTTON1 = 0x05,

        [Description("X2 mouse button")]
        VK_XBUTTON2 = 0x06,

        [Description("BACKSPACE key")]
        VK_BACK = 0x08,

        [Description("TAB key")]
        VK_TAB = 0x09,

        [Description("CLEAR key")]
        VK_CLEAR = 0x0C,

        [Description("ENTER key")]
        VK_RETURN = 0x0D,

        [Description("SHIFT key")]
        VK_SHIFT = 0x10,

        [Description("CTRL key")]
        VK_CONTROL = 0x11,

        [Description("ALT key")]
        VK_MENU = 0x12,

        [Description("PAUSE key")]
        VK_PAUSE = 0x13,

        [Description("CAPS LOCK key")]
        VK_CAPITAL = 0x14,

        [Description("IME Kana mode")]
        VK_KANA = 0x15,

        [Description("IME Hanguel mode (maintained for compatibility; use VK_HANGUL)")]
        VK_HANGUEL = 0x15,

        [Description("IME Hangul mode")]
        VK_HANGUL = 0x15,

        [Description("IME Junja mode")]
        VK_JUNJA = 0x17,

        [Description("IME final mode")]
        VK_FINAL = 0x18,

        [Description("IME Hanja mode")]
        VK_HANJA = 0x19,

        [Description("IME Kanji mode")]
        VK_KANJI = 0x19,

        [Description("ESC key")]
        VK_ESCAPE = 0x1B,

        [Description("IME convert")]
        VK_CONVERT = 0x1C,

        [Description("IME nonconvert")]
        VK_NONCONVERT = 0x1D,

        [Description("IME accept")]
        VK_ACCEPT = 0x1E,

        [Description("IME mode change request")]
        VK_MODECHANGE = 0x1F,

        [Description("SPACEBAR")]
        VK_SPACE = 0x20,

        [Description("PAGE UP key")]
        VK_PRIOR = 0x21,

        [Description("PAGE DOWN key")]
        VK_NEXT = 0x22,

        [Description("END key")]
        VK_END = 0x23,

        [Description("HOME key")]
        VK_HOME = 0x24,

        [Description("LEFT ARROW key")]
        VK_LEFT = 0x25,

        [Description("UP ARROW key")]
        VK_UP = 0x26,

        [Description("RIGHT ARROW key")]
        VK_RIGHT = 0x27,

        [Description("DOWN ARROW key")]
        VK_DOWN = 0x28,

        [Description("SELECT key")]
        VK_SELECT = 0x29,

        [Description("PRINT key")]
        VK_PRINT = 0x2A,

        [Description("EXECUTE key")]
        VK_EXECUTE = 0x2B,

        [Description("PRINT SCREEN key")]
        VK_SNAPSHOT = 0x2C,

        [Description("INS key")]
        VK_INSERT = 0x2D,

        [Description("DEL key")]
        VK_DELETE = 0x2E,

        [Description("HELP key")]
        VK_HELP = 0x2F,

        [Description("0 key")]
        K_0 = 0x30,

        [Description("1 key")]
        K_1 = 0x31,

        [Description("2 key")]
        K_2 = 0x32,

        [Description("3 key")]
        K_3 = 0x33,

        [Description("4 key")]
        K_4 = 0x34,

        [Description("5 key")]
        K_5 = 0x35,

        [Description("6 key")]
        K_6 = 0x36,

        [Description("7 key")]
        K_7 = 0x37,

        [Description("8 key")]
        K_8 = 0x38,

        [Description("9 key")]
        K_9 = 0x39,

        [Description("A key")]
        K_A = 0x41,

        [Description("B key")]
        K_B = 0x42,

        [Description("C key")]
        K_C = 0x43,

        [Description("D key")]
        K_D = 0x44,

        [Description("E key")]
        K_E = 0x45,

        [Description("F key")]
        K_F = 0x46,

        [Description("G key")]
        K_G = 0x47,

        [Description("H key")]
        K_H = 0x48,

        [Description("I key")]
        K_I = 0x49,

        [Description("J key")]
        K_J = 0x4A,

        [Description("K key")]
        K_K = 0x4B,

        [Description("L key")]
        K_L = 0x4C,

        [Description("M key")]
        K_M = 0x4D,

        [Description("N key")]
        K_N = 0x4E,

        [Description("O key")]
        K_O = 0x4F,

        [Description("P key")]
        K_P = 0x50,

        [Description("Q key")]
        K_Q = 0x51,

        [Description("R key")]
        K_R = 0x52,

        [Description("S key")]
        K_S = 0x53,

        [Description("T key")]
        K_T = 0x54,

        [Description("U key")]
        K_U = 0x55,

        [Description("V key")]
        K_V = 0x56,

        [Description("W key")]
        K_W = 0x57,

        [Description("X key")]
        K_X = 0x58,

        [Description("Y key")]
        K_Y = 0x59,

        [Description("Z key")]
        K_Z = 0x5A,

        [Description("Left Windows key (Natural keyboard)")]
        VK_LWIN = 0x5B,

        [Description("Right Windows key (Natural keyboard)")]
        VK_RWIN = 0x5C,

        [Description("Applications key (Natural keyboard)")]
        VK_APPS = 0x5D,

        [Description("Computer Sleep key")]
        VK_SLEEP = 0x5F,

        [Description("Numeric keypad 0 key")]
        VK_NUMPAD0 = 0x60,

        [Description("Numeric keypad 1 key")]
        VK_NUMPAD1 = 0x61,

        [Description("Numeric keypad 2 key")]
        VK_NUMPAD2 = 0x62,

        [Description("Numeric keypad 3 key")]
        VK_NUMPAD3 = 0x63,

        [Description("Numeric keypad 4 key")]
        VK_NUMPAD4 = 0x64,

        [Description("Numeric keypad 5 key")]
        VK_NUMPAD5 = 0x65,

        [Description("Numeric keypad 6 key")]
        VK_NUMPAD6 = 0x66,

        [Description("Numeric keypad 7 key")]
        VK_NUMPAD7 = 0x67,

        [Description("Numeric keypad 8 key")]
        VK_NUMPAD8 = 0x68,

        [Description("Numeric keypad 9 key")]
        VK_NUMPAD9 = 0x69,

        [Description("Multiply key")]
        VK_MULTIPLY = 0x6A,

        [Description("Add key")]
        VK_ADD = 0x6B,

        [Description("Separator key")]
        VK_SEPARATOR = 0x6C,

        [Description("Subtract key")]
        VK_SUBTRACT = 0x6D,

        [Description("Decimal key")]
        VK_DECIMAL = 0x6E,

        [Description("Divide key")]
        VK_DIVIDE = 0x6F,

        [Description("F1 key")]
        VK_F1 = 0x70,

        [Description("F2 key")]
        VK_F2 = 0x71,

        [Description("F3 key")]
        VK_F3 = 0x72,

        [Description("F4 key")]
        VK_F4 = 0x73,

        [Description("F5 key")]
        VK_F5 = 0x74,

        [Description("F6 key")]
        VK_F6 = 0x75,

        [Description("F7 key")]
        VK_F7 = 0x76,

        [Description("F8 key")]
        VK_F8 = 0x77,

        [Description("F9 key")]
        VK_F9 = 0x78,

        [Description("F10 key")]
        VK_F10 = 0x79,

        [Description("F11 key")]
        VK_F11 = 0x7A,

        [Description("F12 key")]
        VK_F12 = 0x7B,

        [Description("F13 key")]
        VK_F13 = 0x7C,

        [Description("F14 key")]
        VK_F14 = 0x7D,

        [Description("F15 key")]
        VK_F15 = 0x7E,

        [Description("F16 key")]
        VK_F16 = 0x7F,

        [Description("F17 key")]
        VK_F17 = 0x80,

        [Description("F18 key")]
        VK_F18 = 0x81,

        [Description("F19 key")]
        VK_F19 = 0x82,

        [Description("F20 key")]
        VK_F20 = 0x83,

        [Description("F21 key")]
        VK_F21 = 0x84,

        [Description("F22 key")]
        VK_F22 = 0x85,

        [Description("F23 key")]
        VK_F23 = 0x86,

        [Description("F24 key")]
        VK_F24 = 0x87,

        [Description("NUM LOCK key")]
        VK_NUMLOCK = 0x90,

        [Description("SCROLL LOCK key")]
        VK_SCROLL = 0x91,

        [Description("Left SHIFT key")]
        VK_LSHIFT = 0xA0,

        [Description("Right SHIFT key")]
        VK_RSHIFT = 0xA1,

        [Description("Left CONTROL key")]
        VK_LCONTROL = 0xA2,

        [Description("Right CONTROL key")]
        VK_RCONTROL = 0xA3,

        [Description("Left MENU key")]
        VK_LMENU = 0xA4,

        [Description("Right MENU key")]
        VK_RMENU = 0xA5,

        [Description("Browser Back key")]
        VK_BROWSER_BACK = 0xA6,

        [Description("Browser Forward key")]
        VK_BROWSER_FORWARD = 0xA7,

        [Description("Browser Refresh key")]
        VK_BROWSER_REFRESH = 0xA8,

        [Description("Browser Stop key")]
        VK_BROWSER_STOP = 0xA9,

        [Description("Browser Search key")]
        VK_BROWSER_SEARCH = 0xAA,

        [Description("Browser Favorites key")]
        VK_BROWSER_FAVORITES = 0xAB,

        [Description("Browser Start and Home key")]
        VK_BROWSER_HOME = 0xAC,

        [Description("Volume Mute key")]
        VK_VOLUME_MUTE = 0xAD,

        [Description("Volume Down key")]
        VK_VOLUME_DOWN = 0xAE,

        [Description("Volume Up key")]
        VK_VOLUME_UP = 0xAF,

        [Description("Next Track key")]
        VK_MEDIA_NEXT_TRACK = 0xB0,

        [Description("Previous Track key")]
        VK_MEDIA_PREV_TRACK = 0xB1,

        [Description("Stop Media key")]
        VK_MEDIA_STOP = 0xB2,

        [Description("Play/Pause Media key")]
        VK_MEDIA_PLAY_PAUSE = 0xB3,

        [Description("Start Mail key")]
        VK_LAUNCH_MAIL = 0xB4,

        [Description("Select Media key")]
        VK_LAUNCH_MEDIA_SELECT = 0xB5,

        [Description("Start Application 1 key")]
        VK_LAUNCH_APP1 = 0xB6,

        [Description("Start Application 2 key")]
        VK_LAUNCH_APP2 = 0xB7,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ';:' key")]
        VK_OEM_1 = 0xBA,

        [Description("For any country/region, the '+' key")]
        VK_OEM_PLUS = 0xBB,

        [Description("For any country/region, the ',' key")]
        VK_OEM_COMMA = 0xBC,

        [Description("For any country/region, the '-' key")]
        VK_OEM_MINUS = 0xBD,

        [Description("For any country/region, the '.' key")]
        VK_OEM_PERIOD = 0xBE,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '/?' key")]
        VK_OEM_2 = 0xBF,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '`~' key")]
        VK_OEM_3 = 0xC0,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '[{' key")]
        VK_OEM_4 = 0xDB,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '\\|' key")]
        VK_OEM_5 = 0xDC,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ']}' key")]
        VK_OEM_6 = 0xDD,

        [Description("Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the 'single-quote/double-quote' key")]
        VK_OEM_7 = 0xDE,

        [Description("Used for miscellaneous characters; it can vary by keyboard.")]
        VK_OEM_8 = 0xDF,


        [Description("Either the angle bracket key or the backslash key on the RT 102-key keyboard")]
        VK_OEM_102 = 0xE2,

        [Description("IME PROCESS key")]
        VK_PROCESSKEY = 0xE5,


        [Description("Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP")]
        VK_PACKET = 0xE7,

        [Description("Attn key")]
        VK_ATTN = 0xF6,

        [Description("CrSel key")]
        VK_CRSEL = 0xF7,

        [Description("ExSel key")]
        VK_EXSEL = 0xF8,

        [Description("Erase EOF key")]
        VK_EREOF = 0xF9,

        [Description("Play key")]
        VK_PLAY = 0xFA,

        [Description("Zoom key")]
        VK_ZOOM = 0xFB,

        [Description("PA1 key")]
        VK_PA1 = 0xFD,

        [Description("Clear key")]
        VK_OEM_CLEAR = 0xFE,
    }
    #endregion
    #endregion

}
