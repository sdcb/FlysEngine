using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Duality;

namespace FlysEngine.Sprites
{
    public static class SimulationConverters
    {
        public static float DisplaySimulationScale = 64.0f;

        public static SharpDX.Vector2 ToDisplay(this in Xna.Vector2 xnaVector2) => new SharpDX.Vector2
        {
            X = xnaVector2.X * DisplaySimulationScale,
            Y = xnaVector2.Y * DisplaySimulationScale,
        };

        public static float ToDisplay(float value) => value * DisplaySimulationScale;

        public static Xna.Vector2 ToSimulation(this in SharpDX.Vector2 vector2) => new Xna.Vector2
        {
            X = vector2.X / DisplaySimulationScale,
            Y = vector2.Y / DisplaySimulationScale,
        };

        public static float ToSimulation(float value) => value / DisplaySimulationScale;
    }
}
