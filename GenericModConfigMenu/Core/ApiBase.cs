
using ModdingAPI;

namespace GenericModConfigMenu.Core;

public abstract class ApiBase(string id, string displayName)
{
    public static readonly string ConfigFileName = "config.cfg";
    public readonly string ID = id;
    public readonly string DisplayName = displayName;
    internal virtual void AddMenu(OptionsMenu menu, List<SubmenuItemEntry> list) { }
    internal virtual void AddBoolOption(IMod mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<bool, string>? formatValue) { }
    internal virtual void AddNumberOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int interval, int? min, int? max, Func<int, string>? formatValue) { }
    internal virtual void AddNumberOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float interval, float? min, float? max, int? digit, Func<string, string>? formatValue) { }
    internal virtual void AddSelectOption(IMod mod, Func<string> getValue, Action<string> setValue, Func<string> name, string[] selection, Func<string, string>? formatValue) { }
    internal virtual void AddSelectOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int[] selection, Func<int, string>? formatValue) { }
    internal virtual void AddSelectOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float[] selection, Func<float, string>? formatValue) { }
    internal virtual void AddAction(IMod mod, Action action, Func<string> name, bool closeMenu, Action<Action>? beforeClose, Func<bool>? condition) { }

    internal virtual void OpenModOptionMenu(Action onClosed) { }
}

