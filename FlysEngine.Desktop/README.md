# FlysEngine.Desktop
For easier use for FlysEngine in desktop application.

# Simple example
* Create a Windows-form application
* Delete the entire initial Form1 class
* Install the `FlysEngine`, `FlysEngine.Desktop` nuget package
* Add following usings: 
  ```
  using FlysEngine;
  using FlysEngine.Desktop;
  using SharpDX;
  using System;
  using Direct2D = SharpDX.Direct2D1;
  using DWrite = SharpDX.DirectWrite;
  ```
* Replace following code within Main method:
  ```
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
  
          target.DrawText("FPS: " + window.RenderTimer.FramesPerSecond.ToString("F1"),
              bottomRightFont, rectangle, res.GetColor(Color.Red));
  
          target.DrawText("Hello World",
              bottomLeftFont, rectangle, res.GetColor(Color.Purple));
      }
  }
  ```
* And it's done :)

# Final result(same as FlysEngine, simple code): 
![Final Result](FlysTest/FlysTest.png)