using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopVideoRecorder
{
    public class StartRecording : CodeActivity
    {
        public const string FFMPEG_EXE_NAME = "ffmpeg.exe";
        public const string BASE_ARGUMENT = @" -f gdigrab -draw_mouse 1 -i desktop  -y ";
        public const string DEFAULT_VIDEO_CODEC = "libx264";
        public const string DEFAULT_PIXEL_FORMAT = "yuv420p";
        public const int DEFAULT_MAX_DURATION = 600;  // 10min
        public const long DEFAULT_MAX_FILE_SIZE_MB = 2048;  //2048MB
        public const int DEFAULT_FPS = 30;  //30fps
        public const int MAX_FPS = 60;

        public const int DEFAULT_DELAY_AFTER = 500;

        const string MSG_FFMPEG_NOT_FOUND_EXCEPTION = "Not found " + FFMPEG_EXE_NAME;
        const string MSG_FFMPEG_FAILED_TO_START = "Failed to start "+FFMPEG_EXE_NAME +". Please check arguments.";

        [Category("Common")]
        [Description("[Optional]Flag if this avtivity is disable. Default is false.")]
        [DefaultValue(false)]
        public InArgument<bool> DisableActivity { get; set; }

        [Category("Common")]
        [Description("[Optional]Delay time after starting ffmpeg. And check if ffmpeg is running then. Default is 500msec")]
        [DefaultValue(DEFAULT_DELAY_AFTER)]
        public InArgument<int> DelayAfter { get; set; }



        [Category("FFmpeg")]
        [Description("Full path to ffmpeg.exe. This string should end with ffmpeg.exe")]
        [RequiredArgument]
        public InArgument<string> FFmpegFilePath { get; set; }

        [Category("FFmpeg")]
        [Description(@"[Optional]Set additional FFmpeg commandline option if necessary.")]
        public InArgument<string> ExtraOptionString { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]VideoCodec. Default is libx264.")]
        [DefaultValue(DEFAULT_VIDEO_CODEC)]
        public InArgument<string> VideoCodec { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]PixelFormat. Default is yuv420p.")]
        [DefaultValue(DEFAULT_PIXEL_FORMAT)]
        public InArgument<string> PixelFormat { get; set; }


        [Category("FFmpeg")]
        [Description("[Optional]Framerate (FPS:Frame Per Second) Default is 30fps.")]
        [DefaultValue(DEFAULT_FPS)]
        public InArgument<int> FrameRate { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]Max duration (sec). Default is 600sec = 10min")]
        [DefaultValue(DEFAULT_MAX_DURATION)]
        public InArgument<int> MaxDuration { get; set; }


        [Category("FFmpeg")]
        [Description("[Optional]Max file size (Mbyte). Deafult is 2048Mbyte=2GByte")]
        [DefaultValue(DEFAULT_MAX_FILE_SIZE_MB)]
        public InArgument<long> MaxFileSize { get; set; }



        [Category("Output")]
        [Description("[Optional]Use this output in StopRecord Activity if you need to stop ffmpeg process.")]
        public OutArgument<Process> FFmpegProcess { get; set; }

        [Category("Output")]
        [Description("Ouput Video filename. This string should end with .avi, .wmv or .mp4 etc.")]
        [RequiredArgument]
        public InArgument<string> OutputFileName { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            Process ps = null;

            if (!DisableActivity.Get(context))
            {
                FFmpegArgument arguments = new FFmpegArgument();
                arguments.FFmpegFilePath = FFmpegFilePath.Get(context);
                arguments.OutputFileName = OutputFileName.Get(context);
                arguments.VideoCodec = VideoCodec.Get(context);
                arguments.PixelFormat = PixelFormat.Get(context);
                arguments.ExtraOptionString = ExtraOptionString.Get(context);
                arguments.MaxDuration = MaxDuration.Get(context);
                arguments.MaxFileSize = MaxFileSize.Get(context);
                arguments.FrameRate = FrameRate.Get(context);

                ps = StartRecording.Start(arguments,DelayAfter.Get(context));
            }

            FFmpegProcess.Set(context, ps);
        }


        public static Process Start(FFmpegArgument ffmpegArgument, int intDelayAfter)
        {
            Process ps = null;

            #region BuildArgument
            string strArgument = BASE_ARGUMENT;

            //Video Codec:   -vcodec option;
            string strVideoCodec = ffmpegArgument.VideoCodec;
            if (String.IsNullOrEmpty(strVideoCodec))
            {
                strVideoCodec = DEFAULT_VIDEO_CODEC;
            }
            strArgument += " -vcodec " + strVideoCodec;



            //Pixel Format:   -pix_fmt option;
            string strPixelFormat = ffmpegArgument.PixelFormat;
            if (String.IsNullOrEmpty(strPixelFormat))
            {
                strPixelFormat = DEFAULT_PIXEL_FORMAT;
            }
            strArgument += " -pix_fmt " + strPixelFormat;



            //Max Durtion:   -t option;
            int intMaxDuration = ffmpegArgument.MaxDuration;
            if (intMaxDuration == 0)
            {
                intMaxDuration = DEFAULT_MAX_DURATION;
            }
            strArgument += " -t " + intMaxDuration.ToString();



            //Max Filesize:  -fs option
            long longMaxFileSize = ffmpegArgument.MaxFileSize;
            if (longMaxFileSize == 0)
            {
                longMaxFileSize = DEFAULT_MAX_FILE_SIZE_MB;
            }
            strArgument += (" -fs " + (longMaxFileSize << 20).ToString()); // Mbyte to byte



            //Framerate: -framerate option
            int intFrameRate = ffmpegArgument.FrameRate;
            if (intFrameRate > 60 || intFrameRate < 1)
            {
                intFrameRate = DEFAULT_FPS;
            }
            strArgument += " -framerate " + intFrameRate.ToString();


            //Extra Options and Video File Name

            strArgument += " " + ffmpegArgument.ExtraOptionString;

            //Extra Options and Video File Name
            string strOutputVideoFileName = "\"" + ffmpegArgument.OutputFileName + "\"";
            strArgument += " " + strOutputVideoFileName;

            #endregion



            //Delete Video file if already exist.
            if (System.IO.File.Exists(ffmpegArgument.OutputFileName))
            {
                System.IO.File.Delete(ffmpegArgument.OutputFileName);
            }

            //Append "ffmpeg.exe" if the argument does not end with it.
            string strFFmpegFilePath = ffmpegArgument.FFmpegFilePath;
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
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Minimized;

                ps = Process.Start(psi);

                System.Threading.Thread.Sleep(intDelayAfter);

                //Check if ffmpeg is normaly running.
                if (ps.HasExited)
                {
                    throw (new Exception(MSG_FFMPEG_FAILED_TO_START));
                }
            }
            else
            {
                throw new System.IO.FileNotFoundException(MSG_FFMPEG_NOT_FOUND_EXCEPTION);
            }
            #endregion

            return ps;
        }
    }

    public class FFmpegArgument
    {
        public string FFmpegFilePath { get; set; }
        public string OutputFileName { get; set; }
        public string VideoCodec { get; set; }
        public string PixelFormat { get; set; }
        public string ExtraOptionString { get; set; }
        public int MaxDuration { get; set; }
        public long MaxFileSize { get; set; }
        public int FrameRate { get; set; }
    }
}
