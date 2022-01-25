using SharpDX;
using System;
using System.Numerics;

namespace FlysEngine.Sprites.Shapes.Json
{
    public class JsonShape
    {
        public string Type { get; set; }

        public float R { get; set; }

        public float[] Size { get; set; }

        public float[] Offset { get; set; } = new float[2];

        public float[][] Points { get; set; }

        public Shape Create(Vector2 center)
        {
            if (Type == "circle")
            {
                return new CircleShape(this, center);
            }
            else if (Type == "rectangle")
            {
                return new RectangleShape(this, center);
            }
            else if (Type == "polygon")
            {
                return new PolygonShape(this, center);
            }
            else if (Type == "line")
            {
                return new EdgeShape(this, center);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}