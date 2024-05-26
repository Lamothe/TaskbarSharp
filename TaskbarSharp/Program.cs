using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TaskbarSharp.Common;

namespace TaskbarSharp;

public class Program
{
    public static NotifyIcon noty = new NotifyIcon();

    public static void Main()
    {
        try
        {
            // Set default settings
            Settings.TaskbarStyle = 0;
            Settings.PrimaryTaskbarOffset = 0;
            Settings.SecondaryTaskbarOffset = 0;
            Settings.CenterPrimaryOnly = 0;
            Settings.CenterSecondaryOnly = 0;
            Settings.AnimationStyle = "cubiceaseinout";
            Settings.AnimationSpeed = 300;
            Settings.LoopRefreshRate = 400;
            Settings.CenterInBetween = 0;
            Settings.DontCenterTaskbar = 0;
            Settings.FixToolbarsOnTrayChange = 1;
            Settings.OnBatteryAnimationStyle = "cubiceaseinout";
            Settings.OnBatteryLoopRefreshRate = 400;
            Settings.RevertZeroBeyondTray = 1;
            Settings.TaskbarRounding = 0;
            Settings.TaskbarSegments = 0;

            bool stopgiven = false;

            // Read the arguments for the settings
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (var argument in arguments)
            {
                string[] val = Strings.Split(argument, "=");
                if (argument.Contains("-stop"))
                {
                    stopgiven = true;
                }
                if (argument.Contains("-showstartmenu"))
                {
                    Win32.ShowStartMenu();
                    Environment.Exit(0);
                }
                if (argument.Contains("-console="))
                {
                    Win32.AllocConsole();
                    Settings.ConsoleEnabled = 1;
                }
                if (argument.Contains("-tbs="))
                {
                    Settings.TaskbarStyle = Conversions.ToInteger(val[1]);
                }

                if (argument.Contains("-color="))
                {
                    string colorval = val[1];
                    string[] colorsep = colorval.Split(";".ToCharArray());

                    Settings.TaskbarStyleRed = Conversions.ToInteger(colorsep[0]);
                    Settings.TaskbarStyleGreen = Conversions.ToInteger(colorsep[1]);
                    Settings.TaskbarStyleBlue = Conversions.ToInteger(colorsep[2]);
                    Settings.TaskbarStyleAlpha = Conversions.ToInteger(colorsep[3]);
                }

                if (argument.Contains("-ptbo="))
                {
                    Settings.PrimaryTaskbarOffset = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-stbo="))
                {
                    Settings.SecondaryTaskbarOffset = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-cpo="))
                {
                    Settings.CenterPrimaryOnly = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-cso="))
                {
                    Settings.CenterSecondaryOnly = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-as="))
                {
                    Settings.AnimationStyle = val[1];
                }
                if (argument.Contains("-asp="))
                {
                    Settings.AnimationSpeed = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-lr="))
                {
                    Settings.LoopRefreshRate = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-cib="))
                {
                    Settings.CenterInBetween = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-obas="))
                {
                    Settings.OnBatteryAnimationStyle = val[1];
                }
                if (argument.Contains("-oblr="))
                {
                    Settings.OnBatteryLoopRefreshRate = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-ftotc="))
                {
                    Settings.FixToolbarsOnTrayChange = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-rzbt="))
                {
                    Settings.RevertZeroBeyondTray = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-sr="))
                {
                    Settings.SkipResolution = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-sr2="))
                {
                    Settings.SkipResolution2 = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-sr3="))
                {
                    Settings.SkipResolution3 = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-dtbsowm="))
                {
                    Settings.DefaultTaskbarStyleOnWinMax = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-cfsa="))
                {
                    Settings.CheckFullscreenApp = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-dct="))
                {
                    Settings.DontCenterTaskbar = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-hps="))
                {
                    Settings.HidePrimaryStartButton = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-hss="))
                {
                    Settings.HideSecondaryStartButton = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-hpt="))
                {
                    Settings.HidePrimaryNotifyWnd = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-hst="))
                {
                    Settings.HideSecondaryNotifyWnd = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-sti="))
                {
                    // 'Settings.ShowTrayIcon = CInt(val(1))
                    Settings.ShowTrayIcon = 0;
                }
                if (argument.Contains("-tbsom="))
                {
                    Settings.TaskbarStyleOnMax = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-stsb="))
                {
                    Settings.StickyStartButton = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-tpop="))
                {
                    Settings.TotalPrimaryOpacity = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-tsop="))
                {
                    Settings.TotalSecondaryOpacity = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-tbr="))
                {
                    Settings.TaskbarRounding = Conversions.ToInteger(val[1]);
                }
                if (argument.Contains("-tbsg="))
                {
                    Settings.TaskbarSegments = Conversions.ToInteger(val[1]);
                }
            }

            // Kill every other running instance of TaskbarSharp
            try
            {
                foreach (Process prog in Process.GetProcessesByName("TaskbarSharp"))
                {
                    if (!(prog.Id == Process.GetCurrentProcess().Id))
                    {
                        prog.Kill();
                    }
                }
            }
            catch
            {
            }

            // If animation speed is lower than 1 then make it 1. Otherwise it will give an error.
            if (Settings.AnimationSpeed <= 1)
            {
                Settings.AnimationSpeed = 1;
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
            // Win32.PostMessage(Shell_TrayWnd, CUInt(&H111), CType(424, IntPtr), CType(vbNullString, IntPtr))
            while (Handle == default);


            var Win11Taskbar = Win32.FindWindowEx(Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0), (IntPtr)0, "Windows.UI.Composition.DesktopWindowContentBridge", null);
            if (!(Win11Taskbar == (IntPtr)0))
            {
                // Windows 11 Taskbar present
                Settings.DontCenterTaskbar = (int)Math.Round(Conversion.Val(1));
            }

            if (stopgiven == true)
            {
                noty.Visible = false;
                TaskbarCenter.RevertToZero();
                ResetTaskbarStyle();
                Environment.Exit(0);
            }

            if (Settings.ShowTrayIcon == 1)
            {
                TrayIconBuster.TrayIconBuster.RemovePhantomIcons();
            }

            // Just empty startup memory before starting
            ClearMemory();

            // Reset the taskbar style...
            ResetTaskbarStyle();

            if (Settings.ShowTrayIcon == 1)
            {
                noty.Text = "TaskbarSharp (L = Restart) (M = Config) (R = Stop)";
                noty.Icon = My.Resources.Resources.icon;
                noty.Visible = true;
            }

            noty.MouseClick += MnuRef_Click;

            // Start the TaskbarCenterer
            if (!(Settings.DontCenterTaskbar == 1))
            {
                var t1 = new Thread(TaskbarCenter.TaskbarCenterer);
                t1.Start();
            }

            // Start the TaskbarStyler if enabled
            if (Settings.TaskbarStyle == 1 | Settings.TaskbarStyle == 2 | Settings.TaskbarStyle == 3 | Settings.TaskbarStyle == 4 | Settings.TaskbarStyle == 5)
            {
                var t2 = new Thread(TaskbarStyle.TaskbarStyler);
                t2.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void Toaster(string message)
    {
        noty.BalloonTipTitle = "TaskbarSharp";
        noty.BalloonTipText = message;
        noty.Visible = true;
        noty.ShowBalloonTip(3000);
    }

    public static void MnuRef_Click(object sender, MouseEventArgs e)
    {

        if (e.Button == MouseButtons.Left)
        {
            noty.Visible = false;
            Application.Restart();
        }
        else if (e.Button == MouseButtons.Right)
        {
            noty.Visible = false;
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

    public static ArrayList windowHandles = new ArrayList();

    public static void ResetTaskbarStyle()
    {



        GetActiveWindows();



        var trays = new ArrayList();
        foreach (IntPtr trayWnd in windowHandles)
            // 'Console.WriteLine(trayWnd)
            trays.Add(trayWnd);

        foreach (IntPtr tray in trays)
        {
            var trayptr = tray;


            Win32.SendMessage(trayptr, Win32.WM_THEMECHANGED, true, 0);
            Win32.SendMessage(trayptr, Win32.WM_DWMCOLORIZATIONCOLORCHANGED, true, 0);
            Win32.SendMessage(trayptr, Win32.WM_DWMCOMPOSITIONCHANGED, true, 0);



            var tt = new Win32.RECT();
            Win32.GetClientRect(trayptr, ref tt);



            // 'Win32.SetWindowRgn(CType(trayptr, IntPtr), Win32.CreateRoundRectRgn(-1, -1, tt.Right + 1, tt.Bottom - tt.Top + 1, -1, -1), True)
            Win32.SetWindowRgn(trayptr, Win32.CreateRectRgn(tt.Left, tt.Top, tt.Right, tt.Bottom), true);







        }


    }

    public static void RestartExplorer()
    {
        foreach (var MyProcess in Process.GetProcessesByName("explorer"))
            MyProcess.Kill();
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