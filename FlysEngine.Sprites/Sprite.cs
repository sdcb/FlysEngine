using FarseerPhysics.Dynamics;
using FlysEngine.Sprites.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace FlysEngine.Sprites
{
    public class Sprite : IDisposable
    {
        static int _nextId = 0;

        public int Id { get; } = _nextId++;

        public string Name { get; set; }

        public SpriteWindow Window { get; set; }

        public XResource XResource { get; }

        public Body Body { get; }

        public event EventHandler<Sprite> Hit;

        public Dictionary<Type, Behavior> Behaviors { get; } = new Dictionary<Type, Behavior>();

        public Vector2 Center { get; set; }

        public Vector2 Position { get => Body.Position.ToDisplay(); set => Body.Position = value.ToSimulation(); }

        public float Rotation { get => Body.Rotation; set => Body.Rotation = value; }

        public bool DefaultDrawEnabled { get; set; }

        public bool ReadyToRemove { get; set; }

        public Color4 DefaultDrawColor { get; set; } = Colors.Black;

        public Shape[] Shapes { get; private set; }

        public Matrix3x2 Transform => Matrix3x2.CreateRotation(Rotation, Center) * Matrix3x2.CreateTranslation(Position - Center);

        public string[] Frames { get; set; }

        public int FrameId { get; set; }

        public float Alpha { get; set; } = 1.0f;

        public object UserData { get; set; }

        public List<Sprite> Children = new List<Sprite>();

        public Sprite(SpriteWindow game)
        {
            Window = game;
            XResource = game.XResource;
            Body = new Body(game.World);
            Body.UserData = this;
        }

        public T QueryBehavior<T>() where T : Behavior
        {
            var type = typeof(T);
            if (Behaviors.ContainsKey(type)) return (T)Behaviors[type];
            return null;
        }

        public void AddBehavior(Behavior behavior)
        {
            Behaviors.Add(behavior.GetType(), behavior);
            behavior.Sprite = this;
            behavior.OnSpriteSet(this);
        }

        public void SetShapes(params Shape[] value)
        {
            Shapes = value;
            foreach (var fixture in Body.FixtureList.ToList())
                Body.DestroyFixture(fixture);

            Shape.CreateFixtures(value, Body);
        }

        public bool IsMouseOver()
        {
            return Shape.TestPoint(Shapes, XResource.InvertTransformPoint(
                Transform * Window.GlobalTransform,
                Window.MouseClientPosition));
        }

        public virtual void OnUpdate(float dt)
        {
            foreach (var behavior in Behaviors.Values)
            {
                behavior.Update(dt);
            }

            foreach (var child in Children)
            {
                child.OnUpdate(dt);
            }

            if (Hit != null && Body.Enabled)
            {
                foreach (var contact in GetContacts())
                {
                    Hit(this, contact);
                }

                IEnumerable<Sprite> GetContacts()
                {
                    var c = Body.ContactList;
                    while (c != null)
                    {
                        if (c.Contact.IsTouching())
                        {
                            yield return (Sprite)c.Other.UserData;
                        }
                        c = c.Next;
                    }
                }
            }
        }

        public void Draw(ID2D1DeviceContext ctx)
        {
            var old = ctx.Transform;
            ctx.Transform = Transform * old;

            if (Frames != null && Frames.Length >= FrameId)
            {
                ctx.DrawBitmap(
                    XResource.Bitmaps[Frames[FrameId]],
                    Alpha, 
                    BitmapInterpolationMode.Linear);
            }
            foreach (var behavior in Behaviors.Values) behavior.Draw(ctx);
            if (DefaultDrawEnabled)
            {
                foreach (var shape in Shapes) shape.Draw(ctx, XResource.GetColor(DefaultDrawColor));
            }
            ctx.Transform = old;
            foreach (var child in Children)
            {
                child.Draw(ctx);
            }
        }

        internal protected virtual void OnCreateDeviceSizeResources()
        {
            foreach (var behavior in Behaviors.Values) behavior.OnCreateDeviceSizeResources();
        }

        internal protected virtual void OnReleaseDeviceSizeResources()
        {
            foreach (var behavior in Behaviors.Values) behavior.OnReleaseDeviceSizeResources();
        }

        internal protected virtual void OnReleaseDeviceResources()
        {
            foreach (var behavior in Behaviors.Values) behavior.OnReleaseDeviceResources();
        }

        internal protected virtual void OnCreateDeviceResources()
        {
            foreach (var behavior in Behaviors.Values) behavior.OnCreateDeviceResources();
        }

        public virtual void Dispose()
        {
            OnReleaseDeviceSizeResources();
            OnReleaseDeviceResources();

            foreach (var child in Children)
                child.Dispose();
            Children.Clear();

            foreach (Behavior behavior in Behaviors.Values)
                behavior.Dispose();
            Behaviors.Clear();

            Window.World.RemoveBody(Body);
        }

        public override string ToString() => $"{Name}:{Position}";
    }
}
