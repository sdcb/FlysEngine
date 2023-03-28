using FlysEngine.Tools;
using System;
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
            FormBorderStyle = FormBorderStyle.None;
        }

        protected override IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam)
        {
            const int WM_NCHITTEST = 0x84;
            const int HT_CAPTION = 0x2;
            if (DragMoveEnabled)
            {
                if (message == WM_NCHITTEST)
                {
                    return (IntPtr)HT_CAPTION;
                }
            }
            return IntPtr.Zero;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_LAYERED = 0x00080000;
                var baseParams = base.CreateParams;

                baseParams.ExStyle |= (WS_EX_LAYERED);

                return baseParams;
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            layeredWindowCtx.Move(Location);
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
