using FlysEngine.Tools;
using System;
using System.Drawing;
using Vanara.PInvoke;
using Vortice.Direct2D1;

namespace FlysEngine.Desktop
{
    public class LayeredRenderWindow : RenderWindow
    {
        private LayeredWindowContext layeredWindowCtx;

        public bool DragMoveEnabled { get; set; }

        public LayeredRenderWindow()
        {
            layeredWindowCtx = new LayeredWindowContext(Size, Location);
        }

        protected override IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam)
        {
            if (message == (uint)User32.WindowMessage.WM_NCHITTEST)
            {
                const int HT_CAPTION = 0x2;
                if (DragMoveEnabled)
                {
                    return (IntPtr)HT_CAPTION;
                }
            }
            else if (message == (uint)User32.WindowMessage.WM_CREATE)
            {

            }

            return IntPtr.Zero;
        }

        protected override void OnMove(int x, int y)
        {
            layeredWindowCtx.Move(new Point(x, y));
        }

        protected override void OnResize(bool isMinimized, int newWidth, int newHeight)
        {
            layeredWindowCtx.Resize(Size);

            if (!XResource.DeviceAvailable || isMinimized) return;

            OnReleaseDeviceSizeResources();

            XResource.RenderTarget.Target = null;
            DirectXTools.CreateDeviceContextCPUBitmap(XResource.RenderTarget, Size.Width, Size.Height);

            OnCreateDeviceSizeResources();
        }

        protected override void InitializeResources()
        {
            XResource.InitializeDeviceGdiCompatible(Handle.DangerousGetHandle(), Size.Width, Size.Height);

            OnCreateDeviceResources();
            OnCreateDeviceSizeResources();
        }

        protected override void OnPostDraw()
        {
            using (ID2D1GdiInteropRenderTarget gdi = XResource.RenderTarget.QueryInterface<ID2D1GdiInteropRenderTarget>())
            {
                var hdc = gdi.GetDC(DcInitializeMode.Copy);
                layeredWindowCtx.Draw(Handle.DangerousGetHandle(), hdc);
                gdi.ReleaseDC(null);
            }
        }
    }
}
