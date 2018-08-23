using WIC = SharpDX.WIC;
using System.IO;
using SharpDX.IO;

namespace FlysEngine.Tools
{
    public static class WicTools
    {
        public static void SaveD2DBitmap(WIC.ImagingFactory wic, WIC.Bitmap wicBitmap, Stream outputStream)
        {
            using (var encoder = new WIC.BitmapEncoder(wic, WIC.ContainerFormatGuids.Png))
            {
                encoder.Initialize(outputStream);
                using (var frame = new WIC.BitmapFrameEncode(encoder))
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

        public static WIC.FormatConverter CreateWicImage(WIC.ImagingFactory wic, string filename)
        {
            using (var decoder = new WIC.JpegBitmapDecoder(wic))
            using (var decodeStream = new WIC.WICStream(wic, filename, NativeFileAccess.Read))
            {
                decoder.Initialize(decodeStream, WIC.DecodeOptions.CacheOnDemand);
                using (var decodeFrame = decoder.GetFrame(0))
                {
                    var converter = new WIC.FormatConverter(wic);
                    converter.Initialize(decodeFrame, WIC.PixelFormat.Format32bppPBGRA);
                    return converter;
                }
            }
        }
    }
}
