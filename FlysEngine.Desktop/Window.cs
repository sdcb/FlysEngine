using System;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;
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
                    if (Handle == IntPtr.Zero) break;
                    const int SIZE_MINIMIZED = 1;
                    bool isMinimized = (int)wParam == SIZE_MINIMIZED;
                    int newWidth = Macros.LOWORD(lParam);
                    int newHeight = Macros.HIWORD(lParam);
                    Resize?.Invoke(this, new ResizeEventArgs(isMinimized, newWidth, newHeight));
                    OnResize(isMinimized, newWidth, newHeight);
                    break;
                case (uint)WindowMessage.WM_MOVE:
                    {
                        if (Handle == IntPtr.Zero) break;
                        int x = Macros.GET_X_LPARAM(lParam);
                        int y = Macros.GET_Y_LPARAM(lParam);
                        Move?.Invoke(this, new PointEventArgs(x, y));
                        OnMove(x, y);
                    }
                    break;
                case (int)WindowMessage.WM_MOUSEMOVE:
                    {
                        int x = Macros.GET_X_LPARAM(lParam);
                        int y = Macros.GET_X_LPARAM(lParam);
                        MouseMove?.Invoke(this, new PointEventArgs(x, y));
                        OnMouseMove(x, y);
                    }
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public event EventHandler<ResizeEventArgs> Resize;
        public event EventHandler<PointEventArgs> Move;
        public event EventHandler<PointEventArgs> MouseMove;
        protected virtual void OnResize(bool isMinimized, int newWidth, int newHeight) { }
        protected virtual void OnMove(int x, int y) { }
        protected virtual void OnMouseMove(int x, int y) { }

        /// <returns>is handled</returns>
        protected virtual IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam) { return IntPtr.Zero; }

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
