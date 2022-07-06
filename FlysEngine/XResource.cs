using System;
using FlysEngine.Managers;
using FlysEngine.Tools;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.WIC;
using Vortice.DXGI;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using System.Numerics;
using Vortice.UIAnimation;

namespace FlysEngine
{
    public class XResource : IDisposable
    {
        public readonly ID2D1Factory1 Direct2DFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>(Vortice.Direct2D1.FactoryType.SingleThreaded);
        public readonly IDWriteFactory DWriteFactory = DWrite.DWriteCreateFactory<IDWriteFactory>(Vortice.DirectWrite.FactoryType.Shared);
        public readonly TextFormatManager TextFormats;
        public readonly IWICImagingFactory WICFactory = new IWICImagingFactory();
        public readonly BitmapManager Bitmaps;
        public readonly TextLayoutManager TextLayouts;
        private ID2D1SolidColorBrush _solidBrush;

        public IDXGISwapChain1 SwapChain;
        public ID2D1DeviceContext RenderTarget;
        public ID3D11Device D3Device;
        public IUIAnimationManager2 Animation = new();
        public IUIAnimationTransitionLibrary2 TransitionLibrary = new();

        public XResource()
        {
            TextFormats = new TextFormatManager(DWriteFactory);
            Bitmaps = new BitmapManager(WICFactory);
            TextLayouts = new TextLayoutManager(DWriteFactory, TextFormats);
        }

        public IUIAnimationVariable2 CreateAnimation(double initialValue, double finalValue, double durationInSeconds)
        {
            IUIAnimationVariable2 v = Animation.CreateAnimationVariable(initialValue);
            using IUIAnimationTransition2 transition = TransitionLibrary.CreateAccelerateDecelerateTransition(durationInSeconds, finalValue, 0.3, 0.3);
            Animation.ScheduleTransition(v, transition, DurationSinceStart.TotalSeconds);
            return v;
        }

        public ID2D1SolidColorBrush GetColor(Color4 color)
        {
            if (_solidBrush == null)
                throw new InvalidOperationException("Need to call Initialize before call this function.");

            _solidBrush.Color = color;

            return _solidBrush;
        }

        public Vector2 InvertTransformPoint(Matrix3x2 matrix, Vector2 point)
        {
            if (Matrix3x2.Invert(matrix, out Matrix3x2 inverted))
            {
                return new Vector2(
                    x: point.X * inverted.M11 + point.Y * inverted.M21 + inverted.M31,
                    y: point.X * inverted.M12 + point.Y * inverted.M22 + inverted.M32
                    );
            }

            return point;
        }

        public bool DeviceAvailable
            => RenderTarget != null && RenderTarget.NativePointer != IntPtr.Zero;

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
            SwapChain.ResizeBuffers(0, width, height, Format.Unknown, SwapChainFlags.None);
            DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
        }

        public TimeSpan DurationSinceStart { get; private set; }

        public void UpdateLogic(TimeSpan durationSinceLastUpdate)
        {
            DurationSinceStart += durationSinceLastUpdate;
            Animation.Update(DurationSinceStart.TotalSeconds);
        }

        public void InitializeDevice(IntPtr windowHandle)
        {
            D3Device = DirectXTools.CreateD3Device();
            {
                RenderTarget = DirectXTools.CreateRenderTarget(Direct2DFactory, D3Device);
                _solidBrush = RenderTarget.CreateSolidColorBrush(Colors.Black);
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
                _solidBrush = RenderTarget.CreateSolidColorBrush(Colors.Black);
                SwapChain = DirectXTools.CreateSwapChainForHwnd(D3Device, windowHandle);
                //DirectXTools.CreateDeviceSwapChainBitmap(SwapChain, RenderTarget);
                DirectXTools.CreateDeviceContextCPUBitmap(RenderTarget, width, height);
                Bitmaps.SetRenderTarget(RenderTarget);
                TextLayouts.SetRenderTarget(RenderTarget);
            }
        }

        public void ReleaseDeviceResources()
        {
            Bitmaps?.Dispose();
            SwapChain?.Dispose();
            _solidBrush?.Dispose();
            RenderTarget?.Dispose();
            D3Device?.Dispose();
        }

        public virtual void Dispose()
        {
            ReleaseDeviceResources();

            TextLayouts?.Dispose();
            TextFormats?.Dispose();
            Direct2DFactory?.Dispose();
            DWriteFactory?.Dispose();
            WICFactory?.Dispose();
            Animation.Dispose();
            TransitionLibrary.Dispose();
        }
    }
}
