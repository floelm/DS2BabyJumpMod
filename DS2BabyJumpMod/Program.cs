using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DS2BabyJumpMod
{
    class Program
    {
        const string PROCESS_NAME = "DarkSoulsII";
        const int PROCESS_ALL_ACCESS = 0x001F0FFF;

        const int RUN_SPEED_ADDRESS = 0x109C81EC;
        const int SPRINT_SPEED_ADDRESS = 0x109C8208;

        const int SPEED_MODIFIER_ADDRESS = 0x109C82F8;
        const float SPEED_MODIFIER_VALUE = 11f;

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead
        );

        static void Main(string[] args)
        {
            run();
        }

        private static void run()
        {
            IntPtr handle = IntPtr.Zero;

            byte[] speedValueAsBytes = BitConverter.GetBytes(SPEED_MODIFIER_VALUE);

            while (true)
            {
                float? currentSpeedValue = FindValueByProcessAndAddress(handle, (IntPtr)SPEED_MODIFIER_ADDRESS);

                if (null == currentSpeedValue)
                {
                    handle = AttemptFetchingGameHandle();
                    continue;
                }

                if (SPEED_MODIFIER_VALUE != currentSpeedValue)
                {
                    IntPtr bytesWritten = IntPtr.Zero;
                    WriteProcessMemory(handle, (IntPtr)SPEED_MODIFIER_ADDRESS, speedValueAsBytes, speedValueAsBytes.Length, out bytesWritten);
                }

                System.Threading.Thread.Sleep(3000);
            }
        }

        private static float? FindValueByProcessAndAddress(IntPtr process, IntPtr address)
        {
            byte[] dataBuffer = new byte[4];
            IntPtr bytesRead = IntPtr.Zero;

            ReadProcessMemory(process, address, dataBuffer, dataBuffer.Length, out bytesRead);

            if (bytesRead == IntPtr.Zero || bytesRead.ToInt32() < dataBuffer.Length)
            {
                return null;
            }

            return BitConverter.ToSingle(dataBuffer, 0);
        }

        private static IntPtr AttemptFetchingGameHandle()
        {
            Process[] gameProcesses = new Process[0];

            Console.WriteLine("Looking for game...");

            while (gameProcesses.Length == 0)
            {
                System.Threading.Thread.Sleep(3000);
                gameProcesses = Process.GetProcessesByName(PROCESS_NAME);
            }

            Process process = gameProcesses[0];
            IntPtr handle = OpenProcess(PROCESS_ALL_ACCESS, false, (uint)process.Id);

            Console.WriteLine("Game found.");
            Console.WriteLine("Mod running.");
            Console.WriteLine("Keep this window minimized.");

            return handle;
        }
    }
}
