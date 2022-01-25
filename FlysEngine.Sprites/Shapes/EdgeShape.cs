using FlysEngine.Sprites.Shapes.Json;
using System.Numerics;
using Vortice.Direct2D1;
using EngineShapes = FarseerPhysics.Collision.Shapes;

namespace FlysEngine.Sprites.Shapes
{
    public class EdgeShape : Shape
    {
        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public EdgeShape() { }

        public EdgeShape(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public EdgeShape(JsonShape jsonShape, Vector2 center) : base(jsonShape, center)
        {
            var p1 = jsonShape.Points[0];
            var p2 = jsonShape.Points[1];
            P1 = new Vector2(p1[0], p1[1]);
            P2 = new Vector2(p2[0], p2[1]);
        }

        public EdgeShape Clone()
        {
            return new EdgeShape(P1, P2)
            {
                Offset = Offset,
                Center = Center,
            };
        }

        public override void Draw(ID2D1DeviceContext renderTarget, ID2D1SolidColorBrush brush)
        {
            renderTarget.DrawLine(P1.ToPoint(), P2.ToPoint(), brush);
        }

        public override bool TestPoint(Vector2 point) => false;

        public override EngineShapes.Shape ToEngineShape()
        {
            return new EngineShapes.EdgeShape(P1.ToSimulation(), P2.ToSimulation());
        }
    }
}