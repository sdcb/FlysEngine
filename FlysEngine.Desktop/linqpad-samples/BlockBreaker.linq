<Query Kind="Program">
  <NuGetReference Prerelease="true">AdamsLair.Duality.Physics</NuGetReference>
  <NuGetReference Prerelease="true">AdamsLair.Duality.Primitives</NuGetReference>
  <NuGetReference Prerelease="true">FlysEngine.Desktop</NuGetReference>
  <Namespace>Direct2D = Vortice.Direct2D1</Namespace>
  <Namespace>DirectWrite = Vortice.DirectWrite</Namespace>
  <Namespace>EngineShapes = FarseerPhysics.Collision.Shapes</Namespace>
  <Namespace>FarseerPhysics.Common</Namespace>
  <Namespace>FarseerPhysics.Dynamics</Namespace>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Desktop</Namespace>
  <Namespace>FlysEngine.Managers</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Vortice.Mathematics</Namespace>
  <Namespace>Vortice.UIAnimation</Namespace>
  <Namespace>Xna = Duality</Namespace>
</Query>

static class C
{
	public const float DefaultSpeed = 200.0f;
	public const float Restitution = 1.0f, Friction = 0.0f, Density = 1.0f;
	public const float BlockWidth = Width / 9f, BlockHeight = Width / 30, Offset = (Width - BlockWidth * 7) / 8, BallR = Width / 40;
	public const float Width = 300.0f, Height = Width * 0.9f;

	public const float SimDisScale = 64.0f;
	public static float ToSim(float x) => x / SimDisScale;
	public static float ToDis(float x) => x * SimDisScale;
}

class Game : RenderWindow
{
	public World World = new World(Xna.Vector2.Zero);
	public Dictionary<Guid, Sprite> Blocks = new Dictionary<System.Guid, Sprite>();
	public Sprite Breaker, Ball, Walls, FailureArea;
	public IEnumerable<Sprite> Sprites => Blocks.Values.Concat(new[] { Breaker, Ball, Walls, FailureArea });
	public GameState State { get; set; } = GameState.BallOnBreaker;
	private Direct2D.ID2D1Brush BallBrush;
	private List<Direct2D.ID2D1Brush> RectBrushes;
	public Matrix3x2 GlobalTransform { get; set; }

	protected override void OnLoad(EventArgs e)
	{
		Text = "Block Breaker";
		Size = new Size(600, 600);

		for (var row = 0; row < 4; ++row)
		{
			for (var col = 0; col < 7; ++col)
			{
				var block = new Sprite(this)
				{
					Name = $"Blocker-{row}-{col}",
					Position = new Vector2(
						C.Offset + col * (C.Offset + C.BlockWidth),
						C.Offset + row * (C.BlockHeight + C.Offset)),
				};
				block.Shapes = new List<UserQuery.Shape>
				{
					new RectangleShape
					{
						Size = new Vector2(C.BlockWidth, C.BlockHeight)
					},
				};
				block.Body.BodyType = BodyType.Static;
				block.AddBehavior(new BlockBehavior(GetRectBrushByHitPoint, 4 - row, block));
				Blocks.Add(block.Id, block);
			}
		}

		Breaker = CreateBreaker();
		Ball = CreateBallAtTopOf(Breaker);
		Walls = CreateBorder();
		FailureArea = CreateBottom();
		World.ProcessChanges();
	}

