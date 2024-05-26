using Accessibility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskbarSharp;

public static class TaskbarCenterer
{
    public static bool ScreensChanged;

    public static int TaskbarCount;

    public static ArrayList windowHandles = new ArrayList();

    public static bool trayfixed;
    public static IntPtr setposhwnd;
    public static int setpospos;
    public static string setposori;

    public static string initposcalc;
    public static bool initposcalcready;

    public static System.Collections.ObjectModel.Collection<IntPtr> ActiveWindows = new System.Collections.ObjectModel.Collection<IntPtr>();

    public static System.Collections.ObjectModel.Collection<IntPtr> GetActiveWindows()
    {
        windowHandles.Clear();
        Win32.EnumWindows(Enumerator, 0);

        bool maintaskbarfound = false;
        bool sectaskbarfound = false;

        foreach (var Taskbar in windowHandles)
        {
            var sClassName = new StringBuilder("", 256);
            Win32.GetClassName((IntPtr)Taskbar, sClassName, 256);
            if (sClassName.ToString() == "Shell_TrayWnd")
            {
                maintaskbarfound = true;
            }
            if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
            {
                sectaskbarfound = true;
            }
            Console.WriteLine("=" + maintaskbarfound);
        }

        if (maintaskbarfound == false)
        {
            try
            {
                windowHandles.Add(Win32.FindWindow("Shell_TrayWnd", null));
            }
            catch
            {
            }
        }

        if (sectaskbarfound == false)
        {
            if (Screen.AllScreens.Count() >= 2)
            {
                // 'MsgBox(Screen.AllScreens.Count)
                try
                {
                    windowHandles.Add(Win32.FindWindow("Shell_SecondaryTrayWnd", null));
                }
                catch
                {
                }
            }
        }

        return ActiveWindows;
    }

    public static bool Enumerator(IntPtr hwnd, int lParam)
    {
        var sClassName = new StringBuilder("", 256);
        Win32.GetClassName(hwnd, sClassName, 256);
        if (sClassName.ToString() == "Shell_TrayWnd" | sClassName.ToString() == "Shell_SecondaryTrayWnd")
        {
            windowHandles.Add(hwnd);
        }
        return true;
    }

    public struct RectangleX
    {
        public int left;
        public int top;
        public int width;
        public int height;
    }

    public static RectangleX GetLocation(IAccessible acc, int idChild)
    {
        var rect = new RectangleX();
        if (!(acc == null))
        {
            acc.accLocation(out rect.left, out rect.top, out rect.width, out rect.height, idChild);
        }
        return rect;
    }

