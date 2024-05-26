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
    public static int TaskbarCount;

    public static ArrayList windowHandles = new ArrayList();

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
        public int Left;
        public int Top;
        public int Width;
        public int Height;
    }

    public static RectangleX GetLocation(IAccessible acc, int idChild)
    {
        var rect = new RectangleX();
        if (!(acc == null))
        {
            acc.accLocation(out rect.Left, out rect.Top, out rect.Width, out rect.Height, idChild);
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
                    UI.ShowError("TaskbarSharp: Could not find the handle of the taskbar.");
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

                    // Go through each taskbar and result in a unique string containing the current state

                    int i = 0;

                    foreach (var TaskList in TaskObjects)
                    {
                        IAccessible[] children = MSAA.GetAccessibleChildren(TaskList);

                        var TaskListPos = GetLocation(TaskList, 0);

                        int tH = TaskListPos.Height;
                        int tW = TaskListPos.Width;

                        foreach (IAccessible childx in children)
                        {
                            if (childx?.get_accRole(0) as int? == 22) // 0x16 = toolbar
                            {
                                LastChildPos = GetLocation(childx, MSAA.GetAccessibleChildren(childx).Length);
                                break;
                            }
                        }

                        int cL = LastChildPos.Left;
                        int cT = LastChildPos.Top;
                        int cW = LastChildPos.Width;
                        int cH = LastChildPos.Height;

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
                    Application.Restart();
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
            var taskbars = new ArrayList();

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

                taskbars.Add(MSTaskListWClass);
            }

            // Calculate Position for every taskbar and trigger the animator
            var LastChildPos = default(RectangleX);
            var TrayNotifyPos = default(RectangleX);
            foreach (var TaskList in taskbars)
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
                    curleft = TaskListPos.Left;
                    TaskListPos = GetLocation(accessible, 0);

                    Thread.Sleep(30);
                    curleft2 = TaskListPos.Left;
                }
                while (curleft != curleft2);

                // Calculate the exact width of the total icons
                TaskbarWidth = LastChildPos.Left - TaskListPos.Left; // 'TaskbarTotalHeight

                // Get info needed to calculate the position
                TrayWndLeft = Math.Abs(TrayWndPos.Left);
                TrayWndWidth = Math.Abs(TrayWndPos.Width);
                RebarWndLeft = Math.Abs(RebarPos.Left);
                TaskbarLeft = Math.Abs(RebarWndLeft - TrayWndLeft);

                // Calculate new position
                Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarLeft));
                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }
    }
}