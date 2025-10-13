using Avalonia;
using Avalonia.Media.Imaging;

namespace ScreenPresent.Enums;

public enum ImageOrientation
{
    Normal = 1,
    MirrorHorizontal = 2,
    Rotate180 = 3,
    MirrorVertical = 4,
    MirrorHorizontalAndRotate270CW = 5,
    Rotate90CW = 6,
    MirrorHorizontalAndRotate90CW = 7,
    Rotate270CW = 8,
    Unknown = 0
}

public static class BitmapExtensions
{
    public static Bitmap ApplyOrientation(this Bitmap source, ImageOrientation orientation)
    {
        int width = source.PixelSize.Width;
        int height = source.PixelSize.Height;

        // Swap width/height for 90° or 270° rotations
        bool swapDimensions = orientation is ImageOrientation.Rotate90CW or ImageOrientation.Rotate270CW
            or ImageOrientation.MirrorHorizontalAndRotate90CW or ImageOrientation.MirrorHorizontalAndRotate270CW;

        var outputWidth = swapDimensions ? height : width;
        var outputHeight = swapDimensions ? width : height;

        var target = new RenderTargetBitmap(new PixelSize(outputWidth, outputHeight), source.Dpi);

        using var ctx = target.CreateDrawingContext(false);
        var transform = orientation switch
        {
            ImageOrientation.Normal => new Matrix(1, 0, 0, 1, 0, 0),
            ImageOrientation.MirrorHorizontal => new Matrix(-1, 0, 0, 1, width, 0),
            ImageOrientation.Rotate180 => new Matrix(-1, 0, 0, -1, width, height),
            ImageOrientation.MirrorVertical => new Matrix(1, 0, 0, -1, 0, height),
            ImageOrientation.MirrorHorizontalAndRotate270CW => new Matrix(0, 1, 1, 0, 0, 0),
            ImageOrientation.Rotate90CW => new Matrix(0, 1, -1, 0, height, 0),
            ImageOrientation.MirrorHorizontalAndRotate90CW => new Matrix(0, -1, -1, 0, height, width),
            ImageOrientation.Rotate270CW => new Matrix(0, -1, 1, 0, 0, width),
            _ => new Matrix(1, 0, 0, 1, 0, 0),
        };

        ctx.PushTransform(transform);
        ctx.DrawImage(source, new Rect(0, 0, width, height), new Rect(0, 0, width, height));

        return target;
    }
}