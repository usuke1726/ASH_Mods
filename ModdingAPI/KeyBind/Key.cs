
using BepInEx;
using InControl;
using QuickUnityTools.Input;
using UnityEngine;

namespace ModdingAPI.KeyBind;

public enum Trigger
{
    LTrigger,
    RTrigger,
}
public class Key
{
    private enum KeyType
    {
        None, KeyCode, Button, Trigger, ControlType
    }
    private readonly KeyType type;
    private readonly KeyCode? code = null;
    private readonly Button? button = null;
    private readonly Trigger? trigger = null;
    private readonly InputControlType? ctype = null;
    public Key() { type = KeyType.None; }
    public Key(KeyCode code) { type = KeyType.KeyCode; this.code = code; }
    public Key(Button button) { type = KeyType.Button; this.button = button; }
    public Key(Trigger trigger) { type = KeyType.Trigger; this.trigger = trigger; }
    public Key(InputControlType ctype) { type = KeyType.ControlType; this.ctype = ctype; }
    public static bool TryParse(string name, out Key key)
    {
        key = new();
        if (name.IsNullOrWhiteSpace()) return false;
        var s = name.Trim();
        if (s.StartsWith(":"))
        {
            switch (s.ToLower()[1..])
            {
                case "lefttrigger" or "ltrigger" or "lt":
                    key = new(Trigger.LTrigger); return true;
                case "righttrigger" or "rtrigger" or "rt":
                    key = new(Trigger.RTrigger); return true;
                case "leftbumper" or "lbumper" or "lb":
                    key = new(Button.LeftBumper); return true;
                case "rightbumper" or "rbumper" or "rb":
                    key = new(Button.RightBumper); return true;
                case "button1" or "b1" or "1":
                    key = new(Button.Button1); return true;
                case "button2" or "b2" or "2":
                    key = new(Button.Button2); return true;
                case "button3" or "b3" or "3":
                    key = new(Button.Button3); return true;
                case "button4" or "b4" or "4":
                    key = new(Button.Button4); return true;
                case "menubutton" or "mbutton" or "menu" or "mb" or "m":
                    key = new(Button.MenuButton); return true;
                default: return false;
            }
        }
        else if (s.StartsWith('.'))
        {
            return ButtonMap.Map.TryGetValue(s[1..], out key);
        }
        else if (s.StartsWith('@'))
        {
            try
            {
                key = new((InputControlType)Enum.Parse(typeof(InputControlType), s[1..], true));
                return true;
            }
            catch { return false; }
        }
        else
        {
            try
            {
                key = new((KeyCode)Enum.Parse(typeof(KeyCode), s, true));
                return true;
            }
            catch { return false; }
        }
    }

    public static implicit operator Key(KeyCode obj) => new(obj);
    public static implicit operator Key(Button obj) => new(obj);
    public static implicit operator Key(Trigger obj) => new(obj);
    public static implicit operator Key(InputControlType obj) => new(obj);
    public static bool operator ==(Key left, Key right) => left.GetHashCode() == right.GetHashCode();
    public static bool operator !=(Key left, Key right) => !(left == right);
    public override bool Equals(object obj)
    {
        if (obj is Key key) return this == key;
        if (obj is KeyCode k) return this == (Key)k;
        if (obj is Button b) return this == (Key)b;
        if (obj is Trigger t) return this == (Key)t;
        if (obj is InputControlType c) return this == (Key)c;
        return false;
    }
    public override string ToString()
    {
        try { return $".{ButtonMap.Map.First(pair => pair.Value == this).Key}"; } catch { }
        return type switch
        {
            KeyType.KeyCode => code!.ToString(),
            KeyType.Button => button! switch
            {
                Button.Button1 => ":b1",
                Button.Button2 => ":b2",
                Button.Button3 => ":b3",
                Button.Button4 => ":b4",
                Button.MenuButton => ":menu",
                Button.LeftBumper => ":lb",
                Button.RightBumper => ":rb",
                _ => throw new Exception()
            },
            KeyType.Trigger => trigger! switch
            {
                Trigger.LTrigger => ":lt",
                Trigger.RTrigger => ":rt",
                _ => throw new Exception()
            },
            KeyType.ControlType => $"@{ctype}",
            _ => "{None}"
        };
    }
    public override int GetHashCode()
    {
        return type switch
        {
            KeyType.KeyCode => (int)code!,
            KeyType.Button => 1000 + (int)button!,
            KeyType.Trigger => 2000 + (int)trigger!,
            KeyType.ControlType => 3000 + (int)ctype!,
            _ => -1
        };
    }
    private enum ButtonStates
    {
        None, Down, Hold, Up
    }
    private static readonly Dictionary<Button, ButtonStates> buttonStates = new()
    {
        [Button.Button1] = ButtonStates.None,
        [Button.Button2] = ButtonStates.None,
        [Button.Button3] = ButtonStates.None,
        [Button.Button4] = ButtonStates.None,
        [Button.MenuButton] = ButtonStates.None,
        [Button.LeftBumper] = ButtonStates.None,
        [Button.RightBumper] = ButtonStates.None,
    };
    internal static void UpdateButtonState()
    {
        Dictionary<Button, ButtonStates> prevStates = new(buttonStates);
        buttonStates.Clear();
        var input = Singleton<FocusableUserInputManager>.instance.inputWithFocus;
        foreach (Button button in Enum.GetValues(typeof(Button)))
        {
            var state = input.GetButton(button);
            if (state.wasPressed) buttonStates[button] = ButtonStates.Down;
            else if (state.isPressed) buttonStates[button] = ButtonStates.Hold;
            else if (prevStates[button] == ButtonStates.Down || prevStates[button] == ButtonStates.Hold) buttonStates[button] = ButtonStates.Up;
            else buttonStates[button] = ButtonStates.None;
        }
    }

