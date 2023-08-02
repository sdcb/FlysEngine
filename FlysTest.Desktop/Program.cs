using FlysEngine;
using FlysEngine.Desktop;
using System;
using System.Drawing;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;
using FontStyle = Vortice.DirectWrite.FontStyle;

namespace FlysTest.Desktop;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        using LayeredRenderWindow window = new () { Text = "Hello World", DragMoveEnabled = true };
        IDWriteTextFormat bottomRightFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", 16.0f);
        bottomRightFont.FlowDirection = FlowDirection.BottomToTop;
        bottomRightFont.TextAlignment = TextAlignment.Trailing;

        IDWriteTextFormat bottomLeftFont = window.XResource.DWriteFactory.CreateTextFormat("Consolas", FontWeight.Normal, FontStyle.Italic, FontStretch.Normal, 24.0f);
        bottomLeftFont.FlowDirection = FlowDirection.BottomToTop;
        bottomLeftFont.TextAlignment = TextAlignment.Leading;

        window.Draw += Draw;
        RenderLoop.Run(window, () => window.Render(1, 0));

        void Draw(RenderWindow _, ID2D1DeviceContext target)
        {
            XResource res = window.XResource;
            target.Clear(Colors.Transparent);
            RectangleF rectangle = new(0, 0, target.Size.Width, target.Size.Height);

            target.DrawRectangle(
                rectangle,
                res.GetColor(Colors.Blue));

            target.DrawText("😀😁😂🤣😃😄😅😆😉😊😋😎",
                res.TextFormats[36], rectangle, res.GetColor(Colors.Blue),
                DrawTextOptions.EnableColorFont);

            target.DrawText($"{window.XResource.DurationSinceStart:mm':'ss'.'ff}\nFPS: {window.RenderTimer.FramesPerSecond:F1}",
                bottomRightFont, rectangle, res.GetColor(Colors.Red));

            target.DrawText("Hello World",
                bottomLeftFont, rectangle, res.GetColor(Colors.Purple));
        }
    }
}
