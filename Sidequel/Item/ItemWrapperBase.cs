
using System.Collections;
using UnityEngine;

namespace Sidequel.Item;

internal class ItemWrapperBase
{
    private class I18nKeys(string readableName, string readableNamePlural, string description)
    {
        public readonly string ReadableName = readableName;
        public readonly string ReadableNamePlural = readableNamePlural;
        public readonly string Description = description;
    }
    internal readonly string id;
    internal readonly CollectableItem item;
    private int? state = null;
    private readonly Func<int?> getState;
    private string defaultReadableName;
    private string defaultReadableNamePlural;
    private string defaultDescription;
    public ItemWrapperBase(string id, CollectableItem item, Func<int?>? getState = null)
    {
        DataHandler.ValidateItemId(id);
        this.id = id;
        this.item = item;
        defaultReadableName = item.readableName;
        defaultReadableNamePlural = item.readableNamePlural;
        defaultDescription = item.description;
        this.getState = getState ?? (() => null);
        item.name = id;
        OnLocaleChanged();
        DataHandler.Register(this);
    }
    protected ItemWrapperBase(string id, Func<int?>? getState = null) : this(id, DefaultItem(), getState) { }
    private static CollectableItem DefaultItem() => new()
    {
        readableName = "",
        readableNamePlural = "",
        description = "",
    };
    internal static void TryLoad(string name, Func<int?>? getState = null)
    {
        var item = CollectableItem.Load(name);
        if (item != null) new ItemWrapperBase(name, item, getState);
        else Monitor.Log($"item {name} not found", LL.Warning);
    }
    internal virtual void EnsureIconCreated(Sprite resource) { }
    internal void UpdateState()
    {
        var newIdx = getState();
        if (state != newIdx)
        {
            state = newIdx;
            OnLocaleChanged();
        }
    }
    private I18nKeys GetI18nKeys()
    {
        var idx = state == null ? "" : $".{state}";
        var pref = $"item.{id}{idx}";
        return new($"{pref}.name", $"{pref}.namePlural", $"{pref}.description");
    }
    private string GetDefaultLocalizedName() => I18nLocalize($"item.{id}.name");
    private string GetDefaultLocalizedNamePlural() => I18nLocalize($"item.{id}.namePlural");
    internal void OnLocaleChanged()
    {
        if (!State.IsActive) return;
        var i18nKeys = GetI18nKeys();
        string s;
        s = I18nLocalize(i18nKeys.ReadableName);
        if (string.IsNullOrEmpty(s)) s = GetDefaultLocalizedName();
        item.readableName = string.IsNullOrEmpty(s) ? defaultReadableName : s;
        s = I18nLocalize(i18nKeys.ReadableNamePlural);
        if (string.IsNullOrEmpty(s)) s = GetDefaultLocalizedNamePlural();
        item.readableNamePlural = string.IsNullOrEmpty(s) ? defaultReadableNamePlural : s;
        s = I18nLocalize(i18nKeys.Description);
        item.description = string.IsNullOrEmpty(s) ? defaultDescription : s;
        //Debug($"LocaleChanged: \"{item.readableName}\" \"{item.readableNamePlural}\" \"{item.description}\"");
    }
    internal void OnReturnedTitle()
    {
        item.readableName = defaultReadableName;
        item.readableNamePlural = defaultReadableNamePlural;
        item.description = defaultDescription;
    }
    internal IEnumerator PickUpRoutine(int amount = 1) => item.PickUpRoutine(amount);
}

