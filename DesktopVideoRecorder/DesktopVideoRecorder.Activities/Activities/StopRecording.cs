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
    /// Stop recording and terminate ffmpeg
    /// </summary>
    [LocalizedDisplayName(nameof(Resources.StopRecording_DisplayName))]
    [LocalizedDescription(nameof(Resources.StopRecording_Description))]
    public class StopRecording : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        /// <summary>
        /// Process of ffmpeg.exe to stop.
        /// </summary>
        [LocalizedDisplayName(nameof(Resources.StopRecording_FFmpegProcess_DisplayName))]
        [LocalizedDescription(nameof(Resources.StopRecording_FFmpegProcess_Description))]
        [LocalizedCategory(nameof(Resources.FFmpeg_Category))]
        public InArgument<Process> FFmpegProcess { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public StopRecording()
        {
        }

        #endregion


        #region Protected Methods

        /// <summary>
        /// CacheMetadata.
        /// </summary>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (FFmpegProcess == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FFmpegProcess)));

            base.CacheMetadata(metadata);
        }

        /// <summary>
        /// Execute Async
        /// </summary>
        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var ffmpegprocess = FFmpegProcess.Get(context);

            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////

            FFMpegControl.Stop(ffmpegprocess);

            // Outputs
            return (ctx) => {
            };
        }

        #endregion
    }
}

