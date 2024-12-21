using System.Reflection;
using FFMpegCore;
namespace GrowBox.Server.Services;

public static class ImagesToMp4
{
    public static async Task GenerateTimelapse(
        string outputVideoPath, 
        string[] imagePaths)
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            var ffmpegpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, 
                "FFMPEG");
            GlobalFFOptions.Configure(options => options.BinaryFolder = "./FFMPEG");
        }
        else
        {
            // try
            // {
            //     FFmpegLoader.FFmpegPath = "/usr/lib/arm-linux-gnueabihf";
            // }
            // catch (Exception e)
            // {
            // }
        }
        FFMpeg.JoinImageSequence(outputVideoPath, frameRate: 5, imagePaths);
    }
}