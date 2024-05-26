using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TaskbarSharp.Common;

namespace TaskbarSharp;

public class TaskbarStyle
{
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

    public static ArrayList windowHandles = new ArrayList();
    public static ArrayList maximizedwindows = new ArrayList();
    public static ArrayList trays = new ArrayList();
    public static ArrayList traysbackup = new ArrayList();
    public static ArrayList normalwindows = new ArrayList();
    public static ArrayList resetted = new ArrayList();

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

    public static bool IsPhanthom(IntPtr hWnd)
    {
        var CloakedVal = default(int);
        int hRes = Win32.DwmGetWindowAttribute(hWnd, Win32.DWMWINDOWATTRIBUTE.Cloaked, ref CloakedVal, Strings.Len(CloakedVal));
        if (hRes == ~0)
        {
            CloakedVal = 0;
        }
        return Conversions.ToBoolean(CloakedVal) ? true : false;
    }

    public static bool Enumerator2(IntPtr hwnd, int lParam)
    {
        try
        {
            int intRet;
            var wpTemp = new Win32.WINDOWPLACEMENT();
            wpTemp.Length = Marshal.SizeOf(wpTemp);
            intRet = Conversions.ToInteger(Win32.GetWindowPlacement(hwnd, ref wpTemp));
            int style = Win32.GetWindowLong(hwnd, Win32.GWL_STYLE);

            if (IsPhanthom(hwnd) == false) // Fix phanthom windows
            {
                if ((style & Win32.WS_VISIBLE) == Win32.WS_VISIBLE)
                {
                    if (wpTemp.showCmd == 3)
                    {
                        maximizedwindows.Remove(hwnd);
                        maximizedwindows.Add(hwnd);
                    }
                    else
                    {
                        normalwindows.Remove(hwnd);
                        normalwindows.Add(hwnd);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return true;
    }

    public static void Tbsm()
    {
        do
        {
            int windowsold;
            int windowsnew;
            windowsold = maximizedwindows.Count;

            maximizedwindows.Clear();
            Thread.Sleep(250);
            Win32.EnumWindows(Enumerator2, 0);

            windowsnew = maximizedwindows.Count;

            if (!(windowsnew == windowsold))
            {
                foreach (IntPtr tray in traysbackup)
                {
                    foreach (IntPtr normalwindow in normalwindows)
                    {
                        var curmonx = Screen.FromHandle(normalwindow);
                        var curmontbx = Screen.FromHandle(tray);
                        if ((curmonx.DeviceName ?? "") == (curmontbx.DeviceName ?? ""))
                        {
                            trays.Remove(tray);
                            trays.Add(tray);
                        }
                    }
                }

                foreach (IntPtr tray in traysbackup)
                {
                    foreach (IntPtr maxedwindow in maximizedwindows)
                    {
                        var curmonx = Screen.FromHandle(maxedwindow);
                        var curmontbx = Screen.FromHandle(tray);
                        if ((curmonx.DeviceName ?? "") == (curmontbx.DeviceName ?? ""))
                        {
                            trays.Remove(tray);
                            Win32.PostMessage(tray, 0x31EU, (IntPtr)0x1, (IntPtr)0x0);
                        }
                    }
                }
            }
        }

        while (true);
    }

    public static void TaskbarStyler()
    {
        try
        {
            GetActiveWindows();

            var accent = new Win32.AccentPolicy();
            int accentStructSize = Marshal.SizeOf(accent);

            // Select accent based on settings
            if (Settings.TaskbarStyle == 1)
            {
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_TRANSPARANT;
            }

            if (Settings.TaskbarStyle == 2)
            {
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_BLURBEHIND;
            }

            if (Settings.TaskbarStyle == 3)
            {
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            }

            if (Settings.TaskbarStyle == 4)
            {
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
            }

            if (Settings.TaskbarStyle == 5)
            {
                accent.AccentState = Win32.AccentState.ACCENT_ENABLE_GRADIENT;
            }

            accent.AccentFlags = 2; // enable colorize
            accent.GradientColor = BitConverter.ToInt32(new byte[] { (byte)Settings.TaskbarStyleRed, (byte)Settings.TaskbarStyleGreen, (byte)Settings.TaskbarStyleBlue, (byte)Math.Round(Settings.TaskbarStyleAlpha * 2.55d) }, 0);

            // Save accent data
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new Win32.WindowCompositionAttributeData
            {
                Attribute = Win32.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            // Put all TrayWnds into an ArrayList
            foreach (IntPtr trayWnd in windowHandles)
            {
                trays.Add(trayWnd);
                traysbackup.Add(trayWnd);
            }

            if (Settings.DefaultTaskbarStyleOnWinMax == 1)
            {
                var t2 = new Thread(Tbsm);
                t2.Start();
            }

            // Set taskbar style for all TrayWnds each 14 millisecond
            do
            {
                try
                {
                    foreach (IntPtr tray in trays)
                    {
                        Win32.SetWindowCompositionAttribute(tray, ref data);
                    }
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            while (true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static int childLeft;
    public static int childTop;
    public static int childWidth;
    public static int childHeight;

    public static int GetLocation(Accessibility.IAccessible acc, int idChild)
    {
        acc.accLocation(out childLeft, out childTop, out childWidth, out childHeight, idChild);
        return default;
    }

}