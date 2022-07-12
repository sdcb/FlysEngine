using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FlysEngine.Desktop
{
    public class LayeredWindowContext : IDisposable
    {
        UpdateLayeredWindowInfo info;
        BlendFunction blend;
        Point source = new Point();

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

        public void Move(Point destination)
        {
            Marshal.DestroyStructure(info.DestinationPoint, typeof(Point));
            info.DestinationPoint = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Point)));
            Marshal.StructureToPtr(destination, info.DestinationPoint, false);
        }

        public void Draw(IntPtr window, IntPtr hdc)
        {
            info.SourceHdc = hdc;
            var ok = UpdateLayeredWindowIndirect(window, ref info);
            if (!ok)
            {
                Debug.WriteLine("ULW failed!, set EXStyle |= WS_EX_LAYERED(0x00080000)");
            }
        }

        public void Resize(Size size)
        {
            Marshal.DestroyStructure(info.WindowSize, typeof(Size));
            info.WindowSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Size)));
            Marshal.StructureToPtr(size, info.WindowSize, false);
        }

        [DllImport("user32", SetLastError = true, ExactSpelling = true)]
        private extern static bool UpdateLayeredWindowIndirect(
            IntPtr hwnd,
            ref UpdateLayeredWindowInfo ulwInfo);

        public void Dispose()
        {
            Marshal.DestroyStructure(info.BlendFunction, typeof(BlendFunction));
            Marshal.DestroyStructure(info.WindowSize, typeof(Size));
            Marshal.DestroyStructure(info.SourcePoint, typeof(Point));
            Marshal.DestroyStructure(info.DestinationPoint, typeof(Point));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UpdateLayeredWindowInfo
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

    [Flags]
    public enum UlwFlags : int
    {
        ColorKey = 1,
        Alpha = 2,
        Opaque = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public BlendFormats AlphaFormat;
    }

    public enum BlendFormats : byte
    {
        Over = 0,
        Alpha = 1,
    }
}
