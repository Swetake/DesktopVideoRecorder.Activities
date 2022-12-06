using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopVideoRecorder
{
    /// <summary>
    /// FFmpeg control class
    /// </summary>
    public class FFMpegControl
    {
        const string FFMPEG_EXE_NAME = "ffmpeg.exe";
        const string MSG_FFMPEG_NOT_FOUND_EXCEPTION = "Not found " + FFMPEG_EXE_NAME;
        const string MSG_FFMPEG_FAILED_TO_START = "Failed to start " + FFMPEG_EXE_NAME + ". Please check arguments. Or set True at DebugMode property to investigate cause of the error";

        const string BASE_ARGUMENT = @" -f gdigrab -draw_mouse 1 -i desktop  -y ";

        const string FFMPEG_PROCESS_NAME = "ffmpeg";
        const int MAX_RETRY_QUIT_COMMAND = 10;
        const int KEY_CHARACTER_CODE_Q = 0x51;
        const int RETRY_SEND_QUIT_COMMAND_WAIT_TIME = 100;
        const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// Start ffmpeg process to record desktop
        /// </summary>
        public static Process Start(FFmpegArgument ffmpegArguments, int intDelayAfter, bool isDebugMode)
        {
            Process ps = null;

            //Build Argument string
            StringBuilder sbArgument = new StringBuilder();

            if (!String.IsNullOrEmpty(ffmpegArguments.VideoSize))
            {
                sbArgument.Append(" -video_size " + ffmpegArguments.VideoSize);
            }
            sbArgument.Append(BASE_ARGUMENT);
            sbArgument.Append(" -vcodec " + ffmpegArguments.VideoCodec);
            sbArgument.Append(" -pix_fmt " + ffmpegArguments.PixelFormat);
            sbArgument.Append(" -t " + ffmpegArguments.MaxDuration.ToString());
            sbArgument.Append(" -fs " + (ffmpegArguments.MaxFileSize << 20).ToString());
            sbArgument.Append(" -framerate " + ffmpegArguments.FrameRate.ToString());
            sbArgument.Append(" " + ffmpegArguments.ExtraOptionString);
            sbArgument.Append(" " + ffmpegArguments.OutputFileName);

            string strArgument = sbArgument.ToString();


            //Delete Video file if already exist.
            if (System.IO.File.Exists(ffmpegArguments.OutputFileName))
            {
                System.IO.File.Delete(ffmpegArguments.OutputFileName);
            }

            //Append "ffmpeg.exe" if the argument does not end with it.
            string strFFmpegFilePath = ffmpegArguments.FFmpegFilePath;
            if (!strFFmpegFilePath.EndsWith(FFMPEG_EXE_NAME))
            {
                strFFmpegFilePath = System.IO.Path.Combine(strFFmpegFilePath, FFMPEG_EXE_NAME);
            }

            #region Run FFmpeg
            //Check ffmpeg.exe
            if (System.IO.File.Exists(strFFmpegFilePath))
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = strFFmpegFilePath;
                psi.Arguments = strArgument;

                psi.CreateNoWindow = false;
                if (isDebugMode)
                {
                    psi.UseShellExecute = false;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardOutput = true;
                }
                else
                {
                    psi.UseShellExecute = true;
                    psi.RedirectStandardError = false;
                    psi.RedirectStandardOutput = false;

                }
                psi.WindowStyle = ProcessWindowStyle.Minimized;
                ps = Process.Start(psi);

                System.Threading.Thread.Sleep(intDelayAfter);

                //Check if ffmpeg is normaly running.
                if (ps.HasExited)
                {
                    if (isDebugMode)
                    {
                        throw (new Exception(ps.StandardError.ReadToEnd()));
                    }
                    else
                    {
                        throw (new Exception(MSG_FFMPEG_FAILED_TO_START));
                    }
                }
            }
            else
            {
                throw new System.IO.FileNotFoundException(MSG_FFMPEG_NOT_FOUND_EXCEPTION);
            }
            #endregion

            return ps;
        }

        /// <summary>
        /// Stop recording and terminate ffmpeg process.
        /// </summary>
        public static void Stop(Process ps)
        {
            try
            {
                if (ps != null && ( ps.ProcessName.Contains(FFMPEG_PROCESS_NAME) || ps.MainModule.FileName.Contains(FFMPEG_PROCESS_NAME)))
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
                        ps.CloseMainWindow();
                        System.Threading.Thread.Sleep(RETRY_SEND_QUIT_COMMAND_WAIT_TIME);
                        if (!ps.HasExited)
                        {
                            ps.Kill();
                        }
                    }
                }
            }
            catch
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
