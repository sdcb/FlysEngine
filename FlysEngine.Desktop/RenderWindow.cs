using FlysEngine.Managers;
using SharpGen.Runtime;
using System.Threading;
using Vanara.PInvoke;
using Vortice.Direct2D1;
using Vortice.DXGI;

namespace FlysEngine.Desktop
{
    public class RenderWindow : Window
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

        public RenderWindow(int width = 800, int height = 600, string title = "Render Window") : base(CreateDefault(width, height, title))
        {
        }

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

        protected virtual void OnPostDraw()
        {
        }

        protected override void OnResize(bool isMinimized, int newWidth, int newHeight)
        {
            if (!isMinimized && XResource.DeviceAvailable)
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
