using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopVideoRecorder
{
    /// <summary>
    /// Container of ffmpeg arguments.
    /// </summary>
    public class FFmpegArgument
   {
        /// <summary>
        /// Full path to ffmpeg.exe.
        /// </summary>
        public string FFmpegFilePath { get; set; }

        /// <summary>
        /// Full path to output file
        /// </summary>
        public string OutputFileName { get; set; }

        /// <summary>
        /// VideoCodec.
        /// </summary>
        public string VideoCodec { get; set; }

        /// <summary>
        /// PixelFormat
        /// </summary>
        public string PixelFormat { get; set; }

        /// <summary>
        /// ExtraOptionString.
        /// </summary>
        public string ExtraOptionString { get; set; }

        /// <summary>
        /// Max duration.
        /// </summary>
        public int MaxDuration { get; set; }

        /// <summary>
        /// Max filesize.
        /// </summary>
        public long MaxFileSize { get; set; }

        /// <summary>
        /// Frame rate
        /// </summary>
        public int FrameRate { get; set; }

        /// <summary>
        /// Video size.
        /// </summary>
        public string VideoSize { get; set; }
    }
}
