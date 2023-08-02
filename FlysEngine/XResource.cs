using System;
using FlysEngine.Managers;
using FlysEngine.Tools;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;
using Vortice.DXGI;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using System.Numerics;
using Vortice.UIAnimation;

namespace FlysEngine;

/// <summary>
/// Managers all the DirectX Devices &amp; Resources of the application.
/// </summary>
public class XResource : IDisposable
{
    /// <summary>
    /// Gets the Direct2D Factory object.
    /// </summary>
    public readonly ID2D1Factory1 Direct2DFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>(Vortice.Direct2D1.FactoryType.SingleThreaded);
    /// <summary>
    /// Gets the DirectWrite Factory object.
    /// </summary>
    public readonly IDWriteFactory DWriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>(Vortice.DirectWrite.FactoryType.Shared);
    /// <summary>
    ///  Managers the TextFormat objects used in the application.
    /// </summary>
    public readonly TextFormatManager TextFormats;
    /// <summary>
    /// Gets the WIC Factory object.
    /// </summary>
    public readonly IWICImagingFactory WICFactory = new();
    /// <summary>
    /// Managers the Bitmap objects used in the application.
    /// </summary>
    public readonly BitmapManager Bitmaps;
    /// <summary>
    /// Managers the TextLayout objects used in the application.
    /// </summary>
    public readonly TextLayoutManager TextLayouts;
    private ID2D1SolidColorBrush _solidBrush;

    /// <summary>
    /// Gets or sets the IDXGISwapChain1 of the application.
    /// </summary>
    public IDXGISwapChain1 SwapChain;
    /// <summary>
    /// Gets or sets the ID2D1DeviceContext of the application.
    /// </summary>
    public ID2D1DeviceContext RenderTarget;
    /// <summary>
    /// Gets or sets the ID3D11Device of the application.
    /// </summary>
    public ID3D11Device D3Device;
    /// <summary>
    /// Gets or sets the IUIAnimationManager2 object of the application. 
    /// </summary>
    public IUIAnimationManager2 Animation = new();
    /// <summary>
    /// Gets or sets the IUIAnimationTransitionLibrary2 object of the application. 
    /// </summary>
    public IUIAnimationTransitionLibrary2 TransitionLibrary = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="XResource"/> class.
    /// </summary>
    public XResource()
    {
        TextFormats = new TextFormatManager(DWriteFactory);
        Bitmaps = new BitmapManager(WICFactory);
        TextLayouts = new TextLayoutManager(DWriteFactory, TextFormats);
    }

    /// <summary>
    /// Creates a new animation variable given a initial value, final value and duration.
    /// </summary>
    /// <param name="initialValue">Initial value of the animation</param>
    /// <param name="finalValue">Final value of the animation</param>
    /// <param name="durationInSeconds"> The duration of the animation in seconds </param>
    /// <returns> The created animation variable </returns>
    public IUIAnimationVariable2 CreateAnimation(double initialValue, double finalValue, double durationInSeconds)
    {
        IUIAnimationVariable2 v = Animation.CreateAnimationVariable(initialValue);
        using IUIAnimationTransition2 transition = TransitionLibrary.CreateAccelerateDecelerateTransition(durationInSeconds, finalValue, 0.2, 0.8);
        Animation.ScheduleTransition(v, transition, DurationSinceStart.TotalSeconds);
        return v;
    }

    /// <summary>
    /// Returns the <see cref="ID2D1SolidColorBrush"/> representing the specified color.
    /// </summary>
    /// <param name="color">The color to test</param>
    /// <returns>The <see cref="ID2D1SolidColorBrush"/> representing the specified color</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resources haven't been initialized.</exception>
    public ID2D1SolidColorBrush GetColor(Color4 color)
    {
        if (_solidBrush == null)
            throw new InvalidOperationException("Need to call Initialize before call this function.");

        _solidBrush.Color = color;

        return _solidBrush;
    }

