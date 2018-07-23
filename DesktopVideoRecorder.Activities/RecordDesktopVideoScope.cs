using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace DesktopVideoRecorder
{
    /// <summary>
    /// Record Desktop Video Scope Activity
    /// </summary>
    public class RecordDesktopVideoScope : NativeActivity
    {
        [Browsable(false)]
        public ActivityAction Body { get; set; }

        [Category("Common")]
        [Description("[Optional]Flag if this avtivity is disable. Default is False.")]
        [DefaultValue(false)]
        public InArgument<bool> IsActivityDisable { get; set; }

        [Category("Common")]
        [Description("[Optional]Delay time after starting ffmpeg. Default is 500msec")]
        [DefaultValue(StartRecording.DEFAULT_DELAY_AFTER)]
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
        [DefaultValue(StartRecording.DEFAULT_VIDEO_CODEC)]
        public InArgument<string> VideoCodec { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]PixelFormat. Default is yuv420p.")]
        [DefaultValue(StartRecording.DEFAULT_PIXEL_FORMAT)]
        public InArgument<string> PixelFormat { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]Framerate (FPS:Frame Per Second) Default is 30fps.")]
        [DefaultValue(StartRecording.DEFAULT_FPS)]
        public InArgument<int> FrameRate { get; set; }

        [Category("FFmpeg")]
        [Description("[Optional]Max duration (sec). Default is 600sec = 10min")]
        [DefaultValue(StartRecording.DEFAULT_MAX_DURATION)]
        public InArgument<int> MaxDuration { get; set; }


        [Category("FFmpeg")]
        [Description("[Optional]Max file size (Mbyte). Deafult is 2048Mbyte=2GByte")]
        [DefaultValue(StartRecording.DEFAULT_MAX_FILE_SIZE_MB)]
        public InArgument<int> MaxFileSize { get; set; }



        [Category("Output")]
        [Description("Ouput Video filename. This string should end with .avi or .mp4 etc.")]
        [RequiredArgument]
        public InArgument<string> OutputFileName { get; set; }



        Process ps = null;


        public RecordDesktopVideoScope()
        {
            Body = new ActivityAction
            {
                Handler = new Sequence { DisplayName = "Do" }
            };
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (!IsActivityDisable.Get(context))
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


                ps = StartRecording.Start(arguments, DelayAfter.Get(context));
            }

            if (Body != null)
            {
                context.ScheduleAction(Body, OnCompleted, OnFaulted);
            }

        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            StopRecording.Stop(ps);
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            StopRecording.Stop(ps);
        }


        protected override void Cancel(NativeActivityContext context)
        {
            StopRecording.Stop(ps);
            base.Cancel(context);
        }
    }



}
