
using HarmonyLib;
using ModdingAPI.KeyBind;
using QuickUnityTools.Input;
using UnityEngine;

namespace ModdingAPI;

public class KeyInfo
{
#pragma warning disable IDE1006
    public readonly KeyCode code;
    public bool shift { get => leftShift || rightShift; }
    public bool ctrl { get => leftCtrl || rightCtrl; }
    public bool alt { get => leftAlt || rightAlt; }
    public bool leftShift { get; internal set; } = false;
    public bool rightShift { get; internal set; } = false;
    public bool leftCtrl { get; internal set; } = false;
    public bool rightCtrl { get; internal set; } = false;
    public bool leftAlt { get; internal set; } = false;
    public bool rightAlt { get; internal set; } = false;
#pragma warning restore IDE1006
    public KeyInfo()
    {
        code = KeyCode.None;
    }
    public KeyInfo(KeyCode code)
    {
        this.code = code;
    }
    public KeyInfo(KeyCode code, KeyInfo from)
    {
        this.code = code;
        leftShift = from.leftShift;
        rightShift = from.rightShift;
        leftCtrl = from.leftCtrl;
        rightCtrl = from.rightCtrl;
        leftAlt = from.leftAlt;
        rightAlt = from.rightAlt;
    }
}

public class InputInterceptor
{
    internal static bool isInPrefix = false;

    public static readonly int ActiveKeyTimeout = 2;
    internal static HashSet<Key> holdingKeys = [];
    internal static HashSet<Key> unholdKeys = [];
    internal static HashSet<Button> unholdButton = [];
    internal static Dictionary<Key, int> acitveKeys = [];
    internal static void SetHoldKeys(HashSet<Key> key)
    {
        holdingKeys = [.. key];
    }
    internal static void AddActiveKey(Key key)
    {
        acitveKeys[key] = ActiveKeyTimeout;
    }
    internal static void AddUnholdKey(Key key)
    {
        unholdKeys.Add(key);
        if (key.TryGetButton(out var button)) unholdButton.Add(button);
        else if (key.TryGetTrigger(out var trigger))
        {
            unholdButton.Add(trigger == Trigger.LTrigger ? Button.LeftBumper : Button.RightBumper);
        }
        else if (key.TryGetCode(out var code) && InControlManager.TryToGetButton(code, out var b))
        {
            unholdButton.Add(b);
        }
    }
    private static void UpdateActiveKey()
    {
        foreach (var k in acitveKeys.Keys.ToList())
        {
            acitveKeys[k]--;
            if (acitveKeys[k] <= 0) acitveKeys.Remove(k);
        }
    }
    public static bool IsIntercepted(KeyCode key)
    {
        return holdingKeys.Contains(key) || acitveKeys.ContainsKey(key);
    }
    public static bool IsIntercepted(Button button)
    {
        foreach (var k in holdingKeys)
        {
            if (InControlManager.TryToGetButton(k, out var b) && b == button) return true;
        }
        foreach (var k in acitveKeys.Keys)
        {
            if (InControlManager.TryToGetButton(k, out var b) && b == button) return true;
        }
        return false;
    }

    internal static void Update()
    {
        Key.UpdateButtonState();
        if (enabledAll) UpdateAll();
        else
        {
            unholdKeys.Clear();
            unholdButton.Clear();
            UpdateActiveKey();
            KeyBind.KeyBind.Update();
        }
    }

