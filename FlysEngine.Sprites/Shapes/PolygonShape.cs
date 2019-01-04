using FarseerPhysics.Common;
using FlysEngine.Sprites.Shapes.Json;
using SharpDX;
using System.Linq;
using Direct2D = SharpDX.Direct2D1;
using EngineShapes = FarseerPhysics.Collision.Shapes;
using Xna = Duality;

namespace FlysEngine.Sprites.Shapes
{
    public class PolygonShape : Shape
    {
        public Vector2[] Points { get; set; }

        public PolygonShape() { }

        public PolygonShape(JsonShape jsonShape, Vector2 center) : base(jsonShape, center)
        {
            Points = jsonShape.Points.Select(x => new Vector2(x[0], x[1])).ToArray();
        }

        public override void Draw(Direct2D.DeviceContext renderTarget, Direct2D.Brush brush)
        {
            var lines = new[]
            {
                new []{ Points[0], Points[1] },
                new []{ Points[1], Points[2] },
                new []{ Points[2], Points[3] },
                new []{ Points[3], Points[0] },
            };
            foreach (var line in lines)
            {
                renderTarget.DrawLine(line[0], line[1], brush, 2.0f);
            }
        }

        public override bool TestPoint(Vector2 point)
        {
            var test = new EngineShapes.PolygonShape(1.0f);
            test.Set(new Vertices(
                Points.Select(x => new Xna.Vector2(x.X, x.Y)).ToList()));

            var transform = new Transform();
            transform.SetIdentity();
            var mouseXna = new Xna.Vector2(point.X, point.Y);

            return test.TestPoint(ref transform, ref mouseXna);
        }

        public override EngineShapes.Shape ToEngineShape()
        {
            var engineShape = new EngineShapes.PolygonShape(ShapeSettings.DefaultDensity);
            engineShape.Set(new Vertices(
                Points.Select(p => (p - Center).ToSimulation()).ToList()));
            return engineShape;
        }
    }
}