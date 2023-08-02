using SharpGen.Runtime.Win32;
using System.IO;
using Vortice.WIC;

namespace FlysEngine.Tools;

/// <summary>
/// Contains tools for working with Windows Imaging Component (WIC).
/// </summary>
public static class WicTools
{
    /// <summary>
    /// Saves an IWICBitmap to a stream in PNG format.
    /// </summary>
    /// <param name="wic">The IWICImagingFactory to use for creating an IWICBitmapEncoder.</param>
    /// <param name="wicBitmap">The IWICBitmap to save.</param>
    /// <param name="outputStream">The stream to save the image data to.</param>
    public static void SaveD2DBitmapAsPngStream(IWICImagingFactory wic, IWICBitmap wicBitmap, Stream outputStream)
    {
        using IWICBitmapEncoder encoder = wic.CreateEncoder(ContainerFormat.Png);
        encoder.Initialize(outputStream);
        using IWICBitmapFrameEncode frame = encoder.CreateNewFrame(out IPropertyBag2 props);
        frame.Initialize();
        frame.SetSize(wicBitmap.Size.Width, wicBitmap.Size.Height);

        System.Guid pixelFormat = wicBitmap.PixelFormat;
        frame.SetPixelFormat(ref pixelFormat);
        frame.WriteSource(wicBitmap);

        frame.Commit();
        encoder.Commit();
    }

    /// <summary>
    /// Creates an IWICFormatConverter for a specified image file, to convert a file into a format useable by IWICBitmap.
    /// </summary>
    /// <param name="wic">The IWICImagingFactory to use for creating an IWICBitmapDecoder.</param>
    /// <param name="fileName">The filename of the image to convert.</param>
    /// <returns>The created IWICFormatConverter.</returns>
    public static IWICFormatConverter CreateWicImage(IWICImagingFactory wic, string fileName)
    {
        using IWICBitmapDecoder decoder = wic.CreateDecoderFromFileName(fileName);
        using IWICStream decodeStream = wic.CreateStream(fileName, FileAccess.Read);
        decoder.Initialize(decodeStream, DecodeOptions.CacheOnDemand);
        using IWICBitmapFrameDecode decodeFrame = decoder.GetFrame(0);
        IWICFormatConverter converter = wic.CreateFormatConverter();
        converter.Initialize(decodeFrame, PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
        return converter;
    }
}
