<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <Reference Relative="..\..\..\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\FlysEngine.dll">C:\Users\flysha.zhou\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\FlysEngine.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.Formatters.Soap.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference Relative="..\..\..\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.Direct2D1.dll">C:\Users\flysha.zhou\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.Direct2D1.dll</Reference>
  <Reference Relative="..\..\..\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.DXGI.dll">C:\Users\flysha.zhou\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.DXGI.dll</Reference>
  <Reference Relative="..\..\..\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.Mathematics.dll">C:\Users\flysha.zhou\source\repos\FlysEngine\FlysEngine\bin\Debug\net40\SharpDX.Mathematics.dll</Reference>
  <NuGetReference>SharpDX.Desktop</NuGetReference>
  <Namespace>FlysEngine</Namespace>
  <Namespace>FlysEngine.Managers</Namespace>
  <Namespace>SharpDX</Namespace>
  <Namespace>SharpDX.Direct2D1</Namespace>
  <Namespace>SharpDX.DXGI</Namespace>
  <Namespace>SharpDX.Windows</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
  <Namespace>System.Windows.Forms</Namespace>
  <Namespace>FlysEngine.Tools</Namespace>
</Query>

void Main()
{
	using (var res = new XResource())
	using (var form = new MyForm()
	{
		FormBorderStyle = FormBorderStyle.None, 
		TopLevel = true, 
		TopMost = true, 
	})
	{
		var layeredWindowCtx = new LayeredWindowContext(form.Size, form.Location);
		
		form.Resize += (o, e) =>
		{
			layeredWindowCtx.Resize(form.Size);
			
			if (form.WindowState != FormWindowState.Minimized && res.DeviceAvailable)
			{
				res.Resize();
				res.RenderTarget.Target = null;
				DirectXTools.CreateDeviceContextCPUBitmap(res.RenderTarget, form.Size.Width, form.Size.Height);
			}
		};

		form.Move += (o, e) =>
		{
			layeredWindowCtx.Move(form.Location);
		};
	
		var fpsManager = new FpsManager();
	
		RenderLoop.Run(form, () => Render());
	
		void Render()
		{
			
			if (!res.DeviceAvailable) res.InitializeDeviceGdiCompatible(form.Handle, form.Size.Width, form.Size.Height);
	
			var dt = fpsManager.BeginFrame();
			{
				res.UpdateLogic(dt);
				res.RenderTarget.BeginDraw();
				Draw(res.RenderTarget);

				using (var gdi = res.RenderTarget.QueryInterface<GdiInteropRenderTarget>())
				{
					var hdc = gdi.GetDC(DeviceContextInitializeMode.Copy);
					layeredWindowCtx.Draw(form.Handle, hdc);
					gdi.ReleaseDC();
				}

				res.RenderTarget.EndDraw();
				fpsManager.EndFrame();
			}
			
			res.SwapChain.Present(1, PresentFlags.None);
		}
	
		void Draw(RenderTarget renderTarget)
		{
			renderTarget.Clear(Color.Transparent.ToColor4());
			renderTarget.DrawRectangle(new RectangleF(0, 0, renderTarget.Size.Width, renderTarget.Size.Height),
				res.GetColor(Color.Blue));
	
			renderTarget.DrawText($"😀 😁 😂 🤣 😃 😄 😅 😆 😉 😊 😋 😎",
				res.TextFormats[24.0f],
				new RectangleF(0, 22, renderTarget.Size.Width, float.MaxValue),
				res.GetColor(Color.White),
				DrawTextOptions.EnableColorFont);
	
			renderTarget.DrawText($"FPS: {fpsManager.Fps}\r\nFT: {fpsManager.FrameTimeMs}",
				res.TextFormats[15.0f],
				new RectangleF(0, 0, float.MaxValue, float.MaxValue),
				res.GetColor(Color.Red));
		}
	}
}

// Define other methods and classes here
public class LayeredWindowContext : IDisposable
{
	UpdateLayeredWindowInfo info;
	BlendFunction blend;
	System.Drawing.Size size;
	System.Drawing.Point source = new System.Drawing.Point(), destination = new System.Drawing.Point();

