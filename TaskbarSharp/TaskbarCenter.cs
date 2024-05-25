using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Accessibility;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32;

namespace TaskbarX
{

    public class TaskbarCenter
    {

        #region Values

        public static bool ScreensChanged;

        public static int TaskbarCount;

        public static ArrayList windowHandles = new ArrayList();

        public static bool trayfixed;
        public static IntPtr setposhwnd;
        public static int setpospos;
        public static string setposori;

        public static string initposcalc;
        public static bool initposcalcready;

        public static bool isanimating;

        public static UserPreferenceChangedEventHandler UserPref = new UserPreferenceChangedEventHandler(HandlePrefChange);

        #endregion

        public static void TaskbarCenterer()
        {
            RevertToZero();

            SystemEvents.DisplaySettingsChanged += DPChange;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            // Start the Looper
            var t1 = new Thread(Looper);
            t1.Start();

            // Start the TrayLoopFix
            if (Settings.FixToolbarsOnTrayChange == 1)
            {
                var t2 = new Thread(TrayLoopFix);
                t2.Start();
            }
        }

        #region Commands

        [DllImport("user32")]
        public static extern int EnumWindows(CallBack Adress, int y);

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        public static System.Collections.ObjectModel.Collection<IntPtr> ActiveWindows = new System.Collections.ObjectModel.Collection<IntPtr>();

