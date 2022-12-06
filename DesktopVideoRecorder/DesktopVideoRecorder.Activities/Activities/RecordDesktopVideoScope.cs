using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Activities.Statements;
using System.ComponentModel;
using DesktopVideoRecorder.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace DesktopVideoRecorder.Activities
{
    /// <summary>
    /// RecordDesktopVideoScope 
    /// </summary>
    [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_Description))]
    public class RecordDesktopVideoScope : ContinuableAsyncNativeActivity
    {

        const string DEFAULT_VIDEO_CODEC = "libx264";
        const string DEFAULT_PIXEL_FORMAT = "yuv420p";
        const int DEFAULT_MAX_DURATION = 600;  // 10min
        const long DEFAULT_MAX_FILE_SIZE_MB = 2048;  //2048MB
        const int DEFAULT_FPS = 30;  //30fps
        const int MAX_FPS = 240;

        const int DEFAULT_DELAY_AFTER = 500;


        #region Properties

        /// <summary>
        /// Body.
        /// </summary>
        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Delay after strating ffmepg process
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_DelayAfter_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_DelayAfter_Description))]
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<int> DelayAfter { get; set; }

        /// <summary>
        /// If set, this activity is disabled.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_DisableActivity_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_DisableActivity_Description))]
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<bool> DisableActivity { get; set; }

        /// <summary>
        /// for additionl option 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_ExtraOptionString_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_ExtraOptionString_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> ExtraOptionString { get; set; }

        /// <summary>
        /// Full path to ffmpeg.exe.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_FFmpegFilePath_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_FFmpegFilePath_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> FFmpegFilePath { get; set; }

        /// <summary>
        /// Framerte (fps) 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_FrameRate_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_FrameRate_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<int> FrameRate { get; set; }

        /// <summary>
        /// Max duration (sec) 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_MaxDuration_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_MaxDuration_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<int> MaxDuration { get; set; }

        /// <summary>
        /// Max file size (MByte)
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_MaxFileSize_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_MaxFileSize_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<long> MaxFileSize { get; set; }

        /// <summary>
        /// Pixel format
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_PixelFormat_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_PixelFormat_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> PixelFormat { get; set; }

        /// <summary>
        /// video codec
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_VideoCodec_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_VideoCodec_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> VideoCodec { get; set; }

        /// <summary>
        /// video size 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_VideoSize_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_VideoSize_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<string> VideoSize { get; set; }


        /// <summary>
        /// output filename 
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_OutputFileName_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_OutputFileName_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public InArgument<string> OutputFileName { get; set; }


        /// <summary>
        /// If set true, this activity run as debug mode.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.RecordDesktopVideoScope_DebugMode_DisplayName))]
        [LocalizedDescription(nameof(Resources.RecordDesktopVideoScope_DebugMode_Description))]
        [LocalizedCategory(nameof(Resources.Common_Category))]
        public InArgument<bool> DebugMode { get; set; }


        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        #endregion


        System.Diagnostics.Process ps = null;

        #region Constructors

        public RecordDesktopVideoScope(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer> (ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public RecordDesktopVideoScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        /// <summary>
        /// cache metadata 
        /// </summary>
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (FFmpegFilePath == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FFmpegFilePath)));
            if (OutputFileName == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(OutputFileName)));

            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// Execute Async 
        /// </summary>
        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext  context, CancellationToken cancellationToken)
        {
            // Inputs
            var delayafter = DelayAfter.Get(context);
            var disableactivity = DisableActivity.Get(context);

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
                if (arguments.FrameRate > MAX_FPS || arguments.FrameRate < 1)
                {
                    arguments.FrameRate = DEFAULT_FPS;
                }

                // Delay After
                int delayAfter = 0;
                if (DelayAfter.Get(context) == 0)
                {
                    delayAfter = DEFAULT_DELAY_AFTER;

                }
                else
                {
                    delayAfter = DelayAfter.Get(context);
                }

                // Debug Mode
                bool isDebugMode;
                if (DebugMode.Get(context))
                {
                    isDebugMode = true;
                }
                else
                {
                    isDebugMode = false;
                }
                #endregion

                ps = FFMpegControl.Start(arguments, delayAfter,isDebugMode);
            }

                return (ctx) => {
                // Schedule child activities
                if (Body != null)
				    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
            };
        }

        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {

            FFMpegControl.Stop(ps);
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            FFMpegControl.Stop(ps);
            Cleanup();
        }

        protected override void Cancel(NativeActivityContext context)
        {
            FFMpegControl.Stop(ps);
            base.Cancel(context);
        }

        #endregion


        #region Helpers

        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

