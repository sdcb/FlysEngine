using FarseerPhysics.Dynamics;
using FlysEngine.Desktop;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using DirectInput = SharpDX.DirectInput;

namespace FlysEngine.Sprites
{
    public class SpriteWindow : RenderWindow
    {
        public World World { get; } = new World(Vector2.Zero.ToSimulation());

        public Dictionary<Guid, Sprite> Sprites { get; } = new Dictionary<Guid, Sprite>();

        public Color ClearColor { get; set; } = Color.CornflowerBlue;

        public Matrix3x2 GlobalTransform { get; protected set; } = Matrix3x2.Identity;

        public bool ShowFPS { get; set; }

        private DirectInput.DirectInput DirectInput { get; } = new DirectInput.DirectInput();

        private DirectInput.Keyboard Keyboard { get; }

        public DirectInput.KeyboardState KeyboardState = new DirectInput.KeyboardState();

        private DirectInput.Mouse Mouse { get; }

        public DirectInput.MouseState MouseState = new DirectInput.MouseState();

        public Vector2 MouseClientPosition = new Vector2();

        public SpriteWindow()
        {
            Keyboard = new DirectInput.Keyboard(DirectInput);
            Keyboard.Acquire();
            Mouse = new DirectInput.Mouse(DirectInput);
            Mouse.Acquire();
        }

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

            if (Focused)
            {
                Keyboard.GetCurrentState(ref KeyboardState);
            }
            else
            {
                KeyboardState.PressedKeys.Clear();
            }

            Mouse.GetCurrentState(ref MouseState);
            MouseClientPosition = PointToClient(System.Windows.Forms.Cursor.Position).ToVector2();

            if (!new Rectangle(0, 0, ClientSize.Width, ClientSize.Height).Contains(
                MouseClientPosition.X, MouseClientPosition.Y))
            {
                MouseState.Z = 0;
            }

            foreach (var sprite in Sprites.Values) sprite.OnUpdate(lastFrameTimeInSecond);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Keyboard.Dispose();
                Mouse.Dispose();
                DirectInput.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
