using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskbarSharp;

const int LoopRefreshRate = 10000;
const uint SWP_NOSIZE = 1U;
const uint SWP_ASYNCWINDOWPOS = 16384U;
const uint SWP_NOACTIVATE = 16U;
const uint SWP_NOSENDCHANGING = 1024U;
const uint SWP_NOZORDER = 4U;

NotifyIcon notifyIcon = new();
Form mainForm = new();

[DllImport("user32.dll", SetLastError = true)]
static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

[DllImport("SHCore.dll", SetLastError = true)]
static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true, CharSet = CharSet.Auto)]
static extern IntPtr FindWindowByClass(string lpClassName, IntPtr zero);

[DllImport("user32.dll")]
static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle2D rectangle);

// Application starts here!
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
var looperTask = Task.Run(async () => await PositionCalculatorLoop(), cancellationTokenSource.Token);

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

async Task PositionCalculatorLoop()
{
    while (!cancellationTokenSource.IsCancellationRequested)
    {
        try
        {
            var shellTrayWnd = FindWindowByClass("Shell_TrayWnd", 0);
            var reBarWindow32 = FindWindowEx(shellTrayWnd, 0, "ReBarWindow32", null);
            var msTaskSwWClass = FindWindowEx(reBarWindow32, 0, "MSTaskSwWClass", null);

            var shellTrayWndRect = new Rectangle2D();
            GetWindowRect(shellTrayWnd, ref shellTrayWndRect);

            var msTaskSwWClassRect = new Rectangle2D();
            GetWindowRect(msTaskSwWClass, ref msTaskSwWClassRect);

            // Calculate new position
            var position = Math.Abs((int)Math.Round(shellTrayWndRect.Width / 2d - (msTaskSwWClassRect.Width - msTaskSwWClassRect.Left) / 2d));
            SetWindowPos(msTaskSwWClass, IntPtr.Zero, position, 0, 0, 0, SWP_NOSIZE | SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOZORDER | SWP_NOSENDCHANGING);

            await Task.Delay(LoopRefreshRate);
        }
        catch (Exception ex)
        {
            ShowException(ex);
        }
    }
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

struct Rectangle2D
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
