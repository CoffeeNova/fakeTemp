using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Gma.UserActivityMonitor
{
    public static partial class HookManager
    {
        /// <summary>
        /// The CallWndProc hook procedure is an application-defined or library-defined callback 
        /// function used with the SetWindowsHookEx function. The HOOKPROC type defines a pointer 
        /// to this callback function. CallWndProc is a placeholder for the application-defined 
        /// or library-defined function name.
        /// </summary>
        /// <param name="nCode">
        /// [in] Specifies whether the hook procedure must process the message. 
        /// If nCode is HC_ACTION, the hook procedure must process the message. 
        /// If nCode is less than zero, the hook procedure must pass the message to the 
        /// CallNextHookEx function without further processing and must return the 
        /// value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies whether the message was sent by the current thread. 
        /// If the message was sent by the current thread, it is nonzero; otherwise, it is zero. 
        /// </param>
        /// <param name="lParam">
        /// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
        /// </param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
        /// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
        /// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
        /// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
        /// procedure does not call CallNextHookEx, the return value should be zero. 
        /// </returns>
        /// <remarks>
        /// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/callwndproc.asp
        /// </remarks>
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        private delegate void WinEventHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);
        //##############################################################################
        #region Mouse hook processing

        /// <summary>
        /// This field is not objectively needed but we need to keep a reference on a delegate which will be 
        /// passed to unmanaged code. To avoid GC to clean it up.
        /// When passing delegates to unmanaged code, they must be kept alive by the managed application 
        /// until it is guaranteed that they will never be called.
        /// </summary>
        private static HookProc s_MouseDelegate;

        /// <summary>
        /// Stores the handle to the mouse hook procedure.
        /// </summary>
        private static int s_MouseHookHandle;

        private static int m_OldX;
        private static int m_OldY;

        /// <summary>
        /// A callback function which will be called every Time a mouse activity detected.
        /// </summary>
        /// <param name="nCode">
        /// [in] Specifies whether the hook procedure must process the message. 
        /// If nCode is HC_ACTION, the hook procedure must process the message. 
        /// If nCode is less than zero, the hook procedure must pass the message to the 
        /// CallNextHookEx function without further processing and must return the 
        /// value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies whether the message was sent by the current thread. 
        /// If the message was sent by the current thread, it is nonzero; otherwise, it is zero. 
        /// </param>
        /// <param name="lParam">
        /// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
        /// </param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
        /// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
        /// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
        /// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
        /// procedure does not call CallNextHookEx, the return value should be zero. 
        /// </returns>
        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //Marshall the data from callback.
                MouseLLHookStruct mouseHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct));

                //detect button clicked
                MouseButtons button = MouseButtons.None;
                short mouseDelta = 0;
                int clickCount = 0;
                bool mouseDown = false;
                bool mouseUp = false;

                switch (wParam)
                {
                    case WM_LBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Left;
                        clickCount = 1;
                        break;
                    case WM_LBUTTONDBLCLK:
                        button = MouseButtons.Left;
                        clickCount = 2;
                        break;
                    case WM_RBUTTONDOWN:
                        mouseDown = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONUP:
                        mouseUp = true;
                        button = MouseButtons.Right;
                        clickCount = 1;
                        break;
                    case WM_RBUTTONDBLCLK:
                        button = MouseButtons.Right;
                        clickCount = 2;
                        break;
                    case WM_MOUSEWHEEL:
                        //If the message is WM_MOUSEWHEEL, the high-order word of MouseData member is the wheel delta. 
                        //One wheel click is defined as WHEEL_DELTA, which is 120. 
                        //(value >> 16) & 0xffff; retrieves the high-order word from the given 32-bit value
                        mouseDelta = (short)((mouseHookStruct.MouseData >> 16) & 0xffff);

                        //TODO: X BUTTONS (I havent them so was unable to test)
                        //If the message is WM_XBUTTONDOWN, WM_XBUTTONUP, WM_XBUTTONDBLCLK, WM_NCXBUTTONDOWN, WM_NCXBUTTONUP, 
                        //or WM_NCXBUTTONDBLCLK, the high-order word specifies which X button was pressed or released, 
                        //and the low-order word is reserved. This value can be one or more of the following values. 
                        //Otherwise, MouseData is not used. 
                        break;
                }

                //generate event 
                MouseEventExtArgs e = new MouseEventExtArgs(
                                                   button,
                                                   clickCount,
                                                   mouseHookStruct.Point.X,
                                                   mouseHookStruct.Point.Y,
                                                   mouseDelta);

                //Mouse up
                if (s_MouseUp != null && mouseUp)
                {
                    s_MouseUp.Invoke(null, e);
                }

                //Mouse down
                if (s_MouseDown != null && mouseDown)
                {
                    s_MouseDown.Invoke(null, e);
                }

                //If someone listens to click and a click is heppened
                if (s_MouseClick != null && clickCount > 0)
                {
                    s_MouseClick.Invoke(null, e);
                }

                //If someone listens to click and a click is heppened
                if (s_MouseClickExt != null && clickCount > 0)
                {
                    s_MouseClickExt.Invoke(null, e);
                }

                //If someone listens to double click and a click is heppened
                if (s_MouseDoubleClick != null && clickCount == 2)
                {
                    s_MouseDoubleClick.Invoke(null, e);
                }

                //Wheel was moved
                if (s_MouseWheel != null && mouseDelta != 0)
                {
                    s_MouseWheel.Invoke(null, e);
                }

                //If someone listens to move and there was a change in coordinates raise move event
                if ((s_MouseMove != null || s_MouseMoveExt != null) && (m_OldX != mouseHookStruct.Point.X || m_OldY != mouseHookStruct.Point.Y))
                {
                    m_OldX = mouseHookStruct.Point.X;
                    m_OldY = mouseHookStruct.Point.Y;
                    if (s_MouseMove != null)
                    {
                        s_MouseMove.Invoke(null, e);
                    }

                    if (s_MouseMoveExt != null)
                    {
                        s_MouseMoveExt.Invoke(null, e);
                    }
                }

                if (e.Handled)
                {
                    return -1;
                }
            }

            //call next hook
            return CallNextHookEx(s_MouseHookHandle, nCode, wParam, lParam);
        }
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static void EnsureSubscribedToGlobalMouseEvents()
        {
            // install Mouse hook only if it is not installed and must be installed
            if (s_MouseHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                s_MouseDelegate = MouseHookProc;
                //install hook
                var mar = LoadLibrary("user32.dll");
                s_MouseHookHandle = SetWindowsHookEx(
                WH_MOUSE_LL,
                s_MouseDelegate,
                mar,
                0);
                //Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]

                //If SetWindowsHookEx fails.
                if (s_MouseHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                     int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void TryUnsubscribeFromGlobalMouseEvents()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_MouseClick == null &&
                s_MouseDown == null &&
                s_MouseMove == null &&
                s_MouseUp == null &&
                s_MouseClickExt == null &&
                s_MouseMoveExt == null &&
                s_MouseWheel == null &&
                s_MouseClickBlock == null)
            {
                ForceUnsunscribeFromGlobalMouseEvents();
            }
        }

        private static void ForceUnsunscribeFromGlobalMouseEvents()
        {
            if (s_MouseHookHandle != 0)
            {
                //uninstall hook
                int result = UnhookWindowsHookEx(s_MouseHookHandle);
                //reset invalid handle
                s_MouseHookHandle = 0;
                //Free up for GC
                s_MouseDelegate = null;
                //if failed and exception must be thrown
                if (result == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void EnsureSubscribedToGlobalMouseClickBlockEvents()
        {
            // install Mouse hook only if it is not installed and must be installed
            if (s_MouseHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                s_MouseDelegate = HookBlock;
                //install hook
                var mar = LoadLibrary("user32.dll");
                s_MouseHookHandle = SetWindowsHookEx(
                    WH_MOUSE_LL,
                    s_MouseDelegate,
                    mar,
                    0);
                //  Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0])

                //If SetWindowsHookEx fails.
                if (s_MouseHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        private static void TryUnsubscribeFromGlobalMouseClickBlockEvents()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_MouseClick == null &&
                s_MouseDown == null &&
                s_MouseMove == null &&
                s_MouseUp == null &&
                s_MouseClickExt == null &&
                s_MouseMoveExt == null &&
                s_MouseWheel == null &&
                s_MouseClickBlock == null)
            {
                ForceUnsunscribeFromGlobalMouseClickBlockEvents();
            }
        }

        private static void ForceUnsunscribeFromGlobalMouseClickBlockEvents()
        {
            if (s_MouseHookHandle != 0)
            {
                //uninstall hook
                int result = UnhookWindowsHookEx(s_MouseHookHandle);
                //reset invalid handle
                s_MouseHookHandle = 0;
                //Free up for GC
                s_MouseDelegate = null;
                //if failed and exception must be thrown
                if (result == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion

        //##############################################################################
        #region Keyboard hook processing

        /// <summary>
        /// This field is not objectively needed but we need to keep a reference on a delegate which will be 
        /// passed to unmanaged code. To avoid GC to clean it up.
        /// When passing delegates to unmanaged code, they must be kept alive by the managed application 
        /// until it is guaranteed that they will never be called.
        /// </summary>
        private static HookProc s_KeyboardDelegate;

        /// <summary>
        /// Stores the handle to the Keyboard hook procedure.
        /// </summary>
        private static int s_KeyboardHookHandle;

        /// <summary>
        /// A callback function which will be called every Time a keyboard activity detected.
        /// </summary>
        /// <param name="nCode">
        /// [in] Specifies whether the hook procedure must process the message. 
        /// If nCode is HC_ACTION, the hook procedure must process the message. 
        /// If nCode is less than zero, the hook procedure must pass the message to the 
        /// CallNextHookEx function without further processing and must return the 
        /// value returned by CallNextHookEx.
        /// </param>
        /// <param name="wParam">
        /// [in] Specifies whether the message was sent by the current thread. 
        /// If the message was sent by the current thread, it is nonzero; otherwise, it is zero. 
        /// </param>
        /// <param name="lParam">
        /// [in] Pointer to a CWPSTRUCT structure that contains details about the message. 
        /// </param>
        /// <returns>
        /// If nCode is less than zero, the hook procedure must return the value returned by CallNextHookEx. 
        /// If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx 
        /// and return the value it returns; otherwise, other applications that have installed WH_CALLWNDPROC 
        /// hooks will not receive hook notifications and may behave incorrectly as a result. If the hook 
        /// procedure does not call CallNextHookEx, the return value should be zero. 
        /// </returns>
        private static int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            //indicates if any of underlaing events set e.Handled flag
            bool handled = false;

            if (nCode >= 0)
            {
                //read structure KeyboardHookStruct at lParam
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                //raise KeyDown
                if (s_KeyDown != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    s_KeyDown.Invoke(null, e);
                    handled = e.Handled;
                }

                // raise KeyPress
                if (s_KeyPress != null && wParam == WM_KEYDOWN)
                {
                    bool isDownShift = ((GetKeyState(VK_SHIFT) & 0x80) == 0x80 ? true : false);
                    bool isDownCapslock = (GetKeyState(VK_CAPITAL) != 0 ? true : false);

                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);
                    byte[] inBuffer = new byte[2];
                    if (ToAscii(MyKeyboardHookStruct.VirtualKeyCode,
                              MyKeyboardHookStruct.ScanCode,
                              keyState,
                              inBuffer,
                              MyKeyboardHookStruct.Flags) == 1)
                    {
                        char key = (char)inBuffer[0];
                        if ((isDownCapslock ^ isDownShift) && Char.IsLetter(key)) key = Char.ToUpper(key);
                        KeyPressEventArgs e = new KeyPressEventArgs(key);
                        s_KeyPress.Invoke(null, e);
                        handled = handled || e.Handled;
                    }
                }

                // raise KeyUp
                if (s_KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.VirtualKeyCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    s_KeyUp.Invoke(null, e);
                    handled = handled || e.Handled;
                }

            }

            //if event handled in application do not handoff to other listeners
            if (handled)
                return -1;

            //forward to other application
            return CallNextHookEx(s_KeyboardHookHandle, nCode, wParam, lParam);
        }
        private static int HookBlock(int nCode, Int32 wParam, IntPtr lParam)
        {
            return -1;
        }
        private static void EnsureSubscribedToGlobalKeyboardEvents()
        {
            // install Keyboard hook only if it is not installed and must be installed
            if (s_KeyboardHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                s_KeyboardDelegate = KeyboardHookProc;
                //install hook
                var mar = LoadLibrary("user32.dll");
                s_KeyboardHookHandle = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    s_KeyboardDelegate,
                    mar, //Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0])
                    0);

                //If SetWindowsHookEx fails.
                if (s_KeyboardHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        //donky. added blockkeypress
        private static void EnsureSubscribedToGlobalKeyboardBlockEvents()
        {
            // install Keyboard hook only if it is not installed and must be installed
            if (s_KeyboardHookHandle == 0)
            {
                //See comment of this field. To avoid GC to clean it up.
                s_KeyboardDelegate = HookBlock;
                //install hook
                var mar = LoadLibrary("user32.dll");
                s_KeyboardHookHandle = SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    s_KeyboardDelegate,
                    mar,
                    0);
                //  Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0])

                //If SetWindowsHookEx fails.
                if (s_KeyboardHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        private static void TryUnsubscribeFromGlobalKeyboardEvents()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_KeyDown == null &&
                s_KeyUp == null &&
                s_KeyPress == null &&
                s_KeyBlock == null)
            {
                ForceUnsunscribeFromGlobalKeyboardEvents();
            }
        }

        private static void ForceUnsunscribeFromGlobalKeyboardEvents()
        {
            if (s_KeyboardHookHandle != 0)
            {
                //uninstall hook
                int result = UnhookWindowsHookEx(s_KeyboardHookHandle);
                //reset invalid handle
                s_KeyboardHookHandle = 0;
                //Free up for GC
                s_KeyboardDelegate = null;
                //if failed and exception must be thrown
                if (result == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion

        #region Focus hook processing


        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/dd318066(v=vs.85).aspx
        /// </summary>
        private struct EventConstants
        {
            public const int EVENT_OBJECT_CREATE = 0x8000;
            public const int EVENT_OBJECT_SHOW = 0x8002;
            public const int EVENT_OBJECT_REORDER = 0x8004;
            public const int EVENT_OBJECT_FOCUS = 0x8005;
            public const int EVENT_OBJECT_CONTENTSCROLLED = 0x8015;
            public const int EVENT_OBJECT_SELECTION = 0x8006;
            public const int EVENT_OBJECT_SELECTIONREMOVE = 0x8008;
            public const int EVENT_OBJECT_SELECTIONWITHIN = 0x8009;
            public const int EVENT_SYSTEM_FOREGROUND = 0x0003;
            public const int EVENT_OBJECT_SELECTIONADD = 0x0007;
            public const int EVENT_OBJECT_STATECHANGE = 0x800A;
            public const int EVENT_OBJECT_NAMECHANGE = 0x800C;
        }

        private static WinEventHookProc s_GoogleFocusDelegate;
        private static WinEventHookProc s_WebSkypeFocusDelegate;
        private static WinEventHookProc s_ForegroundChangedDelegate;
        private static WinEventHookProc s_WebSkypeWindowCreateDelegate;
        private static WinEventHookProc s_WebSkypeTabNameChangeDelegate;
        private static int s_GoogleFocusHookHandle;
        private static int s_WSFocusHookHandle;
        //private static int s_OwsFocusHookHandle;
        //private static int s_FFwsFocusHookHandle;
        //private static int s_TBwsFocusHookHandle;
        //private static int s_IEwsFocusHookHandle;
        private static int s_ForegroundChangedHookHandle;
        private static int s_WebSkypeWindowCreateHookHandle;
        private static int s_WebSkypeTabNameChangeHookHandle;
        public static Process chromeProcess;
        public static Process operaProcess;
        public static Process firefoxProcess;
        public static Process torProcess;
        public static Process ieProcess;

        private static void GoogleFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {

            //idChild определил опытным путем, это айди адресной строки, при обновлении хрома, возможно изменение этого id, раскомментить ниже и посмотреть при получении фокуса
            //Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook, iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime));
            if (idChild < 0)
            {
                EventArgs e = new EventArgs();
                s_GoogleGotFocus.Invoke(hWnd, e);
            }

        }
        //private static WinEventHookProc WebSkypeFocusHookProc(InternetBrowser browser)
        //{
        //    switch(browser)
        //    {
        //        case InternetBrowser.GoogleChrome:
        //            return WebSkypeFocusHookProc;
        //        case InternetBrowser.Opera:
        //            return OwsFocusHookProc;
        //        case InternetBrowser.Firefox:
        //            return FFwsFocusHookProc;
        //        case InternetBrowser.TorBrowser:
        //            return TBwsFocusHookProc;
        //        case InternetBrowser.InternetExplorer:
        //            return IEwsFocusHookProc;
        //    }
        //    return null;
        //}
        private static void WebSkypeFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            //idChild определил опытным путем, это айди адресной строки, при обновлении хрома, возможно изменение этого id, раскомментить ниже и посмотреть при получении фокуса
            //if (iEvent != 0x800B)
            //{
            //    Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook,  iEvent.ToString("X"), hWnd.ToString("X"), idObject, idChild, dwEventThread, dwmsEventTime));
            //}
                EventArgs e = new EventArgs();
                s_WebSkypeGotFocus.Invoke(hWnd, e);
        }

        //private static void OwsFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //        EventArgs e = new EventArgs();
        //        o_WebSkypeGotFocus.Invoke(hWnd, e);
        //}

        //private static void FFwsFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //        EventArgs e = new EventArgs();
        //        ff_WebSkypeGotFocus.Invoke(hWnd, e);
        //}

        //private static void TBwsFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //        EventArgs e = new EventArgs();
        //        tb_WebSkypeGotFocus.Invoke(hWnd, e);
        //}

        //private static void IEwsFocusHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //        EventArgs e = new EventArgs();
        //        ie_WebSkypeGotFocus.Invoke(hWnd, e);
        //}

        //private static WinEventHookProc WebSkypeTabNameChangeProc(InternetBrowser browser)
        //{
        //    switch (browser)
        //    {
        //        case InternetBrowser.GoogleChrome:
        //            return WebSkypeTabNameChangeProc;
        //        case InternetBrowser.Opera:
        //            return OwsTabNameChangeProc;
        //        case InternetBrowser.Firefox:
        //            return FFwsTabNameChangeProc;
        //        case InternetBrowser.TorBrowser:
        //            return TBwsTabNameChangeProc;
        //        case InternetBrowser.InternetExplorer:
        //            return IEwsTabNameChangeProc;
        //    }
        //    return null;
        //}
        private static void WebSkypeTabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            //idChild определил опытным путем, это айди адресной строки, при обновлении хрома, возможно изменение этого id, раскомментить ниже и посмотреть при получении фокуса
            if (hWnd == IntPtr.Zero)
                return;
                //Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook,  iEvent.ToString("X"), hWnd.ToString("X"), idObject, idChild, dwEventThread, dwmsEventTime));
                //Console.WriteLine(chromeProcess.MainWindowHandle.ToString("X"));
            EventArgs e = new EventArgs();
            s_WebSkypeTabNameChange.Invoke(hWnd, e);
        }

        //private static void OwsTabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //    EventArgs e = new EventArgs();
        //    o_WebSkypeTabNameChange.Invoke(hWnd, e);
        //}

        //private static void FFwsTabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //    EventArgs e = new EventArgs();
        //    ff_WebSkypeTabNameChange.Invoke(hWnd, e);
        //}

        //private static void TBwsTabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //    EventArgs e = new EventArgs();
        //    tb_WebSkypeTabNameChange.Invoke(hWnd, e);
        //}

        //private static void IEwsTabNameChangeProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        //{
        //    EventArgs e = new EventArgs();
        //    ie_WebSkypeTabNameChange.Invoke(hWnd, e);
        //}

        private static void ForegroundChangedHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {

            //idChild определил опытным путем, это айди адресной строки, при обновлении хрома, возможно изменение этого id, раскомментить ниже и посмотреть при получении фокуса
            //Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook, iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime));
            try
            {
                EventArgs e = new EventArgs();
                s_ForegroundChanged.Invoke(hWnd, e);
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }

        private static void WebSkypeWindowCreateHookProc(IntPtr hWinEventHook, int iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {

            //idChild определил опытным путем, это айди адресной строки, при обновлении хрома, возможно изменение этого id, раскомментить ниже и посмотреть при получении фокуса
            //Console.WriteLine(string.Format("hWinEventHook: {0}, iEvent: {1}, hWnd: {2},  idObject: {3}, idChild: {4}, dwEventThread: {5}, dwmsEventTime:{6}", hWinEventHook, iEvent, hWnd, idObject, idChild, dwEventThread, dwmsEventTime));
            if (iEvent == EventConstants.EVENT_OBJECT_CREATE || iEvent == EventConstants.EVENT_OBJECT_STATECHANGE)
            {
                EventArgs e = new EventArgs();
                s_WebSkypeWindowCreate.Invoke(hWnd, e);
            }

        }

        private static void EnsureSubscribedToGoogleFocusEvent()
        {

            // install Focus hook only if it is not installed and must be installed
            if (s_GoogleFocusHookHandle == 0 && chromeProcess != null)
            {

                //See comment of this field. To avoid GC to clean it up.
                s_GoogleFocusDelegate = GoogleFocusHookProc;
                //install hook
                int chromeProcessID = chromeProcess.Id;
                s_GoogleFocusHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_FOCUS, EventConstants.EVENT_OBJECT_FOCUS, IntPtr.Zero, s_GoogleFocusDelegate, chromeProcessID, 0, WINEVENT_OUTOFCONTEXT);

                //If SetWinEventHook fails.
                if (s_GoogleFocusHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                     int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }

        }
        private static void TryUnsubscribeFromGoogleFocusEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_GoogleGotFocus == null)
            {
                ForceUnsunscribeFromGooglelFocusEvent();
            }
        }

        private static void ForceUnsunscribeFromGooglelFocusEvent()
        {
            if (s_GoogleFocusHookHandle != 0)
            {
                //uninstall hook
                bool result = UnhookWinEvent(s_GoogleFocusHookHandle);
                //reset invalid handle
                s_GoogleFocusHookHandle = 0;
                //Free up for GC
                s_GoogleFocusDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        private static bool AnyOfProcess()
        {
            return (chromeProcess != null || operaProcess != null || firefoxProcess!= null || torProcess != null || ieProcess != null) ? true : false;
        }

        private static int ChooseWinEventHook_wsfe(InternetBrowser browser, WinEventHookProc wehp)
        {
            switch (browser)
            {
                case InternetBrowser.GoogleChrome:
                    return chromeProcess!= null ? SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, chromeProcess.Id, 0, WINEVENT_OUTOFCONTEXT): -1;
                case InternetBrowser.Opera:
                    return operaProcess!=null ? SetWinEventHook(EventConstants.EVENT_OBJECT_SHOW, EventConstants.EVENT_OBJECT_SHOW, IntPtr.Zero, wehp, operaProcess.Id, 0, WINEVENT_OUTOFCONTEXT): -1;
                case InternetBrowser.Firefox:
                    return firefoxProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, firefoxProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.TorBrowser:
                    return torProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, torProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.InternetExplorer:
                    return ieProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_SELECTIONREMOVE, EventConstants.EVENT_OBJECT_SELECTIONREMOVE, IntPtr.Zero, wehp, ieProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                default:
                    return -1;
            }
        }
        private static void EnsureSubscribedToWebSkypeFocusEvent(InternetBrowser browser)
        {

            // install Focus hook only if it is not installed and must be installed
            if (s_WSFocusHookHandle == 0)
            {// && chromeProcess!= null


                //See comment of this field. To avoid GC to clean it up.
                s_WebSkypeFocusDelegate = WebSkypeFocusHookProc;
                //install hook
                s_WSFocusHookHandle = ChooseWinEventHook_wsfe(browser, s_WebSkypeFocusDelegate);
                //If SetWinEventHook fails.

                if (s_WSFocusHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                     int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (s_WSFocusHookHandle == -1)
                    s_WSFocusHookHandle = 0;
            }

        }
        private static void TryUnsubscribeFromWebSkypeFocusEvent(InternetBrowser browser)
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_WebSkypeGotFocus == null)
            {
                ForceUnsunscribeFromWebSkypeFocusEvent();
            }
        }

        private static void ForceUnsunscribeFromWebSkypeFocusEvent()
        {
            if (s_WSFocusHookHandle != 0)
            {
                //uninstall hook
                bool result = UnhookWinEvent(s_WSFocusHookHandle);
                //reset invalid handle
                s_WSFocusHookHandle = 0;
                //Free up for GC
                s_WebSkypeFocusDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }


        private static void EnsureSubscribedToForegroundChangedEvent()
        {

            // install Focus hook only if it is not installed and must be installed
            if (s_ForegroundChangedHookHandle == 0)
            {

                //See comment of this field. To avoid GC to clean it up.
                s_ForegroundChangedDelegate = ForegroundChangedHookProc;
                //install hook
                s_ForegroundChangedHookHandle = SetWinEventHook(EventConstants.EVENT_SYSTEM_FOREGROUND, EventConstants.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, s_ForegroundChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);

                //If SetWinEventHook fails.
                if (s_ForegroundChangedHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                     int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }

        }
        private static void TryUnsubscribeFromForegroundChangedEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_ForegroundChanged == null)
            {
                ForceUnsunscribeFromForegroundChangedEvent();
            }
        }

        private static void ForceUnsunscribeFromForegroundChangedEvent()
        {
            if (s_ForegroundChangedHookHandle != 0)
            {
                //uninstall hook
                bool result = UnhookWinEvent(s_ForegroundChangedHookHandle);
                //reset invalid handle
                s_ForegroundChangedHookHandle = 0;
                //Free up for GC
                s_ForegroundChangedDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }

         private static void EnsureSubscribedToWebSkypeWindowCreateEvent()
        {

            // install Focus hook only if it is not installed and must be installed
            if (s_WebSkypeWindowCreateHookHandle == 0 && chromeProcess != null)
            {

                //See comment of this field. To avoid GC to clean it up.
                s_WebSkypeWindowCreateDelegate = WebSkypeWindowCreateHookProc;
                //install hook
                int chromeProcessID = chromeProcess.Id;
                s_WebSkypeWindowCreateHookHandle = SetWinEventHook(EventConstants.EVENT_OBJECT_CREATE, EventConstants.EVENT_OBJECT_STATECHANGE, IntPtr.Zero, s_WebSkypeWindowCreateDelegate, chromeProcessID, 0, WINEVENT_OUTOFCONTEXT);

                //If SetWinEventHook fails.
                if (s_WebSkypeWindowCreateHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                     int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }

        }
        private static void TryUnsubscribeFromWebSkypeWindowCreateEvent()
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_WebSkypeWindowCreate == null)
            {
                ForceUnsunscribeFromWebSkypeWindowCreateEvent();
            }
        }

        private static void ForceUnsunscribeFromWebSkypeWindowCreateEvent()
        {
            if (s_WebSkypeWindowCreateHookHandle != 0)
            {
                //uninstall hook
                bool result = UnhookWinEvent(s_WebSkypeWindowCreateHookHandle);
                //reset invalid handle
                s_WebSkypeWindowCreateHookHandle = 0;
                //Free up for GC
                s_WebSkypeWindowCreateDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        private static int ChooseWinEventHook_wstn(InternetBrowser browser, WinEventHookProc wehp)
        {
            switch (browser)
            {
                case InternetBrowser.GoogleChrome:
                    return chromeProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, wehp, chromeProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.Opera:
                    return operaProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, wehp, operaProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.Firefox:
                    return firefoxProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, wehp, firefoxProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.TorBrowser:
                    return torProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, wehp, torProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                case InternetBrowser.InternetExplorer:
                    return ieProcess != null ? SetWinEventHook(EventConstants.EVENT_OBJECT_NAMECHANGE, EventConstants.EVENT_OBJECT_NAMECHANGE, IntPtr.Zero, wehp, ieProcess.Id, 0, WINEVENT_OUTOFCONTEXT) : -1;
                default:
                    return -1;
            }
        }
        private static void EnsureSubscribedToWebSkypeTabNameChangeEvent(InternetBrowser browser)
        {
            // install Focus hook only if it is not installed and must be installed
            if (s_WebSkypeTabNameChangeHookHandle == 0)
            {// && chromeProcess!= null


                //See comment of this field. To avoid GC to clean it up.
                s_WebSkypeTabNameChangeDelegate = WebSkypeTabNameChangeProc;
                //install hook
                s_WebSkypeTabNameChangeHookHandle = ChooseWinEventHook_wstn(browser, s_WebSkypeTabNameChangeDelegate);
                //If SetWinEventHook fails.

                if (s_WebSkypeTabNameChangeHookHandle == 0)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //do cleanup

                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
                if (s_WebSkypeTabNameChangeHookHandle == -1)
                    s_WebSkypeTabNameChangeHookHandle = 0;
            }
        }
        private static void TryUnsubscribeFromWebSkypeTabNameChangeEvent(InternetBrowser browser)
        {
            //if no subsribers are registered unsubsribe from hook
            if (s_WebSkypeTabNameChange == null)
            {
                ForceUnsunscribeFromWebSkypeTabNameChangeEvent();
            }
        }

        private static void ForceUnsunscribeFromWebSkypeTabNameChangeEvent()
        {
            if (s_WebSkypeTabNameChangeHookHandle != 0)
            {
                //uninstall hook
                bool result = UnhookWinEvent(s_WebSkypeTabNameChangeHookHandle);
                //reset invalid handle
                s_WebSkypeTabNameChangeHookHandle = 0;
                //Free up for GC
                s_WebSkypeTabNameChangeDelegate = null;
                //if failed and exception must be thrown
                if (result == false)
                {
                    //Returns the error code returned by the last unmanaged function called using platform invoke that has the DllImportAttribute.SetLastError flag set. 
                    int errorCode = Marshal.GetLastWin32Error();
                    //Initializes and throws a new instance of the Win32Exception class with the specified error. 
                    throw new Win32Exception(errorCode);
                }
            }
        }
        #endregion
    }
}

