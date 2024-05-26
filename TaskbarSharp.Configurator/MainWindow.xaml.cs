using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.TaskScheduler;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskbarSharp.Common;

namespace TaskbarSharp.Configurator;

public partial class MainWindow
{
    public static System.Collections.ObjectModel.Collection<IntPtr> ActiveWindows = [];

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    public static System.Collections.ObjectModel.Collection<IntPtr> GetActiveWindows()
    {
        windowHandles.Clear();
        Win32.EnumWindows(Enumerator, 0);

        bool maintaskbarfound = false;
        bool sectaskbarfound = false;

        foreach (var Taskbar in windowHandles)
        {
            var sClassName = new StringBuilder("", 256);
            GetClassName((IntPtr)Taskbar, sClassName, 256);
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
                string arglpClassName = "Shell_TrayWnd";
                string arglpWindowName = null;
                windowHandles.Add(Win32.FindWindow(arglpClassName, arglpWindowName));
            }
            catch
            {
            }
        }

        if (sectaskbarfound == false)
        {
            try
            {
                string arglpClassName4 = "Shell_SecondaryTrayWnd";
                string arglpWindowName4 = null;
                if (!(Win32.FindWindow(arglpClassName4, arglpWindowName4) == (IntPtr)0))
                {
                    string arglpClassName3 = "Shell_SecondaryTrayWnd";
                    string arglpWindowName3 = null;
                    if (Win32.FindWindow(arglpClassName3, arglpWindowName3) != default)
                    {
                        string arglpClassName1 = "Shell_SecondaryTrayWnd";
                        string arglpWindowName1 = null;
                        windowHandles.Add(Win32.FindWindow(arglpClassName1, arglpWindowName1));
                    }
                }
            }
            catch
            {
            }
        }

