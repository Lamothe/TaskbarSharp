using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TaskbarX
{

    public class Win32
    {

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.I4)] ShowWindowCommands nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("User32.dll")]
        public static extern bool EnumChildWindows(IntPtr WindowHandle, EnumWindowProcess Callback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClientRect(IntPtr hWnd, ref RECT lpRECT);

        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int w, int h);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowByClass(string lpClassName, IntPtr zero);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        [DllImport("kernel32.dll")]
        public static extern int SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, [MarshalAs(UnmanagedType.I4)] WindowStyles nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);


        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);


        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("gdi32.dll")]
        public static extern int CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, int fnCombineMode);


        public enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1U,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }


        public enum RedrawWindowFlags : uint
        {
            Invalidate = 0x1U,
            InternalPaint = 0x2U,
            Erase = 0x4U,
            Validate = 0x8U,
            NoInternalPaint = 0x10U,
            NoErase = 0x20U,
            NoChildren = 0x40U,
            AllChildren = 0x80U,
            UpdateNow = 0x100U,
            EraseNow = 0x200U,
            Frame = 0x400U,
            NoFrame = 0x800U
        }

        public struct POINTAPI
        {
            public int x;
            public int y;
        }

        public static int WS_BORDER = 8388608;
        public static int WS_DLGFRAME = 4194304;
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public static int WS_VISIBLE = 268435456;

        public struct WINDOWPLACEMENT
        {
            public int Length;
            public int flags;
            public int showCmd;
            public POINTAPI ptMinPosition;
            public POINTAPI ptMaxPosition;
            public RECT rcNormalPosition;
        }

        public enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        public static int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
        public static int WM_DWMCOMPOSITIONCHANGED = 0x31E;
        public static int WM_THEMECHANGED = 0x31A;

        public const int WM_SETREDRAW = 11;

        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public struct MONITORINFO
        {
            public long cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public long dwFlags;
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_TRANSPARANT = 6,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_NORMAL = 150
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;
        public const int WS_MAXIMIZE = 16777216;
        public const long WS_POPUP = 2147483648L;
        public const int WS_EX_LAYERED = 524288;

        public enum WindowStyles
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xC00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        public enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        public delegate bool EnumWindowProcess(IntPtr Handle, IntPtr Parameter);

        public static uint SWP_NOSIZE = 1U;
        public static uint SWP_ASYNCWINDOWPOS = 16384U;
        public static uint SWP_NOACTIVATE = 16U;
        public static uint SWP_NOSENDCHANGING = 1024U;
        public static uint SWP_NOZORDER = 4U;
        public static long WM_COMMAND = 0x111L;
        public static IntPtr HWND_BROADCAST = new IntPtr(65535);
        public static uint WM_SETTINGCHANGE = 26U;
        public static int SMTO_ABORTIFHUNG = 2;



        public static uint TOPMOST_FLAGS = 0x2 | 0x1;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);



        public static void ShowStartMenu()
        {
            var shell = FindWindow("Shell_TrayWnd", null);


            // ' Const keyControl As Byte = &H11
            // ' Const keyEscape As Byte = &H1B
            // ' keybd_event(keyControl, 0, 0, UIntPtr.Zero)
            // ' keybd_event(keyEscape, 0, 0, UIntPtr.Zero)
            // ' Const KEYEVENTF_KEYUP As UInteger = &H2
            // ' keybd_event(keyControl, 0, KEYEVENTF_KEYUP, UIntPtr.Zero)
            // 'keybd_event(keyEscape, 0, KEYEVENTF_KEYUP, UIntPtr.Zero)

            // ' keybd_event(CByte(Keys.LWin), 0, &H0, CType(0, UIntPtr)) : Application.DoEvents() 'Press the Left Win key
            // 'keybd_event(CByte(Keys.LWin), 0, &H0, CType(0, UIntPtr)) : Application.DoEvents() 'Press the Left Win key

            // ' Dim tt As New RECT
            // ' GetWindowRect(shell, tt)

            // ' MsgBox(tt.Top)

            // 'SHOWS DESKTOP
            // 'SendMessage(shell, &H400 + 377, CBool(CType(&H1, IntPtr)), CInt(CType(0, IntPtr)))



            // '  Dim sClassName As New StringBuilder("", 256)
            // '  GetClassName(GetActiveWindow(), sClassName, 256)

            // ' PostMessage(shell, &H400 + 465, CType(&H1, IntPtr), CType(&H10001, IntPtr))
            // ' PostMessage(shell, &H127, CType(&H30001, IntPtr), CType(0, IntPtr))
            // 'SendMessage(shell, &H400 + 377, CBool(CType(&H100, IntPtr)), CInt(CType(1, IntPtr)))

            // 'PostMessage(shell, &H400 + 243, CType(shell, IntPtr), CType(0, IntPtr))
            // 'SetFocus(shell)
            keybd_event((byte)Keys.LWin, 0, 0x0U, (UIntPtr)0); // Press the Left Win key


            keybd_event((byte)Keys.LWin, 0, 0x2U, (UIntPtr)0); // Press the Left Win key
                                                               // '  SetFocus(shell)







            // ' End If

            // '  SetFocus(shell)


            // ' keybd_event(CByte(Keys.LWin), 0, &H2, CType(0, UIntPtr)) : Application.DoEvents() 'Press the Left Win key
            // ' keybd_event(CByte(Keys.LWin), 0, &H2, CType(0, UIntPtr)) : Application.DoEvents() 'Press the Left Win key

            // ' PostMessage(shell, &H112, CType(&HF131, IntPtr), CType(&H1, IntPtr))

            // 'PostMessage(shell, wm_s, CType(&H1, IntPtr), CType(&H10001, IntPtr))

            // ' PostMessage(shell, &H400 + 465, CType(&H1, IntPtr), CType(&H10001, IntPtr))
            // ' PostMessage(shell, &H400 + 443, CType(&H1, IntPtr), CType(0, IntPtr))
            // ' PostMessage(shell, &H400 + 377, CType(&H0, IntPtr), CType(0, IntPtr))



            // ' keybd_event(CByte(Keys.LWin), 0, &H2, CType(0, UIntPtr)) 'Press the Left Win key

        }

        [DllImport("user32.dll")]

        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowText(IntPtr hwnd, StringBuilder lpString, int cch);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr(HandleRef hWnd, [MarshalAs(UnmanagedType.I4)] WindowLongFlags nIndex);


        public enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

    }
}