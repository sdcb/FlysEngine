using FarseerPhysics.Dynamics;
using FlysEngine.Desktop;
using FlysEngine.Managers;
using FlysEngine.Sprites.Shapes;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Direct2D = SharpDX.Direct2D1;

namespace FlysEngine.Sprites
{
    public class Sprite
    {
        public Guid Id { get; } = Guid.NewGuid();

        public RenderWindow RenderWindow { get; set; }

        public XResource XResource { get; }

        public Body Body { get; }

        public event EventHandler<Sprite> Hit;

        public Dictionary<Type, Behavior> Behaviors { get; } = new Dictionary<Type, Behavior>();

        public Vector2 Center { get; set; }

        public Vector2 Position { get => Body.Position.ToDisplay(); set => Body.Position = value.ToSimulation(); }

        public float Rotation { get => Body.Rotation; set => Body.Rotation = value; }

        public bool DefaultDrawEnabled { get; set; } = true;

        public bool IsDestroying { get; set; }

        private List<Shape> _shapes;
        public List<Shape> Shapes
        {
            get => _shapes;
            set
            {
                _shapes = value;
                foreach (var fixture in Body.FixtureList.ToList())
                    Body.DestroyFixture(fixture);

                Shape.CreateFixtures(value, Body);
            }
        }

        public Sprite(SpriteWindow game)
        {
            RenderWindow = game;
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

        public void AddBehavior(Behavior behavior) => Behaviors.Add(behavior.GetType(), behavior);

        public virtual void OnUpdate(RenderTimer timer)
        {
            foreach (var behavior in Behaviors.Values) behavior.Update(timer);

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

        public void Draw(Direct2D.DeviceContext ctx)
        {
            var old = ctx.Transform;
            ctx.Transform = Matrix3x2.Rotation(Rotation, Center) * Matrix3x2.Translation(Position) * old;

            foreach (var behavior in Behaviors.Values) behavior.Draw(ctx);
            if (DefaultDrawEnabled) foreach (var shape in Shapes) shape.Draw(ctx, XResource.GetColor(Color.Yellow));

            ctx.Transform = old;
        }
    }
}
