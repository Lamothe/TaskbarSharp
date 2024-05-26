using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskbarSharp;

public class Program
{
    private static NotifyIcon notifyIcon = new();

    private static Form mainForm = new Form
    {
        Visible = false
    };

    public static void Main()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        var configuration = builder.Build();

        var settings = configuration.GetRequiredSection(nameof(TaskbarSharpSettings)).Get<TaskbarSharpSettings>();

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

            notifyIcon.Text = "TaskbarSharp";
            notifyIcon.Icon = My.Resources.Resources.icon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            notifyIcon.ContextMenuStrip.Items.Add("Toast", null, NotifyIconContextMenuToast_Click);
            notifyIcon.ContextMenuStrip.Items.Add("Restart", null, NotifyIconContextMenuRestart_Click);
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, NotifyIconContextMenuExit_Click);

            SystemEvents.DisplaySettingsChanged += (s, e) => Application.Restart();
            SystemEvents.SessionSwitch += (s, e) => Application.Restart();

            Task.Run(() => TaskbarCenterer.Looper(settings));
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
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

    private static void NotifyIconContextMenuToast_Click(object sender, EventArgs e)
    {
        notifyIcon.BalloonTipTitle = "TaskbarSharp";
        notifyIcon.BalloonTipText = "Hello!";
        notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(3000);
    }

    public static void Toaster(string message)
    {
        notifyIcon.BalloonTipTitle = "TaskbarSharp";
        notifyIcon.BalloonTipText = message;
        notifyIcon.Visible = true;
        notifyIcon.ShowBalloonTip(3000);
    }

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
}