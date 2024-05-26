using System;
using System.Windows.Forms;

namespace TaskbarSharp.Common;

public static class UI
{
    public static void ShowError(Exception ex)
    {
        MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowError(string text)
    {
        MessageBox.Show(text, "TaskbarSharp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
