using Accessibility;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskbarSharp;

public class MSAA
{

    [DllImport("oleacc.dll")]

    public static extern uint WindowFromAccessibleObject(IAccessible pacc, ref IntPtr phwnd);

    [DllImport("oleacc.dll")]

    public static extern uint AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, object[] rgvarChildren, ref int pcObtained);

    [DllImport("oleacc")]
    private static extern int AccessibleObjectFromWindow(int Hwnd, int dwId, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

    [DllImport("oleacc.dll")]
    public static extern uint GetStateText(uint dwStateBit, StringBuilder lpszStateBit, uint cchStateBitMax);

    public static Guid guidAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");

    public static IAccessible[] GetAccessibleChildren(IAccessible objAccessible)
    {
        int childCount;
        try
        {
            childCount = objAccessible.accChildCount;
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
            childCount = 0;
        }

        IAccessible[] accObjects = new IAccessible[childCount];
        int count = 0;

        if (childCount != 0)
        {

            AccessibleChildren(objAccessible, 0, childCount, accObjects, ref count);
        }

        return accObjects;
    }

    public static IAccessible GetAccessibleObjectFromHandle(IntPtr hwnd)
    {
        var accObject = new object();
        IAccessible objAccessible = null;
        if (hwnd != (IntPtr)0)
        {
            AccessibleObjectFromWindow((int)hwnd, 0, ref guidAccessible, ref accObject);
            objAccessible = (IAccessible)accObject;
        }

        return objAccessible;
    }

    public static string GetStateTextFunc(uint stateID)
    {
        uint maxLength = 1024U;
        var focusableStateText = new StringBuilder((int)maxLength);
        var sizeableStateText = new StringBuilder((int)maxLength);
        var moveableStateText = new StringBuilder((int)maxLength);
        var invisibleStateText = new StringBuilder((int)maxLength);
        var pressedStateText = new StringBuilder((int)maxLength);
        var hasPopupStateText = new StringBuilder((int)maxLength);

        if (stateID == (MSAAStateConstants.STATE_SYSTEM_INVISIBLE | MSAAStateConstants.STATE_SYSTEM_FOCUSABLE | MSAAStateConstants.STATE_SYSTEM_HASPOPUP))
        {
            GetStateText(MSAAStateConstants.STATE_SYSTEM_INVISIBLE, invisibleStateText, maxLength);
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength);
            GetStateText(MSAAStateConstants.STATE_SYSTEM_HASPOPUP, hasPopupStateText, maxLength);

            return invisibleStateText.ToString() + "," + focusableStateText.ToString() + "," + hasPopupStateText.ToString();
        }

        if (stateID == (MSAAStateConstants.STATE_SYSTEM_PRESSED | MSAAStateConstants.STATE_SYSTEM_INVISIBLE | MSAAStateConstants.STATE_SYSTEM_FOCUSABLE))
        {
            GetStateText(MSAAStateConstants.STATE_SYSTEM_PRESSED, pressedStateText, maxLength);
            GetStateText(MSAAStateConstants.STATE_SYSTEM_INVISIBLE, invisibleStateText, maxLength);
            GetStateText(MSAAStateConstants.STATE_SYSTEM_PRESSED, focusableStateText, maxLength);

            return pressedStateText.ToString() + "," + focusableStateText.ToString() + "," + focusableStateText.ToString();
        }

        if (stateID == (MSAAStateConstants.STATE_SYSTEM_FOCUSABLE | MSAAStateConstants.STATE_SYSTEM_HASPOPUP))
        {
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength);
            GetStateText(MSAAStateConstants.STATE_SYSTEM_HASPOPUP, hasPopupStateText, maxLength);

            return focusableStateText.ToString() + "," + hasPopupStateText.ToString();
        }

        if (stateID == MSAAStateConstants.STATE_SYSTEM_FOCUSABLE)
        {
            GetStateText(MSAAStateConstants.STATE_SYSTEM_FOCUSABLE, focusableStateText, maxLength);
            return focusableStateText.ToString();
        }

        var stateText = new StringBuilder((int)maxLength);
        GetStateText(stateID, stateText, maxLength);
        return stateText.ToString();
    }

}

class MSAAStateConstants
{

    public static uint STATE_SYSTEM_ALERT_HIGH = 268435456U;

    public static uint STATE_SYSTEM_ALERT_LOW = 67108864U;

    public static uint STATE_SYSTEM_ALERT_MEDIUM = 134217728U;

    public static uint STATE_SYSTEM_ANIMATED = 16384U;

    public static uint STATE_SYSTEM_BUSY = 2048U;

    public static uint STATE_SYSTEM_CHECKED = 16U;

    public static uint STATE_SYSTEM_COLLAPSED = 1024U;

    public static uint STATE_SYSTEM_DEFAULT = 256U;

    public static uint STATE_SYSTEM_EXPANDED = 512U;

    public static uint STATE_SYSTEM_EXTSELECTABLE = 33554432U;

    public static uint STATE_SYSTEM_FLOATING = 4096U;

    public static uint STATE_SYSTEM_FOCUSABLE = 1048576U;

    public static uint STATE_SYSTEM_FOCUSED = 4U;

    public static uint STATE_SYSTEM_HASPOPUP = 1073741824U;

    public static uint STATE_SYSTEM_HOTTRACKED = 128U;

    public static uint STATE_SYSTEM_INVISIBLE = 32768U;

    public static uint STATE_SYSTEM_LINKED = 4194304U;

    public static uint STATE_SYSTEM_MARQUEED = 8192U;

    public static uint STATE_SYSTEM_MIXED = 32U;

    public static uint STATE_SYSTEM_MOVEABLE = 262144U;

    public static uint STATE_SYSTEM_MULTISELECTABLE = 16777216U;

    public static uint STATE_SYSTEM_NORMAL = 0U;

    public static uint STATE_SYSTEM_OFFSCREEN = 65536U;

    public static uint STATE_SYSTEM_PRESSED = 8U;

    public static uint STATE_SYSTEM_READONLY = 64U;

    public static uint STATE_SYSTEM_SELECTABLE = 2097152U;

    public static uint STATE_SYSTEM_SELECTED = 2U;

    public static uint STATE_SYSTEM_SELFVOICING = 524288U;

    public static uint STATE_SYSTEM_SIZEABLE = 131072U;

    public static uint STATE_SYSTEM_TRAVERSED = 8388608U;

    public static uint STATE_SYSTEM_UNAVAILABLE = 1U;

    public static uint STATE_SYSTEM_VALID = 536870911U;
}