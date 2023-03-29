using System;

namespace FlysEngine.Desktop
{
    public class PointEventArgs : EventArgs
    {
        public int X { get; init; }
        public int Y { get; init; }

        public PointEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
