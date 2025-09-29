
using BepInEx;

namespace ModdingAPI.KeyBind;

public class KeyBind : IComparable<KeyBind>
{
    public class ParseResult
    {
        public readonly bool Success;
        public readonly List<KeyBind> KeyBinds;
        public readonly Action Unregister;
        internal ParseResult(bool success, IEnumerable<KeyBind> keyBinds, Action unregister)
        {
            Success = success;
            KeyBinds = [.. keyBinds];
            Unregister = unregister;
        }
        public static implicit operator bool(ParseResult res) => res.Success;
    }
    public static readonly int FiredTimeout = 30;
    private readonly string name;
    private readonly Action callback;
    private readonly List<KeyBindUnit> units;
    private readonly int unitsNum;
    private HashSet<Key> holdingKeys = [];
    private HashSet<Key> waitReleaseKeys = [];
    private int idx = 0;
    private readonly Key lastTrigger;
    private int firedTimeout = 0;
    private int nextUnitTimeout = 0;
    private bool fired = false;
    private bool nextUnit = false;
    private KeyBind(List<KeyBindUnit> units, Action callback, string name)
    {
        this.name = FormatName(name);
        this.units = units;
        this.callback = callback;
        this.units[0].isFirstUnit = true;
        unitsNum = units.Count;
        lastTrigger = units[unitsNum - 1].trigger;
    }

