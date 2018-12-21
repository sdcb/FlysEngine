using FlysEngine.Managers;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace FlysEngine.Desktop
{
    public class RenderWindow : Form
    {
        public XResource XResource { get; } = new XResource();
        public RenderTimer RenderTimer { get; } = new RenderTimer();

        public delegate void RenderWindowAction(RenderWindow window);
        public delegate void DrawAction(RenderWindow window, DeviceContext renderTarget);
        public delegate void UpdateLogicAction(RenderWindow window, float lastFrameTimeInSecond);

        public event RenderWindowAction CreateDeviceResources;
        public event DrawAction Draw;
        public event UpdateLogicAction UpdateLogic;
        public event RenderWindowAction ReleaseDeviceSizeResources;
        public event RenderWindowAction ReleaseDeviceResources;
        public event RenderWindowAction CreateDeviceSizeResources;

        public virtual void Render(int syncInterval, PresentFlags presentFlags)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Thread.Sleep(1);
                return;
            }

            if (!XResource.DeviceAvailable)
            {
                XResource.InitializeDevice(Handle);

                OnCreateDeviceResources();
                OnCreateDeviceSizeResources();
            }

            float lastFrameTimeInSecond = RenderTimer.BeginFrame();
            RenderCore(syncInterval, presentFlags, lastFrameTimeInSecond);
            RenderTimer.EndFrame();
        }

        private void RenderCore(int syncInterval, PresentFlags presentFlags, float lastFrameTimeInSecond)
        {
            try
            {
                // Freeze logic when render time is slow
                if (lastFrameTimeInSecond < 0.2f)
                {
                    XResource.UpdateLogicByTimeNow(RenderTimer.DurationSinceStart);
                    OnUpdateLogic(lastFrameTimeInSecond);
                }

                XResource.RenderTarget.BeginDraw();
                {
                    OnDraw(XResource.RenderTarget);
                }
                XResource.RenderTarget.EndDraw();

                XResource.SwapChain.Present(syncInterval, presentFlags);
            }
            catch (SharpDXException e)
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

        protected virtual void OnDraw(DeviceContext renderTarget)
        {
            Draw?.Invoke(this, renderTarget);
        }

        protected virtual void OnCreateDeviceResources()
        {
            CreateDeviceResources?.Invoke(this);
        }

        protected override void Dispose(bool disposing)
        {
            OnReleaseDeviceSizeResources();

            OnReleaseDeviceResources();

            XResource.Dispose();

            base.Dispose(disposing);
        }
    }
}
