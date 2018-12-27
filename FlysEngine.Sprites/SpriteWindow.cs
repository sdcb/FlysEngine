using FarseerPhysics.Dynamics;
using FlysEngine.Desktop;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysEngine.Sprites
{
    public class SpriteWindow : RenderWindow
    {
        public World World { get; } = new World(Vector2.Zero.ToSimulation());

        public Dictionary<Guid, Sprite> Sprites { get; } = new Dictionary<Guid, Sprite>();

        public Color ClearColor { get; set; } = Color.CornflowerBlue;

        public Matrix3x2 GlobalTransform { get; set; } = Matrix3x2.Identity;

        public bool ShowFPS { get; set; }

        public void AddSprites(params Sprite[] sprites)
        {
            if (sprites == null) throw new ArgumentNullException(nameof(sprites));

            foreach (var sprite in sprites)
            {
                Sprites.Add(sprite.Id, sprite);
            }
        }

        protected override void OnUpdateLogic(float lastFrameTimeInSecond)
        {
            base.OnUpdateLogic(lastFrameTimeInSecond);

            foreach (var sprite in Sprites.Values) sprite.OnUpdate(RenderTimer);
            World.Step(lastFrameTimeInSecond);

            var toDestroyIds = Sprites.Values.Where(x => x.ReadyToRemove).Select(x => x.Id).ToList();
            foreach (var id in toDestroyIds)
            {
                Sprites[id].Dispose();
                Sprites.Remove(id);
            }
            World.ProcessChanges();
        }

        protected override void OnDraw(DeviceContext renderTarget)
        {
            base.OnDraw(renderTarget);

            renderTarget.Clear(ClearColor);
            renderTarget.Transform = GlobalTransform;
            foreach (var sprite in Sprites.Values) sprite.Draw(renderTarget);

            renderTarget.Transform = Matrix3x2.Identity;
            if (ShowFPS)
            {
                renderTarget.DrawText($"FPS: {RenderTimer.FramesPerSecond:F1}",
                    XResource.TextFormats[12.0f],
                    new RectangleF(0, 0, renderTarget.Size.Width, renderTarget.Size.Height),
                    XResource.GetColor(Color.DimGray));
            }
        }

        protected override void OnCreateDeviceResources()
        {
            base.OnCreateDeviceResources();
            foreach (var sprite in Sprites.Values) sprite.OnCreateDeviceResources();
        }

        protected override void OnCreateDeviceSizeResources()
        {
            base.OnCreateDeviceSizeResources();
            foreach (var sprite in Sprites.Values) sprite.OnCreateDeviceSizeResources();
        }

        protected override void OnReleaseDeviceResources()
        {
            base.OnReleaseDeviceResources();
            foreach (var sprite in Sprites.Values) sprite.OnReleaseDeviceResources();
        }

        protected override void OnReleaseDeviceSizeResources()
        {
            base.OnReleaseDeviceSizeResources();
            foreach (var sprite in Sprites.Values) sprite.OnReleaseDeviceSizeResources();
        }
    }
}
