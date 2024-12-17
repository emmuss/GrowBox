using System.Reflection;
using System.Runtime.InteropServices;
using FFMediaToolkit;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace GrowBox.Server.Services;

public static class ImagesToMp4
{
    public static async Task GenerateTimelapse(
        string outputVideoPath, 
        string[] imagePaths, 
        CancellationToken cancellationToken, 
        int? width = null, 
        int? height = null, 
        TimeSpan? singleImageDuration = null)
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix)
        {
            var ffmpegpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, 
                "FFMPEG");
            FFmpegLoader.FFmpegPath = ffmpegpath;
        }
        else
        { 
            FFmpegLoader.FFmpegPath = "/usr/lib/arm-linux-gnueabihf";
        }

        var firstImage = imagePaths.FirstOrDefault();
        var imgDur = singleImageDuration ?? TimeSpan.FromSeconds(0.5);
        if (firstImage == null)
            return;
        
        var image = await Image.LoadAsync<Rgba32>(firstImage, cancellationToken);
        width = width ?? image.Width;
        height = height ?? image.Height;
        image.Dispose();
        
        // You can set there codec, bitrate, frame rate and many other options.
        var settings = new VideoEncoderSettings(
            width: width.Value, 
            height: height.Value, 
            framerate: 30, 
            codec: VideoCodec.H264)
        {
            EncoderPreset = EncoderPreset.Fast,
            CRF = 17
        };
        using var mediaOutput = MediaBuilder.CreateContainer(Path.GetFullPath(outputVideoPath)).WithVideo(settings).Create();
        
        imagePaths = imagePaths.ToArray();

        var previousDuration = TimeSpan.Zero;
        foreach (var imagePath in imagePaths)
        {
            using var otherImage = await Image.LoadAsync<Rgba32>(imagePath, cancellationToken);
            var pixelData = MemoryMarshal.AsBytes(otherImage.GetPixelMemoryGroup().ToArray()[0].Span).ToArray();
            while (mediaOutput.Video.CurrentDuration - previousDuration < imgDur)
                mediaOutput.Video.AddFrame(
                    ImageData.FromArray(
                        pixelData, 
                        ImagePixelFormat.Rgba32, 
                        otherImage.Width, 
                        otherImage.Height));
            previousDuration = mediaOutput.Video.CurrentDuration;
        }
    }
}