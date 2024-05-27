using Accessibility;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskbarSharp;

public static class Win32
{
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("SHCore.dll", SetLastError = true)]
    public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr FindWindowByClass(string lpClassName, IntPtr zero);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

    [DllImport("oleacc.dll")]
    public static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, object[] rgvarChildren, ref int pcObtained);

    [DllImport("oleacc")]
    public static extern int AccessibleObjectFromWindow(int Hwnd, int dwId, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

    public enum PROCESS_DPI_AWARENESS
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    public static uint SWP_NOSIZE = 1U;
    public static uint SWP_ASYNCWINDOWPOS = 16384U;
    public static uint SWP_NOACTIVATE = 16U;
    public static uint SWP_NOSENDCHANGING = 1024U;
    public static uint SWP_NOZORDER = 4U;
}
