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

        internal static Stack<TreeViewItem> GetNodes(UIElement element)
        {

            Stack<TreeViewItem> tempNodePath = new Stack<TreeViewItem>();
            // Walk up the element tree to the nearest tree view item. 
            TreeViewItem container = element as TreeViewItem;

            while ((element != null))
            {

                container = element as TreeViewItem;
                if (container != null)
                    tempNodePath.Push(container);
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
            return tempNodePath;
        }
    }
}
