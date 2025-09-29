
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using BepInEx;

namespace ModdingAPI.KeyBind;

public class KeyBindUnit
{
    internal static readonly int WithinMax = 120;
    private const int WithinDefault = 30;
    internal readonly Key trigger;
    internal readonly HashSet<Key> hold;
    internal readonly int within;
    internal bool isFirstUnit = false;
    public KeyBindUnit(Key trigger, HashSet<Key> hold, int within = WithinDefault)
    {
        this.trigger = trigger;
        this.hold = [.. hold];
        if (within > WithinMax)
        {
            Monitor.SLog($"tool large withinFrames value (value: {within}, max: {WithinMax})", LogLevel.Warning);
            within = WithinMax;
        }
        this.within = Math.Max(1, within);
    }
    public KeyBindUnit(Key trigger) : this(trigger, []) { }
    public override string ToString()
    {
        string ret = "";
        if (hold.Any())
        {
            var holds = string.Join('+', hold.Select(k => k.ToString()));
            ret = $"{trigger}({holds})";
        }
        else
        {
            ret = trigger.ToString();
        }
        if (!isFirstUnit && within != WithinDefault) ret += $"[{within}]";
        return ret;
    }
    public static string UnitsToString(IReadOnlyList<KeyBindUnit> units) => string.Join(' ', units.Select(c => c.ToString()));

    private static readonly Regex keyPattern = new(@"^([:@.][a-zA-Z0-9]+|[a-zA-Z][a-zA-Z0-9]*)");
    /// <summary>
    /// See <c>keybindins.json</c> on <c>ModsPath</c> or <see cref="KeyBindingsData.comments"/> for the keybind syntax.
    /// </summary>
    public static bool TryParse(string query, [NotNullWhen(true)] out List<KeyBindUnit>? units, [NotNullWhen(false)] out string? error)
    {
        units = null;
        var s = query.TrimStart();
        List<KeyBindUnit> _units = [];
        while (!s.IsNullOrWhiteSpace())
        {
            int idx;
            var m = keyPattern.Match(s);
            if (!m.Success)
            {
                error = "pattern not match";
                return false;
            }
            if (!Key.TryParse(m.Value, out var key))
            {
                error = $"wrong key \"{m.Value}\"";
                return false;
            }
            s = s[m.Value.Length..].TrimStart();
            HashSet<Key> holdKeys = [];
            int? within = null;
            if (s.StartsWith('('))
            {
                idx = s.IndexOf(')');
                if (idx < -1)
                {
                    error = "not found closing parenthesis";
                    return false;
                }
                foreach (var hold in s[1..idx].Split('+'))
                {
                    if (Key.TryParse(hold.Trim(), out var k)) holdKeys.Add(k);
                    else
                    {
                        error = $"wrong key \"{hold.Trim()}\"";
                        return false;
                    }
                }
                s = s[(s.IndexOf(')') + 1)..].TrimStart();
            }
            if (s.StartsWith('['))
            {
                idx = s.IndexOf(']');
                if (idx < -1)
                {
                    error = "not found closing bracket";
                    return false;
                }
                var intstr = s[1..idx].Trim();
                if (int.TryParse(intstr, out var val)) within = val;
                else
                {
                    error = $"cannot parse \"{intstr}\" to integer";
                    return false;
                }
                s = s[(s.IndexOf(')') + 1)..].TrimStart();
            }
            _units.Add(within == null ? new(key, holdKeys) : new(key, holdKeys, (int)within));
        }
        if (!_units.Any())
        {
            error = "empty query";
            return false;
        }
        units = _units;
        error = null;
        return true;
    }
}

public class KeyBindUnitEntry
{
    private enum Type { Query, Units, }
    public bool IsValid { get; private set; } = false;
    private readonly Type type;
    private readonly string query = null!;
    private readonly List<KeyBindUnit> _units = null!;
    public KeyBindUnitEntry(string query) { type = Type.Query; this.query = query; }
    public KeyBindUnitEntry(IReadOnlyList<KeyBindUnit> units) { type = Type.Units; _units = [.. units]; }
    public static implicit operator KeyBindUnitEntry(string query) => new(query);
    public static implicit operator KeyBindUnitEntry(KeyBindUnit units) => new([units]);
    public static implicit operator KeyBindUnitEntry(KeyBindUnit[] units) => new(units);
    public static implicit operator KeyBindUnitEntry(List<KeyBindUnit> units) => new(units);
    public bool TryToGetKeyUnits([NotNullWhen(true)] out List<KeyBindUnit>? units, [NotNullWhen(false)] out string? error)
    {
        units = null;
        error = null;
        if (type == Type.Query)
        {
            if (KeyBindUnit.TryParse(query, out units, out error)) return true;
            else error = $"Invalid keybind query \"{query}\": {error}";
        }
        else if (type == Type.Units)
        {
            if (_units.Any())
            {
                units = _units;
                return true;
            }
            else error = "empty KeyBindUnit list";
        }
        else throw new Exception("unexpected type");
        return false;
    }
}

