# FlysEngine [![NuGet](https://img.shields.io/nuget/v/FlysEngine.svg)](https://nuget.org/packages/FlysEngine) [![QQ](https://img.shields.io/badge/QQ_Group-579060605-52B6EF?style=social&logo=tencent-qq&logoColor=000&logoWidth=20)](https://jq.qq.com/?_wv=1027&k=K4fBqpyQ)
Real-time 2D rendering utilities based on SharpDX/Direct2D.

# Packages
| Package              | runtimes |                                                  NuGet Package                                                   |
| -------------------- | :------: | :--------------------------------------------------------------------------------------------------------------: |
| `FlysEngine`         |  `net6`  |         [![NuGet](https://img.shields.io/nuget/v/FlysEngine.svg)](https://nuget.org/packages/FlysEngine)         |
| `FlysEngine.Desktop` |  `net6`  | [![NuGet](https://img.shields.io/nuget/v/FlysEngine.Desktop.svg)](https://nuget.org/packages/FlysEngine.Desktop) |
| `FlysEngine.Sprites` |  `net6`  | [![NuGet](https://img.shields.io/nuget/v/FlysEngine.Sprites.svg)](https://nuget.org/packages/FlysEngine.Sprites) |

# Simple example
(Refer to /tree/master/FlysTest)
* Final result:
  ![Final Result](FlysTest/FlysTest.png)
* Create a Windows-form application
* Delete the entire Form1 class
* Install the `FlysEngine` nuget package
* Install the `SharpDX.Desktop` nuget package for convenient
* In `Program.cs` file, using following namespaces:
  ```csharp
  using FlysEngine;
  using FlysEngine.Desktop;
  using FlysEngine.Managers;
  using Vortice.Direct2D1;
  using Vortice.DirectWrite;
  using Vortice.Mathematics;
  using FlowDirection = Vortice.DirectWrite.FlowDirection;
  using FontStyle = Vortice.DirectWrite.FontStyle;
  ```
* Replace Main method's content to following code:
  ```csharp
  using (var res = new XResource())
  using (var form = new Form() { Text = "Hello World" })
  {
    var timer = new RenderTimer();
    IDWriteTextFormat bottomRightFont = res.DWriteFactory.CreateTextFormat("Consolas", 16.0f);
    bottomRightFont.FlowDirection = Vortice.DirectWrite.FlowDirection.BottomToTop;
    bottomRightFont.TextAlignment = TextAlignment.Trailing;

    IDWriteTextFormat bottomLeftFont = res.DWriteFactory.CreateTextFormat("Consolas", FontWeight.Normal, Vortice.DirectWrite.FontStyle.Italic, FontStretch.Normal, 24.0f);
    bottomLeftFont.FlowDirection = Vortice.DirectWrite.FlowDirection.BottomToTop;
    bottomLeftFont.TextAlignment = TextAlignment.Leading;

    form.Resize += (o, e) =>
    {
      if (form.WindowState != FormWindowState.Minimized && res.DeviceAvailable)
      {
        res.Resize();
      }
    };

    RenderLoop.Run(form, () => Render());

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
      Rect rectangle = new Rect(0, 0, target.Size.Width, target.Size.Height);

      target.DrawRectangle(
        new Rect(10, 10, target.Size.Width - 20, target.Size.Height - 20),
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
  ```
* Compile and run.

# License
Apache License 2.0