    public static void Looper(TaskbarSharpSettings settings)
    {
        try
        {
            // This loop will check if the taskbar changes and requires a move
            GetActiveWindows();

            var Taskbars = new ArrayList();

            // Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            foreach (var Taskbar in windowHandles)
            {
                var sClassName = new StringBuilder("", 256);
                Win32.GetClassName((IntPtr)Taskbar, sClassName, 256);
                var MSTaskListWClass = IntPtr.Zero;

                Console.WriteLine(sClassName.ToString());


                if (sClassName.ToString() == "Shell_TrayWnd")
                {
                    var ReBarWindow32 = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "ReBarWindow32", null);
                    var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);
                }

                if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
                {
                    var WorkerW = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "WorkerW", null);
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, (IntPtr)0, "MSTaskListWClass", null);
                }

                if (MSTaskListWClass == IntPtr.Zero)
                {
                    UI.ShowError("TaskbarSharp: Could not find the handle of the taskbar. Your current version or build of Windows may not be supported.");
                    Environment.Exit(0);
                }

                Taskbars.Add(MSTaskListWClass);
            }

            var TaskObject = new List<IAccessible>();
            foreach (var TaskList in Taskbars)
            {
                var accessiblex = MSAA.GetAccessibleObjectFromHandle((IntPtr)TaskList);
                TaskObject.Add(accessiblex);
            }

            var TaskObjects = TaskObject;

            // Start the endless loop
            var oldresults = default(string);
            var LastChildPos = default(RectangleX);
            while (true)
            {
                try
                {
                    string results = null;

                    if (settings.CheckFullscreenApp)
                    {
                        var activewindow = Win32.GetForegroundWindow();
                        var curmonx = Screen.FromHandle(activewindow);
                        var activewindowsize = new Win32.RECT();
                        Win32.GetWindowRect(activewindow, ref activewindowsize);

                        if (activewindowsize.Top == curmonx.Bounds.Top & activewindowsize.Bottom == curmonx.Bounds.Bottom & activewindowsize.Left == curmonx.Bounds.Left & activewindowsize.Right == curmonx.Bounds.Right)
                        {
                            Console.WriteLine("Fullscreen App detected " + activewindowsize.Bottom + "," + activewindowsize.Top + "," + activewindowsize.Left + "," + activewindowsize.Right);

                            do
                            {
                                Thread.Sleep(500);
                                activewindow = Win32.GetForegroundWindow();
                                Win32.GetWindowRect(activewindow, ref activewindowsize);
                                Thread.Sleep(500);
                            }

                            while (activewindowsize.Top == curmonx.Bounds.Top & activewindowsize.Bottom == curmonx.Bounds.Bottom & activewindowsize.Left == curmonx.Bounds.Left & activewindowsize.Right == curmonx.Bounds.Right);
                            Console.WriteLine("Fullscreen App deactivated");
                        }
                    }

                    // Go through each taskbar and result in a unique string containing the current state

                    int i = 0;

                    foreach (var TaskList in TaskObjects)
                    {
                        IAccessible[] children = MSAA.GetAccessibleChildren(TaskList);

                        var TaskListPos = GetLocation(TaskList, 0);

                        int tH = TaskListPos.height;
                        int tW = TaskListPos.width;

                        foreach (IAccessible childx in children)
                        {
                            if (childx?.get_accRole(0) as int? == 22) // 0x16 = toolbar
                            {
                                LastChildPos = GetLocation(childx, MSAA.GetAccessibleChildren(childx).Length);
                                break;
                            }
                        }

                        int cL = LastChildPos.left;
                        int cT = LastChildPos.top;
                        int cW = LastChildPos.width;
                        int cH = LastChildPos.height;

                        try
                        {
                            int testiferror = cL;
                        }
                        catch (Exception ex)
                        {
                            UI.ShowError(ex);

                            // Current taskbar is empty go to next taskbar.
                        }

                        int TaskbarCount;
                        int TrayWndSize;

                        // Get the end position of the last icon in the taskbar
                        TaskbarCount = cL + cW;

                        // Gets the width of the whole taskbars placeholder
                        TrayWndSize = tW;

                        // Put the results into a string ready to be matched for differences with last loop
                        results = results + "H" + TaskbarCount + TrayWndSize;

                        initposcalcready = true;

                        i += 1;
                    }

                    if (!((results ?? "") == (oldresults ?? "")))
                    {
                        // Something has changed we can now calculate the new position for each taskbar

                        initposcalcready = false;
                        initposcalc = results;

                        // Start the PositionCalculator
                    }
                    Task.Run(() => PositionCalculator(settings));


                    // Save current results for next loop
                    oldresults = results;

                    Thread.Sleep(settings.LoopRefreshRate);
                }
                catch (Exception ex)
                {
                    UI.ShowError(ex);

                    // Lost taskbar handles restart application
                    if (ex.ToString().Contains("NullReference") | ex.ToString().Contains("Missing method"))
                    {
                        IntPtr Handle;
                        do
                        {
                            Handle = default;
                            Thread.Sleep(250);
                            Handle = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
                        }
                        while (Handle == default);
                        Thread.Sleep(1000);
                        Application.Restart();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }
    }

    private static void PositionCalculator(TaskbarSharpSettings settings)
    {
        try
        {
            // Calculate the new positions and pass them through to the animator

            var Taskbars = new ArrayList();

            // Put all Taskbars into an ArrayList based on each TrayWnd in the TrayWnds ArrayList
            foreach (var Taskbar in windowHandles)
            {
                var sClassName = new StringBuilder("", 256);
                Win32.GetClassName((IntPtr)Taskbar, sClassName, 256);

                var MSTaskListWClass = IntPtr.Zero;

                if (sClassName.ToString() == "Shell_TrayWnd")
                {
                    var ReBarWindow32 = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "ReBarWindow32", null);
                    var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);
                }

                if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
                {
                    var WorkerW = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "WorkerW", null);
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, (IntPtr)0, "MSTaskListWClass", null);
                }

                Win32.SetWindowLong((IntPtr)Taskbar, (Win32.WindowStyles)Win32.GWL_EXSTYLE, 0x80);

                if (MSTaskListWClass == IntPtr.Zero)
                {
                    Console.WriteLine("TaskbarSharp: Could not find the handle of the taskbar.");
                    Thread.Sleep(1000);
                    continue;
                }

                Taskbars.Add(MSTaskListWClass);
            }

            // Calculate Position for every taskbar and trigger the animator
            var LastChildPos = default(RectangleX);
            var TrayNotifyPos = default(RectangleX);
            foreach (var TaskList in Taskbars)
            {
                var sClassName = new StringBuilder("", 256);
                Win32.GetClassName((IntPtr)TaskList, sClassName, 256);
                RectangleX TaskListPos;

                var accessible = MSAA.GetAccessibleObjectFromHandle((IntPtr)TaskList);
                IAccessible[] children = MSAA.GetAccessibleChildren(accessible);

                TaskListPos = GetLocation(accessible, 0);

                foreach (IAccessible childx in children)
                {
                    if (childx?.get_accRole(0) as int? == 22) // 0x16 = toolbar
                    {
                        LastChildPos = GetLocation(childx, MSAA.GetAccessibleChildren(childx).Length);
                        break;
                    }
                }

                var RebarHandle = Win32.GetParent((IntPtr)TaskList);
                var accessible3 = MSAA.GetAccessibleObjectFromHandle(RebarHandle);

                var RebarClassName = new StringBuilder("", 256);
                Win32.GetClassName(RebarHandle, RebarClassName, 256);

                int TaskbarWidth;
                int TrayWndLeft;
                int TrayWndWidth;
                int RebarWndLeft;
                int TaskbarLeft;
                int Position;
                int curleft;
                int curleft2;

                var TrayWndHandle = Win32.GetParent(Win32.GetParent((IntPtr)TaskList));

                var TrayWndClassName = new StringBuilder("", 256);
                Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256);

                // Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
                if (TrayWndClassName.ToString() == "ReBarWindow32")
                {
                    Win32.SendMessage(TrayWndHandle, 11, false, 0);
                    TrayWndHandle = Win32.GetParent(Win32.GetParent(Win32.GetParent((IntPtr)TaskList)));

                    var TrayNotify = Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "TrayNotifyWnd", null);
                    var accessible4 = MSAA.GetAccessibleObjectFromHandle(TrayNotify);
                    TrayNotifyPos = GetLocation(accessible4, 0);

                    Win32.SendMessage(Win32.GetParent(TrayWndHandle), 11, false, 0);
                }

                Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256);
                var accessible2 = MSAA.GetAccessibleObjectFromHandle(TrayWndHandle);

                var TrayWndPos = GetLocation(accessible2, 0);
                var RebarPos = GetLocation(accessible3, 0);

                // If the taskbar is still moving then wait until it's not (This will prevent unneeded calculations that trigger the animator)
                do
                {
                    curleft = TaskListPos.left;
                    TaskListPos = GetLocation(accessible, 0);

                    Thread.Sleep(30);
                    curleft2 = TaskListPos.left;
                }
                while (curleft != curleft2);

                // Calculate the exact width of the total icons
                TaskbarWidth = LastChildPos.left - TaskListPos.left; // 'TaskbarTotalHeight

                // Get info needed to calculate the position
                TrayWndLeft = Math.Abs(TrayWndPos.left);
                TrayWndWidth = Math.Abs(TrayWndPos.width);
                RebarWndLeft = Math.Abs(RebarPos.left);
                TaskbarLeft = Math.Abs(RebarWndLeft - TrayWndLeft);

                // Calculate new position
                if (TrayWndClassName.ToString() == "Shell_TrayWnd")
                {
                    Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarLeft));
                }
                else
                {
                    Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarWidth / 2d - TaskbarLeft)) + settings.TaskbarOffset;
                }

                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }
    }
}