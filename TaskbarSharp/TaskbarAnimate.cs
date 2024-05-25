using System;
using System.Collections;
using System.Diagnostics;

namespace TaskbarX
{

    public class TaskbarAnimate
    {

        public static ArrayList current = new ArrayList();

        public static void Animate(IntPtr hwnd, int oldpos, string orient, EasingDelegate easing, int valueToReach, int duration, bool isPrimary, int width)
        {


            try
            {

                if (Math.Abs(valueToReach - oldpos) == 0)
                {
                    // The difference is 0 so there is no need to trigger the animator.
                    return;
                }



                if (Settings.RevertZeroBeyondTray == 1)
                {
                    // Prevent moving beyond Tray area.
                    var TrayPos2 = default(Win32.RECT);
                    Win32.GetWindowRect(Win32.GetParent(hwnd), ref TrayPos2);
                    int rightposition = valueToReach + width;

                    if (orient == "H")
                    {
                        if (rightposition >= TrayPos2.Right - TrayPos2.Left)
                        {
                            Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                            return;
                        }
                    }
                    else if (rightposition >= TrayPos2.Bottom - TrayPos2.Top)
                    {
                        Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                        return;
                    }
                }


                if (valueToReach == oldpos | Math.Abs(valueToReach - oldpos) <= 10)
                {
                    // Prevent Wiggling (if the new position has a difference of 10 or lower then there is no reason to move)
                    return;
                }

                foreach (var tt in current)
                {
                    if ((IntPtr)tt == hwnd)
                    {
                        // If hwnd is already getting animated then hwnd is in this arraylist and exit the animator because it's uneeded.
                        return;
                    }
                }

                // Console.WriteLine(CInt((valueToReach - oldpos).ToString.Replace("-", "")))

                current.Add(hwnd);

                var sw = new Stopwatch();
                int originalValue = oldpos;
                int elapsed = new int();
                int minValue;

                if (originalValue <= valueToReach)
                {
                    minValue = originalValue;
                }
                else
                {
                    minValue = valueToReach;
                }

                int maxValue = Math.Abs(valueToReach - originalValue);
                bool increasing = originalValue < valueToReach;

                elapsed = 0;
                sw.Start();

                if (isPrimary == true)
                {
                    TaskbarCenter.isanimating = true;
                }

                while (!(elapsed >= duration))
                {

                    elapsed = (int)sw.ElapsedMilliseconds;

                    int newValue = (int)Math.Round(easing(elapsed, minValue, maxValue, duration));

                    if (!increasing)
                    {
                        newValue = originalValue + valueToReach - newValue;
                    }

                    if (orient == "H")
                    {
                        Win32.SetWindowPos(hwnd, IntPtr.Zero, newValue, 0, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                    }
                    else
                    {
                        Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, newValue, 0, 0, Win32.SWP_NOSIZE | Win32.SWP_ASYNCWINDOWPOS | Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | Win32.SWP_NOSENDCHANGING);
                    }
                }

                if (isPrimary == true)
                {
                    TaskbarCenter.isanimating = false;
                }

                sw.Stop();
                current.Remove(hwnd);

                MainType.ClearMemory();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}