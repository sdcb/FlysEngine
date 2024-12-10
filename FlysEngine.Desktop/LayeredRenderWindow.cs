using FlysEngine.Tools;
using System;
using System.Windows.Forms;
using Vortice.Direct2D1;
using System.ComponentModel;

namespace FlysEngine.Desktop
{
    public class LayeredRenderWindow : RenderWindow
    {
        private LayeredWindowContext layeredWindowCtx;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DragMoveEnabled { get; set; }

        public LayeredRenderWindow()
        {
            layeredWindowCtx = new LayeredWindowContext(Size, Location);
            FormBorderStyle = FormBorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HT_CAPTION = 0x2;

            base.WndProc(ref m);

            if (DragMoveEnabled)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)(HT_CAPTION);
                }
            }
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

        protected override void OnResize(EventArgs e)
        {
            layeredWindowCtx.Resize(Size);

            if (!XResource.DeviceAvailable || WindowState == FormWindowState.Minimized) return;

            OnReleaseDeviceSizeResources();

            XResource.RenderTarget.Target = null;
            DirectXTools.CreateDeviceContextCPUBitmap(XResource.RenderTarget, Size.Width, Size.Height);

            OnCreateDeviceSizeResources();
        }

        protected override void InitializeResources()
        {
            XResource.InitializeDeviceGdiCompatible(Handle, Size.Width, Size.Height);

            OnCreateDeviceResources();
            OnCreateDeviceSizeResources();
        }

        protected override void OnPostDraw()
        {
            using (ID2D1GdiInteropRenderTarget gdi = XResource.RenderTarget.QueryInterface<ID2D1GdiInteropRenderTarget>())
            {
                var hdc = gdi.GetDC(DcInitializeMode.Copy);
                layeredWindowCtx.Draw(Handle, hdc);
                gdi.ReleaseDC(null);
            }
        }
    }
}