	protected override void OnCreateDeviceResources()
	{
		BallBrush = XResource.RenderTarget.CreateRadialGradientBrush(
			new Direct2D.RadialGradientBrushProperties { RadiusX = C.BallR, RadiusY = C.BallR, },
			XResource.RenderTarget.CreateGradientStopCollection(new[]
			{
				new Direct2D.GradientStop{ Color = Colors.White, Position = 0.0f},
				new Direct2D.GradientStop{ Color = Colors.Black, Position = 1.0f},
			}));
		RectBrushes = new[] { Colors.Red, Colors.Orange, Colors.Green, Colors.Blue, Colors.Purple }.Select(c => (Direct2D.ID2D1Brush)
			XResource.RenderTarget.CreateLinearGradientBrush(
					new Direct2D.LinearGradientBrushProperties { StartPoint = new Vector2(), EndPoint = new Vector2(C.BlockWidth / 1.5f, C.BlockHeight / 1.5f) },
					XResource.RenderTarget.CreateGradientStopCollection(new[]
					{
						new Direct2D.GradientStop{ Color = c, Position = 0.1f},
						new Direct2D.GradientStop{ Color = Colors.White, Position = 0.7f},
						new Direct2D.GradientStop{ Color = c, Position = 1.0f},
					}))).ToList();
	}

	protected override void OnReleaseDeviceResources()
	{
		BallBrush.Dispose();
		foreach (var brush in RectBrushes) brush.Dispose();
		RectBrushes.Clear();
	}

	private Direct2D.ID2D1Brush GetRectBrushByHitPoint(int hitpoint)
	{
		if (hitpoint <= 4) return RectBrushes[hitpoint - 1];
		return RectBrushes[4];
	}

	Sprite CreateBorder()
	{
		var lines = new[]
		{
			new []{0, 0, C.Width, 0},
			new []{C.Width, 0, C.Width, C.Height},
			new []{0, 0, 0, C.Height},
		};

		var sprite = new Sprite(this)
		{
			Name = "Border",
			Shapes = lines.Select(x =>
				(Shape)new EdgeShape(new Vector2(x[0], x[1]), new Vector2(x[2], x[3]))).ToList()
		};
		sprite.Body.BodyType = BodyType.Kinematic;
		return sprite;
	}

	Sprite CreateBottom()
	{
		var sprite = new Sprite(this)
		{
			Name = "Bottom",
			Shapes = new List<Shape>
			{
				(Shape)new EdgeShape(new Vector2(0, C.Height), new Vector2(C.Width, C.Height))
			},
		};
		sprite.Body.BodyType = BodyType.Static;
		sprite.Hit += (o, e) =>
		{
			if (e == Ball && State == GameState.Started)
			{
				Ball.Body.LinearVelocity = ToXnaVector2(Vector2.Zero);
				State = GameState.Failure;
				$"Mark as failure".Dump();
			}
		};
		return sprite;
	}

	Sprite CreateBallAtTopOf(Sprite breaker)
	{
		var ball = new Sprite(this)
		{
			Name = $"Ball",
			Shapes = new List<UserQuery.Shape>
			{
				new CircleShape(C.BallR)
			},
		};
		ball.Body.BodyType = BodyType.Dynamic;
		ball.AddBehavior(new BallBehavior(() => BallBrush, ball));
		SetBallOnBreaker(ball, breaker);

		return ball;
	}

	Sprite CreateBreaker()
	{
		var sprite = new Sprite(this)
		{
			Name = $"Breaker",
			Position = new Vector2(C.Width / 2 - C.BlockWidth * 1.5f / 2, C.Height - C.BallR * 3), // Center
			Center = new Vector2(C.BlockWidth * 1.5f / 2, 0),
			Shapes = new List<UserQuery.Shape>
			{
				new RectangleShape
				{
					Size = new Vector2(C.BlockWidth * 1.5f, C.BlockHeight),
				}
			},
		};
		sprite.Body.BodyType = BodyType.Kinematic;
		sprite.AddBehavior(new BreakerBehavior(GetRectBrushByHitPoint, 5, sprite));

		return sprite;
	}

	protected override void OnUpdateLogic(float lastFrameTimeInSecond)
	{
		base.OnUpdateLogic(lastFrameTimeInSecond);

		foreach (var sprite in Sprites) sprite.OnUpdate(RenderTimer);
		World.Step(lastFrameTimeInSecond);

		var toDestroyIds = Blocks.Values.Where(x => x.IsDestroying).Select(x => x.Id).ToList();
		foreach (var id in toDestroyIds)
		{
			World.RemoveBody(Blocks[id].Body);
			Blocks.Remove(id);
		}
		World.ProcessChanges();
	}

