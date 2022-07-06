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
  <Namespace>Vortice.UIAnimation</Namespace>
</Query>

using (var window = new RenderWindow() { Text = "Hello World" })
{
	DirectWrite.IDWriteTextFormat bottomRightFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", 16.0f);
	bottomRightFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomRightFont.TextAlignment = DirectWrite.TextAlignment.Trailing;
	
	DirectWrite.IDWriteTextFormat bottomLeftFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", DirectWrite.FontWeight.Normal, DirectWrite.FontStyle.Italic, 24.0f);
	bottomLeftFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomLeftFont.TextAlignment = DirectWrite.TextAlignment.Leading;

	IUIAnimationStoryboard2 sb = window.XResource.Animation.CreateStoryboard();
	
	
	IUIAnimationVariable2 v = window.XResource.CreateAnimation(10, 36, 1);
	window.Draw += Draw;
	RenderLoop.Run(window, () => window.Render(1, 0));

	void Draw(RenderWindow _, Direct2D.ID2D1DeviceContext target)
	{
		XResource res = window.XResource;
		target.Clear(Colors.CornflowerBlue);
		Rect rectangle = new (0, 0, target.Size.Width, target.Size.Height);

		target.DrawRectangle(
			new Rect(10, 10, target.Size.Width - 20, target.Size.Height - 20),
			res.GetColor(Colors.Blue));

		target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎",
			res.TextFormats[36], rectangle, res.GetColor(Colors.Blue),
			Direct2D.DrawTextOptions.EnableColorFont);

		target.DrawText($"{window.XResource.DurationSinceStart:mm':'ss'.'ff}\nFPS: {window.RenderTimer.FramesPerSecond:F1}",
			bottomRightFont, rectangle, res.GetColor(Colors.Red));

		target.DrawText("Hello World",
			bottomLeftFont, rectangle, res.GetColor(Colors.Purple));
	}
}