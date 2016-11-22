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

namespace CoffeeJelly.tempa
{
    internal class Internal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="valueName"></param>
        /// <param name="keyLocation"></param>
        /// <param name="saveValue"></param>
        /// <exception cref="InvalidOperationException">Ошибка записи в реестр. Подробности во внутреннем исключении.</exception>
        internal static void SaveRegistrySettings(ref bool lastValue, string valueName, string keyLocation, bool saveValue)
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
        internal static void SaveRegistrySettings(string valueName, string keyLocation, bool saveValue)
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
        internal static void SaveRegistrySettings(ref string lastValue, string valueName, string keyLocation, string saveValue)
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

        internal static string SaveRegistrySettings(string valueName, string keyLocation, string saveValue)
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
        internal static void CheckRegistrySettings(ref string keyValue, string valueName, string keyLocation, string keyDefaultValue)
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
        internal static void CheckRegistrySettings(ref bool keyValue, string valueName, string keyLocation, bool keyDefaultValue)
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
        internal static bool CheckRegistrySettings(string valueName, string keyLocation, bool keyDefaultValue)
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
        internal static string CheckRegistrySettings(string valueName, string keyLocation, string keyDefaultValue)
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

        internal static Stack<TreeViewItem> GetNodes(UIElement element)
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

        internal static FrameworkElement FindVisualChildElement(DependencyObject element, Type childType)
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

        internal static FrameworkElement FindVisualParentElement(DependencyObject element, Type parentType)
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
        internal static List<FrameworkElement> GetChildElementsByType(FrameworkElement parentElement, Type childElementType)
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
        internal static void ChildControls(FrameworkElement parentElement, List<FrameworkElement> Controls)
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
        internal static FrameworkElement GetChildElementByName(FrameworkElement parentElement, string childElementName)
        {
            var allElem = new List<FrameworkElement>();
            //создадим список всех элементов в родительском элементе
            ChildControls(parentElement, allElem);
            foreach (FrameworkElement elem in allElem)
                if (elem.Name == childElementName && elem.Visibility != Visibility.Hidden)
                    return elem;
            return null;
        }

        internal static void CopyResource(string resourceName, string outputFileFullPath)
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

        internal static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                uint lpdwProcessId;
                GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        internal static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                Win32Callback childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;

        }

        internal static bool EnumWindow(IntPtr handle, IntPtr pointer)
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

        internal static string GetProgramName<T>()
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
        internal static Encoding GetEncoding(string filename)
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

        internal static Encoding GetEncoding(Stream stream)
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


        internal static string ArchiveDataFileName(string dateFormat, DateTime initDate, DateTime finalDate, string restName)
        {
            return $"{initDate.Date.ToString(dateFormat)}-{finalDate.Date.ToString(dateFormat)} {restName}";
        }

        internal static bool ArchiveDataFileNameValidation(string dateFormat, string fileName)
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

        internal static bool DateValidation(string dateString, string dateFormat)
        {
            DateTime dt;
            return DateTime.TryParseExact(
                    dateString,
                    dateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dt);
        }

        internal static Task Delay(double milliseconds)
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

        internal static string MakeExcelReportFileNameFromDataFilePath(string dataFilePath)
        {
            string dataFileName = System.IO.Path.GetFileNameWithoutExtension(dataFilePath);
            return dataFileName + " report.xlsm";
        }

        internal delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

    }
}