    public override string ToString() => $"{name} => {ToQuery()}";
    public string ToQuery() => KeyBindUnit.UnitsToString(units);
    private static string FormatName(string name)
    {
        if (name.IsNullOrWhiteSpace()) return "<unspecified name>";
        if (name.Length > 20) return name[0..18] + "..";
        else return name;
    }
    public static bool RegisterKeyBind(KeyBind keybind, out Action unregister)
    {
        keybinds.Add(keybind);
        unregister = () => keybinds.Remove(keybind);
        return true;
    }
    public static bool RegisterKeyBind(KeyBind keybind) => RegisterKeyBind(keybind, out var _);
    public static bool RegisterKeyBind(IKeyBindingsData keybindings, IEnumerable<string> keys, Action callback, out ParseResult result, string name = "", bool allowDefault = false)
    {
        List<KeyBindUnitEntry> entries = [.. keybindings.GetKeyBinds(keys, allowDefault)];
        return RegisterKeyBind(entries, callback, out result, name);
    }
    public static bool RegisterKeyBind(IKeyBindingsData keybindings, string key, Action callback, out ParseResult result, string name = "", bool allowDefault = false) => RegisterKeyBind(keybindings, [key], callback, out result, name, allowDefault);
    public static bool RegisterKeyBind(IKeyBindingsData keybindings, IEnumerable<string> keys, Action callback, string name = "", bool allowDefault = false) => RegisterKeyBind(keybindings, keys, callback, out var _, name, allowDefault);
    public static bool RegisterKeyBind(IKeyBindingsData keybindings, string key, Action callback, string name = "", bool allowDefault = false) => RegisterKeyBind(keybindings, key, callback, out var _, name, allowDefault);
    public static bool RegisterKeyBind(List<KeyBindUnitEntry> entries, Action callback, out ParseResult result, string name = "")
    {
        result = new(false, [], null!);
        if (entries.Count == 0) return false;
        List<KeyBind> keyBinds = [];
        List<string> errors = [];
        foreach (var entry in entries)
        {
            if (entry.TryToGetKeyUnits(out var units, out var error))
            {
                keyBinds.Add(new(units, callback, name));
            }
            else
            {
                errors.Add(error);
            }
        }
        if (errors.Any())
        {
            Monitor.SLog(string.Join("\n", errors), LogLevel.Error);
        }
        if (keyBinds.Any())
        {
            keybinds.AddRange(keyBinds);
            result = new(true, keyBinds, () =>
            {
                foreach (var k in keyBinds) keybinds.Remove(k);
            });
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool RegisterKeyBind(KeyBindUnitEntry entry, Action callback, out ParseResult result, string name = "", List<KeyBindUnitEntry>? aliases = null)
    {
        return RegisterKeyBind([entry, .. (aliases ?? [])], callback, out result, name);
    }
    public static bool RegisterKeyBind(List<KeyBindUnitEntry> entries, Action callback, string name = "") => RegisterKeyBind(entries, callback, out var _, name);
    public static bool RegisterKeyBind(KeyBindUnitEntry entry, Action callback, string name = "", List<KeyBindUnitEntry>? aliases = null) => RegisterKeyBind(entry, callback, out var _, name, aliases);
    private void Invoke() { Reset(); callback(); }
    private void Reset()
    {
        idx = 0;
        fired = false;
        nextUnit = false;
        waitReleaseKeys.Clear();
    }
    private void OnKeyActive(Key key)
    {
        if (fired && lastTrigger == key) Reset();
        if (nextUnit && units[idx - 1].trigger == key) Reset();
    }
    private void UpdateState()
    {
        holdingKeys.Clear();
        nextUnit = false;
        if (fired)
        {
            firedTimeout--;
            if (firedTimeout <= 0) Reset();
            return;
        }
        if (idx > 0)
        {
            nextUnitTimeout--;
            if (nextUnitTimeout <= 0) Reset();
        }
        var unit = units[idx];
        foreach (var key in waitReleaseKeys.ToList())
        {
            if (key.KeyUp()) waitReleaseKeys.Remove(key);
        }
        var isAllHoldKeyHeld = true;
        foreach (var key in unit.hold)
        {
            if (key.KeyHold() || key.KeyDown()) holdingKeys.Add(key);
            else isAllHoldKeyHeld = false;
        }
        var triggerKeyDown = unit.trigger.KeyDown();
        if (triggerKeyDown)
        {
            if (waitReleaseKeys.Any() || !isAllHoldKeyHeld)
            {
                Reset();
                return;
            }
            idx++;
            hold.UnionWith(holdingKeys);
            if (idx == unitsNum)
            {
                idx = 0;
                fired = true;
                // Monitor.SLog($"keybind {this} fired!", LogLevel.Debug);
                firedTimeout = FiredTimeout;
            }
            else
            {
                nextUnit = true;
                waitReleaseKeys = [.. holdingKeys];
                waitReleaseKeys.ExceptWith(units[idx].hold);
                nextUnitTimeout = units[idx].within;
            }
        }
        else
        {
            hold.UnionWith(holdingKeys);
        }
    }

    private static List<KeyBind> keybinds = [];
    private static Dictionary<Key, int> prevHold = [];
    private static HashSet<Key> holdOnInvoked = [];
    private static HashSet<Key> hold = [];
    private static HashSet<Key> unhold = [];
    internal static void ResetAll()
    {
        foreach (var keybind in keybinds) keybind.Reset();
    }
    internal static void Update()
    {
        foreach (var key in prevHold.Keys.ToList())
        {
            if (prevHold[key] > 0) prevHold[key]--;
        }
        foreach (var key in hold)
        {
            if (!prevHold.ContainsKey(key)) prevHold[key] = FiredTimeout;
        }
        hold.Clear();
        Dictionary<Key, List<KeyBind>> firedBinds = [];
        foreach (var keybind in keybinds)
        {
            keybind.UpdateState();
            if (keybind.fired)
            {
                if (firedBinds.ContainsKey(keybind.lastTrigger))
                {
                    firedBinds[keybind.lastTrigger].Add(keybind);
                }
                else
                {
                    firedBinds[keybind.lastTrigger] = [keybind];
                }
            }
        }
        InputInterceptor.SetHoldKeys(hold);
        unhold = [.. prevHold.Keys];
        unhold.ExceptWith(hold);
        foreach (var key in unhold)
        {
            var timeout = prevHold[key];
            prevHold.Remove(key);
            if (!hold.Any() && timeout > 0)
            {
                if (firedBinds.ContainsKey(key))
                {
                    InputInterceptor.AddActiveKey(key);
                    var triggerdBind = TopPriority(firedBinds[key]);
                    foreach (var keybind in keybinds) keybind.OnKeyActive(key);
                    holdOnInvoked = [.. hold];
                    triggerdBind.Invoke();
                    firedBinds.Remove(key);
                }
                else if (holdOnInvoked.Contains(key))
                {
                    holdOnInvoked.Remove(key);
                }
                else
                {
                    InputInterceptor.AddUnholdKey(key);
                }
            }
        }
        foreach (var key in firedBinds.Keys)
        {
            if (hold.Contains(key)) continue;
            var triggerdBind = TopPriority(firedBinds[key]);
            foreach (var keybind in keybinds) keybind.OnKeyActive(key);
            InputInterceptor.AddActiveKey(key);
            triggerdBind.Invoke();
            holdOnInvoked = [.. hold];
        }
    }
    public static void ListKeyBinds()
    {
        var body = string.Join("\n", keybinds.Select(k => k.ToString()).OrderBy(s => s));
        Monitor.SLog($"\n====== KeyBinds ======\n\n{body}\n\n======================\n");
    }

    private static KeyBind TopPriority(List<KeyBind> keybinds)
    {
        return keybinds.Max();
    }
    int IComparable<KeyBind>.CompareTo(KeyBind other)
    {
        var p1 = ToPriority();
        var p2 = other.ToPriority();
        var num = Math.Min(p1.Count, p2.Count);
        for (int i = 0; i < num; i++)
        {
            if (p1[i] < p2[i]) return -1;
            if (p1[i] > p2[i]) return 1;
        }
        return 0;
    }
    private List<int> ToPriority()
    {
        List<int> ret = [units.Count];
        for (int i = units.Count - 1; i >= 0; i--)
        {
            ret.Add(units[i].hold.Count);
        }
        return ret;
    }
}

