using System;
using System.Drawing;

namespace FlysEngine.Desktop;

/// <summary>
/// Provides data for the mouse hover event.
/// </summary>
public class PointEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointEventArgs"/> class.
    /// </summary>
    /// <param name="p">The <see cref="System.Drawing.Point"/> that has </param>
    public PointEventArgs(in Point p)
    {
        X = p.X;
        Y = p.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointEventArgs"/> class.
    /// </summary>
    /// <param name="x">The horizontal position of the mouse.</param>
    /// <param name="y">The vertical position of the mouse.</param>
    public PointEventArgs(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the horizontal position of the mouse.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Gets the vertical position of the mouse.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Converts this <see cref="PointEventArgs"/> object to a <see cref="System.Drawing.Point"/> object.
    /// </summary>
    /// <returns>The <see cref="System.Drawing.Point"/> Object.</returns>
    public Point ToPoint() => new(X, Y);
}