        return ActiveWindows;
    }

    public static bool Enumerator(IntPtr hwnd, int lParam)
    {
        var sClassName = new StringBuilder("", 256);
        GetClassName(hwnd, sClassName, 256);
        if (sClassName.ToString() == "Shell_TrayWnd" | sClassName.ToString() == "Shell_SecondaryTrayWnd")
        {
            windowHandles.Add(hwnd);
        }
        return true;
    }

    public static ArrayList windowHandles = new ArrayList();

    public static void RevertToZero()
    {
        try
        {
            Process.Start("TaskbarSharp.exe", "-stop");
        }
        catch
        {
        }
    }

    private readonly Bitmap bmp = new Bitmap(1, 1);

    private System.Drawing.Color GetColorAt(int x, int y)
    {
        var bounds = new System.Drawing.Rectangle(x, y, 1, 1);

        using (var g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size);
        }

        return bmp.GetPixel(0, 0);
    }

    private async void Window_Loaded(object sender, EventArgs e)
    {
        this.InitializeComponent();

        if (this.Background.ToString() == "#FF000000")
        {
            var mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, 10, 10, 10);
            var mySolidColorBrush2 = new SolidColorBrush();
            mySolidColorBrush2.Color = System.Windows.Media.Color.FromArgb(255, 20, 20, 20);
            placeholder.Fill = mySolidColorBrush;
            Background = mySolidColorBrush;
            ListBox1.Background = mySolidColorBrush2;
        }

        if (this.Background.ToString() == "#FFFFFFFF")
        {
            var mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, 240, 240, 240);
            var mySolidColorBrush2 = new SolidColorBrush();
            mySolidColorBrush2.Color = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
            placeholder.Fill = mySolidColorBrush;
            Background = mySolidColorBrush2;
            ListBox1.Background = mySolidColorBrush;
        }


        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
        if (isElevated)
        {
            var adminDialog = new ContentDialog()
            {
                Title = "Warning!",
                Content = "Please DON'T run the Configurator as Administrator. This may cause the start-up task not to work properly!",
                PrimaryButtonText = "Ok"
            };
            var result = await adminDialog.ShowAsync();
        }

        ListBox1.SelectedIndex = 0;

        tbrounding.Value = 0d;

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        try
        {
            using (var ts = new TaskService())
            {
                var td = ts.GetTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));

                if (td == null)
                {
                    return;
                }

                var cfg = td.Definition.Actions.ToString().Replace('"' + AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe" + '"', "");

                var arguments = cfg.Split(" ".ToCharArray());

                foreach (var argument in arguments)
                {
                    var val = Strings.Split(argument, "=");
                    Console.WriteLine(val[0]);
                    if (argument.Contains("-tbs="))
                    {
                        if (Conversions.ToInteger(val[1]) == 0)
                        {
                            this.RadioButton1.IsChecked = true;
                        }
                        if (Conversions.ToInteger(val[1]) == 1)
                        {
                            this.RadioButton3.IsChecked = true;
                        }
                        if (Conversions.ToInteger(val[1]) == 2)
                        {
                            this.RadioButton4.IsChecked = true;
                        }
                    }

                    if (argument.Contains("-tbr="))
                    {
                        this.tbrounding.Text = val[1];
                    }

                    if (argument.Contains("-tbsg="))
                    {
                        if (val[1] == "1")
                        {
                            this.tbsegments.IsChecked = true;
                        }
                    }

                    if (argument.Contains("-ptbo="))
                    {
                        this.NumericUpDown1.Text = val[1];
                    }
                    if (argument.Contains("-stbo="))
                    {
                        this.NumericUpDown2.Text = val[1];
                    }
                    if (argument.Contains("-cpo="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox2.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-cso="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox3.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-sr="))
                    {
                        this.NumericUpDown7.Text = val[1];
                    }
                    if (argument.Contains("-sr2="))
                    {
                        this.NumericUpDown7_Copy.Text = val[1];
                    }
                    if (argument.Contains("-sr3="))
                    {
                        this.NumericUpDown7_Copy1.Text = val[1];
                    }
                    if (argument.Contains("-lr="))
                    {
                        this.NumericUpDown3.Text = val[1];
                    }
                    if (argument.Contains("-cib="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox1.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-oblr="))
                    {
                        this.NumericUpDown5.Text = val[1];
                    }
                    if (argument.Contains("-ftotc="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox4.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-rzbt="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox4_Copy.IsChecked = true;
                        }
                        if (val[1] == "0")
                        {
                            this.CheckBox4_Copy.IsChecked = false;
                        }
                    }

                    if (argument.Contains("-dtbsowm="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox10.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-cfsa="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox9.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-dct="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox11.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hps="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox12.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hss="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox13.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hpt="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox14.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hst="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox15.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-sti="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox16.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-console"))
                    {
                        this.checkboxconsole.IsChecked = true;
                    }

                }

                Console.WriteLine(td.Definition.Actions.ToString());

                LogonTrigger lg = (LogonTrigger)td.Definition.Triggers[0];
                var times = lg.Delay;

                this.NumericUpDown6.Value = (double)times.Seconds;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

        ListBox item = (ListBox)sender;
        int index = item.SelectedIndex;

        this.TabControl1.SelectedIndex = index;

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    public void Button_Click_1(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (Process prog in Process.GetProcesses())
            {
                if (prog.ProcessName == "TaskbarSharp")
                {
                    prog.Kill();
                }
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }

        Thread.Sleep(50);

        var t1 = new Thread(RevertToZero);
        t1.Start();

        Thread.Sleep(1000);

        string parameters = "";

        if (this.RadioButton1.IsChecked is { } arg23 && arg23 == true)
        {
            parameters += "-tbs=0 ";
        }
        if (this.RadioButton3.IsChecked is { } arg25 && arg25 == true)
        {
            parameters += "-tbs=2 ";
        }
        if (this.RadioButton4.IsChecked is { } arg26 && arg26 == true)
        {
            parameters += "-tbs=3 ";
        }

        parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

        parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

        if (this.tbrounding.Text != default)
        {
            parameters += "-tbr=" + this.tbrounding.Text + " ";
        }

        if (this.tbsegments.IsChecked is { } arg29 && arg29 == true)
        {
            parameters += "-tbsg=1 ";
        }

        if (this.NumericUpDown1.Text != default)
        {
            parameters += "-ptbo=" + this.NumericUpDown1.Text + " ";
        }
        if (this.NumericUpDown2.Text != default)
        {
            parameters += "-stbo=" + this.NumericUpDown2.Text + " ";
        }

        if (this.CheckBox1.IsChecked is { } arg30 && arg30 == true)
        {
            parameters += "-cib=1 ";
        }

        if (this.NumericUpDown3.Text != default)
        {
            parameters += "-lr=" + this.NumericUpDown3.Text + " ";
        }

        if (this.NumericUpDown5.Text != default)
        {
            parameters += "-oblr=" + this.NumericUpDown5.Text + " ";
        }

        if (this.NumericUpDown7.Text != default)
        {
            parameters += "-sr=" + this.NumericUpDown7.Text + " ";
        }

        if (this.NumericUpDown7_Copy.Text != default)
        {
            parameters += "-sr2=" + this.NumericUpDown7_Copy.Text + " ";
        }

        if (this.NumericUpDown7_Copy1.Text != default)
        {
            parameters += "-sr3=" + this.NumericUpDown7_Copy1.Text + " ";
        }

        if (this.CheckBox2.IsChecked is { } arg31 && arg31 == true)
        {
            parameters += "-cpo=1 ";
        }

        if (this.CheckBox3.IsChecked is { } arg32 && arg32 == true)
        {
            parameters += "-cso=1 ";
        }

        if (this.CheckBox4.IsChecked is { } arg33 && arg33 == true)
        {
            parameters += "-ftotc=1 ";
        }

        if (this.CheckBox4_Copy.IsChecked is { } arg34 && arg34 == true)
        {
            parameters += "-rzbt=1 ";
        }

        if (this.CheckBox4_Copy.IsChecked is { } arg35 && arg35 == false)
        {
            parameters += "-rzbt=0 ";
        }

        if (this.Checkbox10.IsChecked is { } arg36 && arg36 == true)
        {
            parameters += "-dtbsowm=1 ";
        }
        if (this.Checkbox9.IsChecked is { } arg37 && arg37 == true)
        {
            parameters += "-cfsa=1 ";
        }
        if (this.CheckBox11.IsChecked is { } arg38 && arg38 == true)
        {
            parameters += "-dct=1 ";
        }
        if (this.Checkbox12.IsChecked is { } arg39 && arg39 == true)
        {
            parameters += "-hps=1 ";
        }
        if (this.Checkbox13.IsChecked is { } arg40 && arg40 == true)
        {
            parameters += "-hss=1 ";
        }
        if (this.Checkbox14.IsChecked is { } arg41 && arg41 == true)
        {
            parameters += "-hpt=1 ";
        }
        if (this.Checkbox15.IsChecked is { } arg42 && arg42 == true)
        {
            parameters += "-hst=1 ";
        }
        if (this.Checkbox16.IsChecked is { } arg43 && arg43 == true)
        {
            parameters += "-sti=1 ";
        }
        if (this.checkboxconsole.IsChecked is { } arg44 && arg44 == true)
        {
            parameters += "-console ";
        }

        try
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));
                ts.RootFolder.DeleteTask("TaskbarSharp");
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }

        try
        {
            using (var ts = new TaskService())
            {

                var td = ts.NewTask();
                int delay = Conversions.ToInteger(this.NumericUpDown6.Text);

                td.RegistrationInfo.Description = "Center taskbar icons";

                td.Triggers.Add(new LogonTrigger()
                {
                    UserId = WindowsIdentity.GetCurrent().Name,
                    Delay = TimeSpan.FromSeconds(delay)
                });

                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.RunOnlyIfIdle = false;
                td.Settings.IdleSettings.RestartOnIdle = false;
                td.Settings.IdleSettings.StopOnIdleEnd = false;
                td.Settings.Hidden = true;
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                td.RegistrationInfo.Author = "Chris Andriessen";

                td.Actions.Add(new ExecAction('"' + AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe" + '"', parameters, null));
                Process.Start("TaskbarSharp.exe", parameters);

                ts.RootFolder.RegisterTaskDefinition("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""), td);
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }
    }

    public static int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
    public static int WM_DWMCOMPOSITIONCHANGED = 0x31E;
    public static int WM_THEMECHANGED = 0x31A;

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (Process prog in Process.GetProcesses())
            {
                if (prog.ProcessName == "TaskbarSharp")
                {
                    prog.Kill();
                }
            }
        }
        catch
        {
        }

        Thread.Sleep(50);

        var t1 = new Thread(RevertToZero);
        t1.Start();
    }

    private async void Button_Click_3(object sender, RoutedEventArgs e)
    {
        try
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));
                ts.RootFolder.DeleteTask("TaskbarSharp");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        var deleteFileDialog = new ContentDialog()
        {
            Title = "Taskscheduler",
            Content = "Taskschedule Removed!",
            PrimaryButtonText = "Ok"
        };
        var result = await deleteFileDialog.ShowAsync();

    }

    private async void Button_Click_33(object sender, RoutedEventArgs e)
    {

        string parameters = "";

        if (this.RadioButton1.IsChecked is { } arg45 && arg45 == true)
        {
            parameters += "-tbs=0 ";
        }
        if (this.RadioButton3.IsChecked is { } arg47 && arg47 == true)
        {
            parameters += "-tbs=2 ";
        }
        if (this.RadioButton4.IsChecked is { } arg48 && arg48 == true)
        {
            parameters += "-tbs=3 ";
        }

        parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

        parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

        if (this.tbrounding.Text != default)
        {
            parameters += "-tbr=" + this.tbrounding.Text + " ";
        }

        if (this.tbsegments.IsChecked is { } arg51 && arg51 == true)
        {
            parameters += "-tbsg=1 ";
        }

        if (this.NumericUpDown1.Text != default)
        {
            parameters += "-ptbo=" + this.NumericUpDown1.Text + " ";
        }
        if (this.NumericUpDown2.Text != default)
        {
            parameters += "-stbo=" + this.NumericUpDown2.Text + " ";
        }

        if (this.CheckBox1.IsChecked is { } arg52 && arg52 == true)
        {
            parameters += "-cib=1 ";
        }

        if (this.NumericUpDown3.Text != default)
        {
            parameters += "-lr=" + this.NumericUpDown3.Text + " ";
        }

        if (this.NumericUpDown5.Text != default)
        {
            parameters += "-oblr=" + this.NumericUpDown5.Text + " ";
        }

        if (this.NumericUpDown7.Text != default)
        {
            parameters += "-sr=" + this.NumericUpDown7.Text + " ";
        }

        if (this.NumericUpDown7_Copy.Text != default)
        {
            parameters += "-sr2=" + this.NumericUpDown7_Copy.Text + " ";
        }

        if (this.NumericUpDown7_Copy1.Text != default)
        {
            parameters += "-sr3=" + this.NumericUpDown7_Copy1.Text + " ";
        }

        if (this.CheckBox2.IsChecked is { } arg53 && arg53 == true)
        {
            parameters += "-cpo=1 ";
        }

        if (this.CheckBox3.IsChecked is { } arg54 && arg54 == true)
        {
            parameters += "-cso=1 ";
        }

        if (this.CheckBox4.IsChecked is { } arg55 && arg55 == true)
        {
            parameters += "-ftotc=1 ";
        }

        if (this.CheckBox4_Copy.IsChecked is { } arg56 && arg56 == true)
        {
            parameters += "-rzbt=1 ";
        }

        if (this.CheckBox4_Copy.IsChecked is { } arg57 && arg57 == false)
        {
            parameters += "-rzbt=0 ";
        }

        if (this.Checkbox10.IsChecked is { } arg58 && arg58 == true)
        {
            parameters += "-dtbsowm=1 ";
        }
        if (this.Checkbox9.IsChecked is { } arg59 && arg59 == true)
        {
            parameters += "-cfsa=1 ";
        }
        if (this.CheckBox11.IsChecked is { } arg60 && arg60 == true)
        {
            parameters += "-dct=1 ";
        }
        if (this.Checkbox12.IsChecked is { } arg61 && arg61 == true)
        {
            parameters += "-hps=1 ";
        }
        if (this.Checkbox13.IsChecked is { } arg62 && arg62 == true)
        {
            parameters += "-hss=1 ";
        }
        if (this.Checkbox14.IsChecked is { } arg63 && arg63 == true)
        {
            parameters += "-hpt=1 ";
        }
        if (this.Checkbox15.IsChecked is { } arg64 && arg64 == true)
        {
            parameters += "-hst=1 ";
        }
        if (this.Checkbox16.IsChecked is { } arg65 && arg65 == true)
        {
            parameters += "-sti=1 ";
        }
        if (this.checkboxconsole.IsChecked is { } arg66 && arg66 == true)
        {
            parameters += "-console ";
        }

        try
        {
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));
                ts.RootFolder.DeleteTask("TaskbarSharp");
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }

        try
        {
            using (var ts = new TaskService())
            {
                var td = ts.NewTask();
                int delay = Conversions.ToInteger(this.NumericUpDown6.Text);

                td.RegistrationInfo.Description = "Center taskbar icons";

                td.Triggers.Add(new LogonTrigger()
                {
                    UserId = WindowsIdentity.GetCurrent().Name,
                    Delay = TimeSpan.FromSeconds(delay)
                });

                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.RunOnlyIfIdle = false;
                td.Settings.IdleSettings.RestartOnIdle = false;
                td.Settings.IdleSettings.StopOnIdleEnd = false;
                td.Settings.Hidden = true;
                td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                td.RegistrationInfo.Author = "Chris Andriessen";

                td.Actions.Add(new ExecAction(AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe", parameters, null));

                ts.RootFolder.RegisterTaskDefinition("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""), td);

            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }

        var deleteFileDialog = new ContentDialog()
        {
            Title = "Taskscheduler",
            Content = "Taskschedule Created!",
            PrimaryButtonText = "Ok"
        };
        var result = await deleteFileDialog.ShowAsync();

    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {

        if (!char.IsNumber(Conversions.ToChar(e.Text)))
        {
            if (!(e.Text == "-"))
            {
                e.Handled = true;
            }
        }

    }

    private void Button_Click_5(object sender, RoutedEventArgs e)
    {
        try
        {

            using (var ts = new TaskService())
            {

                var td = ts.GetTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));

                string cfg = td.Definition.Actions.ToString().Replace(AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe", "");

                string[] arguments = cfg.Split(" ".ToCharArray());

                foreach (var argument in arguments)
                {
                    string[] val = Strings.Split(argument, "=");
                    Console.WriteLine(val[0]);
                    if (argument.Contains("-tbs="))
                    {
                        if (Conversions.ToInteger(val[1]) == 0)
                        {
                            this.RadioButton1.IsChecked = true;
                        }
                        if (Conversions.ToInteger(val[1]) == 1)
                        {
                            this.RadioButton3.IsChecked = true;
                        }
                        if (Conversions.ToInteger(val[1]) == 2)
                        {
                            this.RadioButton4.IsChecked = true;
                        }
                    }

                    if (argument.Contains("-color="))
                    {
                        string colorval = val[1];
                        string[] colorsep = colorval.Split(";".ToCharArray());
                    }

                    if (argument.Contains("-tbr="))
                    {
                        this.tbrounding.Text = val[1];
                    }

                    if (argument.Contains("-tbsg="))
                    {
                        if (val[1] == "1")
                        {
                            this.tbsegments.IsChecked = true;
                        }
                    }

                    if (argument.Contains("-ptbo="))
                    {
                        this.NumericUpDown1.Text = val[1];
                    }
                    if (argument.Contains("-stbo="))
                    {
                        this.NumericUpDown2.Text = val[1];
                    }
                    if (argument.Contains("-cpo="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox2.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-cso="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox3.IsChecked = true;
                        }
                    }

                    if (argument.Contains("-sr="))
                    {
                        this.NumericUpDown7.Text = val[1];
                    }
                    if (argument.Contains("-sr2="))
                    {
                        this.NumericUpDown7_Copy.Text = val[1];
                    }
                    if (argument.Contains("-sr3="))
                    {
                        this.NumericUpDown7_Copy1.Text = val[1];
                    }
                    if (argument.Contains("-lr="))
                    {
                        this.NumericUpDown3.Text = val[1];
                    }
                    if (argument.Contains("-cib="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox1.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-oblr="))
                    {
                        this.NumericUpDown5.Text = val[1];
                    }
                    if (argument.Contains("-ftotc="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox4.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-rzbt="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox4_Copy.IsChecked = true;
                        }
                        if (val[1] == "0")
                        {
                            this.CheckBox4_Copy.IsChecked = false;
                        }
                    }
                    if (argument.Contains("-dtbsowm="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox10.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-cfsa="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox9.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-dct="))
                    {
                        if (val[1] == "1")
                        {
                            this.CheckBox11.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hps="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox12.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hss="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox13.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hpt="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox14.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-hst="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox15.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-sti="))
                    {
                        if (val[1] == "1")
                        {
                            this.Checkbox16.IsChecked = true;
                        }
                    }
                    if (argument.Contains("-console"))
                    {
                        this.checkboxconsole.IsChecked = true;
                    }

                }

                Console.WriteLine(td.Definition.Actions.ToString());

                LogonTrigger lg = (LogonTrigger)td.Definition.Triggers[0];
                var times = lg.Delay;

                this.NumericUpDown6.Value = (double)times.Seconds;
            }
        }
        catch
        {
        }

    }

    private void Button_Click_6(object sender, RoutedEventArgs e)
    {
        this.RadioButton1.IsChecked = true;
        this.Checkbox10.IsChecked = false;
        this.NumericUpDown1.Text = "0";
        this.NumericUpDown2.Text = "0";
        this.CheckBox1.IsChecked = false;
        this.NumericUpDown6.Text = "6";
        this.CheckBox2.IsChecked = false;
        this.CheckBox3.IsChecked = false;
        this.CheckBox4.IsChecked = true;
        this.CheckBox4_Copy.IsChecked = true;
        this.Checkbox9.IsChecked = false;
        this.CheckBox11.IsChecked = false;
        this.Checkbox12.IsChecked = false;
        this.Checkbox13.IsChecked = false;
        this.Checkbox14.IsChecked = false;
        this.Checkbox15.IsChecked = false;
        this.Checkbox16.IsChecked = false;
        this.checkboxconsole.IsChecked = false;
        this.NumericUpDown3.Text = "400";
        this.NumericUpDown5.Text = "400";
        this.NumericUpDown7.Text = "0";
        this.NumericUpDown7_Copy.Text = "0";
        this.NumericUpDown7_Copy1.Text = "0";
    }

    private void Button_Click_Restart(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == "TaskbarSharp")
                {
                    process.Kill();
                }
            }
        }
        catch (Exception ex)
        {
            UI.ShowError(ex);
        }

        Thread.Sleep(50);

        var t1 = new Thread(RevertToZero);
        t1.Start();

        Thread.Sleep(1000);

        string parameters = "";

        if (this.RadioButton1.IsChecked is { } arg67 && arg67 == true)
        {
            parameters += "-tbs=0 ";
        }
        if (this.RadioButton3.IsChecked is { } arg69 && arg69 == true)
        {
            parameters += "-tbs=2 ";
        }
        if (this.RadioButton4.IsChecked is { } arg70 && arg70 == true)
        {
            parameters += "-tbs=3 ";
        }

        parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";
        parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

        if (this.tbrounding.Text != default)
        {
            parameters += "-tbr=" + this.tbrounding.Text + " ";
        }

        if (this.tbsegments.IsChecked is { } arg73 && arg73 == true)
        {
            parameters += "-tbsg=1 ";
        }

        if (this.NumericUpDown1.Text != default)
        {
            parameters += "-ptbo=" + this.NumericUpDown1.Text + " ";
        }
        if (this.NumericUpDown2.Text != default)
        {
            parameters += "-stbo=" + this.NumericUpDown2.Text + " ";
        }

        if (this.CheckBox1.IsChecked is { } arg74 && arg74 == true)
        {
            parameters += "-cib=1 ";
        }

        if (this.NumericUpDown3.Text != default)
        {
            parameters += "-lr=" + this.NumericUpDown3.Text + " ";
        }

        if (this.NumericUpDown5.Text != default)
        {
            parameters += "-oblr=" + this.NumericUpDown5.Text + " ";
        }

        if (this.NumericUpDown7.Text != default)
        {
            parameters += "-sr=" + this.NumericUpDown7.Text + " ";
        }

        if (this.NumericUpDown7_Copy.Text != default)
        {
            parameters += "-sr2=" + this.NumericUpDown7_Copy.Text + " ";
        }

        if (this.NumericUpDown7_Copy1.Text != default)
        {
            parameters += "-sr3=" + this.NumericUpDown7_Copy1.Text + " ";
        }

        if (this.CheckBox2.IsChecked is { } arg75 && arg75 == true)
        {
            parameters += "-cpo=1 ";
        }

        if (this.CheckBox3.IsChecked is { } arg76 && arg76 == true)
        {
            parameters += "-cso=1 ";
        }

        if (this.CheckBox4.IsChecked is { } arg77 && arg77 == true)
        {
            parameters += "-ftotc=1 ";
        }
        if (this.CheckBox4_Copy.IsChecked is { } arg78 && arg78 == true)
        {
            parameters += "-rzbt=1 ";
        }
        if (this.CheckBox4_Copy.IsChecked is { } arg79 && arg79 == false)
        {
            parameters += "-rzbt=0 ";
        }

        if (this.Checkbox10.IsChecked is { } arg80 && arg80 == true)
        {
            parameters += "-dtbsowm=1 ";
        }
        if (this.Checkbox9.IsChecked is { } arg81 && arg81 == true)
        {
            parameters += "-cfsa=1 ";
        }
        if (this.CheckBox11.IsChecked is { } arg82 && arg82 == true)
        {
            parameters += "-dct=1 ";
        }
        if (this.Checkbox12.IsChecked is { } arg83 && arg83 == true)
        {
            parameters += "-hps=1 ";
        }
        if (this.Checkbox13.IsChecked is { } arg84 && arg84 == true)
        {
            parameters += "-hss=1 ";
        }
        if (this.Checkbox14.IsChecked is { } arg85 && arg85 == true)
        {
            parameters += "-hpt=1 ";
        }
        if (this.Checkbox15.IsChecked is { } arg86 && arg86 == true)
        {
            parameters += "-hst=1 ";
        }
        if (this.Checkbox16.IsChecked is { } arg87 && arg87 == true)
        {
            parameters += "-sti=1 ";
        }
        if (this.Checkbox16.IsChecked is { } arg88 && arg88 == true)
        {
            parameters += "-console ";
        }

        Process.Start(@"..\..\..\TaskbarSharp\bin\Debug\TaskbarSharp.exe", parameters);
    }

    private async void Button_Click_8(object sender, RoutedEventArgs e)
    {

        var deleteFileDialog = new ContentDialog()
        {
            Title = "Uninstall TaskbarSharp?",
            Content = "Are you sure you want to uninstall TaskbarSharp?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No"
        };
        var result = await deleteFileDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask("TaskbarSharp" + " " + WindowsIdentity.GetCurrent().Name.Replace(@"\", ""));
                    ts.RootFolder.DeleteTask("TaskbarSharp");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                foreach (Process prog in Process.GetProcesses())
                {
                    if (prog.ProcessName == "TaskbarSharp")
                    {
                        prog.Kill();
                    }
                }
            }
            catch
            {
            }

            Thread.Sleep(50);

            var t1 = new Thread(RevertToZero);
            t1.Start();

            var ffFileDialog = new ContentDialog()
            {
                Title = "Ready for removal.",
                Content = "The Taskschedule is successfully removed." + Constants.vbNewLine + "You can now remove TaskbarSharp's files.",
                PrimaryButtonText = "Ok"
            };
            var results2 = await ffFileDialog.ShowAsync();

            Process.Start(AppDomain.CurrentDomain.BaseDirectory);

            Environment.Exit(0);
        }
    }
}