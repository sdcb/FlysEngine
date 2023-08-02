using FlysEngine.Tools;
using System;
using System.Drawing;
using Vanara.PInvoke;
using Vortice.Direct2D1;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.User32.WindowStyles;
using static Vanara.PInvoke.User32.SetWindowPosFlags;

namespace FlysEngine.Desktop;

/// <summary>
/// A window class that supports layering and rendering of bitmaps.
/// </summary>
public class LayeredRenderWindow : RenderWindow
{
    private readonly LayeredWindowContext layeredWindowCtx;

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredRenderWindow"/> class.
    /// </summary>
    public LayeredRenderWindow()
    {
        layeredWindowCtx = new LayeredWindowContext(Size, Location);

        {
            // Remove border
            nint style = GetWindowLong(Handle, WindowLongFlags.GWL_STYLE);
            style &= ~(nint)(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
            SetWindowLong(Handle, WindowLongFlags.GWL_STYLE, style);
        }

        {
            // Layered window
            nint style = GetWindowLong(Handle, WindowLongFlags.GWL_EXSTYLE);
            style |= (nint)WindowStylesEx.WS_EX_LAYERED;
            SetWindowLong(Handle, WindowLongFlags.GWL_EXSTYLE, style);
        }

        SetWindowPos(Handle, HWND.NULL, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOOWNERZORDER);
    }

    /// <inheritdoc />
    protected override void OnMove(PointEventArgs e)
    {
        layeredWindowCtx.Move(new Point(e.X, e.Y));
    }

    /// <inheritdoc />
    protected override void OnResize(ResizeEventArgs e)
    {
        layeredWindowCtx.Resize(Size);

        if (!XResource.DeviceAvailable || e.IsMinimized) return;

        OnReleaseDeviceSizeResources();

        XResource.RenderTarget.Target = null;
        DirectXTools.CreateDeviceContextCPUBitmap(XResource.RenderTarget, Size.Width, Size.Height);

        OnCreateDeviceSizeResources();
    }

    /// <inheritdoc />
    protected override void InitializeResources()
    {
        XResource.InitializeDeviceGdiCompatible(Handle.DangerousGetHandle(), Size.Width, Size.Height);

        OnCreateDeviceResources();
        OnCreateDeviceSizeResources();
    }

    /// <inheritdoc />
    protected override void OnPostDraw()
    {
        using ID2D1GdiInteropRenderTarget gdi = XResource.RenderTarget.QueryInterface<ID2D1GdiInteropRenderTarget>();
        IntPtr hdc = gdi.GetDC(DcInitializeMode.Copy);
        layeredWindowCtx.Draw(Handle.DangerousGetHandle(), hdc);
        gdi.ReleaseDC(null);
    }
}