    /// <summary>
    /// Gets a new point from an invert applied Matrix3x2 object and a input in Vector2 
    /// </summary>
    /// <param name="matrix">A matrix to invert</param>
    /// <param name="point">A point to invert</param>
    /// <returns> A new Vector2 </returns>
    public static Vector2 InvertTransformPoint(Matrix3x2 matrix, Vector2 point)
    {
        if (Matrix3x2.Invert(matrix, out Matrix3x2 inverted))
        {
            return new Vector2(
                x: point.X * inverted.M11 + point.Y * inverted.M21 + inverted.M31,
                y: point.X * inverted.M12 + point.Y * inverted.M22 + inverted.M32
                );
        }

        return point;
    }

    /// <summary>
    /// Gets a value indicating whether the Direct3D device resources have been initialized.
    /// </summary>
    public bool DeviceAvailable
        => RenderTarget != null && RenderTarget.NativePointer != IntPtr.Zero;

    /// <summary>
    /// Resizes the window of the application if the device is available.
    /// </summary>
    /// <param name="width">Width of the window. Default : 0</param>
    /// <param name="height">Height of the window. Default : 0</param>
    public void Resize(int width = 0, int height = 0)
    {
        if (!DeviceAvailable)
            return;

        RenderTarget.Target = null;
        SwapChain.ResizeBuffers(0, width, height, Format.Unknown, SwapChainFlags.None);
        DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
    }

    /// <summary>
    /// Gets or sets the duration of the application.
    /// </summary>
    public TimeSpan DurationSinceStart { get; private set; }

    /// <summary>
    /// Updates the application's logic.
    /// </summary>
    /// <param name="durationSinceLastUpdate"> The duration since last update.</param>
    public void UpdateLogic(TimeSpan durationSinceLastUpdate)
    {
        DurationSinceStart += durationSinceLastUpdate;
        Animation.Update(DurationSinceStart.TotalSeconds);
    }

    /// <summary>
    /// Initializes all device resources for the specified window handle.
    /// </summary>
    /// <param name="windowHandle">The handle of the window to attach the devices to.</param>
    public void InitializeDevice(IntPtr windowHandle)
    {
        D3Device = DirectXTools.CreateD3Device();
        {
            RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
            _solidBrush = RenderTarget.CreateSolidColorBrush(Colors.Black);
            SwapChain = DirectXTools.CreateSwapChainForHwnd(D3Device, windowHandle);
            DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
            Bitmaps.SetRenderTarget(RenderTarget);
        }
    }

    /// <summary>
    /// Initializes the application's Direct3D resources using the provided window handle and size.
    /// </summary>
    /// <param name="windowHandle">The handle of the window to attach the devices to.</param>
    /// <param name="width">The width of the application window.</param>
    /// <param name="height">The height of the application window.</param>
    public void InitializeDeviceGdiCompatible(IntPtr windowHandle, int width, int height)
    {
        D3Device = DirectXTools.CreateD3Device();
        {
            RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
            _solidBrush = RenderTarget.CreateSolidColorBrush(Colors.Black);
            SwapChain = DirectXTools.CreateSwapChainForHwnd(D3Device, windowHandle);
            //DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
            DirectXTools.CreateDeviceContextCPUBitmap(RenderTarget, width, height);
            Bitmaps.SetRenderTarget(RenderTarget);
        }
    }

    /// <summary>
    /// Releases all device resources currently allocated by this object.
    /// </summary>
    public void ReleaseDeviceResources()
    {
        Bitmaps?.Dispose();
        SwapChain?.Dispose();
        _solidBrush?.Dispose();
        RenderTarget?.Dispose();
        D3Device?.Dispose();
    }

    /// <summary>
    /// Disposes all resources currently allocated by this object.
    /// </summary>
    public virtual void Dispose()
    {
        ReleaseDeviceResources();

        TextLayouts?.Dispose();
        TextFormats?.Dispose();
        Direct2DFactory?.Dispose();
        DWriteFactory?.Dispose();
        WICFactory?.Dispose();
        Animation.Dispose();
        TransitionLibrary.Dispose();

        GC.SuppressFinalize(this);
    }
}
