using System;
using Direct2D1 = SharpDX.Direct2D1;
using Direct3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using DWrite = SharpDX.DirectWrite;
using WIC = SharpDX.WIC;
using Animation = SharpDX.Animation;
using FlysEngine.Managers;
using SharpDX;
using FlysEngine.Tools;
using System.Diagnostics;

namespace FlysEngine
{
    public class XResource : IDisposable
    {
        public readonly Direct2D1.Factory1 Direct2DFactory = new Direct2D1.Factory1();
        public readonly DWrite.Factory DWriteFactory = new DWrite.Factory(DWrite.FactoryType.Shared);
        public readonly TextFormatManager TextFormats;
        public readonly WIC.ImagingFactory2 WICFactory = new WIC.ImagingFactory2();
        public readonly BitmapManager Bitmaps;
        public readonly TextLayoutManager TextLayouts;
        private Direct2D1.SolidColorBrush _solidBrush;

        public readonly Animation.Manager AnimationManager = new Animation.Manager();
        public readonly Animation.TransitionLibrary TransitionLibrary = new Animation.TransitionLibrary();

        public DXGI.SwapChain1 SwapChain;
        public Direct2D1.DeviceContext RenderTarget;
        public Direct3D11.Device D3Device;

        public XResource()
        {
            TextFormats = new TextFormatManager(DWriteFactory);
            Bitmaps = new BitmapManager(WICFactory);
            TextLayouts = new TextLayoutManager(DWriteFactory, TextFormats);
        }

        public Direct2D1.SolidColorBrush GetColor(Color color)
        {
            if (_solidBrush == null)
                throw new InvalidOperationException("Need to call Initialize before call this function.");
            
            _solidBrush.Color = color;

            return _solidBrush;
        }

        public Vector2 InvertTransformPoint(Matrix3x2 matrix, Vector2 point)
        {
            return Matrix3x2.TransformPoint(Matrix3x2.Invert(matrix), point);
        }

        public bool DeviceAvailable
            => RenderTarget != null && !RenderTarget.IsDisposed;

        /// <summary>
        /// Note: For desktop, width and height can be not specified, will auto fetch from window.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(int width = 0, int height = 0)
        {
            if (!DeviceAvailable)
                return;

            RenderTarget.Target = null;
            SwapChain.ResizeBuffers(0, width, height, DXGI.Format.Unknown, DXGI.SwapChainFlags.None);
            DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
        }

        public Animation.Variable CreateAnimation(double initialValue, double finalValue, double duration)
        {
            var var = new Animation.Variable(AnimationManager, initialValue);
            AnimationManager.ScheduleTransition(var,
                TransitionLibrary.AccelerateDecelerate(duration, finalValue, 0.2, 0.8), DurationSinceStart.TotalSeconds);
            return var;
        }

        public TimeSpan DurationSinceStart { get; private set; }
        
        public void UpdateLogic(TimeSpan durationSinceLastUpdate)
        {
            DurationSinceStart += durationSinceLastUpdate;
            AnimationManager.Update(DurationSinceStart.TotalSeconds);
        }

        public void InitializeDevice(IntPtr windowHandle)
        {
            D3Device = DirectXTools.CreateD3Device();
            {
                RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
                _solidBrush = new Direct2D1.SolidColorBrush(RenderTarget, Color.Black);
                SwapChain = DirectXTools.CreateSwapChainForHwnd(D3Device, windowHandle);
                DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
                Bitmaps.SetRenderTarget(RenderTarget);
                TextLayouts.SetRenderTarget(RenderTarget);
            }
        }

        public void InitializeDeviceGdiCompatible(IntPtr windowHandle, int width, int height)
        {
            D3Device = DirectXTools.CreateD3Device();
            {
                RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
                _solidBrush = new Direct2D1.SolidColorBrush(RenderTarget, Color.Black);
                SwapChain = DirectXTools.CreateSwapChainForHwnd(D3Device, windowHandle);
                //DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
                DirectXTools.CreateDeviceContextCPUBitmap(RenderTarget, width, height);
                Bitmaps.SetRenderTarget(RenderTarget);
                TextLayouts.SetRenderTarget(RenderTarget);
            }
        }

        public void InitializeDevice(int width, int height, object swapChainPanel)
        {
            var nativePanel = ComObject.As<DXGI.ISwapChainPanelNative>(swapChainPanel);
            Debug.Assert(nativePanel != null, $"{nameof(swapChainPanel)} should not be null.");

            D3Device = DirectXTools.CreateD3Device();
            {
                RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
                _solidBrush = new Direct2D1.SolidColorBrush(RenderTarget, Color.Black);
                SwapChain = DirectXTools.CreateSwapChain(width, height, D3Device);
                nativePanel.SwapChain = SwapChain;
                DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
                Bitmaps.SetRenderTarget(RenderTarget);
                TextLayouts.SetRenderTarget(RenderTarget);
            }
        }

        public void InitializeDevice(ComObject coreWindow)
        {
            D3Device = DirectXTools.CreateD3Device();
            {
                RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
                _solidBrush = new Direct2D1.SolidColorBrush(RenderTarget, Color.Black);
                SwapChain = DirectXTools.CreateSwapChainForCoreWindow(D3Device, coreWindow);
                DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
                Bitmaps.SetRenderTarget(RenderTarget);
                TextLayouts.SetRenderTarget(RenderTarget);
            }
        }

        public void ReleaseDeviceResources()
        {
            Bitmaps.Dispose();
            SwapChain.Dispose();
            _solidBrush.Dispose();
            RenderTarget.Dispose();
            D3Device.Dispose();
        }

        public virtual void Dispose()
        {
            ReleaseDeviceResources();

            TransitionLibrary.Dispose();
            AnimationManager.Dispose();

            TextLayouts.Dispose();
            TextFormats.Dispose();
            Direct2DFactory.Dispose();
            DWriteFactory.Dispose();
            WICFactory.Dispose();
        }
    }
}
