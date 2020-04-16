using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BannerLord.Common
{
    public static class UACChecker
    {
        private const int ERROR_ELEVATION_REQUIRED = 740;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(string lpApplicationName,
           string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
           ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
           CreationFlags dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
           [In] ref STARTUPINFO lpStartupInfo,
           out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
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

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [Flags]
        private enum CreationFlags : uint
        {
            CREATE_SUSPENDED = 0x4
        }

        public static bool RequiresElevation(string Filename)
        {
            var requiresElevation = false;
            var win32error = 0;

            // These are struct's required by CreateProcess API
            //var pInfo = new PROCESS_INFORMATION();
            var sInfo = new STARTUPINFO();
            var pSec = new SECURITY_ATTRIBUTES();
            var tSec = new SECURITY_ATTRIBUTES();

            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            // Attempt to start "Filename" in a suspended state
            var success = CreateProcess(null, Filename,
                ref pSec, ref tSec, false, CreationFlags.CREATE_SUSPENDED,
                IntPtr.Zero, null, ref sInfo, out var pInfo);

            if (success)
                requiresElevation = false; // "Filename" started, so no elevation is required (maybe we are already elevated)
            else
            {
                // An error occurred, find out what it is.
                win32error = Marshal.GetLastWin32Error();
                if (win32error == ERROR_ELEVATION_REQUIRED)
                    requiresElevation = true; // The error states that "Filename" could not start because it requires elevation
            }


            // We don't actually want "Filename" to run, so kill the process and close the handles in pInfo
            TerminateProcess(pInfo.hProcess, 0);
            CloseHandle(pInfo.hThread);
            CloseHandle(pInfo.hProcess);

            // If there was an error, and that error was NOT elevation is required then throw an exception
            if ((win32error != 0) && (win32error != ERROR_ELEVATION_REQUIRED))
                throw new Win32Exception(win32error);

            return requiresElevation;
        }
    }
}
