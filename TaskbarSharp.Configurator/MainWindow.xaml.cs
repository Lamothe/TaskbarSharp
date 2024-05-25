using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Win32.TaskScheduler;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskbarSharp.Common;

namespace TaskbarSharp.Configurator
{
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
                this.placeholder.Fill = mySolidColorBrush;
                this.Background = mySolidColorBrush;
                this.ListBox1.Background = mySolidColorBrush2;
            }

            if (this.Background.ToString() == "#FFFFFFFF")
            {
                var mySolidColorBrush = new SolidColorBrush();
                mySolidColorBrush.Color = System.Windows.Media.Color.FromArgb(255, 240, 240, 240);
                var mySolidColorBrush2 = new SolidColorBrush();
                mySolidColorBrush2.Color = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
                this.placeholder.Fill = mySolidColorBrush;
                this.Background = mySolidColorBrush2;
                this.ListBox1.Background = mySolidColorBrush;
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

            // Checkbox10.Visibility = Visibility.Hidden

            this.ListBox1.SelectedIndex = 0;

            this.sAlpha.Value = 50d;
            this.tpop.Value = 100d;
            this.tsop.Value = 100d;

            this.tbrounding.Value = 0d;

            this.ComboBox1.Items.Add("none");
            this.ComboBox1.Items.Add("linear");
            this.ComboBox1.Items.Add("expoeaseout");
            this.ComboBox1.Items.Add("expoeasein");
            this.ComboBox1.Items.Add("expoeaseinout");
            this.ComboBox1.Items.Add("expoeaseoutin");
            this.ComboBox1.Items.Add("circeaseout");
            this.ComboBox1.Items.Add("circeasein");
            this.ComboBox1.Items.Add("circeaseinout");
            this.ComboBox1.Items.Add("circeaseoutin");
            this.ComboBox1.Items.Add("quadeaseout");
            this.ComboBox1.Items.Add("quadeasein");
            this.ComboBox1.Items.Add("quadeaseinout");
            this.ComboBox1.Items.Add("quadeaseoutin");
            this.ComboBox1.Items.Add("sineeaseout");
            this.ComboBox1.Items.Add("sineeasein");
            this.ComboBox1.Items.Add("sineeaseinout");
            this.ComboBox1.Items.Add("sineeaseoutin");
            this.ComboBox1.Items.Add("cubiceaseout");
            this.ComboBox1.Items.Add("cubiceasein");
            this.ComboBox1.Items.Add("cubiceaseinout");
            this.ComboBox1.Items.Add("cubiceaseoutin");
            this.ComboBox1.Items.Add("quarteaseout");
            this.ComboBox1.Items.Add("quarteasein");
            this.ComboBox1.Items.Add("quarteaseinout");
            this.ComboBox1.Items.Add("quarteaseoutin");
            this.ComboBox1.Items.Add("quinteaseout");
            this.ComboBox1.Items.Add("quinteasein");
            this.ComboBox1.Items.Add("quinteaseinout");
            this.ComboBox1.Items.Add("quinteaseoutin");
            this.ComboBox1.Items.Add("elasticeaseout");
            this.ComboBox1.Items.Add("elasticeasein");
            this.ComboBox1.Items.Add("elasticeaseinout");
            this.ComboBox1.Items.Add("elasticeaseoutin");
            this.ComboBox1.Items.Add("bounceeaseout");
            this.ComboBox1.Items.Add("bounceeasein");
            this.ComboBox1.Items.Add("bounceeaseinout");
            this.ComboBox1.Items.Add("bounceeaseoutin");
            this.ComboBox1.Items.Add("backeaseout");
            this.ComboBox1.Items.Add("backeasein");
            this.ComboBox1.Items.Add("backeaseinout");
            this.ComboBox1.Items.Add("backeaseoutin");

            this.ComboBox1.SelectedItem = "cubiceaseinout";

            this.ComboBox2.Items.Add("none");
            this.ComboBox2.Items.Add("linear");
            this.ComboBox2.Items.Add("expoeaseout");
            this.ComboBox2.Items.Add("expoeasein");
            this.ComboBox2.Items.Add("expoeaseinout");
            this.ComboBox2.Items.Add("expoeaseoutin");
            this.ComboBox2.Items.Add("circeaseout");
            this.ComboBox2.Items.Add("circeasein");
            this.ComboBox2.Items.Add("circeaseinout");
            this.ComboBox2.Items.Add("circeaseoutin");
            this.ComboBox2.Items.Add("quadeaseout");
            this.ComboBox2.Items.Add("quadeasein");
            this.ComboBox2.Items.Add("quadeaseinout");
            this.ComboBox2.Items.Add("quadeaseoutin");
            this.ComboBox2.Items.Add("sineeaseout");
            this.ComboBox2.Items.Add("sineeasein");
            this.ComboBox2.Items.Add("sineeaseinout");
            this.ComboBox2.Items.Add("sineeaseoutin");
            this.ComboBox2.Items.Add("cubiceaseout");
            this.ComboBox2.Items.Add("cubiceasein");
            this.ComboBox2.Items.Add("cubiceaseinout");
            this.ComboBox2.Items.Add("cubiceaseoutin");
            this.ComboBox2.Items.Add("quarteaseout");
            this.ComboBox2.Items.Add("quarteasein");
            this.ComboBox2.Items.Add("quarteaseinout");
            this.ComboBox2.Items.Add("quarteaseoutin");
            this.ComboBox2.Items.Add("quinteaseout");
            this.ComboBox2.Items.Add("quinteasein");
            this.ComboBox2.Items.Add("quinteaseinout");
            this.ComboBox2.Items.Add("quinteaseoutin");
            this.ComboBox2.Items.Add("elasticeaseout");
            this.ComboBox2.Items.Add("elasticeasein");
            this.ComboBox2.Items.Add("elasticeaseinout");
            this.ComboBox2.Items.Add("elasticeaseoutin");
            this.ComboBox2.Items.Add("bounceeaseout");
            this.ComboBox2.Items.Add("bounceeasein");
            this.ComboBox2.Items.Add("bounceeaseinout");
            this.ComboBox2.Items.Add("bounceeaseoutin");
            this.ComboBox2.Items.Add("backeaseout");
            this.ComboBox2.Items.Add("backeasein");
            this.ComboBox2.Items.Add("backeaseinout");
            this.ComboBox2.Items.Add("backeaseoutin");

            this.ComboBox2.SelectedItem = "cubiceaseinout";
            this.startbutton_shortcut.Text = '"' + AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe" + '"' + " -showstartmenu";

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
                                this.RadioButton2.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 2)
                            {
                                this.RadioButton3.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 3)
                            {
                                this.RadioButton4.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 4)
                            {
                                this.RadioButtontc.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 5)
                            {
                                this.RadioButtonoq.IsChecked = true;
                            }

                        }

                        if (argument.Contains("-color="))
                        {
                            string colorval = val[1];
                            string[] colorsep = colorval.Split(";".ToCharArray());

                            this.sRed.Value = Conversions.ToDouble(colorsep[0]);
                            this.sGreen.Value = Conversions.ToDouble(colorsep[1]);
                            this.sBlue.Value = Conversions.ToDouble(colorsep[2]);
                            this.sAlpha.Value = Conversions.ToDouble(colorsep[3]);
                        }

                        if (argument.Contains("-tpop="))
                        {
                            this.tpop.Value = Conversions.ToDouble(val[1]);
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

                        if (argument.Contains("-tsop="))
                        {
                            this.tsop.Value = Conversions.ToDouble(val[1]);
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
                        if (argument.Contains("-as="))
                        {
                            this.ComboBox1.SelectedItem = val[1];

                        }
                        if (argument.Contains("-asp="))
                        {
                            this.NumericUpDown4.Text = val[1];
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
                        if (argument.Contains("-obas="))
                        {
                            this.ComboBox2.SelectedItem = val[1];
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
            if (this.RadioButton2.IsChecked is { } arg24 && arg24 == true)
            {
                parameters += "-tbs=1 ";
            }
            if (this.RadioButton3.IsChecked is { } arg25 && arg25 == true)
            {
                parameters += "-tbs=2 ";
            }
            if (this.RadioButton4.IsChecked is { } arg26 && arg26 == true)
            {
                parameters += "-tbs=3 ";
            }
            if (this.RadioButtontc.IsChecked is { } arg27 && arg27 == true)
            {
                parameters += "-tbs=4 ";
            }
            if (this.RadioButtonoq.IsChecked is { } arg28 && arg28 == true)
            {
                parameters += "-tbs=5 ";
            }

            parameters += "-color=" + this.tRed.Text.ToString() + ";" + this.tGreen.Text.ToString() + ";" + this.tBlue.Text.ToString() + ";" + this.tAlpha.Text.ToString().Replace("%", "") + " ";

            parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

            parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

            if (this.ComboBox1.SelectedItem is not null)
            {
                parameters += "-as=" + this.ComboBox1.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.ComboBox2.SelectedItem is not null)
            {
                parameters += "-obas=" + this.ComboBox2.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.tbrounding.Text != default)
            {
                parameters += "-tbr=" + this.tbrounding.Text + " ";
            }

            if (this.tbsegments.IsChecked is { } arg29 && arg29 == true)
            {
                parameters += "-tbsg=1 ";
            }

            if (this.NumericUpDown4.Text != default)
            {
                parameters += "-asp=" + this.NumericUpDown4.Text + " ";
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
                // MessageBox.Show(ex.Message)
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
                // MessageBox.Show(ex.Message)
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
            if (this.RadioButton2.IsChecked is { } arg46 && arg46 == true)
            {
                parameters += "-tbs=1 ";
            }
            if (this.RadioButton3.IsChecked is { } arg47 && arg47 == true)
            {
                parameters += "-tbs=2 ";
            }
            if (this.RadioButton4.IsChecked is { } arg48 && arg48 == true)
            {
                parameters += "-tbs=3 ";
            }
            if (this.RadioButtontc.IsChecked is { } arg49 && arg49 == true)
            {
                parameters += "-tbs=4 ";
            }
            if (this.RadioButtonoq.IsChecked is { } arg50 && arg50 == true)
            {
                parameters += "-tbs=5 ";
            }

            parameters += "-color=" + this.tRed.Text.ToString() + ";" + this.tGreen.Text.ToString() + ";" + this.tBlue.Text.ToString() + ";" + this.tAlpha.Text.ToString().Replace("%", "") + " ";

            parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

            parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

            if (this.ComboBox1.SelectedItem is not null)
            {
                parameters += "-as=" + this.ComboBox1.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.ComboBox2.SelectedItem is not null)
            {
                parameters += "-obas=" + this.ComboBox2.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.NumericUpDown4.Text != default)
            {
                parameters += "-asp=" + this.NumericUpDown4.Text + " ";
            }

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
                // MessageBox.Show(ex.Message)
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
                // MessageBox.Show(ex.Message)
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
                                this.RadioButton2.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 2)
                            {
                                this.RadioButton3.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 3)
                            {
                                this.RadioButton4.IsChecked = true;
                            }

                            if (Conversions.ToInteger(val[1]) == 4)
                            {
                                this.RadioButtontc.IsChecked = true;
                            }
                            if (Conversions.ToInteger(val[1]) == 5)
                            {
                                this.RadioButtonoq.IsChecked = true;
                            }

                        }

                        if (argument.Contains("-color="))
                        {
                            string colorval = val[1];
                            string[] colorsep = colorval.Split(";".ToCharArray());

                            this.sRed.Value = Conversions.ToDouble(colorsep[0]);
                            this.sGreen.Value = Conversions.ToDouble(colorsep[1]);
                            this.sBlue.Value = Conversions.ToDouble(colorsep[2]);
                            this.sAlpha.Value = Conversions.ToDouble(colorsep[3]);
                        }

                        if (argument.Contains("-tpop="))
                        {
                            this.tpop.Value = Conversions.ToDouble(val[1]);
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

                        if (argument.Contains("-tsop="))
                        {
                            this.tsop.Value = Conversions.ToDouble(val[1]);
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
                        if (argument.Contains("-as="))
                        {
                            this.ComboBox1.SelectedItem = val[1];

                        }
                        if (argument.Contains("-asp="))
                        {
                            this.NumericUpDown4.Text = val[1];
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
                        if (argument.Contains("-obas="))
                        {
                            this.ComboBox2.SelectedItem = val[1];
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
            this.ComboBox1.SelectedItem = "cubiceaseinout";
            this.ComboBox2.SelectedItem = "cubiceaseinout";
            this.NumericUpDown4.Text = "300";
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
            this.sAlpha.Value = 50d;
            this.tpop.Value = 100d;
            this.tsop.Value = 100d;
            this.sRed.Value = 0d;
            this.sGreen.Value = 0d;
            this.sBlue.Value = 0d;

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
                MessageBox.Show(ex.Message, "TaskbarSharp Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (this.RadioButton2.IsChecked is { } arg68 && arg68 == true)
            {
                parameters += "-tbs=1 ";
            }
            if (this.RadioButton3.IsChecked is { } arg69 && arg69 == true)
            {
                parameters += "-tbs=2 ";
            }
            if (this.RadioButton4.IsChecked is { } arg70 && arg70 == true)
            {
                parameters += "-tbs=3 ";
            }
            if (this.RadioButtontc.IsChecked is { } arg71 && arg71 == true)
            {
                parameters += "-tbs=4 ";
            }
            if (this.RadioButtonoq.IsChecked is { } arg72 && arg72 == true)
            {
                parameters += "-tbs=5 ";
            }

            parameters += "-color=" + this.tRed.Text.ToString() + ";" + this.tGreen.Text.ToString() + ";" + this.tBlue.Text.ToString() + ";" + this.tAlpha.Text.ToString().Replace("%", "") + " ";

            parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

            parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

            if (this.ComboBox1.SelectedItem is not null)
            {
                parameters += "-as=" + this.ComboBox1.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.ComboBox2.SelectedItem is not null)
            {
                parameters += "-obas=" + this.ComboBox2.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.tbrounding.Text != default)
            {
                parameters += "-tbr=" + this.tbrounding.Text + " ";
            }

            if (this.tbsegments.IsChecked is { } arg73 && arg73 == true)
            {
                parameters += "-tbsg=1 ";
            }

            if (this.NumericUpDown4.Text != default)
            {
                parameters += "-asp=" + this.NumericUpDown4.Text + " ";
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
            else
            {
            }
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            CallAnimation();
        }

        private void CallAnimation()
        {
            int xx = (int)Math.Round(this.NumericUpDown4.Value);
            string an = this.ComboBox1.Text;

            if (an == "linear")
            {
                var t1 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.Linear, 500, xx));
                t1.Start();
            }
            else if (an == "none")
            {
            }
            // Dim t0 As Thread = New Thread(Sub() Animate(CType(0, IntPtr), 0, "", AddressOf Easings.Linear, 500, 1))
            // t0.Start()
            else if (an == "expoeaseout")
            {
                var t2 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ExpoEaseOut, 500, xx));
                t2.Start();
            }
            else if (an == "expoeasein")
            {
                var t3 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ExpoEaseIn, 500, xx));
                t3.Start();
            }
            else if (an == "expoeaseinout")
            {
                var t4 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ExpoEaseInOut, 500, xx));
                t4.Start();
            }
            else if (an == "expoeaseoutin")
            {
                var t5 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ExpoEaseOutIn, 500, xx));
                t5.Start();
            }
            else if (an == "circeaseout")
            {
                var t6 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CircEaseOut, 500, xx));
                t6.Start();
            }
            else if (an == "circeasein")
            {
                var t7 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CircEaseIn, 500, xx));
                t7.Start();
            }
            else if (an == "circeaseinout")
            {
                var t8 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CircEaseInOut, 500, xx));
                t8.Start();
            }
            else if (an == "circeaseoutin")
            {
                var t9 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CircEaseOutIn, 500, xx));
                t9.Start();
            }
            else if (an == "quadeaseout")
            {
                var t10 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuadEaseOut, 500, xx));
                t10.Start();
            }
            else if (an == "quadeasein")
            {
                var t11 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuadEaseIn, 500, xx));
                t11.Start();
            }
            else if (an == "quadeaseinout")
            {
                var t12 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuadEaseInOut, 500, xx));
                t12.Start();
            }
            else if (an == "quadeaseoutin")
            {
                var t13 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuadEaseOutIn, 500, xx));
                t13.Start();
            }
            else if (an == "sineeaseout")
            {
                var t14 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.SineEaseOut, 500, xx));
                t14.Start();
            }
            else if (an == "sineeasein")
            {
                var t15 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.SineEaseIn, 500, xx));
                t15.Start();
            }
            else if (an == "sineeaseinout")
            {
                var t16 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.SineEaseInOut, 500, xx));
                t16.Start();
            }
            else if (an == "sineeaseoutin")
            {
                var t17 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.SineEaseOutIn, 500, xx));
                t17.Start();
            }
            else if (an == "cubiceaseout")
            {
                var t18 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CubicEaseOut, 500, xx));
                t18.Start();
            }
            else if (an == "cubiceasein")
            {
                var t19 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CubicEaseIn, 500, xx));
                t19.Start();
            }
            else if (an == "cubiceaseinout")
            {
                var t20 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CubicEaseInOut, 500, xx));
                t20.Start();
            }
            else if (an == "cubiceaseoutin")
            {
                var t21 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.CubicEaseOutIn, 500, xx));
                t21.Start();
            }
            else if (an == "quarteaseout")
            {
                var t22 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuartEaseOut, 500, xx));
                t22.Start();
            }
            else if (an == "quarteasein")
            {
                var t23 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuartEaseIn, 500, xx));
                t23.Start();
            }
            else if (an == "quarteaseinout")
            {
                var t24 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuartEaseInOut, 500, xx));
                t24.Start();
            }
            else if (an == "quarteaseoutin")
            {
                var t25 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuartEaseOutIn, 500, xx));
                t25.Start();
            }
            else if (an == "quinteaseout")
            {
                var t26 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuintEaseOut, 500, xx));
                t26.Start();
            }
            else if (an == "quinteasein")
            {
                var t27 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuintEaseIn, 500, xx));
                t27.Start();
            }
            else if (an == "quinteaseinout")
            {
                var t28 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuintEaseInOut, 500, xx));
                t28.Start();
            }
            else if (an == "quinteaseoutin")
            {
                var t29 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.QuintEaseOutIn, 500, xx));
                t29.Start();
            }
            else if (an == "elasticeaseout")
            {
                var t30 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ElasticEaseOut, 500, xx));
                t30.Start();
            }
            else if (an == "elasticeasein")
            {
                var t31 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ElasticEaseIn, 500, xx));
                t31.Start();
            }
            else if (an == "elasticeaseinout")
            {
                var t32 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ElasticEaseInOut, 500, xx));
                t32.Start();
            }
            else if (an == "elasticeaseoutin")
            {
                var t33 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.ElasticEaseOutIn, 500, xx));
                t33.Start();
            }
            else if (an == "bounceeaseout")
            {
                var t34 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BounceEaseOut, 500, xx));
                t34.Start();
            }
            else if (an == "bounceeasein")
            {
                var t35 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BounceEaseIn, 500, xx));
                t35.Start();
            }
            else if (an == "bounceeaseinout")
            {
                var t36 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BounceEaseInOut, 500, xx));
                t36.Start();
            }
            else if (an == "bounceeaseoutin")
            {
                var t37 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BounceEaseOutIn, 500, xx));
                t37.Start();
            }
            else if (an == "backeaseout")
            {
                var t38 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BackEaseOut, 500, xx));
                t38.Start();
            }
            else if (an == "backeasein")
            {
                var t39 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BackEaseIn, 500, xx));
                t39.Start();
            }
            else if (an == "backeaseinout")
            {
                var t40 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BackEaseInOut, 500, xx));
                t40.Start();
            }
            else if (an == "backeaseoutin")
            {
                var t41 = new Thread(() => Animate((IntPtr)0, 0, "", Easings.BackEaseOutIn, 500, xx));
                t41.Start();
            }
        }

        private void Animate(IntPtr hwnd, int oldpos, string orient, EasingDelegate easing, int valueToReach, int duration)
        {

            try
            {

                var sw = new Stopwatch();
                int originalValue = oldpos;
                int elapsed = new int();
                int minValue = Math.Min(originalValue, valueToReach);
                int maxValue = Math.Abs(valueToReach - originalValue);
                bool increasing = originalValue < valueToReach;

                elapsed = 0;
                sw.Start();

                while (!(elapsed >= duration))
                {
                    elapsed = (int)sw.ElapsedMilliseconds;

                    int newValue = (int)Math.Round(Math.Truncate(easing(elapsed, minValue, maxValue, duration)));

                    if (!increasing)
                    {
                        newValue = originalValue + valueToReach - newValue;
                    }

                    this.Dispatcher.Invoke(() => this.Slider1.Value = (double)newValue);

                    Console.WriteLine(newValue);

                }

                sw.Stop();

                this.Dispatcher.Invoke(() => this.Slider1.Value = 0d);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (!(this.ComboBox1.SelectedIndex == this.ComboBox1.Items.Count))
            {
                this.ComboBox1.SelectedIndex = this.ComboBox1.SelectedIndex + 1;
            }
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            if (!(this.ComboBox1.SelectedIndex == 0))
            {
                this.ComboBox1.SelectedIndex = this.ComboBox1.SelectedIndex - 1;
            }
        }

        private void Alpha_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(this.sAlpha.Value);
            this.tAlpha.Text = val.ToString() + "%";

            this.colorprev.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Math.Round(this.sRed.Value), (byte)Math.Round(this.sGreen.Value), (byte)Math.Round(this.sBlue.Value)));

            this.colorprev.Opacity = this.sAlpha.Value / 100d;



            CalculateHexColor2();
        }

        private void Blue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(this.sBlue.Value);
            this.tBlue.Text = val.ToString();

            this.colorprev.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Math.Round(this.sRed.Value), (byte)Math.Round(this.sGreen.Value), (byte)Math.Round(this.sBlue.Value)));

            this.colorprev.Opacity = this.sAlpha.Value / 100d;

            CalculateHexColor2();
        }

        private void Green_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(this.sGreen.Value);
            this.tGreen.Text = val.ToString();

            this.colorprev.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Math.Round(this.sRed.Value), (byte)Math.Round(this.sGreen.Value), (byte)Math.Round(this.sBlue.Value)));

            this.colorprev.Opacity = this.sAlpha.Value / 100d;

            CalculateHexColor2();
        }

        private void Red_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(this.sRed.Value);
            this.tRed.Text = val.ToString();

            this.colorprev.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Math.Round(this.sRed.Value), (byte)Math.Round(this.sGreen.Value), (byte)Math.Round(this.sBlue.Value)));

            this.colorprev.Opacity = this.sAlpha.Value / 100d;

            CalculateHexColor2();
        }

        public void CalculateHexColor2()
        {
            try
            {
                var myColor = System.Drawing.Color.FromArgb((int)Math.Round(this.sRed.Value), (int)Math.Round(this.sGreen.Value), (int)Math.Round(this.sBlue.Value));
                string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");

                this.hexcolorbox.Text = "#" + hex;
            }
            catch
            {
            }
        }

        public void CalculateHexColor()
        {
            try
            {
                var color = ColorTranslator.FromHtml(this.hexcolorbox.Text);
                int r = Convert.ToInt16(color.R);
                int g = Convert.ToInt16(color.G);
                int b = Convert.ToInt16(color.B);

                this.sRed.Value = (double)r;
                this.sGreen.Value = (double)g;
                this.sBlue.Value = (double)b;
            }
            catch
            {
            }
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            var t1 = new Thread(ColorThread);
            t1.Start();
        }

        public void ColorThread()
        {
            var lpPoint = default(Win32.PointAPI);
            bool x = Win32.GetAsyncKeyState(1) == 0;

            do
            {
                Thread.Sleep(1);
                Win32.GetCursorPos(ref lpPoint);

                Console.WriteLine(GetColorAt(lpPoint.x, lpPoint.y));

                var colorp = GetColorAt(lpPoint.x, lpPoint.y);

                this.Dispatcher.Invoke(() =>
                    {
                        // sAlpha.Value = colorp.A
                        this.sRed.Value = (double)colorp.R;
                        this.sGreen.Value = (double)colorp.G;
                        this.sBlue.Value = (double)colorp.B;
                    });
            }

            while (Win32.GetAsyncKeyState(1) == 0);
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {
            string parameters = "";

            if (this.RadioButton1.IsChecked is { } arg89 && arg89 == true)
            {
                parameters += "-tbs=0 ";
            }
            if (this.RadioButton2.IsChecked is { } arg90 && arg90 == true)
            {
                parameters += "-tbs=1 ";
            }
            if (this.RadioButton3.IsChecked is { } arg91 && arg91 == true)
            {
                parameters += "-tbs=2 ";
            }
            if (this.RadioButton4.IsChecked is { } arg92 && arg92 == true)
            {
                parameters += "-tbs=3 ";
            }
            if (this.RadioButtontc.IsChecked is { } arg93 && arg93 == true)
            {
                parameters += "-tbs=4 ";
            }
            if (this.RadioButtonoq.IsChecked is { } arg94 && arg94 == true)
            {
                parameters += "-tbs=5 ";
            }

            parameters += "-color=" + this.tRed.Text.ToString() + ";" + this.tGreen.Text.ToString() + ";" + this.tBlue.Text.ToString() + ";" + this.tAlpha.Text.ToString().Replace("%", "") + " ";

            parameters += "-tpop=" + this.tpopla.Text.ToString().Replace("%", "") + " ";

            parameters += "-tsop=" + this.tsopla.Text.ToString().Replace("%", "") + " ";

            if (this.ComboBox1.SelectedItem is not null)
            {
                parameters += "-as=" + this.ComboBox1.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.tbrounding.Text != default)
            {
                parameters += "-tbr=" + this.tbrounding.Text + " ";
            }

            if (this.tbsegments.IsChecked is { } arg95 && arg95 == true)
            {
                parameters += "-tbsg=1 ";
            }

            if (this.ComboBox2.SelectedItem is not null)
            {
                parameters += "-obas=" + this.ComboBox2.SelectedItem.ToString().ToLower() + " ";
            }

            if (this.NumericUpDown4.Text != default)
            {
                parameters += "-asp=" + this.NumericUpDown4.Text + " ";
            }

            if (this.NumericUpDown1.Text != default)
            {
                parameters += "-ptbo=" + this.NumericUpDown1.Text + " ";
            }
            if (this.NumericUpDown2.Text != default)
            {
                parameters += "-stbo=" + this.NumericUpDown2.Text + " ";
            }

            if (this.CheckBox1.IsChecked is { } arg96 && arg96 == true)
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


            if (this.CheckBox2.IsChecked is { } arg97 && arg97 == true)
            {
                parameters += "-cpo=1 ";
            }

            if (this.CheckBox3.IsChecked is { } arg98 && arg98 == true)
            {
                parameters += "-cso=1 ";
            }

            if (this.CheckBox4.IsChecked is { } arg99 && arg99 == true)
            {
                parameters += "-ftotc=1 ";
            }

            if (this.CheckBox4_Copy.IsChecked is { } arg100 && arg100 == true)
            {
                parameters += "-rzbt=1 ";
            }

            if (this.CheckBox4_Copy.IsChecked is { } arg101 && arg101 == false)
            {
                parameters += "-rzbt=0 ";
            }

            if (this.Checkbox10.IsChecked is { } arg102 && arg102 == true)
            {
                parameters += "-dtbsowm=1 ";
            }
            if (this.Checkbox9.IsChecked is { } arg103 && arg103 == true)
            {
                parameters += "-cfsa=1 ";
            }
            if (this.CheckBox11.IsChecked is { } arg104 && arg104 == true)
            {
                parameters += "-dct=1 ";
            }
            if (this.Checkbox12.IsChecked is { } arg105 && arg105 == true)
            {
                parameters += "-hps=1 ";
            }
            if (this.Checkbox13.IsChecked is { } arg106 && arg106 == true)
            {
                parameters += "-hss=1 ";
            }
            if (this.Checkbox14.IsChecked is { } arg107 && arg107 == true)
            {
                parameters += "-hpt=1 ";
            }
            if (this.Checkbox15.IsChecked is { } arg108 && arg108 == true)
            {
                parameters += "-hst=1 ";
            }
            if (this.Checkbox16.IsChecked is { } arg109 && arg109 == true)
            {
                parameters += "-sti=1 ";
            }
            if (this.checkboxconsole.IsChecked is { } arg110 && arg110 == true)
            {
                parameters += "-console ";
            }

            this.TextboxLink.Text = '"' + AppDomain.CurrentDomain.BaseDirectory + "TaskbarSharp.exe" + '"' + " " + parameters;
        }

        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            Process.Start("https://docs.microsoft.com/en-us/windows/win32/winauto/microsoft-active-accessibility-and-ui-automation-compared");
        }
    }
}