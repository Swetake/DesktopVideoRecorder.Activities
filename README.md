DesktopVideoRecorder.Activities
==========

DesktopVideoRecorder Activity Package

Table of Contents
-----------------

  * [Requirements](#requirements)
  * [Usage](#usage)
  * [Activity](#activity)

Requirements
------------
　This activity package requires FFmpeg(https://www.ffmpeg.org/).

Usage
-----
  There are 2 ways to record.
  
  * Use **RecordDesktopVideoScope** Activity
    * 1. Put **RecordDesktopVideoScope** Activity 
    * 2. Then put activities you want to record into Do sequence of it.

  * Use **StartRecording** Activity and **StopRecording** Activity
    * 1. Put **StartRecording** Activity at the point of start recording.
    * 2. Put **StopRecording** Activity at the point of finish recording.
    * Note: Basically you have to call StopRecording before workflow terminates .
            Otherwise, recording will be continued until MaxDuration or MaxFileSize.
            
    * You can use PauseEncoding and UnpauseEncoding but this activity provides not pausing recording but pausing encoding. The duration won't be shortened.     
    
Activity
--------
* RecordDesktopVideoScope Avtivity
* StartRecording Activity
  * Common
    * DelayAfter (Int32)
       [Optional] Delay time after starting ffmpeg. And check if ffmpeg is running then. Default is 500msec.
       
    * DisableActivity (Int32)
       [Optional] Flag if this avtivity is disable. Default is false.
 
  * FFmpeg
    * ExtraOptionString (String)
      [Optional]Set additional FFmpeg commandline option if necessary.
    
    * FFmpegFilePath (String)
      Full path to ffmpeg.exe. This string should end with ffmpeg.exe.
      
    * FrameRate (Int32)
      [Optional] Framerate (FPS:Frame Per Second) Default is 30fps.
    
    * MaxDuration (Int32)
      [Optional] Max duration (sec). Default is 600sec = 10min.
    
    * MaxFileSize (Long)
      [Optional] Max file size (Mbyte). Deafult is 2048Mbyte=2GByte.
    
    * PixelFormat (String)
      [Optional] PixelFormat. Default is yuv420p.

    * VideoCodec (String)
      [Optional] VideoCodec. Default is libx264.

  * Output
    * FFmpegProcess (System.Diagnotics.Process)  (This property is only in Start Recording)
      [Optional]Use this output in StopRecord Activity if you need to stop ffmpeg process.
    
    * OutputFileName (String)
      Ouput Video filename. This string should end with .avi, .wmv or .mp4 etc.

* StopRecording Activity
* PasuseEncoding Activity
* UnpasuseEncoding Activity
  * FFmpeg
    * FFmpegProcess  (System.Diagnotics.Process) 
      Process of FFmpeg.exe. Usually, this is from StartRecording Activity
