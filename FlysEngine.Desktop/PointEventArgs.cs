using System;
using System.Drawing;

namespace FlysEngine.Desktop;

public class PointEventArgs : EventArgs
{
    public int X { get; init; }
    public int Y { get; init; }

    public PointEventArgs(in Point p)
    {
        X = p.X;
        Y = p.Y;
    }

    public PointEventArgs(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point ToPoint() => new(X, Y);
}
