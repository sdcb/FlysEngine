using System;

namespace FlysEngine.Desktop;

/// <summary>
/// Provides data for the window resize event.
/// </summary>
public class ResizeEventArgs : EventArgs
{
    /// <summary>
    /// Gets a value indicating whether the window is minimized.
    /// </summary>
    public bool IsMinimized { get; init; }

    /// <summary>
    /// Gets the new width of the window.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the new height of the window.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Initializes a new instance of the ResizeEventArgs class with the specified values.
    /// </summary>
    /// <param name="isMinimized">A value indicating whether the window is minimized.</param>
    /// <param name="newWidth">The new width of the window.</param>
    /// <param name="newHeight">The new height of the window.</param>
    public ResizeEventArgs(bool isMinimized, int newWidth, int newHeight)
    {
        IsMinimized = isMinimized;
        Width = newWidth;
        Height = newHeight;
    }
}
