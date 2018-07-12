using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopVideoRecorder
{
    public class UnpauseEncoding : CodeActivity
    {
        const string FFMPEG_PROCESS_NAME = "ffmpeg";
        const int KEY_CHARACTER_CODE_ENTER = 0x0D;
        const int WM_KEYDOWN = 0x100;

        [Category("FFmpeg")]
        [Description(@"Process for FFmpeg.exe. Usually, this is from StartRecording Activity")]
        [RequiredArgument]
        public InArgument<Process> FFmpegProcess { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            Stop(FFmpegProcess.Get<Process>(context));
        }

        public static void Stop(Process ps)
        {
            if (ps != null && ps.ProcessName.Contains(FFMPEG_PROCESS_NAME))
            {
                IntPtr hwnd = ps.MainWindowHandle;
                CommonLibs.CallPostMessage(hwnd, WM_KEYDOWN, KEY_CHARACTER_CODE_ENTER, 0);  // send "Enter"
            }
        }
    }
}
