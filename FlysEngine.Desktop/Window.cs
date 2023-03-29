using System;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Macros;
using Vanara.PInvoke;
using System.Drawing;
using System.Text;

namespace FlysEngine.Desktop
{

    public class Window : IDisposable
    {
        public Window()
        {
            (HWND hwnd, string className) = WindowHelper.CreateDefault(300, 300, "Window", WindowProc);
            Handle = hwnd;
            _className = className;
        }

        public HWND Handle { get; }

        private readonly string _className;
        private bool _isDisposed;

        public Size Size
        {
            get
            {
                GetWindowRect(Handle, out RECT rect);
                return new Size(rect.Width, rect.Height);
            }
            set
            {
                GetWindowRect(Handle, out RECT rect);
                SetWindowPos(Handle, HWND.NULL, rect.left, rect.top, value.Width, value.Height, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        public Size ClientSize
        {
            get
            {
                GetWindowRect(Handle, out RECT rect);
                return new Size(rect.Width, rect.Height);
            }
            set
            {
                GetWindowRect(Handle, out RECT rect);
                int newWidth = value.Width + (rect.right - rect.left) - GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME) * 2;  // 计算新的窗口宽度
                int newHeight = value.Height + (rect.bottom - rect.top) - GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME) * 2 - GetSystemMetrics(SystemMetric.SM_CYCAPTION);  // 计算新的窗口高度
                SetWindowPos(Handle, HWND.NULL, rect.left, rect.top, newWidth, newHeight, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        public ShowWindowCommand WindowState
        {
            get
            {
                WINDOWPLACEMENT wp = new()
                {
                    length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                };
                GetWindowPlacement(Handle, ref wp);
                return wp.showCmd;
            }
        }

        public Point Location
        {
            get
            {
                GetWindowRect(Handle, out RECT rect);
                return new Point(rect.X, rect.Y);
            }
            set
            {
                GetWindowRect(Handle, out RECT rect);
                SetWindowPos(Handle, HWND.NULL, value.X, value.Y, rect.Width, rect.Height, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        public string Text
        {
            get
            {
                int length = GetWindowTextLength(Handle);
                StringBuilder title = new (length + 1);
                _ = GetWindowText(Handle, title, title.Length);
                return title.ToString();
            }
            set
            {
                SetWindowText(Handle, value);
            }
        }

        public bool Focused => GetForegroundWindow() == Handle;

        public bool Visible
        {
            get
            {
                return IsWindowVisible(Handle);
            }
            set
            {
                bool visible = IsWindowVisible(Handle);
                if (value && !visible)
                {
                    ShowWindow(Handle, ShowWindowCommand.SW_SHOW);
                }
                else if (!value && visible)
                {
                    ShowWindow(Handle, ShowWindowCommand.SW_HIDE);
                }
            }
        }

        public void Show() => Visible = true;

        public void Hide() => Visible = false;

        public Point ScreenToClient(Point point)
        {
            POINT p = new(point.X, point.Y);
            User32.ScreenToClient(Handle, ref p);
            return new Point(p.X, p.Y);
        }

        bool _leftButtonDown = false;
        POINT _leftButtonDownPosition;

        protected virtual IntPtr WindowProc(HWND hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr processed = WndProc(msg, wParam, lParam);
            if (processed != IntPtr.Zero) return processed;

            switch (msg)
            {
                case (uint)WindowMessage.WM_DESTROY:
                    PostQuitMessage(0);
                    return IntPtr.Zero;
                case (uint)WindowMessage.WM_SIZE:
                    {
                        if (Handle == IntPtr.Zero) break;
                        const int SIZE_MINIMIZED = 1;
                        bool isMinimized = (int)wParam == SIZE_MINIMIZED;
                        int newWidth = LOWORD(lParam);
                        int newHeight = HIWORD(lParam);
                        ResizeEventArgs args = new(isMinimized, newWidth, newHeight);
                        OnResize(args);
                        Resize?.Invoke(this, args);
                    }
                    break;
                case (uint)WindowMessage.WM_MOVE:
                    {
                        if (Handle == IntPtr.Zero) break;
                        int x = GET_X_LPARAM(lParam);
                        int y = GET_Y_LPARAM(lParam);
                        PointEventArgs args = new(x, y);
                        OnMove(args);
                        Move?.Invoke(this, args);                        
                    }
                    break;
                case (int)WindowMessage.WM_MOUSEMOVE:
                    {
                        int x = GET_X_LPARAM(lParam);
                        int y = GET_X_LPARAM(lParam);
                        PointEventArgs args = new PointEventArgs(x, y);
                        OnMouseMove(args);
                        MouseMove?.Invoke(this, args);                        
                    }
                    break;
                case (int)WindowMessage.WM_LBUTTONDOWN:
                    {
                        _leftButtonDown = true;
                        _leftButtonDownPosition = new (GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                        PointEventArgs args = new(_leftButtonDownPosition.x, _leftButtonDownPosition.y);
                        OnMouseLeftButtonDown(args);
                        MouseLeftButtonDown?.Invoke(this, args);                        
                    }
                    break;
                case (int)WindowMessage.WM_LBUTTONUP:
                    {
                        POINT upPosition = new(
                                GET_X_LPARAM(lParam),
                                GET_Y_LPARAM(lParam));
                        OnMouseLeftButtonUp(new PointEventArgs(_leftButtonDownPosition.x, _leftButtonDownPosition.y));
                        MouseLeftButtonUp?.Invoke(this, new PointEventArgs(_leftButtonDownPosition.x, _leftButtonDownPosition.y));                        

                        if (_leftButtonDown)
                        {
                            // 如果鼠标在按下和释放期间保持在相同的区域内，则视为点击事件
                            if (Math.Abs(upPosition.x - _leftButtonDownPosition.x) < 5 &&
                                Math.Abs(upPosition.y - _leftButtonDownPosition.y) < 5)
                            {
                                // 处理鼠标左键点击事件
                                OnClick(new PointEventArgs(_leftButtonDownPosition.x, _leftButtonDownPosition.y));
                                Click?.Invoke(this, new PointEventArgs(_leftButtonDownPosition.x, _leftButtonDownPosition.y));
                            }
                        }
                        _leftButtonDown = false;
                    }
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public event EventHandler<EventArgs> Load;
        public event EventHandler<ResizeEventArgs> Resize;
        public event EventHandler<PointEventArgs> Move;
        public event EventHandler<PointEventArgs> MouseMove;
        public event EventHandler<PointEventArgs> Click;
        public event EventHandler<PointEventArgs> MouseLeftButtonDown;
        public event EventHandler<PointEventArgs> MouseLeftButtonUp;

        protected virtual void OnLoad(EventArgs e) { }
        protected virtual void OnResize(ResizeEventArgs e) { }
        protected virtual void OnMove(PointEventArgs e) { }
        protected virtual void OnMouseMove(PointEventArgs e) { }
        protected virtual void OnClick(PointEventArgs e) { }
        protected virtual void OnMouseLeftButtonDown(PointEventArgs e) { }
        protected virtual void OnMouseLeftButtonUp(PointEventArgs e) { }

        /// <returns>is handled</returns>
        protected virtual IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam) { return IntPtr.Zero; }

        internal void EnterMessageLoop()
        {
            Show();
            OnLoad(EventArgs.Empty);
            Load?.Invoke(this, EventArgs.Empty);            
            UpdateWindow(Handle);
        }

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                UnregisterClass(_className, HINSTANCE.NULL);
                _isDisposed = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~Window()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }
}