	protected override void OnDraw(Direct2D.ID2D1DeviceContext ctx)
	{
		ctx.Clear(Colors.CornflowerBlue);

		ctx.DrawText($"FPS: {RenderTimer.FramesPerSecond:F1}",
			XResource.TextFormats[10.0f],
			new RectangleF(0, 0, ctx.Size.Width, ctx.Size.Height),
			XResource.GetColor(Colors.Red));

		float scale = ctx.Size.Height / C.Height;
		ctx.Transform = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation((ctx.Size.Width - C.Width * scale) / 2, 0);
		GlobalTransform = ctx.Transform;
		foreach (var sprite in Sprites) sprite.Draw(ctx);
	}

	protected override void OnMouseMove(PointEventArgs e)
	{
		base.OnMouseMove(e);

		if (State == GameState.BallOnBreaker)
		{
			SetBallOnBreaker(Ball, Breaker);
		}

		Matrix3x2.Invert(GlobalTransform, out Matrix3x2 inverted);
		Vector2 invertPoint = Vector2.Transform(new Vector2(e.X, e.Y), inverted);

		Breaker.QueryBehavior<BreakerBehavior>().MoveX(invertPoint.X);
	}

	protected override void OnClick(PointEventArgs e)
	{
		base.OnClick(e);

		if (State == GameState.BallOnBreaker)
		{
			Ball.Body.LinearVelocity = ToXnaVector2(new Vector2(
				(float)Math.Sin(Breaker.Rotation) * C.DefaultSpeed,
				(float)-Math.Cos(Breaker.Rotation) * C.DefaultSpeed));
			State = GameState.Started;
		}
		else if (State == GameState.Failure || State == GameState.Started)
		{
			SetBallOnBreaker(Ball, Breaker);
			Ball.Body.LinearVelocity = ToXnaVector2(Vector2.Zero);
			State = GameState.BallOnBreaker;
		}
	}
}

public abstract class Shape
{
	public Vector2 Center { get; set; }

	public Vector2 Offset { get; set; }

	public abstract bool TestPoint(Vector2 point);

	public abstract EngineShapes.Shape ToEngineShape();

	public abstract void Draw(
		Direct2D.ID2D1DeviceContext renderTarget,
		Direct2D.ID2D1Brush brush);

	public static bool TestPoint(IEnumerable<Shape> shapes, Vector2 point)
	{
		return shapes.Any(shape => shape.TestPoint(point));
	}

	public static void CreateFixtures(
		IEnumerable<Shape> shapes,
		Body body)
	{
		foreach (var shape in shapes)
		{
			var engineShape = shape.ToEngineShape();
			var fixture = body.CreateFixture(engineShape);
			fixture.Restitution = C.Restitution;
			fixture.Friction = C.Friction;
		}
	}
}

public class RectangleShape : Shape
{
	public Vector2 Size { get; set; }

	public RectangleF Rect => new (Offset.X, Offset.Y, Size.X, Size.Y);

	public override void Draw(Direct2D.ID2D1DeviceContext renderTarget, Direct2D.ID2D1Brush brush) => renderTarget.FillRectangle(Rect, brush);

	public override bool TestPoint(Vector2 point) => Rect.Contains(point.X, point.Y);

	public override EngineShapes.Shape ToEngineShape()
	{
		var offset = Offset - Center;
		var shape = new EngineShapes.PolygonShape(C.Density);
		shape.Set(new Vertices(new[]
		{
			ToXnaVector2(offset),
			ToXnaVector2(offset + new Vector2(Size.X, 0)),
			ToXnaVector2(offset + Size),
			ToXnaVector2(offset + new Vector2(0, Size.Y)),
		}));
		return shape;
	}
}

