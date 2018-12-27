<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>FlysEngine.Sprites</NuGetReference>
  <Namespace>Direct2D = SharpDX.Direct2D1</Namespace>
  <Namespace>DWrite = SharpDX.DirectWrite</Namespace>
  <Namespace>EngineShapes = FarseerPhysics.Collision.Shapes</Namespace>
  <Namespace>FarseerPhysics.Common</Namespace>
  <Namespace>FarseerPhysics.Dynamics</Namespace>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Desktop</Namespace>
  <Namespace>FlysEngine.Managers</Namespace>
  <Namespace>FlysEngine.Sprites</Namespace>
  <Namespace>FlysEngine.Sprites.Shapes</Namespace>
  <Namespace>SharpDX</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Xna = Duality</Namespace>
</Query>

static class C
{
	public const float DefaultSpeed = 200.0f;
	public const float Restitution = 1.0f, Friction = 0.0f, Density = 1.0f;
	public const float BlockWidth = Width / 9f, BlockHeight = Width / 30, Offset = (Width - BlockWidth * 7) / 8, BallR = Width / 40;
	public const float Width = 300.0f, Height = Width * 0.9f;
}

class Game : SpriteWindow
{
	public Dictionary<Guid, Sprite> Blocks = new Dictionary<Guid, Sprite>();
	public Sprite Breaker, Ball, Walls, FailureArea;
	public GameState State { get; set; } = GameState.BallOnBreaker;
	private Direct2D.Brush BallBrush;
	private List<Direct2D.Brush> RectBrushes;
	
	public Game()
	{
		Text = "Block Breaker";
		ShowFPS = true;
		AddSprites(Enumerable.Range(0, 4)
			.Select(row => new { Row = row, Cols = Enumerable.Range(0, 7) })
			.SelectMany(k => k.Cols.Select(col => new { Row = k.Row, Col = col }))
			.Select(id =>
			{
				var block = new Sprite(this)
				{
					Name = $"Blocker-{id.Row}-{id.Col}",
					Position = new Vector2(
						C.Offset + id.Col * (C.Offset + C.BlockWidth),
						C.Offset + id.Row * (C.BlockHeight + C.Offset)),
				};
				block.SetShapes(new RectangleShape { Size = new Vector2(C.BlockWidth, C.BlockHeight) });
				block.Body.BodyType = BodyType.Static;
				block.AddBehavior(new BlockBehavior(GetRectBrushByHitPoint, 4 - id.Row, block));
				Blocks.Add(block.Id, block);
				return block;
			}).ToArray());

		AddSprites(Breaker = CreateBreaker());
		AddSprites(Ball = CreateBallAtTopOf(Breaker));
		AddSprites(Walls = CreateBorder());
		AddSprites(FailureArea = CreateBottom());
		World.ProcessChanges();
	}

	protected override void OnCreateDeviceResources()
	{
		BallBrush = new Direct2D.RadialGradientBrush(XResource.RenderTarget,
			new Direct2D.RadialGradientBrushProperties { RadiusX = C.BallR, RadiusY = C.BallR, },
			new Direct2D.GradientStopCollection(XResource.RenderTarget, new[]
			{
				new Direct2D.GradientStop{ Color = Color.White, Position = 0.0f},
				new Direct2D.GradientStop{ Color = Color.Black, Position = 1.0f},
			}));
		RectBrushes = new[] { Color.Yellow, Color.Orange, Color.Green, Color.Blue, Color.Purple }.Select(c => (Direct2D.Brush)
			new Direct2D.LinearGradientBrush(XResource.RenderTarget,
					new Direct2D.LinearGradientBrushProperties { StartPoint = Vector2.Zero, EndPoint = new Vector2(C.BlockWidth, C.BlockHeight) },
					new Direct2D.GradientStopCollection(XResource.RenderTarget, new[]
					{
						new Direct2D.GradientStop{ Color = c, Position = 0.1f},
						new Direct2D.GradientStop{ Color = Color.White, Position = 0.7f},
						new Direct2D.GradientStop{ Color = c, Position = 1.0f},
					}))).ToList();
	}

	protected override void OnReleaseDeviceResources()
	{
		BallBrush.Dispose();
		foreach (var brush in RectBrushes) brush.Dispose();
		RectBrushes.Clear();
	}

