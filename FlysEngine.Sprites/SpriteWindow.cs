using FarseerPhysics.Dynamics;
using FlysEngine.Desktop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Vanara.PInvoke;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace FlysEngine.Sprites
{
    public class SpriteWindow : RenderWindow
    {
        public World World { get; } = new World(Vector2.Zero.ToSimulation());

        public Dictionary<int, Sprite> Sprites { get; } = new Dictionary<int, Sprite>();

        public Color4 ClearColor { get; set; } = Colors.CornflowerBlue;

        public Matrix3x2 GlobalTransform { get; protected set; } = Matrix3x2.Identity;

        public bool ShowFPS { get; set; }

        public DeviceInput DeviceInput = new();
        
        public Point MouseClientPosition = new();

        public void AddSprites(params Sprite[] sprites)
        {
            if (sprites == null) throw new ArgumentNullException(nameof(sprites));

            foreach (Sprite sprite in sprites)
            {
                Sprites.Add(sprite.Id, sprite);
            }
        }

        public static Point GetMousePosition()
        {
            POINT point;
            User32.GetCursorPos(out point);

            return new Point(point.X, point.Y);
        }

        protected override void OnUpdateLogic(float lastFrameTimeInSecond)
        {
            base.OnUpdateLogic(lastFrameTimeInSecond);

            if (Focused)
            {
                DeviceInput.UpdateKeyboard();
            }
            else
            {
                DeviceInput.ClearKeyboard();
            }

            DeviceInput.UpdateMouse();
            MouseClientPosition = ScreenToClient(GetMousePosition());

            if (!new RectangleF(0, 0, ClientSize.Width, ClientSize.Height).Contains(MouseClientPosition))
            {
                DeviceInput.MouseState.Z = 0;
            }

            foreach (var sprite in Sprites.Values) sprite.OnUpdate(lastFrameTimeInSecond);

            List<int> toDestroyIds = Sprites.Values.Where(x => x.ReadyToRemove).Select(x => x.Id).ToList();
            if (toDestroyIds.Count > 0)
            {
                foreach (int id in toDestroyIds)
                {
                    Sprites[id].Dispose();
                    Sprites.Remove(id);
                }
                World.ProcessChanges();
            }
        }

        protected override void OnDraw(ID2D1DeviceContext renderTarget)
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
                    XResource.GetColor(Colors.DimGray));
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DeviceInput?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