    //private string ToCol(bool a) => a ? Style.Str("Y", "red") : Style.Str("N", "blue");
    //private void PrintTriggerStates(InputControl control, Trigger? type)
    //{
    //    var t = type == Trigger.LTrigger ? Trigger.LTrigger : Trigger.RTrigger;
    //    Monitor.SLogNoDup($"TriggerButtonStates.{t}", $"Trigger {trigger} (is: {ToCol(control.IsPressed)}, was: {ToCol(control.WasPressed)}, released: {ToCol(control.WasReleased)})");
    //}
    public bool KeyDown()
    {
        switch (type)
        {
            case KeyType.KeyCode:
                return Input.GetKeyDown((KeyCode)code!);
            case KeyType.Button:
                return buttonStates[(Button)button!] == ButtonStates.Down;
            case KeyType.Trigger:
                var device = InputManager.ActiveDevice;
                var control = trigger == Trigger.LTrigger ? device.LeftTrigger : device.RightTrigger;
                return control.WasPressed;
            case KeyType.ControlType:
                var cTypeControl = InputManager.ActiveDevice.GetControl((InputControlType)ctype!);
                return cTypeControl.WasPressed;
            default: return false;
        }
    }
    public bool KeyHold()
    {
        switch (type)
        {
            case KeyType.KeyCode:
                return Input.GetKey((KeyCode)code!);
            case KeyType.Button:
                return buttonStates[(Button)button!] == ButtonStates.Hold;
            case KeyType.Trigger:
                var device = InputManager.ActiveDevice;
                var control = trigger == Trigger.LTrigger ? device.LeftTrigger : device.RightTrigger;
                return control.IsPressed && !control.WasPressed;
            case KeyType.ControlType:
                var cTypeControl = InputManager.ActiveDevice.GetControl((InputControlType)ctype!);
                return cTypeControl.IsPressed && !cTypeControl.WasPressed;
            default: return false;
        }
    }
    public bool KeyUp()
    {
        switch (type)
        {
            case KeyType.KeyCode:
                return Input.GetKeyUp((KeyCode)code!);
            case KeyType.Button:
                return buttonStates[(Button)button!] == ButtonStates.Up;
            case KeyType.Trigger:
                var device = InputManager.ActiveDevice;
                var control = trigger == Trigger.LTrigger ? device.LeftTrigger : device.RightTrigger;
                return control.WasReleased;
            case KeyType.ControlType:
                var cTypeControl = InputManager.ActiveDevice.GetControl((InputControlType)ctype!);
                return cTypeControl.WasReleased;
            default: return false;
        }
    }

    public bool TryGetCode(out KeyCode code)
    {
        if (type == KeyType.KeyCode)
        {
            code = (KeyCode)this.code!;
            return true;
        }
        else
        {
            code = KeyCode.None;
            return false;
        }
    }
    public bool TryGetButton(out Button button)
    {
        if (type == KeyType.Button)
        {
            button = (Button)this.button!;
            return true;
        }
        else
        {
            button = Button.Button1;
            return false;
        }
    }
    public bool TryGetTrigger(out Trigger trigger)
    {
        if (type == KeyType.Trigger)
        {
            trigger = (Trigger)this.trigger!;
            return true;
        }
        else
        {
            trigger = Trigger.LTrigger;
            return false;
        }
    }
    public bool TryGetCType(out InputControlType ctype)
    {
        if (type == KeyType.ControlType)
        {
            ctype = (InputControlType)this.ctype!;
            return true;
        }
        else
        {
            ctype = InputControlType.None;
            return false;
        }
    }
    internal bool KeyDown(bool backdoor)
    {
        if (backdoor && type == KeyType.KeyCode) return UnityEngine.Input.GetKeyDown((KeyCode)code!);
        return KeyDown();
    }
    internal bool KeyHold(bool backdoor)
    {
        if (backdoor && type == KeyType.KeyCode) return UnityEngine.Input.GetKey((KeyCode)code!);
        return KeyHold();
    }
    internal bool KeyUp(bool backdoor)
    {
        if (backdoor && type == KeyType.KeyCode) return UnityEngine.Input.GetKeyUp((KeyCode)code!);
        return KeyUp();
    }
}

