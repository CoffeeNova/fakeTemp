using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NWTweak;

namespace tempa
{
    class Internal
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
    }
}
