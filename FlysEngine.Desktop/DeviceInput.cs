using System;
using Vortice.DirectInput;

namespace FlysEngine.Desktop;

/// <summary>
/// Represents a device input object that manages keyboard and mouse states and input devices.
/// </summary>
public class DeviceInput : IDisposable
{
    private IDirectInput8 DirectInput { get; } = DInput.DirectInput8Create();

    private IDirectInputDevice8 Keyboard { get; }

    /// <summary>
    /// Keyboard state information.
    /// </summary>
    public KeyboardState KeyboardState = new();

    private IDirectInputDevice8 Mouse { get; }

    /// <summary>
    /// Mouse state information.
    /// </summary>
    public MouseState MouseState = new();

    /// <summary>
    /// Constructs a DeviceInput object that initializes a keyboard and mouse device.
    /// </summary>
    public DeviceInput()
    {
        Keyboard = DirectInput.CreateDevice(PredefinedDevice.SysKeyboard);
        Keyboard.SetDataFormat<RawKeyboardState>();
        Keyboard.Acquire().CheckError();

        Mouse = DirectInput.CreateDevice(PredefinedDevice.SysMouse);
        Mouse.SetDataFormat<RawMouseState>();
        Mouse.Acquire().CheckError();
    }

    /// <summary>
    /// Updates the keyboard state information.
    /// </summary>
    public void UpdateKeyboard() => Keyboard.GetCurrentKeyboardState(ref KeyboardState);

    /// <summary>
    /// Updates the mouse state information.
    /// </summary>
    public void UpdateMouse() => Mouse.GetCurrentMouseState(ref MouseState);

    /// <summary>
    /// Updates the keyboard and mouse state information.
    /// </summary>
    public void UpdateAll()
    {
        UpdateKeyboard();
        UpdateMouse();
    }

    /// <summary>
    /// Clears the keyboard state information.
    /// </summary>
    public void ClearKeyboard()
    {
        KeyboardState = new();
    }

    /// <summary>
    /// Cleans up all resources associated with the keyboard and mouse input devices.
    /// </summary>
    public void Dispose()
    {
        Keyboard.Dispose();
        Mouse.Dispose();
        DirectInput.Dispose();

        GC.SuppressFinalize(this);
    }
}