public class CircleShape : Shape
{
	public CircleShape(float r) { R = r; }

	public float R { get; private set; }

	public Direct2D.Ellipse Ellipse => new Direct2D.Ellipse(Center + Offset, R, R);

	public CircleShape Clone()
	{
		return new CircleShape(R)
		{
			Offset = Offset,
			Center = Center,
		};
	}

	public override void Draw(Direct2D.ID2D1DeviceContext renderTarget, Direct2D.ID2D1Brush brush)
	{
		renderTarget.DrawEllipse(Ellipse, brush, 1.0f);
	}

	public override bool TestPoint(Vector2 point)
	{
		return Vector2.DistanceSquared(Center + Offset, point)
			< R * R;
	}

	public override EngineShapes.Shape ToEngineShape()
	{
		return new EngineShapes.CircleShape(
				C.ToSim(R), C.Density);
	}
}

public class EdgeShape : Shape
{
	public Vector2 P1 { get; set; }
	public Vector2 P2 { get; set; }

	public EdgeShape(Vector2 p1, Vector2 p2)
	{
		P1 = p1;
		P2 = p2;
	}

	public EdgeShape Clone()
	{
		return new EdgeShape(P1, P2)
		{
			Offset = Offset,
			Center = Center,
		};
	}

	public override void Draw(Direct2D.ID2D1DeviceContext renderTarget, Direct2D.ID2D1Brush brush)
	{
		renderTarget.DrawLine(P1, P2, brush);
	}

	public override bool TestPoint(Vector2 point) => false;

	public override EngineShapes.Shape ToEngineShape()
	{
		return new EngineShapes.EdgeShape(ToXnaVector2(P1), ToXnaVector2(P2));
	}
}

static void SetBallOnBreaker(Sprite ball, Sprite breaker)
{
	ball.Position = new Vector2(
		breaker.Position.X + (breaker.Shapes[0] as RectangleShape).Size.X / 2,
		breaker.Position.Y - C.BallR);
}

static Vector2 ToVector2(Xna.Vector2 xnaVector2) => new Vector2
{
	X = C.ToDis(xnaVector2.X),
	Y = C.ToDis(xnaVector2.Y),
};

static Xna.Vector2 ToXnaVector2(Vector2 vector2) => new Xna.Vector2
{
	X = C.ToSim(vector2.X),
	Y = C.ToSim(vector2.Y),
};

class Sprite
{
	public Guid Id { get; } = Guid.NewGuid();
	public string Name { get; set;}
	public Game Game { get; }
	protected XResource XResource { get; }
	public Vector2 Center { get; set;}

	public event EventHandler<Sprite> Hit;
	public Dictionary<Type, Behavior> Behaviors { get; } = new Dictionary<Type, Behavior>();

	public T QueryBehavior<T>() where T : Behavior
	{
		var type = typeof(T);
		if (Behaviors.ContainsKey(type)) return (T)Behaviors[type];
		return null;
	}

	public void AddBehavior(Behavior behavior) => Behaviors.Add(behavior.GetType(), behavior);

	public Sprite(Game game)
	{
		Game = game;
		XResource = game.XResource;
		Body = new Body(game.World);
		Body.UserData = this;
	}

	public Vector2 Position
	{
		get => ToVector2(Body.Position);
		set => Body.Position = ToXnaVector2(value);
	}

	public float Rotation
	{
		get => Body.Rotation;
		set => Body.Rotation = value;
	}

	public bool SuppressDefaultDraw { get; set; }

	public Matrix3x2 Transform => Matrix3x2.CreateRotation(Rotation, Center) * Matrix3x2.CreateTranslation(Position);

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

	public readonly Body Body;

