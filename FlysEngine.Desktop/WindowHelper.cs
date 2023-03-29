using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;
using Vanara.PInvoke;

namespace FlysEngine.Desktop
{
    public class WindowHelper
    {
        // 导入SetProcessDpiAwareness函数
        [DllImport("Shcore.dll")]
        public static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);

        static WindowHelper()
        {
            SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
        }

        public static (HWND, string className) CreateDefault(int width, int height, string title, WindowProc wndProc)
        {
            string className = Guid.NewGuid().ToString();

            // 注册窗口类
            WNDCLASSEX wndClass = new()
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                style = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW,
                lpfnWndProc = wndProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = HINSTANCE.NULL,
                hIcon = LoadIcon(HINSTANCE.NULL, IDI_APPLICATION),
                hCursor = LoadCursor(HINSTANCE.NULL, IDC_ARROW),
                hbrBackground = SystemColorIndex.COLOR_WINDOW + 1,
                lpszMenuName = null,
                lpszClassName = className,
            };

            if (RegisterClassEx(wndClass) == 0)
            {
                throw new Win32Exception("RegisterClassEx failed");
            }

            // 创建窗口
            SafeHWND hwnd = CreateWindowEx(
                WindowStylesEx.WS_EX_OVERLAPPEDWINDOW,
                className,
                title,
                WindowStyles.WS_OVERLAPPEDWINDOW,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                width,
                height,
                HWND.NULL,
                HMENU.NULL,
                HINSTANCE.NULL,
                IntPtr.Zero);

            if (hwnd.IsNull)
            {
                throw new Exception("CreateWindowEx failed");
            }

            // 显示窗口
            ShowWindow(hwnd, ShowWindowCommand.SW_SHOW);
            UpdateWindow(hwnd);
            return (hwnd, className);
        }
    }
}
