using FlysEngine.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Direct2D = SharpDX.Direct2D1;

namespace FlysEngine.Sprites
{
    public abstract class Behavior
    {
        public Sprite Sprite { get; }
        public Behavior(Sprite sprite) { Sprite = sprite; }
        public virtual void Update(RenderTimer timer) { }
        public virtual void Draw(Direct2D.DeviceContext ctx) { }
    }
}
