using FarseerPhysics.Dynamics;
using FlysEngine.Sprites.Shapes.Json;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using Direct2D = SharpDX.Direct2D1;
using EngineShapes = FarseerPhysics.Collision.Shapes;

namespace FlysEngine.Sprites.Shapes
{
    public abstract class Shape
    {
        public Vector2 Center { get; set; }

        public Vector2 Offset { get; set; }

        public abstract bool TestPoint(Vector2 point);

        public abstract EngineShapes.Shape ToEngineShape();

        public Shape()
        {
        }

        public Shape(JsonShape jsonShape, Vector2 center)
        {
            Center = center;
            Offset = new Vector2(jsonShape.Offset[0], jsonShape.Offset[1]);
        }

        public abstract void Draw(
            Direct2D.DeviceContext renderTarget,
            Direct2D.Brush brush);

        public static bool TestPoint(IEnumerable<Shape> shapes, Vector2 point)
        {
            return shapes.Any(shape => shape.TestPoint(point));
        }

        public static void CreateFixtures(
            IEnumerable<Shape> shapes,
            Body body)
        {
            foreach (var shape in shapes)
            {
                var engineShape = shape.ToEngineShape();
                var fixture = body.CreateFixture(engineShape);
                fixture.Restitution = ShapeSettings.DeafultRestitution;
                fixture.Friction = ShapeSettings.DefaultFriction;
            }
        }
    }
}