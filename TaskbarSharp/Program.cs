using Accessibility;
using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskbarSharp;

const int LoopRefreshRate = 2000;
const uint SWP_NOSIZE = 1U;
const uint SWP_ASYNCWINDOWPOS = 16384U;
const uint SWP_NOACTIVATE = 16U;
const uint SWP_NOSENDCHANGING = 1024U;
const uint SWP_NOZORDER = 4U;

Guid _guidAccessible = new("{618736E0-3C3D-11CF-810C-00AA00389B71}");
NotifyIcon notifyIcon = new();
ArrayList windowHandles = [];
Form mainForm = new();

[DllImport("user32.dll", SetLastError = true)]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

[DllImport("user32.dll", CharSet = CharSet.Auto)]
static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
static extern IntPtr GetParent(IntPtr hWnd);

[DllImport("SHCore.dll", SetLastError = true)]
static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Auto)]
static extern IntPtr FindWindowByClass(string lpClassName, IntPtr zero);

[DllImport("user32.dll")]
static extern uint SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

[DllImport("oleacc.dll")]
static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, object[] rgvarChildren, ref int pcObtained);

[DllImport("oleacc")]
static extern uint AccessibleObjectFromWindow(IntPtr Hwnd, int dwId, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

// Application starts here!
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
SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);

notifyIcon.Text = "TaskbarSharp";
notifyIcon.Icon = Resources.Icon;
notifyIcon.Visible = true;
notifyIcon.MouseClick += NotifyIcon_MouseClick;
notifyIcon.ContextMenuStrip = new ContextMenuStrip();
notifyIcon.ContextMenuStrip.Items.Add("Restart", null, NotifyIconContextMenuRestart_Click);
notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
notifyIcon.ContextMenuStrip.Items.Add("Exit", null, NotifyIconContextMenuExit_Click);

SystemEvents.DisplaySettingsChanged += (s, e) => Application.Restart();
SystemEvents.SessionSwitch += (s, e) => Application.Restart();

var cancellationTokenSource = new CancellationTokenSource();
var looperTask = Task.Run(() => PositionCalculatorLoop(), cancellationTokenSource.Token);

mainForm.Activated += MainForm_Activated;

Application.ThreadException += (s, e) => { ShowException(e.Exception); };
Application.Run(new ApplicationContext(mainForm));

// If the application has been terminated.
notifyIcon.Visible = false;
cancellationTokenSource.Cancel();
looperTask.Wait();
// Application ends here!

void MainForm_Activated(object sender, EventArgs e)
{
    (sender as Form).Hide();
}

void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
{
    notifyIcon.ContextMenuStrip.Show();
}

static void NotifyIconContextMenuRestart_Click(object sender, EventArgs e)
{
    Application.Restart();
}

static void NotifyIconContextMenuExit_Click(object sender, EventArgs e)
{
    Application.Exit();
}

RectangleX GetLocation(IAccessible accessible, int idChild)
{
    var rect = new RectangleX();
    accessible?.accLocation(out rect.Left, out rect.Top, out rect.Width, out rect.Height, idChild);
    return rect;
}

