namespace FlysEngine.Desktop
{
    public static class DesktopExtensions
    {
        public static SharpDX.Vector2 ToVector2(this in System.Drawing.Point point) => new SharpDX.Vector2(point.X, point.Y);
    }
}
