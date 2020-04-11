using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Interop;

namespace BannerLordLauncher
{

    public static class WindowPlacement
    {
        // Rect structure required by Data structure
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            [JsonProperty("left")]
            public int Left;
            [JsonProperty("top")]
            public int Top;
            [JsonProperty("right")]
            public int Right;
            [JsonProperty("bottom")]
            public int Bottom;

            public Rect(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        // Point structure required by Data structure
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            [JsonProperty("y")]
            public int X;
            [JsonProperty("x")]
            public int Y;

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        // Data stores the position, size, and state of a window
        [StructLayout(LayoutKind.Sequential)]
        public struct Data
        {
            [JsonIgnore]
            public int length;
            [JsonIgnore]
            public int flags;
            [JsonProperty("showCmd")]
            public int showCmd;
            [JsonProperty("minPosition")]
            public Point minPosition;
            [JsonProperty("maxPosition")]
            public Point maxPosition;
            [JsonProperty("normalPosition")]
            public Rect normalPosition;
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref Data lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out Data lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        private static void SetPlacement(IntPtr windowHandle, Data placement)
        {
            try
            {
                placement.length = Marshal.SizeOf(typeof(Data));
                placement.flags = 0;
                placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
                SetWindowPlacement(windowHandle, ref placement);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static Data GetPlacement(IntPtr windowHandle)
        {
            GetWindowPlacement(windowHandle, out var placement);
            return placement;
        }

        public static void SetPlacement(this Window window, Data placement)
        {
            WindowPlacement.SetPlacement(new WindowInteropHelper(window).Handle, placement);
        }

        public static Data GetPlacement(this Window window)
        {
            return WindowPlacement.GetPlacement(new WindowInteropHelper(window).Handle);
        }


    }
}
