using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TaskbarSharp;

public class Program
{
    public static NotifyIcon notifyIcon = new();

    public static void Main()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        var configuration = builder.Build();

        var settings = configuration.GetRequiredSection(nameof(TaskbarSharpSettings)).Get<TaskbarSharpSettings>();

        try
        {
            bool stopgiven = false;

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

            // Makes the animations run smoother
            var currentProcess = Process.GetCurrentProcess();
            currentProcess.PriorityClass = ProcessPriorityClass.Idle;

            // Prevent wrong position calculations
            Win32.SetProcessDpiAwareness(Win32.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);

            // Wait for Shell_TrayWnd
            IntPtr Handle;
            do
            {
                Console.WriteLine("Waiting for Shell_TrayWnd");
                Handle = default;
                Thread.Sleep(250);
                var Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
                var TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, (IntPtr)0, "TrayNotifyWnd", null);
                var ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, (IntPtr)0, "ReBarWindow32", null);
                var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                var MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);
                Handle = MSTaskListWClass;
            }
            // Lock the Taskbar
            while (Handle == default);

            var Win11Taskbar = Win32.FindWindowEx(Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0), (IntPtr)0, "Windows.UI.Composition.DesktopWindowContentBridge", null);

            if (stopgiven == true)
            {
                notifyIcon.Visible = false;
                TaskbarCenter.RevertToZero();
                ResetTaskbarStyle();
                Environment.Exit(0);
            }

            TrayIconBuster.TrayIconBuster.RemovePhantomIcons();

            // Just empty startup memory before starting
            ClearMemory();

            // Reset the taskbar style...
            ResetTaskbarStyle();

            notifyIcon.Text = "TaskbarSharp (L = Restart) (M = Config) (R = Stop)";
            notifyIcon.Icon = My.Resources.Resources.icon;
            notifyIcon.Visible = true;

            notifyIcon.MouseClick += MnuRef_Click;

            // Start the TaskbarCenterer
            var t1 = new Thread(TaskbarCenter.TaskbarCenterer);
            t1.Start(settings);

            // Start the TaskbarStyler if enabled
            var t2 = new Thread(TaskbarStyle.TaskbarStyler);
            t2.Start(settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void Toaster(string message)
    {
        notifyIcon.BalloonTipTitle = "TaskbarSharp";
        notifyIcon.BalloonTipText = message;
        notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(3000);
    }

    public static void MnuRef_Click(object sender, MouseEventArgs e)
    {

        if (e.Button == MouseButtons.Left)
        {
            notifyIcon.Visible = false;
            Application.Restart();
        }
        else if (e.Button == MouseButtons.Right)
        {
            notifyIcon.Visible = false;
            TaskbarCenter.RevertToZero();
            ResetTaskbarStyle();
            Environment.Exit(0);
        }
        else if (e.Button == MouseButtons.Middle)
        {
            try
            {
                Process.Start("TaskbarSharp.Configurator.exe");
            }
            catch
            {
            }
        }

    }

    #region Commands

    public static System.Collections.ObjectModel.Collection<IntPtr> ActiveWindows = [];

    public static System.Collections.ObjectModel.Collection<IntPtr> GetActiveWindows()
    {
        windowHandles.Clear();
        Win32.EnumWindows(Enumerator, 0);

        bool mainTaskbarFound = false;
        bool secTaskbarFound = false;

        foreach (var Taskbar in windowHandles)
        {
            var sClassName = new StringBuilder("", 256);
            Win32.GetClassName((IntPtr)Taskbar, sClassName, 256);
            if (sClassName.ToString() == "Shell_TrayWnd")
            {
                mainTaskbarFound = true;
            }
            if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
            {
                secTaskbarFound = true;
            }
            Console.WriteLine("=" + mainTaskbarFound);
        }

        if (mainTaskbarFound == false)
        {
            try
            {
                windowHandles.Add(Win32.FindWindow("Shell_TrayWnd", null));
            }
            catch
            {
            }
        }

        if (secTaskbarFound == false)
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

    public static ArrayList windowHandles = new ArrayList();

    public static void ResetTaskbarStyle()
    {
        GetActiveWindows();

        var trays = new ArrayList();
        foreach (IntPtr trayWnd in windowHandles)
        {
            trays.Add(trayWnd);
        }

        foreach (IntPtr tray in trays)
        {
            var trayptr = tray;

            Win32.SendMessage(trayptr, Win32.WM_THEMECHANGED, true, 0);
            Win32.SendMessage(trayptr, Win32.WM_DWMCOLORIZATIONCOLORCHANGED, true, 0);
            Win32.SendMessage(trayptr, Win32.WM_DWMCOMPOSITIONCHANGED, true, 0);

            var tt = new Win32.RECT();
            Win32.GetClientRect(trayptr, ref tt);
            Win32.SetWindowRgn(trayptr, Win32.CreateRectRgn(tt.Left, tt.Top, tt.Right, tt.Bottom), true);
        }
    }

    public static int ClearMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return Win32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

    #endregion
}