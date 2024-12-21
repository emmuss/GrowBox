using GrowBox.Abstractions.Model.EspApi;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GrowBox.Server.Services;

public static class ImageDrawingExtensions
{
    public static FontCollection? Fonts { get; set; }

    public static Font Roboto20 { get; set; } = LoadFont("Roboto", 20);

    private static Font LoadFont(string fontName, int size)
    {
        if (Fonts is null)
        {
            Fonts = new FontCollection();
            Fonts.Add("Fonts/Roboto-Regular.ttf");
        }

        if (!Fonts.TryGet(fontName, out FontFamily family))
            throw new ArgumentException("Font not found.", nameof(fontName));
        
        return family.CreateFont(size);
    }

    public static float IncrementHeight(this ref float y, string text, TextOptions options, float add = 0)
    {
        return y += TextMeasurer.MeasureAdvance(text, options).Height + add;
    }

    public static Image DrawShadowText(this Image image, string text, Color shadowColor, Color textColor, TextOptions textOptions,
        PointF position, float shadowOffset, bool alignRight = false)
    {
        if (alignRight)
        {
            var x = position.X - TextMeasurer.MeasureSize(text, textOptions).Width;
            var y = position.Y;
            position = new PointF(x, y);
        }
        var font = textOptions.Font;
        // Draw the shadow text
        image.Mutate(x =>
            x.DrawText(text, font, shadowColor, new PointF(position.X + shadowOffset, position.Y + shadowOffset)));

        // Draw the actual text on top of the shadow text
        image.Mutate(x => x.DrawText(text, font, textColor, position));

        return image;
    }
}

public class GrowBoxImageMutator
{
    public async Task<Image> Mutate(byte[] imageBytes, DateTime timestamp, GrowBoxEspRoot growBox, CancellationToken cancellationToken)
    {
        using MemoryStream memoryStream = new(imageBytes);
        var image = await Image.LoadAsync<Rgba32>(memoryStream, cancellationToken);
        var font = ImageDrawingExtensions.Roboto20;
        var textOptions = new TextOptions(font);
        float right = image.Width - 20;
        var textColor = Color.White;
        var shadowColor = Color.Black;
        const float pad = 6;
        float top = 20;
        PointF GetPosition() => new (right, top);
        void DrawText(string txt) {
            image!.DrawShadowText(txt, shadowColor, textColor, textOptions!, GetPosition(), 2f, true);
            top.IncrementHeight(txt, textOptions!, pad);
        }

        DrawText(growBox.Me + " - " + timestamp.ToString("dd.MM.yyyy HH:mm:ss"));
        DrawText($"Temp: {Math.Round(growBox.Temperature, 1)}\u00b0C");
        DrawText($"Hum: {Math.Round(growBox.Humidity, 1)} %");

        // var progressBarColor = Color.Green;
        // var progressBarBorderColor = Color.Black;
        // float borderWidth = 1;
        // float percentage = (float)idx / (float)imagePaths.Length;
        // float progressBarContainerW = 300;
        // float progressBarContainerH = 20;
        // float progressBarWidth = (percentage * (progressBarContainerW - 2 * borderWidth));
        // float offsetX = image.Width - 350;
        // float offsetY = 60;
		      //
        // image.Mutate(x => x
        //     // Fill background
        //     .Fill(
        //         Color.FromRgba(255, 255, 255, 125),
        //         new RectangularPolygon(
        //             offsetX, offsetY,
        //             progressBarContainerW,
        //             progressBarContainerH))
        //     // Draw progress bar border
        //     .Draw( 
        //         new SolidPen(progressBarBorderColor, borderWidth),
        //         new RectangularPolygon(
        //             offsetX, offsetY, 
        //             progressBarContainerW, 
        //             progressBarContainerH))
        //     // Draw filled progress bar
        //     .Fill(
        //         progressBarColor,
        //         new RectangularPolygon(
        //             offsetX + borderWidth, offsetY + borderWidth, 
        //             progressBarWidth, 
        //             progressBarContainerH - borderWidth))
        // );

        return image;
    }
}