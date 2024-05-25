using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskbarSharp.TrayIconBuster
{
    internal class TrayIconBuster
    {
        private const uint TB_BUTTONCOUNT = 0x418U;
        private const uint TB_GETBUTTON = 0x417U;
        private const uint TB_DELETEBUTTON = 0x416U;

        private static object key = new object();

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            private ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public UIntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        public static uint RemovePhantomIcons()
        {
            bool is64bitWin = Environment.Is64BitOperatingSystem;
            var tbb64 = new ToolBarButton64();
            var tbb32 = new ToolBarButton32();
            var td = new TrayData();
            int totalRemovedCount = 0;
            int totalItemCount = 0;

            lock (key)
            {
                for (int pass = 1; pass <= 2; pass++)
                {
                    for (int kind = 0; kind <= 1; kind++)
                    {
                        var hWnd = IntPtr.Zero;
                        if (kind == 0)
                        {
                            FindNestedWindow(ref hWnd, "Shell_TrayWnd");
                            FindNestedWindow(ref hWnd, "TrayNotifyWnd");
                            FindNestedWindow(ref hWnd, "SysPager");
                            FindNestedWindow(ref hWnd, "ToolbarWindow32");
                        }
                        else
                        {
                            // get the hidden icon collection that exists since Windows 7
                            try
                            {
                                FindNestedWindow(ref hWnd, "NotifyIconOverflowWindow");
                                FindNestedWindow(ref hWnd, "ToolbarWindow32");
                            }
                            catch
                            {
                                // fail silently, as NotifyIconOverflowWindow did not exist prior to Win7
                                break;
                            }
                        }
                        // create an object so we can exchange data with other process
                        using (var process = new LP_Process(hWnd))
                        {
                            IntPtr remoteButtonPtr;
                            if (is64bitWin)
                            {
                                remoteButtonPtr = process.Allocate(tbb64);
                            }
                            else
                            {
                                remoteButtonPtr = process.Allocate(tbb32);
                            }
                            process.Allocate(td);
                            uint itemCount = (uint)Math.Round(Math.Truncate((decimal)SendMessage(hWnd, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero)));

                            int removedCount = 0;
                            for (uint item = 0U, loopTo = (uint)(itemCount - 1L); item <= loopTo; item++)
                            {

                                totalItemCount += 1;
                                // index changes when previous items got removed !
                                uint item2 = (uint)(item - removedCount);
                                uint SOK = (uint)Math.Round(Math.Truncate((decimal)SendMessage(hWnd, TB_GETBUTTON, new IntPtr(item2), remoteButtonPtr)));
                                if (SOK != 1L)
                                {
                                    throw new ApplicationException("TB_GETBUTTON failed");
                                }
                                if (is64bitWin)
                                {
                                    process.Read(tbb64, remoteButtonPtr);
                                    process.Read(td, tbb64.dwData);
                                }
                                else
                                {
                                    process.Read(tbb32, remoteButtonPtr);
                                    process.Read(td, tbb32.dwData);
                                }
                                var hWnd2 = td.hWnd;

                                using (var proc = new LP_Process(hWnd2))
                                {
                                    string filename = proc.GetImageFileName();

                                    // a phantom icon has no imagefilename
                                    if (filename is null)
                                    {
                                        SOK = (uint)Math.Round(Math.Truncate((decimal)SendMessage(hWnd, TB_DELETEBUTTON, new IntPtr(item2), IntPtr.Zero)));

                                        removedCount += 1;
                                        totalRemovedCount += 1;
                                    }
                                }
                            }
                        }
                    } // next kind


                }
            } // release lock

            return (uint)totalRemovedCount;
        }

        // Find a topmost or nested window with specified name
        private static void FindNestedWindow(ref IntPtr hWnd, string name)
        {
            if (hWnd == IntPtr.Zero)
            {
                hWnd = FindWindow(name, null);
            }
            else
            {
                hWnd = FindWindowEx(hWnd, IntPtr.Zero, name, null);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageA", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SendMessage(IntPtr Hdc, uint Msg_Const, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowA", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "FindWindowExA", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [StructLayout(LayoutKind.Sequential)]
        public class ToolBarButton32
        {
            public uint iBitmap;
            public uint idCommand;
            public byte fsState;
            public byte fsStyle;
            private byte bReserved0;
            private byte bReserved1;
            public IntPtr dwData;
            public uint iString;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class ToolBarButton64
        {
            public uint iBitmap;
            public uint idCommand;
            public byte fsState;
            public byte fsStyle;
            private byte bReserved0;
            private byte bReserved1;
            private byte bReserved2;
            private byte bReserved3;
            private byte bReserved4;
            private byte bReserved5;
            public IntPtr dwData;
            public uint iString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TrayData
        {
            public IntPtr hWnd;
            public uint uID;
            public uint uCallbackMessage;
            private uint reserved0;
            private uint reserved1;
            public IntPtr hIcon;
        }
    }

    public class LP_Process : IDisposable
    {
        private const uint PROCESS_VM_OPERATION = 0x8U;
        private const uint PROCESS_VM_READ = 0x10U;
        private const uint PROCESS_VM_WRITE = 0x20U;
        private const uint PROCESS_QUERY_INFORMATION = 0x400U;

        private const uint MEM_COMMIT = 0x1000U;
        private const uint MEM_RELEASE = 0x8000U;
        private const uint PAGE_READWRITE = 0x4U;

        private IntPtr hProcess;
        private uint ownerProcessID;
        
        private ArrayList allocations = new ArrayList();

        public LP_Process(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, ref ownerProcessID);

            hProcess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_QUERY_INFORMATION, false, ownerProcessID);
        }

        public void Dispose()
        {
            if (hProcess != IntPtr.Zero)
            {
                foreach (IntPtr ptr in allocations)
                    VirtualFreeEx(hProcess, ptr, 0, MEM_RELEASE);
                CloseHandle(hProcess);
            }
        }

        public string GetImageFileName()
        {
            var sb = new StringBuilder(1024);
            bool OK = GetProcessImageFileName(hProcess, sb, sb.Capacity - 1);
            if (!OK)
            {
                return null;
            }
            return sb.ToString();
        }

        public IntPtr Allocate(object managedObject)
        {
            int size = Marshal.SizeOf(managedObject);
            var ptr = VirtualAllocEx(hProcess, 0, size, MEM_COMMIT, PAGE_READWRITE);

            if (ptr != IntPtr.Zero)
            {
                allocations.Add(ptr);
            }
            return ptr;
        }

        public void Read(object obj, IntPtr ptr)
        {
            using (var pin = new LP_Pinner(obj))
            {
                uint bytesRead = 0U;
                int size = Marshal.SizeOf(obj);
                if (!ReadProcessMemory(hProcess, ptr, pin.Ptr, size, ref bytesRead))
                {
                    int err = GetLastError();
                    string s = "Read failed; err=" + err + "; bytesRead=" + bytesRead;
                    throw new ApplicationException(s);
                }
            }
        }

        public string ReadString(int size, IntPtr ptr)
        {
            var sb = new StringBuilder(size);
            uint bytesRead = 0U;
            if (!ReadProcessMemory(hProcess, ptr, sb, size, ref bytesRead))
            {
                int err = GetLastError();
                string s = "Read failed; err=" + err + "; bytesRead=" + bytesRead;
                throw new ApplicationException(s);
            }
            return sb.ToString();
        }

        public void Write(object obj, int size, IntPtr ptr)
        {
            using (var pin = new LP_Pinner(obj))
            {
                uint bytesWritten = 0U;
                if (!WriteProcessMemory(hProcess, ptr, pin.Ptr, size, ref bytesWritten))
                {
                    int err = GetLastError();
                    string s = "Write failed; err=" + err + "; bytesWritten=" + bytesWritten;
                    throw new ApplicationException(s);
                }
            }
        }

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint procId);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr OpenProcess(uint access, bool inheritHandle, uint procID);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, int address, int size, uint allocationType, uint protection);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address, int size, uint freeType);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr otherAddress, IntPtr localAddress, int size, ref uint bytesWritten);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr otherAddress, IntPtr localAddress, int size, ref uint bytesRead);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr otherAddress, StringBuilder localAddress, int size, ref uint bytesRead);

        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetProcessImageFileName(IntPtr hProcess, StringBuilder fileName, int fileNameSize);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int GetLastError();
    }

    public class LP_Pinner : IDisposable
    {
        private GCHandle handle;
        private bool disposed;

        private IntPtr ptr_Conflict;

        public LP_Pinner(object obj)
        {

            handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            ptr_Conflict = handle.AddrOfPinnedObject();
        }

        ~LP_Pinner()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {

                disposed = true;
                handle.Free();
                ptr_Conflict = IntPtr.Zero;
            }
        }

        public IntPtr Ptr
        {
            get
            {
                return ptr_Conflict;
            }
        }
    }
}