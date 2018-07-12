using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopVideoRecorder
{
    public class StopRecording : CodeActivity
    {
        const string FFMPEG_PROCESS_NAME = "ffmpeg";
        const int MAX_RETRY_QUIT_COMMAND = 10;
        const int KEY_CHARACTER_CODE_Q = 0x51;
        const int RETRY_SEND_QUIT_COMMAND_WAIT_TIME = 100;
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
            try
            {
                if (ps != null && ps.ProcessName.Contains(FFMPEG_PROCESS_NAME))
                {
                    int counter = 0;
                    while ((!ps.HasExited) && (counter < MAX_RETRY_QUIT_COMMAND))
                    {
                        IntPtr hwnd = ps.MainWindowHandle;
                        CommonLibs.CallPostMessage(hwnd, WM_KEYDOWN, KEY_CHARACTER_CODE_Q, 0); //send "Q"
                        System.Threading.Thread.Sleep(RETRY_SEND_QUIT_COMMAND_WAIT_TIME);
                        counter++;
                    }
                    if (!ps.HasExited && counter >= MAX_RETRY_QUIT_COMMAND)
                    {
                        ps.Kill();
                    }
                }
            }catch
            {
                // If there is an exception, Kill all ffmpeg process
                Process[] processes = Process.GetProcessesByName(FFMPEG_PROCESS_NAME);
                foreach (Process p in processes)
                {
                    p.Kill();
                }
            }
        }
    }

}
