using System;
using Vortice.Direct2D1;

namespace FlysEngine.Sprites
{
    public abstract class Behavior : IDisposable
    {
        public Sprite Sprite { get; internal set; }
        public virtual void Update(float dt) { }
        public virtual void Draw(ID2D1DeviceContext ctx) {}
        internal protected virtual void OnSpriteSet(Sprite sprite) {}
        internal protected virtual void OnCreateDeviceSizeResources() {}
        internal protected virtual void OnReleaseDeviceSizeResources() {}
        internal protected virtual void OnReleaseDeviceResources() {}
        internal protected virtual void OnCreateDeviceResources() {}
        public virtual void Dispose() { }
    }
}
