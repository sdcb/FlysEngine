using System;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Macros;
using Vanara.PInvoke;
using System.Drawing;
using System.Text;

namespace FlysEngine.Desktop;


/// <summary>
/// A wrapper class for working with Windows using PInvoke.
/// </summary>
public class Window : IDisposable
{
    /// <summary>
    /// Constructs a new instance of <see cref="Window"/>.
    /// </summary>
    public Window()
    {
        (HWND hwnd, string className) = WindowHelper.CreateDefault(300, 300, "Window", WindowProc);
        Handle = hwnd;
        _className = className;
    }

    /// <summary>
    /// Gets the handle of this window.
    /// </summary>
    public HWND Handle { get; }

    private readonly string _className;
    private bool _isDisposed;

    /// <summary>
    /// Gets or sets the size of the window.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the client area size of the window.
    /// </summary>
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

    /// <summary>
    /// Gets the current state of the window.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the location of the window.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the text for the window.
    /// </summary>
    public string Text
    {
        get
        {
            int length = GetWindowTextLength(Handle);
            StringBuilder title = new(length + 1);
            _ = GetWindowText(Handle, title, title.Length);
            return title.ToString();
        }
        set
        {
            SetWindowText(Handle, value);
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the window has focus.
    /// </summary>
    public bool Focused => GetForegroundWindow() == Handle;

    /// <summary>
    /// Gets or sets a value that indicates whether the window is visible.
    /// </summary>
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

    /// <summary>
    /// Shows the window.
    /// </summary>
    public void Show() => Visible = true;

    /// <summary>
    /// Hides the window.
    /// </summary>
    public void Hide() => Visible = false;

    /// <summary>
    /// Converts a point in screen coordinates to client coordinates.
    /// </summary>
    public Point ScreenToClient(Point point)
    {
        POINT p = new(point.X, point.Y);
        User32.ScreenToClient(Handle, ref p);
        return new Point(p.X, p.Y);
    }

    bool _leftButtonDown = false;
    Point _leftButtonDownPosition;

    private IntPtr WindowProc(HWND hWnd, uint msg, IntPtr wParam, IntPtr lParam)
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
                    PointEventArgs args = new(x, y);
                    OnMouseMove(args);
                    MouseMove?.Invoke(this, args);
                }
                break;
            case (int)WindowMessage.WM_LBUTTONDOWN:
                {
                    _leftButtonDown = true;
                    _leftButtonDownPosition = new(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam));
                    PointEventArgs args = new(_leftButtonDownPosition);
                    OnMouseLeftButtonDown(args);
                    MouseLeftButtonDown?.Invoke(this, args);
                }
                break;
            case (int)WindowMessage.WM_LBUTTONUP:
                {
                    Point upPosition = new(
                            GET_X_LPARAM(lParam),
                            GET_Y_LPARAM(lParam));
                    OnMouseLeftButtonUp(new PointEventArgs(_leftButtonDownPosition));
                    MouseLeftButtonUp?.Invoke(this, new PointEventArgs(_leftButtonDownPosition));

                    if (_leftButtonDown)
                    {
                        // 如果鼠标在按下和释放期间保持在相同的区域内，则视为点击事件
                        if (Math.Abs(upPosition.X - _leftButtonDownPosition.X) < 5 &&
                            Math.Abs(upPosition.Y - _leftButtonDownPosition.Y) < 5)
                        {
                            // 处理鼠标左键点击事件
                            OnClick(new PointEventArgs(_leftButtonDownPosition));
                            Click?.Invoke(this, new PointEventArgs(_leftButtonDownPosition));
                        }
                    }
                    _leftButtonDown = false;
                }
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    /// <summary>
    /// Occurs when the control is loaded.
    /// </summary>
    public event EventHandler<EventArgs> Load;

    /// <summary>
    /// Occurs when the control is resized.
    /// </summary>
    public event EventHandler<ResizeEventArgs> Resize;

    /// <summary>
    /// Occurs when the control is moved.
    /// </summary>
    public event EventHandler<PointEventArgs> Move;

    /// <summary>
    /// Occurs when the mouse pointer moves over the control.
    /// </summary>
    public event EventHandler<PointEventArgs> MouseMove;

    /// <summary>
    /// Occurs when the control is clicked by the mouse.
    /// </summary>
    public event EventHandler<PointEventArgs> Click;

    /// <summary>
    /// Occurs when the left mouse button is pressed while the mouse pointer is over the control.
    /// </summary>
    public event EventHandler<PointEventArgs> MouseLeftButtonDown;

    /// <summary>
    /// Occurs when the left mouse button is released while the mouse pointer is over the control.
    /// </summary>
    public event EventHandler<PointEventArgs> MouseLeftButtonUp;

    ///<summary>
    ///Raises the <see cref="E:Load"/> event.
    ///</summary>
    ///<param name="e">An <see cref="EventArgs"/> containing the event data.</param>
    protected virtual void OnLoad(EventArgs e) { }
    /// <summary>
    /// Called when the control is being resized.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnResize(ResizeEventArgs e) { }

    /// <summary>
    /// Called when the control is being moved.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMove(PointEventArgs e) { }

    /// <summary>
    /// Called when the mouse is moved over the control.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMouseMove(PointEventArgs e) { }

    /// <summary>
    /// Called when the control is clicked.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnClick(PointEventArgs e) { }

    /// <summary>
    /// Called when the left mouse button is pressed over the control.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMouseLeftButtonDown(PointEventArgs e) { }

    /// <summary>
    /// Called when the left mouse button is released over the control.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnMouseLeftButtonUp(PointEventArgs e) { }


    /// <summary>
    /// Overrides the WndProc function in order to handle Windows messages.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="wParam">The wParam parameter of the message.</param>
    /// <param name="lParam">The lParam parameter of the message.</param>
    /// <returns>The result of the message processing. </returns>
    protected virtual IntPtr WndProc(uint message, IntPtr wParam, IntPtr lParam)
    {
        return IntPtr.Zero;
    }

    internal void EnterMessageLoop()
    {
        Show();
        OnLoad(EventArgs.Empty);
        Load?.Invoke(this, EventArgs.Empty);
        UpdateWindow(Handle);
    }

    #region Dispose Pattern

    /// <summary>
    /// Disposes of the resources (managed and unmanaged) used by the WindowClass.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Release managed resources here.
            }

            // Release unmanaged resources and set large fields to null.
            UnregisterClass(_className, HINSTANCE.NULL);
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Finalizer of the <see cref="Window"/> class.
    /// </summary>
    ~Window()
    {
        // Do not modify this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
    #endregion
}
