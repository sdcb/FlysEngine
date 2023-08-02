using System;
using SharpGen.Runtime;
using System.Drawing;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct2D1;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace FlysEngine.Tools;

/// <summary>
/// Provides tools for working with DirectX.
/// </summary>
public static class DirectXTools
{
    /// <summary>
    /// Creates a new Direct3D 11 device.
    /// </summary>
    /// <returns>The new device.</returns>
    public static ID3D11Device CreateD3Device()
    {
        Vortice.Direct3D.FeatureLevel[] supportedFeatureLevels = new[]
        {
            Vortice.Direct3D.FeatureLevel.Level_11_1,
            Vortice.Direct3D.FeatureLevel.Level_11_0,
            Vortice.Direct3D.FeatureLevel.Level_10_1,
            Vortice.Direct3D.FeatureLevel.Level_10_0,
        };

        DriverType[] supportedDrivers = new[]
        {
            DriverType.Hardware,
            DriverType.Warp,
        };

        Result result = default;
        foreach (DriverType driver in supportedDrivers)
        {
            result = D3D11.D3D11CreateDevice(null, driver, DeviceCreationFlags.BgraSupport, supportedFeatureLevels, out ID3D11Device device);
            if (result.Success)
            {
                return device;
            }
        }

        throw new SharpGenException(result);
    }

    /// <summary>
    /// Creates a new Direct2D device context for rendering to a swap chain.
    /// </summary>
    /// <param name="factory2d">The factory used to create the device.</param>
    /// <param name="device3d">The Direct3D device.</param>
    /// <returns>The new device context.</returns>
    public static ID2D1DeviceContext CreateRenderTarget(
        ID2D1Factory1 factory2d,
        ID3D11Device device3d)
    {
        IDXGIDevice dxgiDevice = device3d.QueryInterface<IDXGIDevice>();
        using ID2D1Device device2d = factory2d.CreateDevice(dxgiDevice);
        return device2d.CreateDeviceContext(DeviceContextOptions.None);
    }

    /// <summary>
    /// Creates a new swap chain for a window.
    /// </summary>
    /// <param name="device">The Direct3D device.</param>
    /// <param name="hwnd">The handle of the window to create the swap chain for.</param>
    /// <returns>The new swap chain.</returns>
    public static IDXGISwapChain1 CreateSwapChainForHwnd(
        ID3D11Device device,
        IntPtr hwnd)
    {
        IDXGIDevice dxgiDevice = device.QueryInterface<IDXGIDevice>();
        IDXGIFactory2 dxgiFactory = dxgiDevice.GetAdapter().GetParent<IDXGIFactory2>();
        SwapChainDescription1 dxgiDesc = new()
        {
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 2,
        };
        return dxgiFactory.CreateSwapChainForHwnd(
            device,
            hwnd,
            dxgiDesc);
    }

    /// <summary>
    /// Creates a Direct2D bitmap linked to a swap chain surface.
    /// </summary>
    /// <param name="swapChain">The swap chain to link to.</param>
    /// <param name="target">The device context to use.</param>
    public static void CreateDeviceSwapChainBitmap(
        IDXGISwapChain1 swapChain,
        ID2D1DeviceContext target)
    {
        using IDXGISurface surface = swapChain.GetBuffer<IDXGISurface>(0);
        BitmapProperties1 props = new()
        {
            BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
            PixelFormat = new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Ignore)
        };
        using ID2D1Bitmap1 bitmap = target.CreateBitmapFromDxgiSurface(surface, props);
        target.Target = bitmap;
    }

    /// <summary>
    /// Creates a Direct2D bitmap that can be used on or off-screen.
    /// </summary>
    /// <param name="target">The device context to use.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    public static void CreateDeviceContextCPUBitmap(
        ID2D1DeviceContext target, int width, int height)
    {
        BitmapProperties1 props = new()
        {
            BitmapOptions = BitmapOptions.Target | BitmapOptions.GdiCompatible,
            PixelFormat = new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied)
        };
        using ID2D1Bitmap1 bitmap = target.CreateBitmap(new Size(width, height), IntPtr.Zero, 0, props);
        target.Target = bitmap;
    }
}