	private Direct2D.Brush GetRectBrushByHitPoint(int hitpoint)
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
			DefaultDrawEnabled = true, 
			DefaultDrawColor = Color.Blue, 
		};
		sprite.SetShapes(lines
			.Select(x => (Shape)new EdgeShape(new Vector2(x[0], x[1]), new Vector2(x[2], x[3])))
			.ToArray());
		sprite.Body.BodyType = BodyType.Kinematic;
		return sprite;
	}

	Sprite CreateBottom()
	{
		var sprite = new Sprite(this) { Name = "Bottom" };
		sprite.SetShapes((Shape)new EdgeShape(new Vector2(0, C.Height), new Vector2(C.Width, C.Height)));
		sprite.Body.BodyType = BodyType.Static;
		sprite.Hit += (o, e) =>
		{
			if (e == Ball && State == GameState.Started)
			{
				Ball.Body.LinearVelocity = Vector2.Zero.ToSimulation();
				State = GameState.Failure;
				$"Mark as failure".Dump();
			}
		};
		return sprite;
	}

	Sprite CreateBallAtTopOf(Sprite breaker)
	{
		var ball = new Sprite(this){ Name = $"Ball", };
		ball.SetShapes(new CircleShape(C.BallR));
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
		};
		sprite.SetShapes(new RectangleShape
		{
			Size = new Vector2(C.BlockWidth * 1.5f, C.BlockHeight),
		});
		sprite.Body.BodyType = BodyType.Kinematic;
		sprite.AddBehavior(new BreakerBehavior(GetRectBrushByHitPoint, 5, sprite));

		return sprite;
	}

	protected override void OnCreateDeviceSizeResources()
	{
		base.OnCreateDeviceSizeResources();
		float scale = XResource.RenderTarget.Size.Height / C.Height;
		GlobalTransform = Matrix3x2.Scaling(scale) * Matrix3x2.Translation((XResource.RenderTarget.Size.Width - C.Width * scale) / 2, 0);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (State == GameState.BallOnBreaker)
		{
			SetBallOnBreaker(Ball, Breaker);
		}

		var invertPoint = Matrix3x2.TransformPoint(Matrix3x2.Invert(GlobalTransform), new Vector2(e.X, e.Y));

		Breaker.QueryBehavior<BreakerBehavior>().MoveX(invertPoint.X);
	}

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);

		if (State == GameState.BallOnBreaker)
		{
			Ball.Body.LinearVelocity = new Vector2(
				(float)Math.Sin(Breaker.Rotation) * C.DefaultSpeed,
				(float)-Math.Cos(Breaker.Rotation) * C.DefaultSpeed).ToSimulation();
			State = GameState.Started;
		}
		else if (State == GameState.Failure || State == GameState.Started)
		{
			SetBallOnBreaker(Ball, Breaker);
			Ball.Body.LinearVelocity = Vector2.Zero.ToSimulation();
			State = GameState.BallOnBreaker;
		}
	}
}

static void SetBallOnBreaker(Sprite ball, Sprite breaker)
{
	ball.Position = new Vector2(
		breaker.Position.X + (breaker.Shapes[0] as RectangleShape).Size.X / 2,
		breaker.Position.Y - C.BallR);
}


class RectColorBehavior : Behavior
{
	public int HitPoint { get; set; }

	public Func<int, Direct2D.Brush> BrushGetter;

	public RectColorBehavior(Func<int, Direct2D.Brush> brushGetter, int hitPoint, Sprite sprite) : base(sprite)
	{
		HitPoint = hitPoint;
		BrushGetter = brushGetter;
	}

	public override void Draw(Direct2D.DeviceContext ctx)
	{
		var rect = (RectangleShape)Sprite.Shapes[0];
		ctx.FillRectangle(rect.Rect, BrushGetter(HitPoint));
		ctx.DrawRectangle(rect.Rect, Sprite.XResource.GetColor(Color.Black), 0.5f);
	}
}

class BlockBehavior : RectColorBehavior
{
	public BlockBehavior(Func<int, Direct2D.Brush> brushGetter, int hitPoint, Sprite sprite)
		: base(brushGetter, hitPoint, sprite)
	{
		sprite.Hit += OnHit;
	}

	void OnHit(object sender, Sprite e)
	{
		HitPoint--;
		if (HitPoint == 0) Sprite.ReadyToRemove = true;
	}
}

class BreakerBehavior : RectColorBehavior
{
	float _dx = 0;
	SharpDX.Animation.Variable _a;

	public BreakerBehavior(Func<int, Direct2D.Brush> brushGetter, int hitPoint, Sprite sprite)
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
			_a = Sprite.XResource.CreateAnimation(calcRotation, 0, 1.0);
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
	Func<Direct2D.Brush> BallBrushGetter;

	public BallBehavior(Func<Direct2D.Brush> ballBrushGetter, Sprite sprite) : base(sprite)
	{
		BallBrushGetter = ballBrushGetter;
	}

	public override void Draw(Direct2D.DeviceContext ctx)
	{
		var shape = (CircleShape)Sprite.Shapes[0];
		ctx.FillEllipse(shape.Ellipse, BallBrushGetter());
	}
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
		RenderLoop.Run(window, () => window.Render(0, 0));
	}
}