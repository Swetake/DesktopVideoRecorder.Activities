using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using DesktopVideoRecorder.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace DesktopVideoRecorder.Activities
{
    /// <summary>
    /// Start ffmepg to record desktop
    /// </summary>
    [LocalizedDisplayName(nameof(Resources.StartRecording_DisplayName))]
    [LocalizedDescription(nameof(Resources.StartRecording_Description))]
    public class StartRecording : ContinuableAsyncCodeActivity
    {

        const string DEFAULT_VIDEO_CODEC = "libx264";
        const string DEFAULT_PIXEL_FORMAT = "yuv420p";
        const int DEFAULT_MAX_DURATION = 600;  // 10min
        const long DEFAULT_MAX_FILE_SIZE_MB = 2048;  //2048MB
        const int DEFAULT_FPS = 30;  //30fps
        const int MAX_FPS = 60;

        const int DEFAULT_DELAY_AFTER = 500;

        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// for additionl option 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_ExtraOptionString_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_ExtraOptionString_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> ExtraOptionString { get; set; }

        /// <summary>
        /// Full path to ffmpeg.exe.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_FFmpegFilePath_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_FFmpegFilePath_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> FFmpegFilePath { get; set; }

        /// <summary>
        /// Frame rate (fps).
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_FrameRate_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_FrameRate_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<int> FrameRate { get; set; }

        /// <summary>
        /// Max duration (sec).
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_MaxDuration_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_MaxDuration_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<int> MaxDuration { get; set; }

        /// <summary>
        /// Max file size (MByte).
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_MaxFileSize_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_MaxFileSize_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<long> MaxFileSize { get; set; }

        /// <summary>
        /// Pixel format. Default is yuv420p
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_PixelFormat_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_PixelFormat_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> PixelFormat { get; set; }


        /// <summary>
        /// Video codec. Default is libx264.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_VideoCodec_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_VideoCodec_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> VideoCodec { get; set; }

        /// <summary>
        /// Video size. (eg. 1920x1024) If not set, desktop size is applied.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_VideoSize_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_VideoSize_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> VideoSize { get; set; }


        /// <summary>
        /// Process of ffmpeg.exe. For stop this later.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_FFmpegProcess_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_FFmpegProcess_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]

        public OutArgument<Process> FFmpegProcess { get; set; }

        /// <summary>
        /// Full path to outpput file.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_OutputFileName_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_OutputFileName_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public InArgument<string> OutputFileName { get; set; }

        /// <summary>
        /// Delay time after starting ffmpeg process.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_DelayAfter_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_DelayAfter_Description))]
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<int> DelayAfter { get; set; }

        /// <summary>
        /// If set true, this activity is disabled.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StartRecording_DisableActivity_DisplayName))]
        [LocalizedDescription(nameof(Resources.StartRecording_DisableActivity_Description))]
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<bool> DisableActivity { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Constractor
        /// </summary>
        public StartRecording()
        {
        }

        #endregion


        #region Protected Methods

        /// <summary>
        /// CacheMetadata
        /// </summary>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (FFmpegFilePath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FFmpegFilePath)));
            if (OutputFileName == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(OutputFileName)));

            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// Execute Async.
        /// </summary>
        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
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
                arguments.VideoSize = VideoSize.Get(context);

                #region BuildArgument

                //Video Codec:   -vcodec option;
                if (String.IsNullOrEmpty(arguments.VideoCodec))
                {
                    arguments.VideoCodec = DEFAULT_VIDEO_CODEC;
                }

                //Pixel Format:   -pix_fmt option;
                if (String.IsNullOrEmpty(arguments.PixelFormat))
                {
                    arguments.PixelFormat = DEFAULT_PIXEL_FORMAT;
                }

                //Max Durtion:   -t option;
                if (arguments.MaxDuration == 0)
                {
                    arguments.MaxDuration = DEFAULT_MAX_DURATION;
                }

                //Max Filesize:  -fs option
                if (arguments.MaxFileSize == 0)
                {
                    arguments.MaxFileSize = DEFAULT_MAX_FILE_SIZE_MB;
                }

                //Framerate: -framerate option
                if (arguments.FrameRate > 120 || arguments.FrameRate < 1)
                {
                    arguments.FrameRate = DEFAULT_FPS;
                }

                // Delay After
                int delayAfter = 0;
                if (DelayAfter.Get(context) == 0)
                {
                    delayAfter = DEFAULT_DELAY_AFTER;

                }else
                {
                    delayAfter = DelayAfter.Get(context);
                }
                #endregion



                ps =  FFMpegControl.Start(arguments, delayAfter);

            }
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////

            // Outputs
            return (ctx) => {
                FFmpegProcess.Set(ctx, ps);
            };
        }

        #endregion
    }
}

