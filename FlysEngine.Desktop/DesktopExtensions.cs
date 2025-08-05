using System.Numerics;

namespace FlysEngine.Desktop
{
    public static class DesktopExtensions
    {
        public static Vector2 ToVector2(this in System.Drawing.Point point) => new(point.X, point.Y);
    }
}
