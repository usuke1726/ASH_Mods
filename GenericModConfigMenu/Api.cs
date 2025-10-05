
using System.Reflection;
using IGenericModConfigMenu;
using ModdingAPI;
using ModdingAPI.IO;

namespace GenericModConfigMenu;

using Core;

public class Api : ApiBase, IGenericModConfigMenuApi
{
    private static readonly SortedDictionary<string, ApiBase> apis = [];
    private static bool showingMenu = false;
    public Api() : base(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_GUID)
    {
        MainMenu.AddMenuItem(new("Mod Config", menu => () =>
        {
            showingMenu = true;
            List<SubmenuItemEntry> items = [];
            foreach (var api in apis.Values)
            {
                api.AddMenu(menu, items);
            }
            MainMenu.ShowSubMenu(menu, items, onClosed: () =>
            {
                showingMenu = false;
            });
        }));
    }
    public void OpenModOptionMenu(IMod mod) => OpenModOptionMenu(mod.UniqueID);
    public void OpenModOptionMenu(string uniqueID)
    {
        if (showingMenu) return;
        if (apis.TryGetValue(uniqueID, out var api))
        {
            showingMenu = true;
            api.OpenModOptionMenu(() =>
            {
                showingMenu = false;
            });
        }
    }
    public void Register<IPoco>(IMod mod, Action reset, Action<IPoco> import, Func<IPoco> export, string? displayName = null, string? comments = null, Dictionary<string, string>? propertyComments = null, bool titleScreenOnly = false)
        where IPoco : class
    {
        if (!ConfigFile<IPoco>.IsValidPocoClass(out var errors, out var isEmptyPoco))
        {
            Monitor.Log(string.Join('\n', errors), LL.Error);
            return;
        }
        apis[mod.UniqueID] = new ApiBody<IPoco>(mod, reset, import, export, displayName, comments, propertyComments, isEmptyPoco);
    }
    private ApiBase? GetApi(IMod mod) => apis.TryGetValue(mod.UniqueID, out var api) ? api : null;
    public new void AddBoolOption(IMod mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<bool, string>? formatValue = null)
    {
        GetApi(mod)?.AddBoolOption(mod, getValue, setValue, name, formatValue);
    }

    public new void AddNumberOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int interval = 1, int? min = null, int? max = null, Func<int, string>? formatValue = null)
    {
        GetApi(mod)?.AddNumberOption(mod, getValue, setValue, name, interval, min, max, formatValue);
    }
    public new void AddNumberOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float interval = 0.01f, float? min = null, float? max = null, int? digit = 2, Func<string, string>? formatValue = null)
    {
        GetApi(mod)?.AddNumberOption(mod, getValue, setValue, name, interval, min, max, digit, formatValue);
    }
    public new void AddSelectOption(IMod mod, Func<string> getValue, Action<string> setValue, Func<string> name, string[] selection, Func<string, string>? formatValue = null)
    {
        GetApi(mod)?.AddSelectOption(mod, getValue, setValue, name, selection, formatValue);
    }
    public new void AddSelectOption(IMod mod, Func<int> getValue, Action<int> setValue, Func<string> name, int[] selection, Func<int, string>? formatValue = null)
    {
        GetApi(mod)?.AddSelectOption(mod, getValue, setValue, name, selection, formatValue);
    }
    public new void AddSelectOption(IMod mod, Func<float> getValue, Action<float> setValue, Func<string> name, float[] selection, Func<float, string>? formatValue = null)
    {
        GetApi(mod)?.AddSelectOption(mod, getValue, setValue, name, selection, formatValue);
    }
    public new void AddAction(IMod mod, Action action, Func<string> name, bool closeMenu = false, Action<Action>? beforeClose = null, Func<bool>? condition = null)
    {
        GetApi(mod)?.AddAction(mod, action, name, closeMenu, beforeClose, condition);
    }

    private static readonly HashSet<Type> allowedTypes = [typeof(int), typeof(bool), typeof(float), typeof(string)];
    public static bool IsValidPocoClass<T>()
    {
        bool ret = true;
        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                Type type = prop.PropertyType;
                if (!allowedTypes.Contains(type))
                {
                    Monitor.Log($"Unsupported type {type} of property {prop.Name}", LL.Error);
                    ret = false;
                }
            }
            else
            {
                Monitor.Log($"Cannot read property {prop.Name}", LL.Error);
                ret = false;
            }
        }
        return ret;
    }
}

