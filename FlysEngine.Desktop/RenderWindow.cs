using FlysEngine.Managers;
using SharpGen.Runtime;
using System;
using System.Threading;
using Vanara.PInvoke;
using Vortice.Direct2D1;
using Vortice.DXGI;
using static Vanara.PInvoke.User32;

namespace FlysEngine.Desktop;

/// <summary>
/// Represents a window that renders graphics using Direct2D.
/// </summary>
public class RenderWindow : Window
{
    /// <summary>
    /// Gets the XResource instance for the window.
    /// </summary>
    public XResource XResource { get; } = new XResource();
    /// <summary>
    /// Gets the RenderTimer instance for the window.
    /// </summary>
    public RenderTimer RenderTimer { get; } = new RenderTimer();

    /// <summary>
    /// Represents the method that will handle events that have no event data.
    /// </summary>
    /// <param name="window">The render window.</param>
    public delegate void RenderWindowAction(RenderWindow window);
    /// <summary>
    /// Represents the method that represents the draw action.
    /// </summary>
    /// <param name="window">The render window.</param>
    /// <param name="renderTarget">The ID2D1DeviceContext instance used for rendering.</param>
    public delegate void DrawAction(RenderWindow window, ID2D1DeviceContext renderTarget);
    /// <summary>
    /// Represents the method that represents the update logic action.
    /// </summary>
    /// <param name="window">The render window.</param>
    /// <param name="lastFrameTimeInSecond">The time taken by the last frame, in seconds.</param>
    public delegate void UpdateLogicAction(RenderWindow window, float lastFrameTimeInSecond);

    /// <summary>
    /// Occurs when device resources are created.
    /// </summary>
    public event RenderWindowAction CreateDeviceResources;
    /// <summary>
    /// Occurs when the render action is called.
    /// </summary>
    public event DrawAction Draw;
    /// <summary>
    /// Occurs when the update logic action is called.
    /// </summary>
    public event UpdateLogicAction UpdateLogic;
    /// <summary>
    /// Occurs when device size resources are released.
    /// </summary>
    public event RenderWindowAction ReleaseDeviceSizeResources;
    /// <summary>
    /// Occurs when device resources are released.
    /// </summary>
    public event RenderWindowAction ReleaseDeviceResources;
    /// <summary>
    /// Occurs when device size resources are created.
    /// </summary>
    public event RenderWindowAction CreateDeviceSizeResources;

    /// <summary>
    /// Gets or sets a value indicating whether the window can be dragged by the user.
    /// </summary>
    public bool DragMoveEnabled { get; set; }

    /// <summary>
    /// Renders the graphics on the window.
    /// </summary>
    /// <param name="syncInterval">The sync interval to use.</param>
    /// <param name="presentFlags">The present flags to use.</param>
    public virtual void Render(int syncInterval, PresentFlags presentFlags)
    {
        if (WindowState == ShowWindowCommand.SW_SHOWMINIMIZED)
        {
            Thread.Sleep(1);
            return;
        }

        if (!XResource.DeviceAvailable)
        {
            InitializeResources();
        }

        float lastFrameTimeInSecond = RenderTimer.BeginFrame();
        RenderCore(syncInterval, presentFlags, lastFrameTimeInSecond);
        RenderTimer.EndFrame();
    }

    /// <summary>
    /// Handles messages sent to the window.
    /// </summary>
    /// <param name="message">The message sent to the window.</param>
    /// <param name="wParam">Additional message data.</param>
    /// <param name="lParam">Additional message data.</param>
    /// <returns>A pointer to the LRESULT message.</returns>
    protected override IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam)
    {
        if (message == (uint)WindowMessage.WM_NCHITTEST)
        {
            const int HT_CAPTION = 0x2;
            if (DragMoveEnabled)
            {
                return (IntPtr)HT_CAPTION;
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Initializes the resources for the window.
    /// </summary>
    protected virtual void InitializeResources()
    {
        XResource.InitializeDevice(Handle.DangerousGetHandle());

        OnCreateDeviceResources();
        OnCreateDeviceSizeResources();
    }

    private void RenderCore(int syncInterval, PresentFlags presentFlags, float lastFrameTimeInSecond)
    {
        try
        {
            // Freeze logic when render time is slow
            if (lastFrameTimeInSecond < 0.2f)
            {
                XResource.UpdateLogic(RenderTimer.DurationSinceLastFrame);
                OnUpdateLogic(lastFrameTimeInSecond);
            }

            XResource.RenderTarget.BeginDraw();
            {
                OnDraw(XResource.RenderTarget);
                OnPostDraw();
            }
            XResource.RenderTarget.EndDraw();

            XResource.SwapChain.Present(syncInterval, presentFlags);
        }
        catch (SharpGenException e)
        {
            unchecked
            {
                const int DeviceRemoved = (int)0x887a0005;
                const int DeviceReset = (int)0x887A0007;
                if (e.ResultCode == DeviceRemoved || e.ResultCode == DeviceReset)
                {
                    OnReleaseDeviceSizeResources();
                    OnReleaseDeviceResources();
                    XResource.ReleaseDeviceResources();
                }
                else
                {
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Post render action.
    /// </summary>
    protected virtual void OnPostDraw()
    {
    }

    /// <summary>
    /// Occurs when the window is resized.
    /// </summary>
    /// <param name="e">The resize event data.</param>
    protected override void OnResize(ResizeEventArgs e)
    {
        if (!e.IsMinimized && XResource.DeviceAvailable)
        {
            OnReleaseDeviceSizeResources();
            XResource.Resize();
            OnCreateDeviceSizeResources();
        }
    }

    /// <summary>
    /// Occurs when device size resources are created.
    /// </summary>
    protected virtual void OnCreateDeviceSizeResources()
    {
        CreateDeviceSizeResources?.Invoke(this);
    }

    /// <summary>
    /// Occurs when device size resources are released.
    /// </summary>
    protected virtual void OnReleaseDeviceSizeResources()
    {
        XResource.TextFormats.ReleaseResources(includeSizeDependent: true);
        ReleaseDeviceSizeResources?.Invoke(this);
    }

    /// <summary>
    /// Occurs when device resources are released.
    /// </summary>
    protected virtual void OnReleaseDeviceResources()
    {
        ReleaseDeviceResources?.Invoke(this);
    }

    /// <summary>
    /// Occurs when update logic action is called.
    /// </summary>
    /// <param name="lastFrameTimeInSecond">The time taken by the last frame, in seconds.</param>
    protected virtual void OnUpdateLogic(float lastFrameTimeInSecond)
    {
        UpdateLogic?.Invoke(this, lastFrameTimeInSecond);
    }

    /// <summary>
    /// Occurs when the draw action is called.
    /// </summary>
    /// <param name="renderTarget">The ID2D1DeviceContext instance used for rendering.</param>
    protected virtual void OnDraw(ID2D1DeviceContext renderTarget)
    {
        Draw?.Invoke(this, renderTarget);
    }

    /// <summary>
    /// Occurs when device resources are created.
    /// </summary>
    protected virtual void OnCreateDeviceResources()
    {
        CreateDeviceResources?.Invoke(this);
    }

    /// <summary>
    /// Disposes of the resources used by the window.
    /// </summary>
    /// <param name="disposing">True if disposing is allowed</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            OnReleaseDeviceSizeResources();

            OnReleaseDeviceResources();

            XResource.Dispose();
        }

        base.Dispose(disposing);
    }
}