    public class Callbacks(Action<KeyInfo>? onKeydown = null, Action<KeyInfo>? onKeyhold = null, Action<KeyInfo>? onKeyup = null)
    {
        public Action<KeyInfo>? onKeydown = onKeydown;
        public Action<KeyInfo>? onKeyhold = onKeyhold;
        public Action<KeyInfo>? onKeyup = onKeyup;
    }
    private static int justEnabledAllCountDown = 0;
    private static int disableAllCountDown = 0;
    private static event Action? onDisabledAll = null;
    public static bool enabledAll { get; private set; } = false;
    private static bool handleEscapeKey = false;
    private static Array? codes = null;
    private static Callbacks? masterAction = null;
    private static readonly Dictionary<KeyCode, float> holdTime = [];
    private static readonly float activateTime = 1f;
    public static void EnableAll(Callbacks action, Action? onDisabled = null, bool handleEscapeKey = false)
    {
        if (enabledAll) return;
        EnableAllBase(action, onDisabled, handleEscapeKey);
    }
    public static void EnableAll(HashSet<KeyCode> codes, Callbacks action, Action? onDisabled = null, bool handleEscapeKey = false)
    {
        if (enabledAll) return;
        EnableAllBase(action, onDisabled, handleEscapeKey);
        InputInterceptor.codes = codes.ToArray();
    }
    private static void EnableAllBase(Callbacks action, Action? onDisabled, bool handleEscapeKey)
    {
        KeyBind.KeyBind.ResetAll();
        holdingKeys.Clear();
        acitveKeys.Clear();
        justEnabledAllCountDown = 2;
        enabledAll = true;
        onDisabledAll += onDisabled;
        InputInterceptor.handleEscapeKey = handleEscapeKey;
        masterAction = action;
    }
    public static void DisableAll()
    {
        if (disableAllCountDown <= 0) disableAllCountDown = 2;
    }
    public static void DisableAll(Action onDisabled)
    {
        InputInterceptor.onDisabledAll += onDisabled;
        DisableAll();
    }
    private static void UpdateAll()
    {
        if (!enabledAll) return;
        if (justEnabledAllCountDown > 0)
        {
            justEnabledAllCountDown--;
            return;
        }
        if (disableAllCountDown > 0)
        {
            disableAllCountDown--;
            if (disableAllCountDown == 0)
            {
                enabledAll = false;
                handleEscapeKey = false;
                codes = null;
                masterAction = null;
                onDisabledAll?.Invoke();
                onDisabledAll = null;
            }
            return;
        }
        if (!handleEscapeKey && Input.GetKey(KeyCode.Escape)) DisableAll();
        if (masterAction == null) return;
        KeyInfo modifier = new()
        {
            leftShift = Input.GetKey(KeyCode.LeftShift),
            rightShift = Input.GetKey(KeyCode.RightShift),
            leftCtrl = Input.GetKey(KeyCode.LeftControl),
            rightCtrl = Input.GetKey(KeyCode.RightControl),
            leftAlt = Input.GetKey(KeyCode.LeftAlt),
            rightAlt = Input.GetKey(KeyCode.RightAlt),
        };
        List<KeyCode> downKeys = [];
        List<KeyCode> holdKeys = [];
        List<KeyCode> upKeys = [];
        foreach (KeyCode code in codes ?? Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(code))
            {
                downKeys.Add(code);
                if (!holdKeys.Contains(code)) holdTime[code] = 0;
                holdTime[code] += Time.deltaTime;
            }
            else if (Input.GetKey(code))
            {
                if (!holdKeys.Contains(code)) holdTime[code] = 0;
                if (holdTime[code] > activateTime)
                {
                    holdKeys.Add(code);
                }
                else
                {
                    holdTime[code] += Time.deltaTime;
                }
            }
            else if (Input.GetKeyUp(code))
            {
                upKeys.Add(code);
                holdTime[code] = 0;
            }
            else
            {
                holdTime[code] = 0;
            }
        }
        foreach (var k in downKeys) masterAction.onKeydown?.Invoke(new(k, modifier));
        foreach (var k in holdKeys) masterAction.onKeyhold?.Invoke(new(k, modifier));
        foreach (var k in upKeys) masterAction.onKeyup?.Invoke(new(k, modifier));
    }
}

[HarmonyPatch(typeof(GameUserInput))]
internal class FocusableUserInputPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("GetRawButton")]
    public static bool GetRawButton(Button button, ref ButtonState __result)
    {
        if (InputInterceptor.isInPrefix) return true;
        if (InputInterceptor.enabledAll)
        {
            __result = default;
            return false;
        }
        if (InputInterceptor.IsIntercepted(button))
        {
            __result = default;
            return false;
        }
        if (button == Button.MenuButton && InControlManager.PressedViewButtonAsOtherThanMenuButton())
        {
            __result = default;
            return false;
        }
        if (InputInterceptor.unholdButton.Contains(button))
        {
            var set = GameUserInput.sharedActionSet;
            var action = button switch
            {
                Button.Button1 => set.button1,
                Button.Button2 => set.button2,
                Button.Button3 => set.button3,
                Button.Button4 => set.button4,
                Button.MenuButton => set.menuButton,
                Button.LeftBumper => set.leftBumper,
                Button.RightBumper => set.rightBumper,
                _ => throw new Exception()
            };
            __result = new ButtonState(button, true, true, set.GetButtonName(action));
            return false;
        }
        return true;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("GetRawStick")]
    public static bool GetRawStick(Stick stick, ref StickState __result)
    {
        if (InputInterceptor.isInPrefix) return true;
        if (InputInterceptor.enabledAll)
        {
            __result = default;
            return false;
        }
        return true;
    }
}

