
using InControl;
using QuickUnityTools.Input;
using UnityEngine;

namespace ModdingAPI.KeyBind;
using InKey = InControl.Key;

internal static class InControlManager
{
    private static bool IsViewButtonBinded()
    {
        var source = new DeviceBindingSource(InputControlType.View);
        GameActionSet set = GameUserInput.sharedActionSet;
        return (
            set.button1.HasBinding(source) ||
            set.button2.HasBinding(source) ||
            set.button3.HasBinding(source) ||
            set.button4.HasBinding(source) ||
            set.leftBumper.HasBinding(source) ||
            set.rightBumper.HasBinding(source)
        );
    }
    private static InputControlType GetInputControlOfViewButton()
    {
        return IsViewButtonBinded() ? InputControlType.View : InputControlType.Command;
    }
    public static BindingSource? ToInputControlType(KeyCode code)
    {
        InputControlType? i = code switch
        {
            KeyCode.JoystickButton0 or KeyCode.Joystick1Button0 => InputControlType.Action1,
            KeyCode.JoystickButton1 or KeyCode.Joystick1Button1 => InputControlType.Action2,
            KeyCode.JoystickButton2 or KeyCode.Joystick1Button2 => InputControlType.Action3,
            KeyCode.JoystickButton3 or KeyCode.Joystick1Button3 => InputControlType.Action4,
            KeyCode.JoystickButton4 or KeyCode.Joystick1Button4 => InputControlType.LeftBumper,
            KeyCode.JoystickButton5 or KeyCode.Joystick1Button5 => InputControlType.RightBumper,
            KeyCode.JoystickButton6 or KeyCode.Joystick1Button6 => GetInputControlOfViewButton(),
            KeyCode.JoystickButton7 or KeyCode.Joystick1Button7 => InputControlType.Command,
            _ => null,
        };
        if (i == null) return null;
        else return new DeviceBindingSource((InputControlType)i!);
    }
    public static BindingSource? ToKey(KeyCode code)
    {
        InKey? k = code switch
        {
            KeyCode.None => InKey.None,
            KeyCode.Backspace => InKey.Backspace,
            KeyCode.Delete => InKey.Delete,
            KeyCode.Tab => InKey.Tab,
            KeyCode.Return => InKey.Return,
            KeyCode.Escape => InKey.Escape,
            KeyCode.Space => InKey.Space,
            KeyCode.Keypad0 => InKey.Pad0,
            KeyCode.Keypad1 => InKey.Pad1,
            KeyCode.Keypad2 => InKey.Pad2,
            KeyCode.Keypad3 => InKey.Pad3,
            KeyCode.Keypad4 => InKey.Pad4,
            KeyCode.Keypad5 => InKey.Pad5,
            KeyCode.Keypad6 => InKey.Pad6,
            KeyCode.Keypad7 => InKey.Pad7,
            KeyCode.Keypad8 => InKey.Pad8,
            KeyCode.Keypad9 => InKey.Pad9,
            KeyCode.KeypadPeriod => InKey.PadPeriod,
            KeyCode.KeypadDivide => InKey.PadDivide,
            KeyCode.KeypadMultiply => InKey.PadMultiply,
            KeyCode.KeypadMinus => InKey.PadMinus,
            KeyCode.KeypadPlus => InKey.PadPlus,
            KeyCode.KeypadEnter => InKey.PadEnter,
            KeyCode.KeypadEquals => InKey.PadEquals,
            KeyCode.UpArrow => InKey.UpArrow,
            KeyCode.DownArrow => InKey.DownArrow,
            KeyCode.RightArrow => InKey.RightArrow,
            KeyCode.LeftArrow => InKey.LeftArrow,
            KeyCode.Insert => InKey.Insert,
            KeyCode.Home => InKey.Home,
            KeyCode.End => InKey.End,
            KeyCode.PageUp => InKey.PageUp,
            KeyCode.PageDown => InKey.PageDown,
            KeyCode.F1 => InKey.F1,
            KeyCode.F2 => InKey.F2,
            KeyCode.F3 => InKey.F3,
            KeyCode.F4 => InKey.F4,
            KeyCode.F5 => InKey.F5,
            KeyCode.F6 => InKey.F6,
            KeyCode.F7 => InKey.F7,
            KeyCode.F8 => InKey.F8,
            KeyCode.F9 => InKey.F9,
            KeyCode.F10 => InKey.F10,
            KeyCode.F11 => InKey.F11,
            KeyCode.F12 => InKey.F12,
            KeyCode.Alpha0 => InKey.Key0,
            KeyCode.Alpha1 => InKey.Key1,
            KeyCode.Alpha2 => InKey.Key2,
            KeyCode.Alpha3 => InKey.Key3,
            KeyCode.Alpha4 => InKey.Key4,
            KeyCode.Alpha5 => InKey.Key5,
            KeyCode.Alpha6 => InKey.Key6,
            KeyCode.Alpha7 => InKey.Key7,
            KeyCode.Alpha8 => InKey.Key8,
            KeyCode.Alpha9 => InKey.Key9,
            KeyCode.Quote => InKey.Quote,
            KeyCode.Comma => InKey.Comma,
            KeyCode.Minus => InKey.Minus,
            KeyCode.Period => InKey.Period,
            KeyCode.Slash => InKey.Slash,
            KeyCode.Semicolon => InKey.Semicolon,
            KeyCode.Less => InKey.None,
            KeyCode.Equals => InKey.Equals,
            KeyCode.LeftBracket => InKey.LeftBracket,
            KeyCode.Backslash => InKey.Backslash,
            KeyCode.RightBracket => InKey.RightBracket,
            KeyCode.BackQuote => InKey.Backquote,
            KeyCode.A => InKey.A,
            KeyCode.B => InKey.B,
            KeyCode.C => InKey.C,
            KeyCode.D => InKey.D,
            KeyCode.E => InKey.E,
            KeyCode.F => InKey.F,
            KeyCode.G => InKey.G,
            KeyCode.H => InKey.H,
            KeyCode.I => InKey.I,
            KeyCode.J => InKey.J,
            KeyCode.K => InKey.K,
            KeyCode.L => InKey.L,
            KeyCode.M => InKey.M,
            KeyCode.N => InKey.N,
            KeyCode.O => InKey.O,
            KeyCode.P => InKey.P,
            KeyCode.Q => InKey.Q,
            KeyCode.R => InKey.R,
            KeyCode.S => InKey.S,
            KeyCode.T => InKey.T,
            KeyCode.U => InKey.U,
            KeyCode.V => InKey.V,
            KeyCode.W => InKey.W,
            KeyCode.X => InKey.X,
            KeyCode.Y => InKey.Y,
            KeyCode.Z => InKey.Z,
            KeyCode.Numlock => InKey.Numlock,
            KeyCode.CapsLock => InKey.CapsLock,
            KeyCode.RightShift => InKey.RightShift,
            KeyCode.LeftShift => InKey.LeftShift,
            KeyCode.RightControl => InKey.RightControl,
            KeyCode.LeftControl => InKey.LeftControl,
            KeyCode.RightAlt => InKey.RightAlt,
            KeyCode.LeftAlt => InKey.LeftAlt,
            KeyCode.LeftCommand => InKey.LeftCommand,
            KeyCode.RightCommand => InKey.RightCommand,
            KeyCode.AltGr => InKey.AltGr,
            _ => null
        };
        if (k == null) return null;
        else return new KeyBindingSource((InKey)k!);
    }
    private const float WatchBindedInControlTypeTimeSpan = 3.0f;
    private static float watchBindedInControlTypeLastTime = 0f;
    public static void WatchBindedInputControlType()
    {
        if (Time.time - watchBindedInControlTypeLastTime < WatchBindedInControlTypeTimeSpan) return;
        watchBindedInControlTypeLastTime = Time.time;
        GameActionSet set = GameUserInput.sharedActionSet;
        foreach (InputControlType k in Enum.GetValues(typeof(InputControlType)))
        {
            var source = new DeviceBindingSource(k);
            if (set.menuButton.HasBinding(source)) Monitor.SLog($"=== menuButton -> {k}", LogLevel.Warning);
            if (set.button1.HasBinding(source)) Monitor.SLog($"=== button1 -> {k}", LogLevel.Warning);
            if (set.button2.HasBinding(source)) Monitor.SLog($"=== button2 -> {k}", LogLevel.Warning);
            if (set.button3.HasBinding(source)) Monitor.SLog($"=== button3 -> {k}", LogLevel.Warning);
            if (set.button4.HasBinding(source)) Monitor.SLog($"=== button4 -> {k}", LogLevel.Warning);
            if (set.leftBumper.HasBinding(source)) Monitor.SLog($"=== leftBumper -> {k}", LogLevel.Warning);
            if (set.rightBumper.HasBinding(source)) Monitor.SLog($"=== rightBumper -> {k}", LogLevel.Warning);
        }
    }
    internal static bool PressedViewButtonAsOtherThanMenuButton()
    {
        return (
            IsViewButtonBinded() &&
            ButtonMap.ViewButtons.Any(k =>
                k.KeyDown(true) || k.KeyHold(true)
            )
        );
    }
    public static bool IsViewButton(Key key)
    {
        return ButtonMap.viewButtons.Contains(key);
    }
    public static bool TryToGetButton(Key key, out Button button)
    {
        button = Button.Button1;
        if (key.TryGetButton(out var bt)) { button = bt; return true; }
        BindingSource? source = null;
        if (key.TryGetTrigger(out var trigger))
        {
            var triggerType = trigger == Trigger.LTrigger ? InputControlType.LeftTrigger : InputControlType.RightTrigger;
            source = new DeviceBindingSource(triggerType);
        }
        else if (key.TryGetCType(out var ctype))
        {
            source = new DeviceBindingSource(ctype);
        }
        else if (key.TryGetCode(out var code))
        {
            source = ToInputControlType(code) ?? ToKey(code);
        }
        if (source == null) return false;
        GameActionSet set = GameUserInput.sharedActionSet;
        Button? b = null;
        if (set.button1.HasBinding(source)) b = Button.Button1;
        else if (set.button2.HasBinding(source)) b = Button.Button2;
        else if (set.button3.HasBinding(source)) b = Button.Button3;
        else if (set.button4.HasBinding(source)) b = Button.Button4;
        else if (set.menuButton.HasBinding(source)) b = Button.MenuButton;
        else if (set.leftBumper.HasBinding(source)) b = Button.LeftBumper;
        else if (set.rightBumper.HasBinding(source)) b = Button.RightBumper;
        if (b != null)
        {
            button = (Button)b;
            return true;
        }
        else return false;
    }
}

