using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace FlysEngine.Desktop
{
    public static class RenderLoop
    {
        public static void Run(HWND hwnd, RenderCallback renderCallback)
        {
            while (true)
            {
                // Process all waiting messages
                while (PeekMessage(out MSG message, HWND.NULL, 0, 0, PM.PM_REMOVE))
                {
                    if (message.message == (uint)WindowMessage.WM_QUIT)
                    {
                        return;
                    }

                    TranslateMessage(in message);
                    DispatchMessage(in message);
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
}