	public LayeredWindowContext(System.Drawing.Size size, System.Drawing.Point destination)
	{
		blend.SourceConstantAlpha = 0xff;
		blend.AlphaFormat = BlendFormats.Alpha;
		info.BlendFunction = Marshal.AllocHGlobal(Marshal.SizeOf<BlendFunction>());
		Marshal.StructureToPtr(blend, info.BlendFunction, false);

		this.size = size;
		info.WindowSize = Marshal.AllocHGlobal(Marshal.SizeOf<System.Drawing.Size>());
		Marshal.StructureToPtr(size, info.WindowSize, false);

		info.SourcePoint = Marshal.AllocHGlobal(Marshal.SizeOf<Point>());
		Marshal.StructureToPtr(source, info.SourcePoint, false);

		this.destination = destination;
		info.DestinationPoint = Marshal.AllocHGlobal(Marshal.SizeOf<Point>());
		Marshal.StructureToPtr(destination, info.DestinationPoint, false);

		info.StructureSize = Marshal.SizeOf<UpdateLayeredWindowInfo>();
		info.Flags = UlwFlags.Alpha;
	}

	public void Move(System.Drawing.Point destination)
	{
		Marshal.DestroyStructure<System.Drawing.Point>(info.DestinationPoint);
		this.destination = destination;
		info.DestinationPoint = Marshal.AllocHGlobal(Marshal.SizeOf<System.Drawing.Point>());
		Marshal.StructureToPtr(destination, info.DestinationPoint, false);
	}

	public void Draw(IntPtr window, IntPtr hdc)
	{
		info.SourceHdc = hdc;
		var ok = UpdateLayeredWindowIndirect(window, ref info);
		if (!ok)
		{
			Debug.WriteLine("ULW failed!, set EXStyle |= WS_EX_LAYERED(0x00080000)");
		}
	}

	public void Resize(System.Drawing.Size size)
	{
		Marshal.DestroyStructure<System.Drawing.Size>(info.WindowSize);
		this.size = size;
		info.WindowSize = Marshal.AllocHGlobal(Marshal.SizeOf<System.Drawing.Size>());
		Marshal.StructureToPtr(size, info.WindowSize, false);
	}

	[DllImport("user32", SetLastError = true, ExactSpelling = true)]
	private extern static bool UpdateLayeredWindowIndirect(
		IntPtr hwnd,
		ref UpdateLayeredWindowInfo ulwInfo);

	public void Dispose()
	{
		Marshal.DestroyStructure<BlendFunction>(info.BlendFunction);
		Marshal.DestroyStructure<System.Drawing.Size>(info.WindowSize);
		Marshal.DestroyStructure<System.Drawing.Point>(info.SourcePoint);
		Marshal.DestroyStructure<System.Drawing.Point>(info.DestinationPoint);
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct UpdateLayeredWindowInfo
{
	public int StructureSize;
	public IntPtr DestinationHdc;   // HDC
	public IntPtr DestinationPoint; // POINT
	public IntPtr WindowSize;       // Size
	public IntPtr SourceHdc;        // HDC
	public IntPtr SourcePoint;      // POINT
	public int Color;               // RGB
	public IntPtr BlendFunction;    // BlendFunction
	public UlwFlags Flags;
	public IntPtr prcDirty;
}

[Flags]
public enum UlwFlags : int
{
	ColorKey = 1,
	Alpha = 2,
	Opaque = 4,
}

[StructLayout(LayoutKind.Sequential)]
public struct BlendFunction
{
	public byte BlendOp;
	public byte BlendFlags;
	public byte SourceConstantAlpha;
	public BlendFormats AlphaFormat;
}

public enum BlendFormats : byte
{
	Over = 0,
	Alpha = 1,
}

class MyForm : Form
{
	protected override CreateParams CreateParams
	{
		get
		{
			const int WS_EX_LAYERED = 0x00080000;
			var baseParams = base.CreateParams;

			baseParams.ExStyle |= (WS_EX_LAYERED);

			return baseParams;
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);

		const int WM_NCHITTEST = 0x84;
		const int HT_CAPTION = 0x2;
		if (m.Msg == WM_NCHITTEST)
		{
			m.Result = (IntPtr)(HT_CAPTION);
		}
	}
}