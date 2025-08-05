using System.Numerics;
using S = Duality;

namespace FlysEngine.Sprites
{
    public static class SimulationConverters
    {
        public static float DisplaySimulationScale = 64.0f;

        public static Vector2 ToDisplay(this in S.Vector2 sp) => new()
        {
            X = sp.X * DisplaySimulationScale,
            Y = sp.Y * DisplaySimulationScale,
        };

        public static float ToDisplay(float value) => value * DisplaySimulationScale;

        public static S.Vector2 ToSimulation(this in Vector2 p) => new()
        {
            X = p.X / DisplaySimulationScale,
            Y = p.Y / DisplaySimulationScale,
        };

        public static float ToSimulation(float value) => value / DisplaySimulationScale;
    }
}
