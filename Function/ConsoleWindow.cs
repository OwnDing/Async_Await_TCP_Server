using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Blank_TCP_Server.Function
{
    /// <summary>
    /// 用于隐藏及显示Console终端
    /// </summary>
    public static class ConsoleWindow
    {
        public static void ShowConsoleWindow()
        {
            var handle = SafeNativeMethods.GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                SafeNativeMethods.AllocConsole();
            }
            else
            {
                SafeNativeMethods.ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = SafeNativeMethods.GetConsoleWindow();

            SafeNativeMethods.ShowWindow(handle, SW_HIDE);
        }

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    internal static class SafeNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
