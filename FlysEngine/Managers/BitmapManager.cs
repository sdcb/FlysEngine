using System;
using System.Collections.Generic;
using Vortice.Direct2D1;
using Vortice.WIC;

namespace FlysEngine.Managers
{
    public class BitmapManager : IDisposable
    {
        private readonly IWICImagingFactory _imagingFactory;
        private readonly Dictionary<string, ID2D1Bitmap1> _bmps = new();
        private ID2D1DeviceContext _renderTarget;

        public BitmapManager(IWICImagingFactory imagingFactory)
        {
            _imagingFactory = imagingFactory;
        }

        public void SetRenderTarget(ID2D1DeviceContext renderTarget)
        {
            _renderTarget = renderTarget;
        }

        public ID2D1Bitmap1 this[string filename]
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

        private static ID2D1Bitmap1 CreateD2dBitmap(
            IWICImagingFactory imagingFactory,
            string filename,
            ID2D1DeviceContext renderTarget)
        {
            using IWICBitmapDecoder decoder = imagingFactory.CreateDecoderFromFileName(filename);
            using IWICBitmapFrameDecode frame = decoder.GetFrame(0);

            using IWICFormatConverter converter = imagingFactory.CreateFormatConverter();
            converter.Initialize(frame, PixelFormat.Format32bppPBGRA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
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
