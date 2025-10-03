
using UnityEngine;

namespace ModdingAPI.Events;

using Lang = SystemLanguage;

public class LocaleChangedEventArgs(Lang newLang, Lang oldLang) : EventArgs()
{
    public readonly Lang NewLanguage = newLang;
    public readonly Lang OldLanguage = oldLang;
}
public class GameSavingEventArgs(int saveSlot) : EventArgs()
{
    public readonly int SaveSlot = saveSlot;
}

public interface ISystemEvents
{
    event EventHandler<LocaleChangedEventArgs>? LocaleChanged;
    event EventHandler<GameSavingEventArgs>? BeforeSaving;
}

internal class SystemEvents : ISystemEvents
{
    internal static SystemEvents instance = new();
    public event EventHandler<LocaleChangedEventArgs>? LocaleChanged;
    public event EventHandler<GameSavingEventArgs>? BeforeSaving;
    internal static void OnLocaleChanged(Lang newLang, Lang oldLang) => instance.LocaleChanged?.Invoke(null, new(newLang, oldLang));
    internal static void OnBeforeSaving(int saveSlot) => instance.BeforeSaving?.Invoke(null, new(saveSlot));
}

