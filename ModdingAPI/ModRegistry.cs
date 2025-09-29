
using ModdingAPI.Events;

namespace ModdingAPI;

public interface IModRegistry
{
    public void Add(Mod mod);
    object? GetApi(string uniqueID);
    TInterface? GetApi<TInterface>(string uniqueID) where TInterface : class;
}

internal class ModRegistry : IModRegistry
{
    internal static ModRegistry instance = new();
    internal readonly Dictionary<string, Mod> mods = [];
    internal static bool CanAdd(Mod mod)
    {
        return !instance.mods.ContainsKey(mod.UniqueID);
    }
    internal static void OnLanguageChanged()
    {
        if (Config.Language.ToLower() != "system") return;
        var oldLanguage = API_I18n.CurrentLanguage;
        API_I18n.UpdateLanguage();
        var newLanguage = API_I18n.CurrentLanguage;
        if (oldLanguage == newLanguage) return;
        Monitor.SLog($"system language changed");
        ModdingApiMod.instance.I18n_ = new API_I18n(ModdingApiMod.instance);
        foreach (var mod in instance.mods.Values)
        {
            mod.I18n_ = new API_I18n(mod);
        }
        SystemEvents.OnLocaleChanged(newLanguage, oldLanguage);
    }
    public void Add(Mod mod)
    {
        mods[mod.UniqueID] = mod;
    }
    public object? GetApi(string uniqueID)
    {
        if (mods.TryGetValue(uniqueID, out var api))
        {
            return api.GetApi();
        }
        else
        {
            Monitor.SLog(I18n_.Localize("ModRegistry.Error.ApiNotFound", uniqueID), LogLevel.Error);
            return null;
        }
    }
    public TInterface? GetApi<TInterface>(string uniqueID) where TInterface : class
    {
        var api = GetApi(uniqueID);
        if (api == null) return null;
        if (!typeof(TInterface).IsInterface)
        {
            Monitor.SLog(I18n_.Localize("ModRegistry.Error.ApiNotInterface", typeof(TInterface).FullName), LogLevel.Error);
            return null;
        }
        if (!typeof(TInterface).IsPublic)
        {
            Monitor.SLog(I18n_.Localize("ModRegistry.Error.ApiNotPublic", typeof(TInterface).FullName), LogLevel.Error);
            return null;
        }
        try
        {
            return api as TInterface;
        }
        catch (Exception e)
        {
            Monitor.SLog(I18n_.Localize("ModRegistry.Error.FailedCastingToInterafce", typeof(TInterface).FullName, e), LogLevel.Error);
            return null;
        }
    }
}

