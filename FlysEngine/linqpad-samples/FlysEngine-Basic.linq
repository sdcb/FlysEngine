<Query Kind="Statements">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <NuGetReference Prerelease="true">FlysEngine</NuGetReference>
  <NuGetReference Prerelease="true">FlysEngine.Desktop</NuGetReference>
  <Namespace>DirectWrite = Vortice.DirectWrite</Namespace>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Desktop</Namespace>
  <Namespace>FlysEngine.Managers</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>Vortice.Direct2D1</Namespace>
  <Namespace>Vortice.DirectWrite</Namespace>
  <Namespace>Vortice.Mathematics</Namespace>
</Query>

using (var res = new XResource())
using (var form = new Form() { Text = "Hello World" })
{
	RenderTimer timer = new ();
	DirectWrite.IDWriteTextFormat bottomRightFont = res.DWriteFactory.CreateTextFormat("Consolas", 16.0f);
	bottomRightFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomRightFont.TextAlignment = DirectWrite.TextAlignment.Trailing;
	
	DirectWrite.IDWriteTextFormat bottomLeftFont = res.DWriteFactory.CreateTextFormat("Consolas", FontWeight.Normal, DirectWrite.FontStyle.Italic, 24.0f);
	bottomLeftFont.FlowDirection = DirectWrite.FlowDirection.BottomToTop;
	bottomLeftFont.TextAlignment = DirectWrite.TextAlignment.Leading;

	form.Resize += (o, e) =>
	{
		if (form.WindowState != FormWindowState.Minimized && res.DeviceAvailable)
		{
			res.Resize();
		}
	};

	form.Show();
	RenderLoop.Run(form.Handle, () => Render());

	void Render()
	{
		if (!res.DeviceAvailable) res.InitializeDevice(form.Handle);

		var target = res.RenderTarget;

		timer.BeginFrame();
		target.BeginDraw();
		Draw(target);
		target.EndDraw();
		res.SwapChain.Present(1, 0);
		timer.EndFrame();
	}

	void Draw(ID2D1DeviceContext target)
	{
		target.Clear(Colors.CornflowerBlue);
		RectangleF rectangle = new (0, 0, target.Size.Width, target.Size.Height);

		target.DrawRectangle(
			new RectangleF(10, 10, target.Size.Width - 20, target.Size.Height - 20),
			res.GetColor(Colors.Blue));

		target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎",
			res.TextFormats[36], rectangle, res.GetColor(Colors.Blue),
			DrawTextOptions.EnableColorFont);

		target.DrawText("FPS: " + timer.FramesPerSecond.ToString("F1"),
			bottomRightFont, rectangle, res.GetColor(Colors.Red));

		target.DrawText("Hello World",
			bottomLeftFont, rectangle, res.GetColor(Colors.Purple));
	}
}