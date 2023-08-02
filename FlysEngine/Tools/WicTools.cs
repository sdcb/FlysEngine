using SharpGen.Runtime.Win32;
using System.IO;
using Vortice.WIC;

namespace FlysEngine.Tools;

public static class WicTools
{
    public static void SaveD2DBitmap(IWICImagingFactory wic, IWICBitmap wicBitmap, Stream outputStream)
    {
        using (IWICBitmapEncoder encoder = wic.CreateEncoder(ContainerFormat.Png))
        {
            encoder.Initialize(outputStream);
            using (IWICBitmapFrameEncode frame = encoder.CreateNewFrame(out IPropertyBag2 props))
            {
                frame.Initialize();
                frame.SetSize(wicBitmap.Size.Width, wicBitmap.Size.Height);

                var pixelFormat = wicBitmap.PixelFormat;
                frame.SetPixelFormat(ref pixelFormat);
                frame.WriteSource(wicBitmap);

                frame.Commit();
                encoder.Commit();
            }
        }
    }

    public static IWICFormatConverter CreateWicImage(IWICImagingFactory wic, string fileName)
    {
        using (IWICBitmapDecoder decoder = wic.CreateDecoderFromFileName(fileName))
        using (IWICStream decodeStream = wic.CreateStream(fileName, FileAccess.Read))
        {
            decoder.Initialize(decodeStream, DecodeOptions.CacheOnDemand);
            using (IWICBitmapFrameDecode decodeFrame = decoder.GetFrame(0))
            {
                var converter = wic.CreateFormatConverter();
                converter.Initialize(decodeFrame, PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
                return converter;
            }
        }
    }
}
