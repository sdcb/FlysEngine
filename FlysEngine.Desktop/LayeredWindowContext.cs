using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FlysEngine.Desktop;

/// <summary>
/// Represents a wrapper around UpdateLayeredWindow API function.
/// </summary>
internal class LayeredWindowContext : IDisposable
{
    private UpdateLayeredWindowInfo info;
    private BlendFunction blend;
    private Point source = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredWindowContext"/> class.
    /// </summary>
    /// <param name="size">The size of the window to draw.</param>
    /// <param name="destination">The destination point at which to draw the window.</param>
    public LayeredWindowContext(Size size, Point destination)
    {
        blend.SourceConstantAlpha = 0xff;
        blend.AlphaFormat = BlendFormats.Alpha;
        info.BlendFunction = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BlendFunction)));
        Marshal.StructureToPtr(blend, info.BlendFunction, false);

        info.WindowSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Size)));
        Marshal.StructureToPtr(size, info.WindowSize, false);

        info.SourcePoint = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
        Marshal.StructureToPtr(source, info.SourcePoint, false);

        info.DestinationPoint = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
        Marshal.StructureToPtr(destination, info.DestinationPoint, false);

        info.StructureSize = Marshal.SizeOf(typeof(UpdateLayeredWindowInfo));
        info.Flags = UlwFlags.Alpha;
    }

    /// <summary>
    /// Moves the window to a new location specified by <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">The destination point at which to move the window.</param>
    public void Move(Point destination)
    {
        Marshal.DestroyStructure(info.DestinationPoint, typeof(Point));
        info.DestinationPoint = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
        Marshal.StructureToPtr(destination, info.DestinationPoint, false);
    }

    /// <summary>
    /// Draws the window on the screen.
    /// </summary>
    /// <param name="window">The window handle on which to draw.</param>
    /// <param name="hdc">The device context handle of the window.</param>
    public void Draw(IntPtr window, IntPtr hdc)
    {
        info.SourceHdc = hdc;
        bool ok = UpdateLayeredWindowIndirect(window, ref info);
        if (!ok)
        {
            Debug.WriteLine("ULW failed!, set EXStyle |= WS_EX_LAYERED(0x00080000)");
        }
    }

    /// <summary>
    /// Resizes the window.
    /// </summary>
    /// <param name="size">The new size of the window.</param>
    public void Resize(Size size)
    {
        Marshal.DestroyStructure(info.WindowSize, typeof(Size));
        info.WindowSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Size)));
        Marshal.StructureToPtr(size, info.WindowSize, false);
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="LayeredWindowContext"/> object.
    /// </summary>
    public void Dispose()
    {
        Marshal.DestroyStructure(info.BlendFunction, typeof(BlendFunction));
        Marshal.DestroyStructure(info.WindowSize, typeof(Size));
        Marshal.DestroyStructure(info.SourcePoint, typeof(Point));
        Marshal.DestroyStructure(info.DestinationPoint, typeof(Point));
    }

    [DllImport("user32", SetLastError = true, ExactSpelling = true)]
    private static extern bool UpdateLayeredWindowIndirect(IntPtr hwnd, ref UpdateLayeredWindowInfo ulwInfo);
}

/// <summary>
/// Represents structure for SetWindowRgn API function.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct UpdateLayeredWindowInfo
{
    public int StructureSize;
    public IntPtr DestinationHdc;   // HDC
    public IntPtr DestinationPoint; // POINT
    public IntPtr WindowSize;       // Size
    public IntPtr SourceHdc;        // HDC
    public IntPtr SourcePoint;      // POINT
    public int Color;               // RGB
    public IntPtr BlendFunction;    // BlendFunction
    public UlwFlags Flags;
    public IntPtr prcDirty;
}

/// <summary>
/// Enumeration for the flags used with the <see cref="UpdateLayeredWindowInfo"/> structure.
/// </summary>
[Flags]
internal enum UlwFlags : int
{
    ColorKey = 1,
    Alpha = 2,
    Opaque = 4,
}

/// <summary>
/// Represents structure for BlendFunction parameter of the <see cref="UpdateLayeredWindowInfo"/> structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct BlendFunction
{
    public byte BlendOp;
    public byte BlendFlags;
    public byte SourceConstantAlpha;
    public BlendFormats AlphaFormat;
}

/// <summary>
/// Enumeration for the alpha format of the <see cref="BlendFunction"/> structure.
/// </summary>
internal enum BlendFormats : byte
{
    Over = 0,
    Alpha = 1,
}
