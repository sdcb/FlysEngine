using System;
using Direct3D = Vortice.Direct3D;
using Direct3D11 = Vortice.Direct3D11;
using Direct2D1 = Vortice.Direct2D1;
using DXGI = Vortice.DXGI;
using SharpGen.Runtime;
using System.Drawing;

namespace FlysEngine.Tools
{
    public static class DirectXTools
    {
        public static Direct3D11.ID3D11Device CreateD3Device()
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

            Result result = default;
            foreach (Direct3D.DriverType driver in supportedDrivers)
            {
                result = Direct3D11.D3D11.D3D11CreateDevice(null, driver, Direct3D11.DeviceCreationFlags.BgraSupport, supportedFeatureLevels, out Direct3D11.ID3D11Device device);
                if (result.Success)
                {
                    return device;
                }
            }

            throw new SharpGenException(result);
        }

        public static Direct2D1.ID2D1DeviceContext CreateRenderTarget(
            Direct2D1.ID2D1Factory1 factory2d,
            Direct3D11.ID3D11Device device3d)
        {
            var dxgiDevice = device3d.QueryInterface<DXGI.IDXGIDevice>();
            using (Direct2D1.ID2D1Device device2d = factory2d.CreateDevice(dxgiDevice))
            {
                return device2d.CreateDeviceContext(Direct2D1.DeviceContextOptions.None);
            }
        }

        public static DXGI.IDXGISwapChain1 CreateSwapChainForHwnd(
            Direct3D11.ID3D11Device device,
            IntPtr hwnd)
        {
            DXGI.IDXGIDevice dxgiDevice = device.QueryInterface<DXGI.IDXGIDevice>();
            DXGI.IDXGIFactory2 dxgiFactory = dxgiDevice.GetAdapter().GetParent<DXGI.IDXGIFactory2>();
            DXGI.SwapChainDescription1 dxgiDesc = new()
            {
                Format = DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
            };
            return dxgiFactory.CreateSwapChainForHwnd(
                device,
                hwnd,
                dxgiDesc);
        }

        public static DXGI.IDXGISwapChain1 CreateSwapChain(
            int width,
            int height,
            Direct3D11.ID3D11Device device)
        {
            DXGI.IDXGIDevice dxgiDevice = device.QueryInterface<DXGI.IDXGIDevice>();
            DXGI.IDXGIFactory2 dxgiFactory = dxgiDevice.GetAdapter().GetParent<DXGI.IDXGIFactory2>();
            DXGI.SwapChainDescription1 dxgiDesc = new ()
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
            return dxgiFactory.CreateSwapChain(
                device,
                dxgiDesc);
        }

        public static DXGI.IDXGISwapChain1 CreateSwapChainForCoreWindow(
            Direct3D11.ID3D11Device device, 
            ComObject coreWindow)
        {
            DXGI.IDXGIDevice dxgiDevice = device.QueryInterface<DXGI.IDXGIDevice>();
            DXGI.IDXGIFactory2 dxgiFactory = dxgiDevice.GetAdapter().GetParent<DXGI.IDXGIFactory2>();
            DXGI.SwapChainDescription1 dxgiDesc = new ()
            {
                Format = DXGI.Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = DXGI.Scaling.Stretch,
                SwapEffect = DXGI.SwapEffect.FlipSequential,
            };
            return dxgiFactory.CreateSwapChainForCoreWindow(
                device,
                coreWindow, 
                dxgiDesc);
        }

        public static void CreateDeviceSwapChainBitmap(
            DXGI.IDXGISwapChain1 swapChain,
            Direct2D1.ID2D1DeviceContext target)
        {
            using (var surface = swapChain.GetBuffer<DXGI.IDXGISurface>(0))
            {
                var props = new Direct2D1.BitmapProperties1
                {
                    BitmapOptions = Direct2D1.BitmapOptions.Target | Direct2D1.BitmapOptions.CannotDraw,
                    PixelFormat = new Vortice.DCommon.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Ignore)
                };
                using (var bitmap = target.CreateBitmapFromDxgiSurface(surface, props))
                {
                    target.Target = bitmap;
                }
            }
        }

        public static void CreateDeviceContextCPUBitmap(
            Direct2D1.ID2D1DeviceContext target, int width, int height)
        {
            Direct2D1.BitmapProperties1 props = new()
            {
                BitmapOptions = Direct2D1.BitmapOptions.Target | Direct2D1.BitmapOptions.GdiCompatible,
                PixelFormat = new Vortice.DCommon.PixelFormat(DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied)
            };
            using (Direct2D1.ID2D1Bitmap1 bitmap = target.CreateBitmap(new Size(width, height), IntPtr.Zero, 0, ref props))
            {
                target.Target = bitmap;
            }
        }
    }
}
