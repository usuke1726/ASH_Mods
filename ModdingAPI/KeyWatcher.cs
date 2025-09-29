
using QuickUnityTools.Input;
using UnityEngine;

namespace ModdingAPI;

public enum ArrowKey
{
    Up,
    Down,
    Left,
    Right,
}
public enum Arrow8Key
{
    Up, Down, Left, Right,
    UpLeft, DownLeft, UpRight, DownRight
};

public class KeyWatcher : IDisposable
{
    private static HashSet<KeyWatcher> watchers = [];
    private class Callbacks(Action? onKeydown, Action? onKeyhold, Action? onKeyup)
    {
        public Action? onKeydown = onKeydown;
        public Action? onKeyhold = onKeyhold;
        public Action? onKeyup = onKeyup;
    }
    private readonly Dictionary<KeyCode, Callbacks> codeCallbacks = [];
    private readonly Dictionary<ArrowKey, Callbacks> arrowCallbacks = [];
    private readonly Dictionary<Arrow8Key, Callbacks> arrow8Callbacks = [];
    public KeyWatcher()
    {
        watchers.Add(this);
    }
    public void Watch(KeyCode code,
        Action? onKeydown = null,
        Action? onKeyhold = null,
        Action? onKeyup = null)
    {
        codeCallbacks[code] = new(onKeydown, onKeyhold, onKeyup);
        UpdateRegisteredCodes();
    }
    public void Watch(ArrowKey ar,
        Action? onKeydown = null,
        Action? onKeyhold = null,
        Action? onKeyup = null) => arrowCallbacks[ar] = new(onKeydown, onKeyhold, onKeyup);
    public void Watch(Arrow8Key ar,
        Action? onKeydown = null,
        Action? onKeyhold = null,
        Action? onKeyup = null) => arrow8Callbacks[ar] = new(onKeydown, onKeyhold, onKeyup);

    public void Unwatch(KeyCode code)
    {
        codeCallbacks.Remove(code);
        UpdateRegisteredCodes();
    }
    public void Unwatch(ArrowKey ar) => arrowCallbacks.Remove(ar);
    public void Unwatch(Arrow8Key ar) => arrow8Callbacks.Remove(ar);

    private static HashSet<KeyCode> registeredCodes = [];
    private static void UpdateRegisteredCodes()
    {
        registeredCodes.Clear();
        foreach (var w in watchers) registeredCodes.UnionWith(w.codeCallbacks.Keys);
    }

    private static void Invoke(KeyCode code, Action<Callbacks> action)
    {
        foreach (var w in watchers)
        {
            if (w.codeCallbacks.TryGetValue(code, out var c)) action(c);
        }
    }
    private static void Invoke(ArrowKey ar, Action<Callbacks> action)
    {
        foreach (var w in watchers)
        {
            if (w.arrowCallbacks.TryGetValue(ar, out var c)) action(c);
        }
    }
    private static void Invoke(Arrow8Key ar, Action<Callbacks> action)
    {
        foreach (var w in watchers)
        {
            if (w.arrow8Callbacks.TryGetValue(ar, out var c)) action(c);
        }
    }
    internal static void Update()
    {
        if (InputInterceptor.enabledAll) return;
        foreach (var code in registeredCodes)
        {
            if (Input.GetKeyDown(code)) Invoke(code, c => c.onKeydown?.Invoke());
            if (Input.GetKey(code)) Invoke(code, c => c.onKeyhold?.Invoke());
            if (Input.GetKeyUp(code)) Invoke(code, c => c.onKeyup?.Invoke());
        }
        var input = Singleton<FocusableUserInputManager>.instance.inputWithFocus;
        UpdateLStick(input.leftStick);
    }