	public bool IsDestroying { get; set; }

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
	public void Draw(Direct2D.ID2D1DeviceContext ctx)
	{
		var old = ctx.Transform;
		ctx.Transform = Transform * old;

		foreach (var behavior in Behaviors.Values) behavior.Draw(ctx);
		if (!SuppressDefaultDraw) foreach (var shape in Shapes) shape.Draw(ctx, XResource.GetColor(Colors.Yellow));

		ctx.Transform = old;
	}

	public override string ToString() => $"{Name}@{Position}";
}

class RectColorBehavior : Behavior
{
	public int HitPoint { get; set; }

	public Func<int, Direct2D.ID2D1Brush> BrushGetter;

	public RectColorBehavior(Func<int, Direct2D.ID2D1Brush> brushGetter, int hitPoint, Sprite sprite) : base(sprite)
	{
		HitPoint = hitPoint;
		BrushGetter = brushGetter;
		Sprite.SuppressDefaultDraw = true;
	}

	public override void Draw(Direct2D.ID2D1DeviceContext ctx)
	{
		var rect = (RectangleShape)Sprite.Shapes[0];
		ctx.FillRectangle(rect.Rect, BrushGetter(HitPoint));
		ctx.DrawRectangle(rect.Rect, Sprite.Game.XResource.GetColor(Colors.Black), 0.5f);
	}
}

class BlockBehavior : RectColorBehavior
{
	public BlockBehavior(Func<int, Direct2D.ID2D1Brush> brushGetter, int hitPoint, Sprite sprite)
		: base(brushGetter, hitPoint, sprite)
	{
		sprite.Hit += OnHit;
	}

	void OnHit(object sender, Sprite e)
	{
		HitPoint--;
		if (HitPoint == 0) Sprite.IsDestroying = true;
	}
}

class BreakerBehavior : RectColorBehavior
{
	float _dx = 0;
	IUIAnimationVariable2 _a;
	
	public BreakerBehavior(Func<int, Direct2D.ID2D1Brush> brushGetter, int hitPoint, Sprite sprite)
		: base(brushGetter, hitPoint, sprite)
	{
	}

	public override void Update(RenderTimer timer)
	{
		base.Update(timer);
		Sprite.Rotation = GetCurrentRotation();
		
		float calcRotation = -_dx / C.Width * 3 * (float)Math.PI;
		if (Math.Abs(calcRotation) > Math.Abs(Sprite.Rotation))
		{
			if (_a != null) _a.Dispose();
			_a = Sprite.Game.XResource.CreateAnimation(calcRotation, 0, 1.0);
		}
		_dx = 0;
	}

	float GetCurrentRotation() => _a == null ? 0 : (float)_a.Value;

	public void MoveX(float x)
	{
		_dx = Sprite.Position.X - x;
		Sprite.Position = new Vector2(x, Sprite.Position.Y);
	}
}

class BallBehavior : Behavior
{
	Func<Direct2D.ID2D1Brush> BallBrushGetter;

	public BallBehavior(Func<Direct2D.ID2D1Brush> ballBrushGetter, Sprite sprite) : base(sprite)
	{
		BallBrushGetter = ballBrushGetter;
		Sprite.SuppressDefaultDraw = true;
	}

	public override void Draw(Direct2D.ID2D1DeviceContext ctx)
	{
		var shape = (CircleShape)Sprite.Shapes[0];
		ctx.FillEllipse(shape.Ellipse, BallBrushGetter());
	}
}

abstract class Behavior
{
	public Sprite Sprite { get; }
	public Behavior(Sprite sprite) { Sprite = sprite; }
	public virtual void Update(RenderTimer timer) { }
	public virtual void Draw(Direct2D.ID2D1DeviceContext ctx) { }
}

enum GameState
{
	BallOnBreaker,
	Started,
	Failure,
}

static void Main()
{
	FarseerPhysics.Settings.VelocityThreshold = 0.0f;
	using (var window = new Game())
	{
		RenderLoop.Run(window, () => window.Render(1, 0));
	}
}