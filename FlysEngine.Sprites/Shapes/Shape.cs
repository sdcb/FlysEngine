using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
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

    public class RectangleShape : Shape
    {
        public Vector2 Size { get; set; }

        public RectangleF Rect => new RectangleF(Offset.X, Offset.Y, Size.X, Size.Y);

        public override void Draw(Direct2D.DeviceContext renderTarget, Direct2D.Brush brush) => renderTarget.FillRectangle(Rect, brush);

        public override bool TestPoint(Vector2 point) => Rect.Contains(point);

        public override EngineShapes.Shape ToEngineShape()
        {
            var offset = Offset - Center;
            var shape = new EngineShapes.PolygonShape(ShapeSettings.DefaultDensity);
            shape.Set(new Vertices(new[]
            {
                offset.ToSimulation(),
                (offset + new Vector2(Size.X, 0)).ToSimulation(),
                (offset + Size).ToSimulation(),
                (offset + new Vector2(0, Size.Y)).ToSimulation(),
            }));
            return shape;
        }
    }

    public class CircleShape : Shape
    {
        public CircleShape(float r) { R = r; }

        public float R { get; private set; }

        public Direct2D.Ellipse Ellipse => new Direct2D.Ellipse(Center + Offset, R, R);

        public CircleShape Clone()
        {
            return new CircleShape(R)
            {
                Offset = Offset,
                Center = Center,
            };
        }

        public override void Draw(Direct2D.DeviceContext renderTarget, Direct2D.Brush brush)
        {
            renderTarget.DrawEllipse(Ellipse, brush, 1.0f);
        }

        public override bool TestPoint(Vector2 point)
        {
            return Vector2.DistanceSquared(Center + Offset, point)
                < R * R;
        }

        public override EngineShapes.Shape ToEngineShape()
        {
            return new EngineShapes.CircleShape(
                    SimulationConverters.ToSimulation(R), ShapeSettings.DefaultDensity);
        }
    }

    public class EdgeShape : Shape
    {
        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public EdgeShape(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public EdgeShape Clone()
        {
            return new EdgeShape(P1, P2)
            {
                Offset = Offset,
                Center = Center,
            };
        }

        public override void Draw(Direct2D.DeviceContext renderTarget, Direct2D.Brush brush)
        {
            renderTarget.DrawLine(P1, P2, brush);
        }

        public override bool TestPoint(Vector2 point) => false;

        public override EngineShapes.Shape ToEngineShape()
        {
            return new EngineShapes.EdgeShape(P1.ToSimulation(), P2.ToSimulation());
        }
    }
}