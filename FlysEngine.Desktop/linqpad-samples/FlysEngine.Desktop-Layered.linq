<Query Kind="Statements">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference>FlysEngine</NuGetReference>
  <NuGetReference>FlysEngine.Desktop</NuGetReference>
  <Namespace>Direct2D = Vortice.Direct2D1</Namespace>
  <Namespace>DirectWrite = Vortice.DirectWrite</Namespace>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Desktop</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>Vortice.Mathematics</Namespace>
</Query>

using (var window = new LayeredRenderWindow() { Text = "Hello World", DragMoveEnabled = true })
{
	DirectWrite.IDWriteTextFormat bottomRightFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", 16.0f);
	bottomRightFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomRightFont.TextAlignment = DirectWrite.TextAlignment.Trailing;

	DirectWrite.IDWriteTextFormat bottomLeftFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", DirectWrite.FontWeight.Normal, DirectWrite.FontStyle.Italic, 24.0f);
	bottomLeftFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomLeftFont.TextAlignment = DirectWrite.TextAlignment.Leading;

	window.Draw += Draw;
	RenderLoop.Run(window, () => window.Render(1, 0));

	void Draw(RenderWindow _, Direct2D.ID2D1DeviceContext target)
	{
		XResource res = window.XResource;
		target.Clear(Color4.Transparent);
		RectangleF rectangle = new RectangleF(0, 0, target.Size.Width, target.Size.Height);

		target.DrawRectangle(
			rectangle,
			res.GetColor(Color4.Blue));

		target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎",
			res.TextFormats[36], rectangle, res.GetColor(Color4.Blue),
			Direct2D.DrawTextOptions.EnableColorFont);

		target.DrawText($"{window.XResource.DurationSinceStart:mm':'ss'.'ff}\nFPS: {window.RenderTimer.FramesPerSecond:F1}",
			bottomRightFont, rectangle, res.GetColor(Color4.Red));

		target.DrawText("Hello World",
			bottomLeftFont, rectangle, res.GetColor(Color4.Purple));
	}
}