using System;
using System.Collections.Generic;
using WIC = Vortice.WIC;
using Direct2D1 = Vortice.Direct2D1;

namespace FlysEngine.Managers
{
    public class BitmapManager : IDisposable
    {
        private readonly WIC.IWICImagingFactory2 _imagingFactory;
        private readonly Dictionary<string, Direct2D1.ID2D1Bitmap1> _bmps = new Dictionary<string, Direct2D1.ID2D1Bitmap1>();
        private Direct2D1.ID2D1DeviceContext _renderTarget;

        public BitmapManager(WIC.IWICImagingFactory2 imagingFactory)
        {
            _imagingFactory = imagingFactory;
        }

        public void SetRenderTarget(Direct2D1.ID2D1DeviceContext renderTarget)
        {
            _renderTarget = renderTarget;
        }

        public Direct2D1.ID2D1Bitmap1 this[string filename]
        {
            get
            {
                LoadBitmap(filename);
                return _bmps[filename];
            }
        }

        private void LoadBitmap(string filename)
        {
            if (!_bmps.ContainsKey(filename))
            {
                _bmps[filename] = CreateD2dBitmap(
                    _imagingFactory,
                    filename,
                    _renderTarget);
            }
        }

        public bool UnloadBitmap(string filename)
        {
            if (_bmps.ContainsKey(filename))
            {
                var bmp = _bmps[filename];
                bmp.Dispose();
                _bmps.Remove(filename);
                return true;
            }
            return false;
        }

        private static Direct2D1.ID2D1Bitmap1 CreateD2dBitmap(
            WIC.IWICImagingFactory2 imagingFactory,
            string filename,
            Direct2D1.ID2D1DeviceContext renderTarget)
        {
            using WIC.IWICBitmapDecoder decoder = imagingFactory.CreateDecoderFromFileName(filename);
            using WIC.IWICBitmapFrameDecode frame = decoder.GetFrame(0);

            using WIC.IWICFormatConverter converter = imagingFactory.CreateFormatConverter();
            converter.Initialize(frame, WIC.PixelFormat.Format32bppPBGRA, WIC.BitmapDitherType.None, null, 0, WIC.BitmapPaletteType.Custom);
            return renderTarget.CreateBitmapFromWicBitmap(converter, null);
        }

        public void ReleaseDeviceResources()
        {
            foreach (var kv in _bmps)
            {
                kv.Value.Dispose();
            }
            _bmps.Clear();
        }

        public void Dispose()
        {
            ReleaseDeviceResources();
        }
    }
}
