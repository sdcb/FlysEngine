using System.Numerics;

namespace FlysEngine.Desktop;

/// <summary>
/// Provides extension methods for working with desktop applications.
/// </summary>
public static class DesktopExtensions
{
    /// <summary>
    /// Converts the specified <see cref="System.Drawing.Point"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="point">A <see cref="System.Drawing.Point"/> representing the point to convert.</param>
    /// <returns>A <see cref="Vector2"/> representing the converted point.</returns>
    public static Vector2 ToVector2(this in System.Drawing.Point point) => new(point.X, point.Y);
}
