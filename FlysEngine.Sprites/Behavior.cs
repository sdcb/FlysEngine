using FlysEngine.Managers;
using Direct2D = SharpDX.Direct2D1;

namespace FlysEngine.Sprites
{
    public abstract class Behavior
    {
        public Sprite Sprite { get; }
        public Behavior(Sprite sprite) { Sprite = sprite; }
        public virtual void Update(RenderTimer timer) { }
        public virtual void Draw(Direct2D.DeviceContext ctx) {}

        internal protected virtual void OnCreateDeviceSizeResources() {}
        internal protected virtual void OnReleaseDeviceSizeResources() {}
        internal protected virtual void OnReleaseDeviceResources() {}
        internal protected virtual void OnCreateDeviceResources() {}
    }
}
