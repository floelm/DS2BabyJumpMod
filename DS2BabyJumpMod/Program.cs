using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DS2BabyJumpMod
{
    class Program
    {
        const string PROCESS_NAME = "DarkSoulsII";
        const int PROCESS_ALL_ACCESS = 0x001F0FFF;

        const int SPEED_MODIFIER_ADDRESS = 0x109C82F8;

        const float SPEED_MODIFIER_VALUE = 11f;
        const float RESETTED_SPEED_MODIFIER_VALUE = 6f;

        const int R3_JUMP_ENABLED_ADDRESS = 0x11205CFE;

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
            Console.WriteLine("Disable Babyjumps for circle jumps aswell? (default = yes)");

            string userInput = Console.ReadLine();
            bool disabledCircleShortJumps = true;

            if (userInput.ToUpper() == "NO")
            {
                disabledCircleShortJumps = false;
            }

            run(disabledCircleShortJumps);
        }

        private static void run(bool disabledCircleShortJumps)
        {
            IntPtr handle = IntPtr.Zero;

            while (true)
            {
                System.Threading.Thread.Sleep(1000);

                bool shouldDisableShortJumps = true;
                float? currentSpeedValue = FindValueByProcessAndAddress(handle, (IntPtr)SPEED_MODIFIER_ADDRESS);

                if (null == currentSpeedValue)
                {
                    handle = AttemptFetchingGameHandle();
                    continue;
                }

                if (false == disabledCircleShortJumps)
                {
                    shouldDisableShortJumps = R3JumpIsEnabled(handle);
                }

                if (false == shouldDisableShortJumps && RESETTED_SPEED_MODIFIER_VALUE != currentSpeedValue)
                {
                    ResetJumpLength(handle);
                }

                if (true == shouldDisableShortJumps && SPEED_MODIFIER_VALUE != currentSpeedValue)
                {
                    DisableShortJumps(handle);
                }
            }
        }

        private static bool R3JumpIsEnabled(IntPtr handle)
        {
            return FindBoolByProcessAndAddress(handle, (IntPtr)R3_JUMP_ENABLED_ADDRESS);
        }

        private static void ResetJumpLength(IntPtr handle)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            byte[] smallJumpSpeedValueAsBytes = BitConverter.GetBytes(RESETTED_SPEED_MODIFIER_VALUE);
            WriteProcessMemory(handle, (IntPtr)SPEED_MODIFIER_ADDRESS, smallJumpSpeedValueAsBytes, smallJumpSpeedValueAsBytes.Length, out bytesWritten);
        }

        private static void DisableShortJumps(IntPtr handle)
        {
            IntPtr bytesWritten = IntPtr.Zero;
            byte[] fullJumpValueAsBytes = BitConverter.GetBytes(SPEED_MODIFIER_VALUE);
            WriteProcessMemory(handle, (IntPtr)SPEED_MODIFIER_ADDRESS, fullJumpValueAsBytes, fullJumpValueAsBytes.Length, out bytesWritten);
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


        private static bool FindBoolByProcessAndAddress(IntPtr process, IntPtr address)
        {
            byte[] dataBuffer = new byte[4];
            IntPtr bytesRead = IntPtr.Zero;

            ReadProcessMemory(process, address, dataBuffer, dataBuffer.Length, out bytesRead);

            if (bytesRead == IntPtr.Zero || bytesRead.ToInt32() < dataBuffer.Length)
            {
                return true;
            }

            return BitConverter.ToBoolean(dataBuffer, 0);
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
