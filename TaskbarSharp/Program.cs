using Accessibility;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskbarSharp;

public class Program
{
    private const int LoopRefreshRate = 400;
    private static Guid _guidAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");

    private static NotifyIcon notifyIcon = new();
    private static ArrayList windowHandles = [];
    private static Form mainForm = new();

    public static void Main()
    {
        try
        {
            // Kill every other running instance of TaskbarSharp
            try
            {
                foreach (var process in Process.GetProcessesByName("TaskbarSharp"))
                {
                    if (!(process.Id == Environment.ProcessId))
                    {
                        process.Kill();
                    }
                }
            }
            catch
            {
            }

            // Prevent wrong position calculations
            Win32.SetProcessDpiAwareness(Win32.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);

            notifyIcon.Text = "TaskbarSharp";
            notifyIcon.Icon = My.Resources.Resources.icon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Restart", null, NotifyIconContextMenuRestart_Click);
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, NotifyIconContextMenuExit_Click);

            SystemEvents.DisplaySettingsChanged += (s, e) => Application.Restart();
            SystemEvents.SessionSwitch += (s, e) => Application.Restart();

            Task.Run(() => Looper());
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }

        mainForm.Activated += MainForm_Activated;
        Application.Run(new ApplicationContext(mainForm));

        notifyIcon.Visible = false;
    }

    private static void MainForm_Activated(object sender, EventArgs e)
    {
        (sender as Form).Hide();
    }

    private static void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        notifyIcon.ContextMenuStrip.Show();
    }

    private static void NotifyIconContextMenuRestart_Click(object sender, EventArgs e)
    {
        Application.Restart();
    }

    private static void NotifyIconContextMenuExit_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    public struct RectangleX
    {
        public int Left;
        public int Top;
        public int Width;
        public int Height;
    }

    public static RectangleX GetLocation(IAccessible accessible, int idChild)
    {
        var rect = new RectangleX();
        if (!(accessible == null))
        {
            accessible.accLocation(out rect.Left, out rect.Top, out rect.Width, out rect.Height, idChild);
        }
        return rect;
    }

    public static void Looper()
    {
        try
        {
            while (true)
            {
                try
                {
                    Task.Run(() => PositionCalculator());
                    Thread.Sleep(LoopRefreshRate);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    Application.Restart();
                }
            }
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static void PositionCalculator()
    {
        try
        {
            var shellTrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", 0);
            var TrayNotifyWnd = Win32.FindWindowEx(shellTrayWnd, 0, "TrayNotifyWnd", null);

            var reBarWindow32 = Win32.FindWindowEx(shellTrayWnd, 0, "ReBarWindow32", null);
            var msTaskSwWClass = Win32.FindWindowEx(reBarWindow32, 0, "MSTaskSwWClass", null);
            var msTaskListWClass = Win32.FindWindowEx(msTaskSwWClass, 0, "MSTaskListWClass", null);

            if (msTaskListWClass == IntPtr.Zero)
            {
                ShowError("TaskbarSharp: Could not find the handle of the taskbar.");
                Thread.Sleep(1000);
                return;
            }

            var taskList = msTaskListWClass;

            var lastChildPos = default(RectangleX);
            var trayNotifyPos = default(RectangleX);
            var sClassName = new StringBuilder("", 256);
            Win32.GetClassName(msTaskListWClass, sClassName, 256);
            RectangleX taskListPos;

            var accessible = GetAccessibleObjectFromHandle(taskList);
            var children = GetAccessibleChildren(accessible);

            taskListPos = GetLocation(accessible, 0);

            foreach (var child in children)
            {
                if (child?.get_accRole(0) as int? == 22) // 0x16 = toolbar
                {
                    lastChildPos = GetLocation(child, GetAccessibleChildren(child).Length);
                    break;
                }
            }

            var rebarHandle = Win32.GetParent((IntPtr)taskList);
            var accessible3 = GetAccessibleObjectFromHandle(rebarHandle);

            var rebarClassName = new StringBuilder("", 256);
            Win32.GetClassName(rebarHandle, rebarClassName, 256);

            int taskbarWidth;
            int trayWndLeft;
            int trayWndWidth;
            int rebarWndLeft;
            int taskbarLeft;
            int position;
            int curleft;
            int curleft2;

            var TrayWndHandle = Win32.GetParent(Win32.GetParent((IntPtr)taskList));

            var TrayWndClassName = new StringBuilder("", 256);
            Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256);

            // Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
            if (TrayWndClassName.ToString() == "ReBarWindow32")
            {
                Win32.SendMessage(TrayWndHandle, 11, false, 0);
                TrayWndHandle = Win32.GetParent(Win32.GetParent(Win32.GetParent((IntPtr)taskList)));

                var TrayNotify = Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "TrayNotifyWnd", null);
                var accessible4 = GetAccessibleObjectFromHandle(TrayNotify);
                trayNotifyPos = GetLocation(accessible4, 0);

                Win32.SendMessage(Win32.GetParent(TrayWndHandle), 11, false, 0);
            }

            Win32.GetClassName(TrayWndHandle, TrayWndClassName, 256);
            var accessible2 = GetAccessibleObjectFromHandle(TrayWndHandle);

            var trayWndPos = GetLocation(accessible2, 0);
            var rebarPos = GetLocation(accessible3, 0);

            // If the taskbar is still moving then wait until it's not (This will prevent unneeded calculations that trigger the animator)
            do
            {
                curleft = taskListPos.Left;
                taskListPos = GetLocation(accessible, 0);

                Thread.Sleep(30);
                curleft2 = taskListPos.Left;
            }
            while (curleft != curleft2);

            // Calculate the exact width of the total icons
            taskbarWidth = lastChildPos.Left - taskListPos.Left;

            // Get info needed to calculate the position
            trayWndLeft = Math.Abs(trayWndPos.Left);
            trayWndWidth = Math.Abs(trayWndPos.Width);
            rebarWndLeft = Math.Abs(rebarPos.Left);
            taskbarLeft = Math.Abs(rebarWndLeft - trayWndLeft);

            // Calculate new position
            position = Math.Abs((int)Math.Round(trayWndWidth / 2d - taskbarLeft));
            Win32.SetWindowPos((IntPtr)taskList, IntPtr.Zero, position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private static IAccessible[] GetAccessibleChildren(IAccessible objAccessible)
    {
        int childCount;
        try
        {
            childCount = objAccessible.accChildCount;
        }
        catch (Exception ex)
        {
            ShowError(ex);
            childCount = 0;
        }

        var accObjects = new IAccessible[childCount];
        int count = 0;

        if (childCount != 0)
        {
            Win32.AccessibleChildren(objAccessible, 0, childCount, accObjects, ref count);
        }

        return accObjects;
    }

    private static IAccessible GetAccessibleObjectFromHandle(IntPtr hwnd)
    {
        var accObject = new object();
        IAccessible objAccessible = null;
        if (hwnd != (IntPtr)0)
        {
            Win32.AccessibleObjectFromWindow((int)hwnd, 0, ref _guidAccessible, ref accObject);
            objAccessible = (IAccessible)accObject;
        }

        return objAccessible;
    }

    private static void ShowError(Exception ex)
    {
        ShowError(ex.Message);
    }

    private static void ShowError(string message)
    {
        notifyIcon.BalloonTipTitle = "TaskbarSharp Error";
        notifyIcon.BalloonTipText = message;
        notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(3000);
    }
}