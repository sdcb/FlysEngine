using System;
using Vortice.DirectInput;

namespace FlysEngine.Desktop
{
    public class DeviceInput : IDisposable
    {
        private IDirectInput8 DirectInput { get; } = DInput.DirectInput8Create();

        private IDirectInputDevice8 Keyboard { get; }

        public KeyboardState KeyboardState = new KeyboardState();

        private IDirectInputDevice8 Mouse { get; }

        public MouseState MouseState = new MouseState();

        public DeviceInput()
        {
            Keyboard = DirectInput.CreateDevice(PredefinedDevice.SysKeyboard);
            Keyboard.Acquire();
            Mouse = DirectInput.CreateDevice(PredefinedDevice.SysMouse);
            Mouse.Acquire();
        }

        public void UpdateKeyboard() => Keyboard.GetCurrentKeyboardState(ref KeyboardState);

        public void UpdateMouse() => Mouse.GetCurrentMouseState(ref MouseState);

        public void UpdateAll()
        {
            UpdateKeyboard();
            UpdateMouse();
        }

        public void Dispose()
        {
            Keyboard.Dispose();
            Mouse.Dispose();
            DirectInput.Dispose();
        }
    }
}
