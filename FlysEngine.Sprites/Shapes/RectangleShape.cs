using FarseerPhysics.Common;
using FlysEngine.Sprites.Shapes.Json;
using SharpDX;
using Direct2D = SharpDX.Direct2D1;
using EngineShapes = FarseerPhysics.Collision.Shapes;

namespace FlysEngine.Sprites.Shapes
{
    public class RectangleShape : Shape
    {
        public Vector2 Size { get; set; }

        public RectangleF Rect => new RectangleF(Offset.X, Offset.Y, Size.X, Size.Y);

        public RectangleShape() { }

        public RectangleShape(JsonShape jsonShape, Vector2 center) : base(jsonShape, center)
        {
            Size = new Vector2(jsonShape.Size[0], jsonShape.Size[1]);
        }

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
}