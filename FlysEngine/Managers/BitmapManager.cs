using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIC = SharpDX.WIC;
using Direct2D1 = SharpDX.Direct2D1;

namespace FlysEngine.Managers
{
    public class BitmapManager : IDisposable
    {
        private readonly WIC.ImagingFactory _imagingFactory;
        private readonly Dictionary<string, Direct2D1.Bitmap1> _bmps = new Dictionary<string, Direct2D1.Bitmap1>();
        private Direct2D1.DeviceContext _renderTarget;

        public BitmapManager(WIC.ImagingFactory imagingFactory)
        {
            _imagingFactory = imagingFactory;
        }

        public void SetRenderTarget(Direct2D1.DeviceContext renderTarget)
        {
            _renderTarget = renderTarget;
        }

        public Direct2D1.Bitmap1 this[string filename]
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

        private static Direct2D1.Bitmap1 CreateD2dBitmap(
            WIC.ImagingFactory imagingFactory,
            string filename,
            Direct2D1.DeviceContext renderTarget)
        {
            var decoder = new WIC.BitmapDecoder(imagingFactory, filename, WIC.DecodeOptions.CacheOnLoad);
            WIC.BitmapFrameDecode frame = decoder.GetFrame(0);

            var image = new WIC.FormatConverter(imagingFactory);
            image.Initialize(frame, WIC.PixelFormat.Format32bppPBGRA);
            return Direct2D1.Bitmap1.FromWicBitmap(renderTarget, image);
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
