using FarseerPhysics.Common;
using FlysEngine.Sprites.Shapes.Json;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using EngineShapes = FarseerPhysics.Collision.Shapes;

namespace FlysEngine.Sprites.Shapes
{
    public class RectangleShape : Shape
    {
        public Size Size { get; set; }

        public Rect Rect => new Rect(Offset, Size);

        public RectangleShape() { }

        public RectangleShape(JsonShape jsonShape, Vector2 center) : base(jsonShape, center)
        {
            Size = new Size(jsonShape.Size[0], jsonShape.Size[1]);
        }

        public override void Draw(ID2D1DeviceContext renderTarget, ID2D1SolidColorBrush brush) => renderTarget.DrawRectangle(Rect, brush);

        public override bool TestPoint(Vector2 point) => Rect.Contains(point.X, point.Y);

        public override EngineShapes.Shape ToEngineShape()
        {
            var offset = Offset - Center;
            var shape = new EngineShapes.PolygonShape(ShapeSettings.DefaultDensity);
            shape.Set(new Vertices(new[]
            {
                offset.ToSimulation(),
                (offset + new Vector2(Size.Width, 0)).ToSimulation(),
                (offset + new Vector2(Size.Width, Size.Height)).ToSimulation(),
                (offset + new Vector2(0, Size.Height)).ToSimulation(),
            }));
            return shape;
        }
    }
}