using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace FlysEngine.Desktop;

/// <summary>
/// Provides a render loop for a window.
/// </summary>
public static class RenderLoop
{
    /// <summary>
    /// Runs the render loop for the specified window.
    /// </summary>
    /// <param name="window">The window to render.</param>
    /// <param name="renderCallback">The delegate to render the window.</param>
    public static void Run(Window window, RenderCallback renderCallback)
    {
        window.EnterMessageLoop();
        Run(renderCallback);
    }

    /// <summary>
    /// Runs the render loop for the specified window handle.
    /// </summary>
    /// <param name="renderCallback">The delegate to render the window.</param>
    public static void Run(RenderCallback renderCallback)
    {
        while (true)
        {
            // Process all waiting messages
            while (PeekMessage(out MSG msg, HWND.NULL, 0u, 0u, PM.PM_REMOVE))
            {
                if (msg.message == (uint)WindowMessage.WM_QUIT)
                {
                    return;
                }

                TranslateMessage(in msg);
                DispatchMessage(in msg);
            }

            // Call the render callback
            renderCallback();
        }
    }
}

/// <summary>
/// Delegate for the rendering loop.
/// </summary>
public delegate void RenderCallback();
