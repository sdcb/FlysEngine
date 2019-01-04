using System;
using Direct3D = SharpDX.Direct3D;
using Direct3D11 = SharpDX.Direct3D11;
using Direct2D1 = SharpDX.Direct2D1;
using DXGI = SharpDX.DXGI;
using SharpDX;

namespace FlysEngine.Tools
{
    public static class DirectXTools
    {
        public static Direct3D11.Device CreateD3Device()
        {
            var supportedFeatureLevels = new[]
            {
                Direct3D.FeatureLevel.Level_11_1,
                Direct3D.FeatureLevel.Level_11_0,
                Direct3D.FeatureLevel.Level_10_1,
                Direct3D.FeatureLevel.Level_10_0,
            };

            var supportedDrivers = new[]
            {
                Direct3D.DriverType.Hardware,
                Direct3D.DriverType.Warp,
            };

            foreach (var driver in supportedDrivers)
            {
                try
                {
                    return new Direct3D11.Device(
                        driver,
                        Direct3D11.DeviceCreationFlags.BgraSupport,
                        supportedFeatureLevels);
                }
                catch (SharpDX.SharpDXException)
                {
                    if (driver == supportedDrivers[supportedDrivers.Length - 1])
                        throw;
                }
            }

            throw new NotSupportedException();
        }

        public static Direct2D1.DeviceContext CreateRenderTarget(
            Direct2D1.Factory1 factory2d,
            Direct3D11.Device device3d)
        {
            var dxgiDevice = device3d.QueryInterface<DXGI.Device>();
            using (var device2d = new Direct2D1.Device(factory2d, dxgiDevice))
            {
                return new Direct2D1.DeviceContext(
                    device2d,
                    Direct2D1.DeviceContextOptions.None);
            }
        }

        public static DXGI.SwapChain1 CreateSwapChainForHwnd(
            Direct3D11.Device device,
            IntPtr hwnd)
        {
            var dxgiDevice = device.QueryInterface<DXGI.Device>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<DXGI.Factory2>();
            var dxgiDesc = new DXGI.SwapChainDescription1
            {
                Format = DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
            };
            return new DXGI.SwapChain1(
                dxgiFactory,
                device,
                hwnd,
                ref dxgiDesc);
        }

        public static DXGI.SwapChain1 CreateSwapChain(
            int width,
            int height,
            Direct3D11.Device device)
        {
            var dxgiDevice = device.QueryInterface<DXGI.Device>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<DXGI.Factory2>();
            var dxgiDesc = new DXGI.SwapChainDescription1
            {
                Width = width,
                Height = height,
                Format = DXGI.Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = DXGI.Scaling.Stretch,
                SwapEffect = DXGI.SwapEffect.FlipSequential,
            };
            return new DXGI.SwapChain1(
                dxgiFactory,
                device,
                ref dxgiDesc);
        }

        public static DXGI.SwapChain1 CreateSwapChainForCoreWindow(
            Direct3D11.Device device, 
            ComObject coreWindow)
        {
            var dxgiDevice = device.QueryInterface<DXGI.Device>();
            var dxgiFactory = dxgiDevice.Adapter.GetParent<DXGI.Factory2>();
            var dxgiDesc = new DXGI.SwapChainDescription1
            {
                Format = DXGI.Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = DXGI.Scaling.Stretch,
                SwapEffect = DXGI.SwapEffect.FlipSequential,
            };
            return new DXGI.SwapChain1(
                dxgiFactory,
                device,
                coreWindow, 
                ref dxgiDesc);
        }

        public static void CreateDeviceSwapChainBitmap(
            DXGI.SwapChain1 swapChain,
            Direct2D1.DeviceContext target)
        {
            using (var surface = swapChain.GetBackBuffer<DXGI.Surface>(0))
            {
                var props = new Direct2D1.BitmapProperties1
                {
                    BitmapOptions = Direct2D1.BitmapOptions.Target | Direct2D1.BitmapOptions.CannotDraw,
                    PixelFormat = new Direct2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Direct2D1.AlphaMode.Ignore)
                };
                using (var bitmap = new Direct2D1.Bitmap1(target, surface, props))
                {
                    target.Target = bitmap;
                }
            }
        }

        public static void CreateDeviceContextCPUBitmap(
            Direct2D1.DeviceContext target, int width, int height)
        {
            var props = new Direct2D1.BitmapProperties1
            {
                BitmapOptions = Direct2D1.BitmapOptions.Target | Direct2D1.BitmapOptions.GdiCompatible,
                PixelFormat = new Direct2D1.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Direct2D1.AlphaMode.Premultiplied)
            };
            using (var bitmap = new Direct2D1.Bitmap1(target, new Size2(width, height), props))
            {
                target.Target = bitmap;
            }
        }
    }
}