void PositionCalculatorLoop()
{
    while (!cancellationTokenSource.IsCancellationRequested)
    {
        try
        {
            var shellTrayWnd = FindWindowByClass("Shell_TrayWnd", 0);
            var TrayNotifyWnd = FindWindowEx(shellTrayWnd, 0, "TrayNotifyWnd", null);

            var reBarWindow32 = FindWindowEx(shellTrayWnd, 0, "ReBarWindow32", null);
            var msTaskSwWClass = FindWindowEx(reBarWindow32, 0, "MSTaskSwWClass", null);
            var msTaskListWClass = FindWindowEx(msTaskSwWClass, 0, "MSTaskListWClass", null);

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
            CheckNonZeroResult(GetClassName(msTaskListWClass, sClassName, 256), nameof(GetClassName));
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

            var rebarHandle = GetParent(taskList);
            var accessible3 = GetAccessibleObjectFromHandle(rebarHandle);

            var rebarClassName = new StringBuilder("", 256);
            var _ = GetClassName(rebarHandle, rebarClassName, 256);

            int taskbarWidth;
            int trayWndLeft;
            int trayWndWidth;
            int rebarWndLeft;
            int taskbarLeft;
            int position;
            int curleft;
            int curleft2;

            var TrayWndHandle = GetParent(GetParent((IntPtr)taskList));

            var TrayWndClassName = new StringBuilder("", 256);
            CheckNonZeroResult(GetClassName(TrayWndHandle, TrayWndClassName, 256), nameof(GetClassName));

            // Check if TrayWnd = wrong. if it is, correct it (This will be the primary taskbar which should be Shell_TrayWnd)
            if (TrayWndClassName.ToString() == "ReBarWindow32")
            {
                CheckWin32Result(SendMessage(TrayWndHandle, 11, false, 0), nameof(SendMessage));
                TrayWndHandle = GetParent(GetParent(GetParent(taskList)));

                var TrayNotify = FindWindowEx(TrayWndHandle, 0, "TrayNotifyWnd", null);
                var accessible4 = GetAccessibleObjectFromHandle(TrayNotify);
                trayNotifyPos = GetLocation(accessible4, 0);

                CheckWin32Result(SendMessage(GetParent(TrayWndHandle), 11, false, 0), nameof(SendMessage));
            }

            CheckNonZeroResult(GetClassName(TrayWndHandle, TrayWndClassName, 256), nameof(GetClassName));
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
            SetWindowPos((IntPtr)taskList, IntPtr.Zero, position, 0, 0, 0, SWP_NOSIZE | SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOZORDER | SWP_NOSENDCHANGING);
            Thread.Sleep(LoopRefreshRate);
        }
        catch (Exception ex)
        {
            ShowException(ex);
            Application.Restart();
        }
    }
}

IAccessible[] GetAccessibleChildren(IAccessible objAccessible)
{
    int childCount;
    try
    {
        childCount = objAccessible.accChildCount;
    }
    catch (Exception ex)
    {
        ShowException(ex);
        childCount = 0;
    }

    var accObjects = new IAccessible[childCount];
    int count = 0;

    if (childCount != 0)
    {
        CheckWin32Result(AccessibleChildren(objAccessible, 0, childCount, accObjects, ref count), nameof(AccessibleChildren));
    }

    return accObjects;
}

IAccessible GetAccessibleObjectFromHandle(IntPtr hwnd)
{
    var accObject = new object();
    IAccessible objAccessible = null;
    if (hwnd != 0)
    {
        CheckWin32Result(AccessibleObjectFromWindow(hwnd, 0, ref _guidAccessible, ref accObject), nameof(AccessibleObjectFromWindow));
        objAccessible = (IAccessible)accObject;
    }

    return objAccessible;
}

void ShowError(string message)
{
    notifyIcon.BalloonTipTitle = "TaskbarSharp Error";
    notifyIcon.BalloonTipText = message;
    notifyIcon.Visible = true;
    notifyIcon.ShowBalloonTip(3000);
}

void ShowException(Exception ex)
{
    ShowError(ex.Message);
}

void CheckWin32Result(uint result, string functionName)
{
    if (result != 0L)
    {
        throw new Win32Exception(Marshal.GetLastWin32Error(), $"The call to {functionName} failed");
    }
}

void CheckNonZeroResult(int result, string functionName)
{
    if (result <= 0)
    {
        throw new Exception($"The call to {functionName} returned no results");
    }
}

struct RectangleX
{
    public int Left;
    public int Top;
    public int Width;
    public int Height;
}

enum PROCESS_DPI_AWARENESS
{
    Process_Per_Monitor_DPI_Aware = 2
}
