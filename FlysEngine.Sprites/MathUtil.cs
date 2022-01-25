using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FlysEngine.Sprites.Shapes
{
    internal static class Vector2Extensions
    {
        public static PointF ToPoint(in this Vector2 v)
        {
            return new PointF(v.X, v.Y);
        }
    }
}
