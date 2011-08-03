using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GR.Win32
{
    class ExternalProcess
    {
        IntPtr process_handle;

        enum ProcessFlags
        {
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_READ = 0x0010,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_QUERY_INFORMATION = 0x0400
        }

        enum MemoryFlags
        {
            PAGE_READWRITE = 0x04,
            MEM_COMMIT = 0x1000,
            MEM_RELEASE = 0x8000
        }

        public ExternalProcess(uint process_id)
        {
            process_handle = OpenProcess((uint)ProcessFlags.PROCESS_VM_OPERATION | (uint)ProcessFlags.PROCESS_VM_READ |
                (uint)ProcessFlags.PROCESS_VM_WRITE | (uint)ProcessFlags.PROCESS_QUERY_INFORMATION, false, process_id);

            if (process_handle == IntPtr.Zero)
            {
                Console.WriteLine("Could not open process " + process_id);
            }
        }

        ~ExternalProcess()
        {
            CloseHandle(process_handle);
        }

        public static ExternalProcess FromWindow(Window window)
        {
            uint process_id;
            GetWindowThreadProcessId(window.Handle, out process_id);

            return new ExternalProcess(process_id);
        }

        public static ExternalProcess Create(string command)
        {
            PROCESS_INFORMATION process_info = new PROCESS_INFORMATION();
            STARTUPINFO startup_info = new STARTUPINFO();

            startup_info.cb = Marshal.SizeOf(startup_info);
            startup_info.dwFlags = STARTF_USESTDHANDLES;

            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();

            bool result = CreateProcess(null, command, ref pSec, ref tSec, true,
                DETACHED_PROCESS, IntPtr.Zero, null, ref startup_info, out process_info);

            int process_id = process_info.dwProcessId;

            if (!result)
            {
                Console.Error.WriteLine("CreateProcess failed");
                return null;
            }

            return new ExternalProcess((uint)process_id);
        }

        public IntPtr AllocateMemory(int size)
        {
            IntPtr pointer = VirtualAllocEx(process_handle, IntPtr.Zero, (uint)size,
                (uint)MemoryFlags.MEM_COMMIT, (uint)MemoryFlags.PAGE_READWRITE);

            if (pointer == IntPtr.Zero) Console.WriteLine("Could not allocate " + size + " bytes of memory in process " + process_handle);

            return pointer;
        }

        public void FreeMemory(IntPtr pointer)
        {
            VirtualFreeEx(process_handle, pointer, UIntPtr.Zero, (uint)MemoryFlags.MEM_RELEASE);
        }

        public void Write(byte[] local_source, IntPtr external_dest, int size)
        {
            //WriteProcessMemory(party_process, dest, source, size, NULL);

            if (!WriteProcessMemory(process_handle, external_dest, local_source, (uint)size, IntPtr.Zero))
            {
                Console.WriteLine("Failed to write to process " + process_handle);
            }
        }

        public void Write(IntPtr local_source, IntPtr external_dest, int size)
        {
            if (!WriteProcessMemory(process_handle, external_dest, local_source, (uint)size, IntPtr.Zero))
            {
                Console.WriteLine("Failed to write to process " + process_handle);
            }
        }

        public void Read(IntPtr external_source, byte[] local_dest, int size)
        {
            //ReadProcessMemory(party_process, source, dest, size, NULL);

            ReadProcessMemory(process_handle, external_source, local_dest, (uint)size, IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);


        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
           uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
           UIntPtr dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           byte[] lpBuffer, uint size, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
          IntPtr lpBuffer, uint size, IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           [Out] byte[] lpBuffer, uint nSize, IntPtr lpNumberOfBytesRead);



        const Int32 STARTF_USESTDHANDLES = 0x00000100;

        const Int32 DETACHED_PROCESS = 0x00000008;

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName,
              string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
              ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
              uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
              [In] ref STARTUPINFO lpStartupInfo,
              out PROCESS_INFORMATION lpProcessInformation);
    }
}
