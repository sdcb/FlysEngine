using FarseerPhysics.Dynamics;
using FlysEngine.Desktop;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlysEngine.Sprites
{
    public class SpriteWindow : RenderWindow
    {
        public World World { get; } = new World(Vector2.Zero.ToSimulation());

        public IEnumerable<Sprite> Sprites { get; }
    }
}
