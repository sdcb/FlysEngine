<Query Kind="Statements">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>FlysEngine</NuGetReference>
  <NuGetReference>FlysEngine.Desktop</NuGetReference>
  <NuGetReference>SharpDX.Desktop</NuGetReference>
  <Namespace>Direct2D = SharpDX.Direct2D1</Namespace>
  <Namespace>DWrite = SharpDX.DirectWrite</Namespace>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Desktop</Namespace>
  <Namespace>SharpDX</Namespace>
  <Namespace>System</Namespace>
</Query>

using (var window = new RenderWindow() { Text = "Hello World" })
{
	var bottomRightFont = new DWrite.TextFormat(window.XResource.DWriteFactory, "Consolas", 16.0f)
	{
		FlowDirection = DWrite.FlowDirection.BottomToTop,
		TextAlignment = DWrite.TextAlignment.Trailing,
	};
	var bottomLeftFont = new DWrite.TextFormat(window.XResource.DWriteFactory, "Consolas",
		DWrite.FontWeight.Normal, DWrite.FontStyle.Italic, 24.0f)
	{
		FlowDirection = DWrite.FlowDirection.BottomToTop,
		TextAlignment = DWrite.TextAlignment.Leading,
	};

	window.Draw += Draw;
	RenderLoop.Run(window, () => window.Render(1, 0));

	void Draw(RenderWindow _, Direct2D.DeviceContext target)
	{
		XResource res = window.XResource;
		target.Clear(Color.CornflowerBlue);
		RectangleF rectangle = new RectangleF(0, 0, target.Size.Width, target.Size.Height);

		target.DrawRectangle(
			new RectangleF(10, 10, target.Size.Width - 20, target.Size.Height - 20),
			res.GetColor(Color.Blue));

		target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎",
			res.TextFormats[36], rectangle, res.GetColor(Color.Blue),
			Direct2D.DrawTextOptions.EnableColorFont);

		target.DrawText($"{window.XResource.DurationSinceStart:mm':'ss'.'ff}\nFPS: {window.RenderTimer.FramesPerSecond:F1}",
			bottomRightFont, rectangle, res.GetColor(Color.Red));

		target.DrawText("Hello World",
			bottomLeftFont, rectangle, res.GetColor(Color.Purple));
	}
}