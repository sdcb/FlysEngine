using FlysEngine;
using FlysEngine.Managers;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace FlysTest
{
    public partial class Form1 : Form
    {
        private readonly XResource xResource = new XResource();
        private readonly FpsManager fpsManager = new FpsManager();

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (WindowState != FormWindowState.Minimized && xResource.DeviceAvailable)
            {
                xResource.Resize();
            }
        }

        public void Render()
        {
            if (!xResource.DeviceAvailable)
            {
                xResource.InitializeDevice(Handle);
            }

            var dt = fpsManager.BeginFrame();
            {
                xResource.UpdateLogic(dt);
                xResource.RenderTarget.BeginDraw();
                {
                    Draw(xResource.RenderTarget);
                }
                xResource.RenderTarget.EndDraw();
                fpsManager.EndFrame();
                xResource.SwapChain.Present(1, 0);
            }
        }

        private void Draw(DeviceContext renderTarget)
        {
            renderTarget.Clear(Color.CornflowerBlue.ToColor4());
            renderTarget.DrawRectangle(new RectangleF(5, 5, ClientSize.Width - 10, ClientSize.Height - 10),
                xResource.Brushes[Color.Blue]);

            renderTarget.DrawText($"😀 😁 😂 🤣 😃 😄 😅 😆 😉 😊 😋 😎",
                xResource.TextFormats[24.0f],
                new RectangleF(0, 22, renderTarget.Size.Width, float.MaxValue),
                xResource.Brushes[Color.White], 
                DrawTextOptions.EnableColorFont);

            renderTarget.DrawText($"FPS: {fpsManager.Fps}\r\nFT: {fpsManager.FrameTimeMs}",
                xResource.TextFormats[22.0f],
                new RectangleF(0, 0, float.MaxValue, float.MaxValue),
                xResource.Brushes[Color.Purple]);
        }
    }
}