        public static System.Collections.ObjectModel.Collection<IntPtr> GetActiveWindows()
        {
            windowHandles.Clear();
            EnumWindows(Enumerator, 0);

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

        public static void SetPos()
        {
            if (setposori == "H")
            {
                do
                {
                    Win32.SetWindowPos(setposhwnd, IntPtr.Zero, setpospos, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                    if (isanimating == true)
                    {
                        break;
                    }
                }
                while (trayfixed != true);
            }
            else
            {
                do
                {
                    Win32.SetWindowPos(setposhwnd, IntPtr.Zero, 0, setpospos, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                    if (isanimating == true)
                    {
                        break;
                    }
                }
                while (trayfixed != true);
            }
        }

        public static void Animate(IntPtr hwnd, int oldpos, string orient, EasingDelegate easing, int valueToReach, int duration, bool isPrimary, int width)
        {
            try
            {
                var t1 = new Thread(() => TaskbarAnimate.Animate(hwnd, oldpos, orient, easing, valueToReach, duration, isPrimary, width));
                t1.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("@Animation Call | " + ex.Message);
            }
        }

        public static bool revertcycle;

        public static void RevertToZero()
        {
            // Put all taskbars back to default position
            GetActiveWindows();

            foreach (Process prog in Process.GetProcesses())
            {
                if (prog.ProcessName == "AcrylicPanel")
                {
                    prog.Kill();
                }
            }

            var Taskbars = new ArrayList();

            foreach (var Taskbar in windowHandles)
            {

                var sClassName = new StringBuilder("", 256);
                Win32.GetClassName((IntPtr)Taskbar, sClassName, 256);

                var MSTaskListWClass = IntPtr.Zero;

                if (sClassName.ToString() == "Shell_TrayWnd")
                {
                    var ReBarWindow32 = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "ReBarWindow32", null);
                    var MStart = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "Start", null);
                    Win32.ShowWindow(MStart, Win32.ShowWindowCommands.Show);

                    var MTray = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "TrayNotifyWnd", null);
                    Win32.SetWindowLong(MTray, (Win32.WindowStyles)Win32.GWL_STYLE, 0x56000000);
                    Win32.SetWindowLong(MTray, (Win32.WindowStyles)Win32.GWL_EXSTYLE, 0x2000);
                    Win32.SendMessage(MTray, 11, true, 0);
                    Win32.ShowWindow(MTray, Win32.ShowWindowCommands.Show);

                    var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                    MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);

                }

                if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
                {
                    var WorkerW = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "WorkerW", null);
                    var SStart = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "Start", null);
                    Win32.ShowWindow(SStart, Win32.ShowWindowCommands.Show);
                    var STray = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "ClockButton", null);
                    Win32.ShowWindow(STray, Win32.ShowWindowCommands.Show);
                    MSTaskListWClass = Win32.FindWindowEx(WorkerW, (IntPtr)0, "MSTaskListWClass", null);
                }

                if (MSTaskListWClass != IntPtr.Zero)
                {
                    Taskbars.Add(MSTaskListWClass);
                }
            }

            foreach (var TaskList in Taskbars)
            {
                Win32.SendMessage(Win32.GetParent(Win32.GetParent((IntPtr)TaskList)), 11, true, 0);
                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
            }
        }

        #endregion

        #region Events

        public static void HandlePrefChange(object sender, UserPreferenceChangedEventArgs e)
        {
            // ' Console.WriteLine(e.Category)
            if (e.Category == UserPreferenceCategory.General)
            {

                Console.WriteLine();
                Thread.Sleep(1000);
                // Wait for Shell_TrayWnd
                IntPtr Handle;
                do
                {
                    Console.WriteLine("Waiting for Shell_TrayWnd");

                    Thread.Sleep(250);
                    Handle = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
                }
                while (Handle == default);

                Application.Restart();

            }
        }

        public static void DPChange(object sender, EventArgs e)
        {
            Console.WriteLine();
            Thread.Sleep(1000);
            // Wait for Shell_TrayWnd
            IntPtr Handle;
            do
            {
                Console.WriteLine("Waiting for Shell_TrayWnd");

                Thread.Sleep(250);
                Handle = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
            }
            while (Handle == default);

            Application.Restart();
        }

        public static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Console.WriteLine();
            Thread.Sleep(1000);
            // Wait for Shell_TrayWnd
            IntPtr Handle;
            do
            {
                Console.WriteLine("Waiting for Shell_TrayWnd");

                Thread.Sleep(250);
                Handle = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
            }
            while (Handle == default);

            Application.Restart();
        }

        #endregion


        #region Looper

        public static void Looper()
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

                        if (Settings.TotalPrimaryOpacity != default)
                        {
                            Win32.SetWindowLong((IntPtr)Taskbar, (Win32.WindowStyles)Win32.GWL_EXSTYLE, 0x80000);
                            Win32.SetLayeredWindowAttributes((IntPtr)Taskbar, 0U, (byte)Math.Round(255d / 100d * (byte)Settings.TotalPrimaryOpacity), 0x2U);
                        }

                        if (Settings.HidePrimaryStartButton == 1)
                        {
                            var MStart = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "Start", null);
                            Win32.ShowWindow(MStart, Win32.ShowWindowCommands.Hide);
                            Win32.SetLayeredWindowAttributes(MStart, 0U, 0, 0x2U);
                        }

                        if (Settings.HidePrimaryNotifyWnd == 1)
                        {
                            var MTray = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "TrayNotifyWnd", null);
                            Win32.ShowWindow(MTray, Win32.ShowWindowCommands.Hide);
                            Win32.SetWindowLong(MTray, (Win32.WindowStyles)Win32.GWL_STYLE, 0x7E000000);
                            Win32.SetWindowLong(MTray, (Win32.WindowStyles)Win32.GWL_EXSTYLE, 0x80000);
                            Win32.SendMessage(MTray, 11, false, 0);
                            Win32.SetLayeredWindowAttributes(MTray, 0U, 0, 0x2U);
                        }

                        var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                        MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);
                    }

                    if (sClassName.ToString() == "Shell_SecondaryTrayWnd")
                    {
                        var WorkerW = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "WorkerW", null);

                        if (Settings.TotalSecondaryOpacity != default)
                        {
                            Win32.SetWindowLong((IntPtr)Taskbar, (Win32.WindowStyles)Win32.GWL_EXSTYLE, 0x80000);
                            Win32.SetLayeredWindowAttributes((IntPtr)Taskbar, 0U, (byte)Math.Round(255d / 100d * (byte)Settings.TotalSecondaryOpacity), 0x2U);
                        }

                        if (Settings.HideSecondaryStartButton == 1)
                        {
                            var SStart = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "Start", null);
                            Win32.ShowWindow(SStart, Win32.ShowWindowCommands.Hide);
                            Win32.SetLayeredWindowAttributes(SStart, 0U, 0, 0x2U);
                        }

                        if (Settings.HideSecondaryNotifyWnd == 1)
                        {
                            var STray = Win32.FindWindowEx((IntPtr)Taskbar, (IntPtr)0, "ClockButton", null);
                            Win32.ShowWindow(STray, Win32.ShowWindowCommands.Hide);
                            Win32.SetLayeredWindowAttributes(STray, 0U, 0, 0x2U);
                        }

                        MSTaskListWClass = Win32.FindWindowEx(WorkerW, (IntPtr)0, "MSTaskListWClass", null);
                    }

                    if (MSTaskListWClass == IntPtr.Zero)
                    {
                        MessageBox.Show("TaskbarSharp: Could not find the handle of the taskbar. Your current version or build of Windows may not be supported.");
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

                        if (!(Settings.SkipResolution == 0))
                        {
                            if (Screen.PrimaryScreen.Bounds.Width == Settings.SkipResolution)
                            {
                                RevertToZero();
                                break;
                            }
                        }

                        if (!(Settings.SkipResolution2 == 0))
                        {
                            if (Screen.PrimaryScreen.Bounds.Width == Settings.SkipResolution2)
                            {
                                RevertToZero();
                                break;
                            }
                        }

                        if (!(Settings.SkipResolution3 == 0))
                        {
                            if (Screen.PrimaryScreen.Bounds.Width == Settings.SkipResolution3)
                            {
                                RevertToZero();
                                break;
                            }
                        }

                        if (Settings.CheckFullscreenApp == 1)
                        {
                            var activewindow = Win32.GetForegroundWindow();
                            var curmonx = Screen.FromHandle(activewindow);
                            var activewindowsize = new Win32.RECT();
                            Win32.GetWindowRect(activewindow, ref activewindowsize);

                            if (activewindowsize.Top == curmonx.Bounds.Top & activewindowsize.Bottom == curmonx.Bounds.Bottom & activewindowsize.Left == curmonx.Bounds.Left & activewindowsize.Right == curmonx.Bounds.Right)
                            {
                                Console.WriteLine("Fullscreen App detected " + activewindowsize.Bottom + "," + activewindowsize.Top + "," + activewindowsize.Left + "," + activewindowsize.Right);

                                Settings.Pause = true;
                                do
                                {
                                    Thread.Sleep(500);
                                    activewindow = Win32.GetForegroundWindow();
                                    Win32.GetWindowRect(activewindow, ref activewindowsize);
                                    Thread.Sleep(500);
                                }

                                while (activewindowsize.Top == curmonx.Bounds.Top & activewindowsize.Bottom == curmonx.Bounds.Bottom & activewindowsize.Left == curmonx.Bounds.Left & activewindowsize.Right == curmonx.Bounds.Right);
                                Console.WriteLine("Fullscreen App deactivated");

                                Settings.Pause = false;
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
                                MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                // Current taskbar is empty go to next taskbar.
                                // 'Continue For
                            }

                            string Orientation;
                            int TaskbarCount;
                            int TrayWndSize;

                            // Get current taskbar orientation (H = Horizontal | V = Vertical)
                            if (tH >= tW)
                            {
                                Orientation = "V";
                            }
                            else
                            {
                                Orientation = "H";
                            }

                            // Get the end position of the last icon in the taskbar
                            if (Orientation == "H")
                            {
                                TaskbarCount = cL + cW;
                            }
                            else
                            {
                                TaskbarCount = cT + cH;
                            }

                            // Gets the width of the whole taskbars placeholder
                            if (Orientation == "H")
                            {
                                TrayWndSize = tW;
                            }
                            else
                            {
                                TrayWndSize = tH;
                            }

                            // Put the results into a string ready to be matched for differences with last loop
                            results = results + Orientation + TaskbarCount + TrayWndSize;

                            initposcalcready = true;

                            i += 1;
                        }

                        if (!((results ?? "") == (oldresults ?? "")))
                        {
                            // Something has changed we can now calculate the new position for each taskbar

                            initposcalcready = false;
                            initposcalc = results;

                            // Start the PositionCalculator
                            var t3 = new Thread(InitPositionCalculator);
                            t3.Start();
                        }

                        // Save current results for next loop
                        oldresults = results;

                        if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                        {
                            Thread.Sleep(Settings.OnBatteryLoopRefreshRate);
                        }
                        else
                        {
                            Thread.Sleep(Settings.LoopRefreshRate);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("@Looper1 | " + ex.Message);

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
                MessageBox.Show("@Looper2 | " + ex.Message);
            }
        }

        #endregion

        #region TrayLoopFix

        public static void TrayLoopFix()
        {

            try
            {
                var Shell_TrayWnd = Win32.FindWindowByClass("Shell_TrayWnd", (IntPtr)0);
                var TrayNotifyWnd = Win32.FindWindowEx(Shell_TrayWnd, (IntPtr)0, "TrayNotifyWnd", null);
                var ReBarWindow32 = Win32.FindWindowEx(Shell_TrayWnd, (IntPtr)0, "ReBarWindow32", null);
                var MSTaskSwWClass = Win32.FindWindowEx(ReBarWindow32, (IntPtr)0, "MSTaskSwWClass", null);
                var MSTaskListWClass = Win32.FindWindowEx(MSTaskSwWClass, (IntPtr)0, "MSTaskListWClass", null);

                var accessible = MSAA.GetAccessibleObjectFromHandle(MSTaskListWClass);

                var accessible2 = MSAA.GetAccessibleObjectFromHandle(TrayNotifyWnd);

                var accessible3 = MSAA.GetAccessibleObjectFromHandle(MSTaskSwWClass);

                var OldTrayNotifyWidth = default(int);
                do
                {

                    var RebarPos = GetLocation(accessible3, 0);
                    var TrayNotifyPos = GetLocation(accessible2, 0);
                    var TaskListPos = GetLocation(accessible, 0);

                    Win32.SendMessage(ReBarWindow32, 11, false, 0);
                    Win32.SendMessage(Win32.GetParent(Shell_TrayWnd), 11, false, 0);

                    int TrayNotifyWidth = 0;
                    string TrayOrientation;

                    // If the TrayNotifyWnd updates then refresh the taskbar
                    if (TaskListPos.height >= TaskListPos.width)
                    {
                        TrayOrientation = "V";
                    }
                    else
                    {
                        TrayOrientation = "H";
                    }

                    TrayNotifyWidth = TrayNotifyPos.width;

                    if (!(TrayNotifyWidth == OldTrayNotifyWidth))
                    {
                        if (!(OldTrayNotifyWidth == 0))
                        {
                            if (!(TaskListPos.left == 0))
                            {
                                if (TrayNotifyPos.left == 3)
                                {
                                    // 
                                    return;
                                }

                                int pos = Math.Abs(TaskListPos.left - RebarPos.left);

                                trayfixed = false;

                                setposhwnd = MSTaskListWClass;
                                setpospos = pos;
                                setposori = TrayOrientation;

                                var t1 = new Thread(SetPos);
                                t1.Start();

                                Thread.Sleep(5);
                                Win32.SendMessage(ReBarWindow32, 11, true, 0);
                                Thread.Sleep(5);
                                Win32.SendMessage(ReBarWindow32, 11, false, 0);
                                Thread.Sleep(5);
                                trayfixed = true;

                            }
                        }
                    }

                    OldTrayNotifyWidth = TrayNotifyWidth;

                    Thread.Sleep(400);
                }

                while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("@TrayLoopFix | " + ex.Message);
            }
        }

        #endregion

        #region PositionCalculator

        public static void InitPositionCalculator()
        {

            string mm;
            string mm2;

            mm = initposcalc;

            do
            {
                Thread.Sleep(10);
            }
            while (initposcalcready != true);

            mm2 = initposcalc;

            if ((mm ?? "") == (mm2 ?? ""))
            {
                // Start the PositionCalculator
                var t3 = new Thread(PositionCalculator);
                t3.Start();
            }
        }

        public static void PositionCalculator()
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
                var NewsAndInterestsPos = default(RectangleX);
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

                    string Orientation;
                    int TaskbarWidth;
                    int TrayWndLeft;
                    int TrayWndWidth;
                    int RebarWndLeft;
                    int TaskbarLeft;
                    int Position;
                    int curleft;
                    int curleft2;
                    IntPtr NewsAndInterestsHandle;
                    
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

                        var NewsAndInterests = Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "DynamicContent1", null);
                        if (!(Conversions.ToInteger(NewsAndInterests.ToString()) == Conversions.ToInteger("0")))
                        {
                            NewsAndInterestsHandle = NewsAndInterests;
                            var accessible5 = MSAA.GetAccessibleObjectFromHandle(NewsAndInterests);
                            NewsAndInterestsPos = GetLocation(accessible5, 0);
                        }

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

                    // Get current taskbar orientation (H = Horizontal | V = Vertical)
                    if (TaskListPos.height >= TaskListPos.width)
                    {
                        Orientation = "V";
                    }
                    else
                    {
                        Orientation = "H";
                    }

                    // Calculate the exact width of the total icons
                    try
                    {
                        if (Orientation == "H")
                        {
                            TaskbarWidth = LastChildPos.left - TaskListPos.left; // 'TaskbarTotalHeight
                        }
                        else
                        {
                            TaskbarWidth = LastChildPos.top - TaskListPos.top;
                        }
                    }
                    catch
                    {
                        TaskbarWidth = 0;
                        // Taskbar is empty just skip
                    }

                    // Get info needed to calculate the position
                    if (Orientation == "H")
                    {
                        TrayWndLeft = Math.Abs(TrayWndPos.left);
                        TrayWndWidth = Math.Abs(TrayWndPos.width);
                        RebarWndLeft = Math.Abs(RebarPos.left);
                        TaskbarLeft = Math.Abs(RebarWndLeft - TrayWndLeft);
                    }
                    else
                    {
                        TrayWndLeft = Math.Abs(TrayWndPos.top);
                        TrayWndWidth = Math.Abs(TrayWndPos.height);
                        RebarWndLeft = Math.Abs(RebarPos.top);
                        TaskbarLeft = Math.Abs(RebarWndLeft - TrayWndLeft);
                    }

                    Console.WriteLine("!" + NewsAndInterestsPos.width);

                    // Calculate new position
                    if (TrayWndClassName.ToString() == "Shell_TrayWnd")
                    {
                        if (Settings.CenterInBetween == 1)
                        {
                            if (Orientation == "H")
                            {
                                double offset = TrayNotifyPos.width / 2d - TaskbarLeft / 2 + NewsAndInterestsPos.width / 2d;
                                Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarWidth / 2d - TaskbarLeft - offset)) + Settings.PrimaryTaskbarOffset;
                            }
                            else
                            {
                                double offset = TrayNotifyPos.height / 2d - TaskbarLeft / 2 + NewsAndInterestsPos.height / 2d;
                                Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarWidth / 2d - TaskbarLeft - offset)) + Settings.PrimaryTaskbarOffset;
                            }
                        }
                        else
                        {
                            Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarWidth / 2d - TaskbarLeft)) + Settings.PrimaryTaskbarOffset;
                        }
                    }
                    else
                    {
                        Position = Math.Abs((int)Math.Round(TrayWndWidth / 2d - TaskbarWidth / 2d - TaskbarLeft)) + Settings.SecondaryTaskbarOffset;
                    }

                    if (Settings.TaskbarSegments == 1)
                    {
                        if (Orientation == "H")
                        {
                            var ttseg = new Win32.RECT();
                            Win32.GetClientRect((IntPtr)TaskList, ref ttseg);
                            var trayseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "TrayNotifyWnd", null), ref trayseg);
                            var clockseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "ClockButton", null), ref clockseg);

                            var startseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "Start", null), ref startseg);

                            var Tasklist_rgn = Win32.CreateRoundRectRgn(TaskbarLeft + Position + 4, ttseg.Top, TaskbarLeft + Position + TaskbarWidth - 2, ttseg.Bottom + 1, Settings.TaskbarRounding, Settings.TaskbarRounding);
                            var NotifyTray_rgn = Win32.CreateRoundRectRgn(TrayNotifyPos.left, 0, TrayNotifyPos.left + TrayNotifyPos.width, TrayNotifyPos.top + TrayNotifyPos.height, Settings.TaskbarRounding, Settings.TaskbarRounding);
                            var Start_rgn = Win32.CreateRoundRectRgn(startseg.Left, 0, startseg.Right, startseg.Bottom, Settings.TaskbarRounding, Settings.TaskbarRounding);
                            var Clock_rgn = Win32.CreateRoundRectRgn(clockseg.Left, 0, clockseg.Right, clockseg.Bottom, Settings.TaskbarRounding, Settings.TaskbarRounding);


                            var Totalreg = Win32.CreateRoundRectRgn(0, 0, 0, 0, 0, 0);
                            Win32.CombineRgn(Totalreg, Tasklist_rgn, NotifyTray_rgn, 2);

                            if (TrayWndClassName.ToString() == "Shell_TrayWnd")
                            {
                                Win32.CombineRgn(Totalreg, Totalreg, Start_rgn, 2);
                            }

                            Win32.SetWindowRgn(TrayWndHandle, Totalreg, true);
                        }
                        else
                        {
                            var ttseg = new Win32.RECT();
                            Win32.GetClientRect((IntPtr)TaskList, ref ttseg);
                            var trayseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "TrayNotifyWnd", null), ref trayseg);
                            var clockseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "ClockButton", null), ref clockseg);

                            var startseg = new Win32.RECT();
                            Win32.GetClientRect(Win32.FindWindowEx(TrayWndHandle, (IntPtr)0, "Start", null), ref startseg);

                            var Tasklist_rgn = Win32.CreateRoundRectRgn(ttseg.Left, TaskbarLeft + Position + 4, ttseg.Right, TaskbarLeft + Position + TaskbarWidth - 2, Settings.TaskbarRounding, Settings.TaskbarRounding);

                            var Totalreg = Win32.CreateRoundRectRgn(0, 0, 0, 0, 0, 0);
                            Win32.CombineRgn(Totalreg, Tasklist_rgn, Tasklist_rgn, 2);
                            Win32.SetWindowRgn(TrayWndHandle, Totalreg, true);
                        }
                    }
                    else if (!(Settings.TaskbarRounding == 0))
                    {
                        Win32.SetWindowRgn(TrayWndHandle, Win32.CreateRoundRectRgn(0, 0, TrayWndPos.width, TrayWndPos.height, Settings.TaskbarRounding, Settings.TaskbarRounding), true);

                    }

                    if (Settings.TaskbarSegments == 1)
                    {
                        if (!(Settings.TaskbarStyle == 0))
                        {
                            Win32.SendMessage(TrayWndHandle, Win32.WM_DWMCOMPOSITIONCHANGED, true, 0);
                        }
                    }

                    // Trigger the animator
                    if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
                    {
                        if (Settings.CenterPrimaryOnly == 1)
                        {
                            if (TrayWndClassName.ToString() == "Shell_TrayWnd")
                            {
                                if (Orientation == "H")
                                {
                                    if (Settings.OnBatteryAnimationStyle == "none")
                                    {
                                        Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                    }
                                    DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, true, TaskbarWidth);
                                }
                                else if (Settings.OnBatteryAnimationStyle == "none")
                                {
                                    Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                }
                                else
                                {
                                    DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, true, TaskbarWidth);
                                }
                            }
                        }
                        else if (Settings.CenterSecondaryOnly == 1)
                        {
                            if (TrayWndClassName.ToString() == "Shell_SecondaryTrayWnd")
                            {
                                if (Orientation == "H")
                                {
                                    if (Settings.OnBatteryAnimationStyle == "none")
                                    {
                                        Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                    }
                                    else
                                    {
                                        DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, false, TaskbarWidth);
                                    }
                                }
                                else if (Settings.OnBatteryAnimationStyle == "none")
                                {
                                    Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                }
                                else
                                {
                                    DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, false, TaskbarWidth);
                                }
                            }
                        }
                        else if (Orientation == "H")
                        {
                            if (Settings.OnBatteryAnimationStyle == "none")
                            {
                                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                            }
                            else
                            {
                                DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, false, TaskbarWidth);
                            }
                        }
                        else if (Settings.OnBatteryAnimationStyle == "none")
                        {
                            Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                        }
                        else
                        {
                            DaAnimator(Settings.OnBatteryAnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, false, TaskbarWidth);
                        }
                    }

                    else if (Settings.CenterPrimaryOnly == 1)
                    {
                        if (TrayWndClassName.ToString() == "Shell_TrayWnd")
                        {
                            if (Orientation == "H")
                            {
                                if (Settings.AnimationStyle == "none")
                                {
                                    Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                }
                                else
                                {
                                    DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, true, TaskbarWidth);
                                }
                            }
                            else if (Settings.AnimationStyle == "none")
                            {
                                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                            }
                            else
                            {
                                DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, true, TaskbarWidth);
                            }
                        }
                    }
                    else if (Settings.CenterSecondaryOnly == 1)
                    {
                        if (TrayWndClassName.ToString() == "Shell_SecondaryTrayWnd")
                        {
                            if (Orientation == "H")
                            {
                                if (Settings.AnimationStyle == "none")
                                {
                                    Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                                }
                                else
                                {
                                    DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, false, TaskbarWidth);
                                }
                            }
                            else if (Settings.AnimationStyle == "none")
                            {
                                Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                            }
                            else
                            {
                                DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, false, TaskbarWidth);
                            }
                        }
                    }
                    else if (Orientation == "H")
                    {
                        if (Settings.AnimationStyle == "none")
                        {
                            Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, Position, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                        }
                        else
                        {
                            DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.left, RebarPos.left, "H", Position, false, TaskbarWidth);
                        }
                    }
                    else if (Settings.AnimationStyle == "none")
                    {
                        Win32.SetWindowPos((IntPtr)TaskList, IntPtr.Zero, 0, Position, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                    }
                    else
                    {
                        DaAnimator(Settings.AnimationStyle, (IntPtr)TaskList, TaskListPos.top, RebarPos.top, "V", Position, false, TaskbarWidth);

                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("@Calculator | " + ex.Message);
            }
        }

        private static void DaAnimator(string animationStyle, IntPtr taskList, int taskListc, int rebarc, string orient, int position, bool isprimary, int width)
        {
            if (animationStyle == "linear")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.Linear, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "expoeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ExpoEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "expoeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ExpoEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "expoeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ExpoEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "expoeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ExpoEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "circeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CircEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "circeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CircEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "circeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CircEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "circeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CircEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quadeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuadEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quadeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuadEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quadeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuadEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quadeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuadEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "sineeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.SineEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "sineeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.SineEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "sineeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.SineEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "sineeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.SineEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "cubiceaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CubicEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "cubiceasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CubicEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "cubiceaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CubicEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "cubiceaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.CubicEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quarteaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuartEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quarteasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuartEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quarteaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuartEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quarteaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuartEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quinteaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuintEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quinteasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuintEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quinteaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuintEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "quinteaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.QuintEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "elasticeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ElasticEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "elasticeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ElasticEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "elasticeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ElasticEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "elasticeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.ElasticEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "bounceeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BounceEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "bounceeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BounceEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "bounceeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BounceEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "bounceeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BounceEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "backeaseout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BackEaseOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "backeasein")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BackEaseIn, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "backeaseinout")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BackEaseInOut, position, Settings.AnimationSpeed, isprimary, width);
            }
            else if (animationStyle == "backeaseoutin")
            {
                Animate(taskList, taskListc - rebarc, orient, Easings.BackEaseOutIn, position, Settings.AnimationSpeed, isprimary, width);
            }
        }

        #endregion

    }
}