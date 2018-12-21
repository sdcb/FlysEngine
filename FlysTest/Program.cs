using FlysEngine;
using FlysEngine.Managers;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Windows;
using System;
using System.Windows.Forms;

using DWrite = SharpDX.DirectWrite;

namespace FlysTest
{
    static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var res = new XResource())
            using (var form = new Form() { Text = "Hello World" })
            {
                var timer = new RenderTimer();
                var bottomRightFont = new DWrite.TextFormat(res.DWriteFactory, "Consolas", 16.0f)
                {
                    FlowDirection = DWrite.FlowDirection.BottomToTop, 
                    TextAlignment = DWrite.TextAlignment.Trailing, 
                };
                var bottomLeftFont = new DWrite.TextFormat(res.DWriteFactory, "Consolas", 
                    DWrite.FontWeight.Normal, DWrite.FontStyle.Italic, 24.0f)
                {
                    FlowDirection = DWrite.FlowDirection.BottomToTop, 
                    TextAlignment = DWrite.TextAlignment.Leading, 
                };

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

                void Draw(DeviceContext target)
                {
                    target.Clear(Color.CornflowerBlue.ToColor4());
                    RectangleF rectangle = new RectangleF(0, 0, target.Size.Width, target.Size.Height);

                    target.DrawRectangle(
                        new RectangleF(10, 10, target.Size.Width - 20, target.Size.Height - 20), 
                        res.GetColor(Color.Blue));

                    target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎", 
                        res.TextFormats[36],  rectangle, res.GetColor(Color.Blue), 
                        DrawTextOptions.EnableColorFont);

                    target.DrawText("FPS: " + timer.FramesPerSecond.ToString("F1"), 
                        bottomRightFont, rectangle, res.GetColor(Color.Red));

                    target.DrawText("Hello World",
                        bottomLeftFont, rectangle, res.GetColor(Color.Purple));
                }
            }
        }
    }
}
