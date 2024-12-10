using FlysEngine.Managers;
using SharpGen.Runtime;
using System;
using System.Threading;
using System.Windows.Forms;
using Vortice.Direct2D1;
using Vortice.DXGI;

namespace FlysEngine.Desktop
{
    public class RenderWindow : Form
    {
        public XResource XResource { get; } = new XResource();
        public RenderTimer RenderTimer { get; } = new RenderTimer();

        public delegate void RenderWindowAction(RenderWindow window);
        public delegate void DrawAction(RenderWindow window, ID2D1DeviceContext renderTarget);
        public delegate void UpdateLogicAction(RenderWindow window, float lastFrameTimeInSecond);

        public event RenderWindowAction CreateDeviceResources;
        public event DrawAction Draw;
        public event UpdateLogicAction UpdateLogic;
        public event RenderWindowAction ReleaseDeviceSizeResources;
        public event RenderWindowAction ReleaseDeviceResources;
        public event RenderWindowAction CreateDeviceSizeResources;

        public virtual void Render(uint syncInterval, PresentFlags presentFlags)
        {
            if (WindowState == FormWindowState.Minimized)
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

        protected virtual void InitializeResources()
        {
            XResource.InitializeDevice(Handle);

            OnCreateDeviceResources();
            OnCreateDeviceSizeResources();
        }

        private void RenderCore(uint syncInterval, PresentFlags presentFlags, float lastFrameTimeInSecond)
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

        protected virtual void OnPostDraw()
        {
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (WindowState != FormWindowState.Minimized && XResource.DeviceAvailable)
            {
                OnReleaseDeviceSizeResources();

                XResource.Resize();

                OnCreateDeviceSizeResources();
            }
        }

        protected virtual void OnCreateDeviceSizeResources()
        {
            CreateDeviceSizeResources?.Invoke(this);
        }

        protected virtual void OnReleaseDeviceSizeResources()
        {
            XResource.TextFormats.ReleaseResources(includeSizeDependent: true);
            ReleaseDeviceSizeResources?.Invoke(this);
        }

        protected virtual void OnReleaseDeviceResources()
        {
            ReleaseDeviceResources?.Invoke(this);
        }

        protected virtual void OnUpdateLogic(float lastFrameTimeInSecond)
        {
            UpdateLogic?.Invoke(this, lastFrameTimeInSecond);
        }

        protected virtual void OnDraw(ID2D1DeviceContext renderTarget)
        {
            Draw?.Invoke(this, renderTarget);
        }

        protected virtual void OnCreateDeviceResources()
        {
            CreateDeviceResources?.Invoke(this);
        }

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
}
