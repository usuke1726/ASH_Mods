
using ModdingAPI;
namespace IGenericModConfigMenu;

public static class Info
{
    public static readonly string ModID = "Quicker1726.GenericModConfigMenu";
}

public interface IGenericModConfigMenuApi
{
    void Register<IPoco>(IMod mod, Action reset, Action<IPoco> import, Func<IPoco> export, string? displayName = null, string? comments = null, Dictionary<string, string>? propertyComments = null, bool titleScreenOnly = false) where IPoco : class;
    void OpenModOptionMenu(string uniqueID);
    void OpenModOptionMenu(IMod mod);
    void AddBoolOption(IMod mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<bool, string>? formatValue = null);
    void AddNumberOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int interval = 1, int? min = null, int? max = null, Func<int, string>? formatValue = null);
    void AddNumberOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float interval = 0.01f, float? min = null, float? max = null, int? digit = 2, Func<string, string>? formatValue = null);
    void AddSelectOption(IMod mod, Func<string> getValue, Action<string> setValue, Func<string> name, string[] selection, Func<string, string>? formatValue = null);
    void AddSelectOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int[] selection, Func<int, string>? formatValue = null);
    void AddSelectOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float[] selection, Func<float, string>? formatValue = null);
    void AddAction(IMod mod, Action action, Func<string> name, bool closeMenu = false, Action<Action>? beforeClose = null, Func<bool>? condition = null);
}

