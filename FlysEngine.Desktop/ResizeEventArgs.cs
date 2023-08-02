using System;

namespace FlysEngine.Desktop;

public class ResizeEventArgs : EventArgs
{
    public bool IsMinimized { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public ResizeEventArgs(bool isMinimized, int newWidth, int newHeight)
    {
        IsMinimized = isMinimized;
        Width = newWidth;
        Height = newHeight;
    }
}
