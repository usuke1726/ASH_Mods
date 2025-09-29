
using UnityEngine;

namespace ModdingAPI.Events;

using Lang = SystemLanguage;

public class LocaleChangedEventArgs(Lang newLang, Lang oldLang) : UpdatedEventArgs()
{
    public readonly Lang NewLanguage = newLang;
    public readonly Lang OldLanguage = oldLang;
}

public interface ISystemEvents
{
    event EventHandler<LocaleChangedEventArgs>? LocaleChanged;
}

internal class SystemEvents : ISystemEvents
{
    internal static SystemEvents instance = new();
    public event EventHandler<LocaleChangedEventArgs>? LocaleChanged;
    internal static void OnLocaleChanged(Lang newLang, Lang oldLang) => instance.LocaleChanged?.Invoke(null, new(newLang, oldLang));
}

