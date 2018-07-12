using System;
using System.Activities;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopVideoRecorder
{
    class CommonLibs
    {
        public static void CallPostMessage(IntPtr hWnd, int Msg, int wParam, int lParam)
        {
            PostMessage(hWnd, Msg, wParam, lParam);
        }
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}