    private class ArrowHoldHandler(ArrowKey ar)
    {
        public static ArrowHoldHandler up = new(ArrowKey.Up);
        public static ArrowHoldHandler down = new(ArrowKey.Down);
        public static ArrowHoldHandler left = new(ArrowKey.Left);
        public static ArrowHoldHandler right = new(ArrowKey.Right);
        public float fastScrollHeldTime = 0;
        public bool lastPressed = false;
        public bool readyToKeydown = false;
        public bool readyToKeyup = false;
        public ArrowKey dir = ar;
        public static bool All8DirFixed(HashSet<ArrowKey> dir) => (
            dir.Count > 0 &&
            up.lastPressed == dir.Contains(ArrowKey.Up) &&
            down.lastPressed == dir.Contains(ArrowKey.Down) &&
            left.lastPressed == dir.Contains(ArrowKey.Left) &&
            right.lastPressed == dir.Contains(ArrowKey.Right)
        );
        public static HashSet<ArrowKey> LastHeldKeys()
        {
            HashSet<ArrowKey> ret = [];
            if (up.lastPressed) ret.Add(ArrowKey.Up);
            if (down.lastPressed) ret.Add(ArrowKey.Down);
            if (left.lastPressed) ret.Add(ArrowKey.Left);
            if (right.lastPressed) ret.Add(ArrowKey.Right);
            return ret;
        }
        private static void Update(ArrowHoldHandler h, bool isHolding)
        {
            if (!h.lastPressed && isHolding) h.readyToKeydown = true;
            if (h.lastPressed && !isHolding) h.readyToKeyup = true;
            h.lastPressed = isHolding;
            if (isHolding) h.fastScrollHeldTime += Time.deltaTime;
            else h.fastScrollHeldTime = 0;
        }
        public static void UpdateArrow4(HashSet<ArrowKey> dir)
        {
            Update(up, dir.Contains(ArrowKey.Up));
            Update(down, dir.Contains(ArrowKey.Down));
            Update(left, dir.Contains(ArrowKey.Left));
            Update(right, dir.Contains(ArrowKey.Right));
        }
    }
    private static float fastArrow8HeldTime = 0;
    public static readonly float fastArrowActivateTime = 0.5f;
    private static readonly float fastArrowStepTime = 0.1f;
    private static ArrowKey DotToArr(float dot, bool isUpDown)
    {
        if (isUpDown)
        {
            return dot > 0 ? ArrowKey.Down : ArrowKey.Up;
        }
        else
        {
            return dot > 0 ? ArrowKey.Left : ArrowKey.Right;
        }
    }
    private static Arrow8Key ToArrow8(HashSet<ArrowKey> dir)
    {
        int updown = dir.Contains(ArrowKey.Up) ? 1 : dir.Contains(ArrowKey.Down) ? -1 : 0;
        int lr = dir.Contains(ArrowKey.Left) ? 1 : dir.Contains(ArrowKey.Right) ? -1 : 0;
        return (updown, lr) switch
        {
            (1, 0) => Arrow8Key.Up,
            (-1, 0) => Arrow8Key.Down,
            (0, 1) => Arrow8Key.Left,
            (0, -1) => Arrow8Key.Right,
            (1, 1) => Arrow8Key.UpLeft,
            (1, -1) => Arrow8Key.UpRight,
            (-1, 1) => Arrow8Key.DownLeft,
            (-1, -1) => Arrow8Key.DownRight,
            _ => throw new Exception()
        };
    }
    private static void TryToInvokeArrow(ArrowHoldHandler h, ArrowKey ar)
    {
        if (h.fastScrollHeldTime > fastArrowActivateTime)
        {
            h.fastScrollHeldTime = fastArrowActivateTime - fastArrowStepTime;
            Invoke(ar, w => w.onKeyhold?.Invoke());
        }
        else if (h.readyToKeydown)
        {
            h.readyToKeydown = false;
            Invoke(ar, w => w.onKeydown?.Invoke());
        }
        else if (h.readyToKeyup)
        {
            h.readyToKeyup = false;
            Invoke(ar, w => w.onKeyup?.Invoke());
        }
    }
    private static void UpdateLStick(StickState s)
    {
        float f1 = Vector2.Dot(Vector2.down, s.vector);
        float f2 = Vector2.Dot(Vector2.left, s.vector);
        HashSet<ArrowKey> dir = (Mathf.Abs(f1) > 0.5f, Mathf.Abs(f2) > 0.5f) switch
        {
            (true, true) => [DotToArr(f1, true), DotToArr(f2, false)],
            (true, false) => [DotToArr(f1, true)],
            (false, true) => [DotToArr(f2, false)],
            _ => [],
        };
        if (ArrowHoldHandler.All8DirFixed(dir))
        {
            fastArrow8HeldTime += Time.deltaTime;
        }
        else
        {
            fastArrow8HeldTime = 0;
            var lastKeys = ArrowHoldHandler.LastHeldKeys();
            if (lastKeys.Count > 0)
            {
                Invoke(ToArrow8(lastKeys), w => w.onKeyup?.Invoke());
            }
            if (dir.Count > 0)
            {
                Invoke(ToArrow8(dir), w => w.onKeydown?.Invoke());
            }
        }
        ArrowHoldHandler.UpdateArrow4(dir);
        if (fastArrow8HeldTime > fastArrowActivateTime)
        {
            fastArrow8HeldTime = fastArrowActivateTime - fastArrowStepTime;
            Invoke(ToArrow8(dir), w => w.onKeyhold?.Invoke());
        }
        TryToInvokeArrow(ArrowHoldHandler.up, ArrowKey.Up);
        TryToInvokeArrow(ArrowHoldHandler.down, ArrowKey.Down);
        TryToInvokeArrow(ArrowHoldHandler.left, ArrowKey.Left);
        TryToInvokeArrow(ArrowHoldHandler.right, ArrowKey.Right);
    }
    private void Dispose(bool isDisposing)
    {
        watchers.Remove(this);
        if (isDisposing) GC.SuppressFinalize(this);
    }
    public void Dispose() => Dispose(true);
    ~KeyWatcher() { Dispose(false); }
}

