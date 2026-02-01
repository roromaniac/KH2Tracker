using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace KhTracker
{
    public class MemoryReader
    {
        //const int PROCESS_WM_READ = 0x0010;
        //const int PROCESS_WM_WRITE = 0x0030;
        const int PROCESS_WM_RW = 0x1F0FFF;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int nSize, ref int lpNumberOfBytesWritten);

        Process process;
        IntPtr processHandle;
        public bool Hooked;
        private bool PCSX2;
        public MemoryReader(bool ps2)
        {
            PCSX2 = ps2;
            try
            {
                if (PCSX2)
                    process = Process.GetProcessesByName("pcsx2")[0];
                else
                    process = Process.GetProcessesByName("KINGDOM HEARTS II FINAL MIX")[0];
                processHandle = OpenProcess(PROCESS_WM_RW, false, process.Id);
            }
            catch (IndexOutOfRangeException)
            {
                Hooked = false;
                return;
            }
            Hooked = true;
        }

        public byte[] ReadMemory(Int64 address, int bytesToRead, bool absolute = false)
        {
            if (process.HasExited)
            {
                throw new Exception();
            }
            int bytesRead = 0;
            byte[] buffer = new byte[bytesToRead];

            ProcessModule processModule = process.MainModule;

            if (PCSX2)
                ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);
            else
                ReadProcessMemory((int)processHandle, !absolute ? (processModule.BaseAddress.ToInt64() + address) : address, buffer, buffer.Length, ref bytesRead);

            return buffer;
        }

        public void WriteMemory(Int64 address, byte[] valueToWrite, bool absolute = false)
        {
            if (process.HasExited)
            {
                throw new Exception();
            }
            int bytesWritten = 0;
            ProcessModule processModule = process.MainModule;

            if (PCSX2)
                WriteProcessMemory((int)processHandle, address, valueToWrite, valueToWrite.Length, ref bytesWritten);
            else
                WriteProcessMemory((int)processHandle, !absolute ? (processModule.BaseAddress.ToInt64() + address) : address, valueToWrite, valueToWrite.Length, ref bytesWritten);
        }

        public void WriteMem(Int32 address, int valueToWrite)
        {
            if (process.HasExited)
            {
                throw new Exception();
            }
            int bytesWritten = 0;
            //byte[] val = new byte[] { (byte)valueToWrite };
            byte[] buffer = new byte[] { (byte)valueToWrite };

            ProcessModule processModule = process.MainModule;

            if (PCSX2)
                WriteProcessMemory((int)processHandle, address, buffer, 1, ref bytesWritten);
            else
                WriteProcessMemory((int)processHandle, processModule.BaseAddress.ToInt64() + address, buffer, 1, ref bytesWritten);
        }

        public long GetBaseAddress()
        {
            ProcessModule processModule = process.MainModule;
            return processModule.BaseAddress.ToInt64();
        }
    }
}
