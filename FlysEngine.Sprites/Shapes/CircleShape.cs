using FlysEngine.Sprites.Shapes.Json;
using SharpDX;
using Direct2D = SharpDX.Direct2D1;
using EngineShapes = FarseerPhysics.Collision.Shapes;

namespace FlysEngine.Sprites.Shapes
{
    public class CircleShape : Shape
    {
        public CircleShape(float r) { R = r; }

        public float R { get; private set; }

        public Direct2D.Ellipse Ellipse => new Direct2D.Ellipse(Center + Offset, R, R);

        public CircleShape(JsonShape jsonShape, Vector2 center) : base(jsonShape, center)
        {
            R = jsonShape.R;
        }

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
}