
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ModdingAPI;

using A1 = Action<BasicMenuItem>;
using A2 = Action;

#pragma warning disable IDE1006
public struct MenuItem
{
    public string name;
    public A1 action;
    public MenuItem(string name, A1 action)
    {
        this.name = name;
        this.action = action;
    }
    public MenuItem(string name, A2 action)
    {
        this.name = name;
        this.action = delegate
        {
            action();
        };
    }
}
#pragma warning restore IDE1006

public class MenuItemEntryCore<T>
{
    public Func<string> getName;
    internal bool useArg;
    internal Func<T, Func<Action>, A1> getActionArg;
    internal Func<T, Func<Action>, A2> getAction;
    public MenuItemEntryCore(Func<string> getName, Func<T, Func<Action>, A1> func)
    {
        useArg = true;
        this.getName = getName;
        getActionArg = func;
        getAction = null!;
    }
    public MenuItemEntryCore(Func<string> getName, Func<T, Func<Action>, A2> func)
    {
        useArg = false;
        this.getName = getName;
        getActionArg = null!;
        getAction = func;
    }

    public MenuItem ToMenuItem(T instance, Func<Action> reflesher)
    {
        return useArg
            ? new MenuItem(getName(), getActionArg.Invoke(instance, reflesher))
            : new MenuItem(getName(), getAction.Invoke(instance, reflesher));
    }
}
public class MenuItemEntry : MenuItemEntryCore<OptionsMenu>
{
    private MenuItemEntry(Func<string> getName, Func<OptionsMenu, Func<Action>, A1> func) : base(getName, func) { }
    private MenuItemEntry(Func<string> getName, Func<OptionsMenu, Func<Action>, A2> func) : base(getName, func) { }
    public MenuItemEntry(Func<string> getName, Func<OptionsMenu, A1> func) : base(getName, (i, r) => func(i)) { }
    public MenuItemEntry(Func<string> getName, Func<OptionsMenu, A2> func) : base(getName, (i, r) => func(i)) { }
    public MenuItemEntry(string name, Func<OptionsMenu, A1> func) : this(() => name, (i, r) => func(i)) { }
    public MenuItemEntry(string name, Func<OptionsMenu, A2> func) : this(() => name, (i, r) => func(i)) { }
    public MenuItemEntry(Func<string> getName, Func<A1> func) : this(getName, i => func()) { }
    public MenuItemEntry(Func<string> getName, Func<A2> func) : this(getName, i => func()) { }
    public MenuItemEntry(Func<string> getName, A1 func) : this(getName, i => func) { }
    public MenuItemEntry(Func<string> getName, A2 func) : this(getName, i => func) { }
    public MenuItemEntry(string name, Func<A1> func) : this(() => name, i => func()) { }
    public MenuItemEntry(string name, Func<A2> func) : this(() => name, i => func()) { }
    public MenuItemEntry(string name, A1 func) : this(() => name, i => func) { }
    public MenuItemEntry(string name, A2 func) : this(() => name, i => func) { }
}
public class SubmenuItemEntry : MenuItemEntryCore<Func<AbstractMenu>>
{
    public SubmenuItemEntry(Func<string> getName, Func<Func<AbstractMenu>, Func<Action>, A1> func) : base(getName, func) { }
    public SubmenuItemEntry(Func<string> getName, Func<Func<AbstractMenu>, Func<Action>, A2> func) : base(getName, func) { }
    public SubmenuItemEntry(string name, Func<Func<AbstractMenu>, Func<Action>, A1> func) : this(() => name, func) { }
    public SubmenuItemEntry(string name, Func<Func<AbstractMenu>, Func<Action>, A2> func) : this(() => name, func) { }
    public SubmenuItemEntry(Func<string> getName, Func<Func<AbstractMenu>, A1> func) : base(getName, (i, r) => func(i)) { }
    public SubmenuItemEntry(Func<string> getName, Func<Func<AbstractMenu>, A2> func) : base(getName, (i, r) => func(i)) { }
    public SubmenuItemEntry(string name, Func<Func<AbstractMenu>, A1> func) : this(() => name, (i, r) => func(i)) { }
    public SubmenuItemEntry(string name, Func<Func<AbstractMenu>, A2> func) : this(() => name, (i, r) => func(i)) { }
    public SubmenuItemEntry(Func<string> getName, Func<A1> func) : this(getName, i => func()) { }
    public SubmenuItemEntry(Func<string> getName, Func<A2> func) : this(getName, i => func()) { }
    public SubmenuItemEntry(Func<string> getName, A1 func) : this(getName, i => func) { }
    public SubmenuItemEntry(Func<string> getName, A2 func) : this(getName, i => func) { }
    public SubmenuItemEntry(string name, Func<A1> func) : this(() => name, i => func()) { }
    public SubmenuItemEntry(string name, Func<A2> func) : this(() => name, i => func()) { }
    public SubmenuItemEntry(string name, A1 func) : this(() => name, i => func) { }
    public SubmenuItemEntry(string name, A2 func) : this(() => name, i => func) { }
}

public static class MainMenu
{
    internal static readonly SortedDictionary<int, List<MenuItemEntry>> itemsToBeAdded = [];
    // Listed in descending order by priority
    public static void AddMenuItem(MenuItemEntry entry, int priority = 0)
    {
        var key = -1 * priority;
        if (itemsToBeAdded.ContainsKey(key))
        {
            itemsToBeAdded[key].Add(entry);
        }
        else
        {
            itemsToBeAdded[key] = [entry];
        }
    }
    private static GameObject? currentDialog = null;
    public static void ShowDialog(string message, Action? onClosed = null, bool? useEnglishFont = null) => ShowDialog(null, message, onClosed, useEnglishFont);
    public static void ShowDialog(OptionsMenu? menu, string message, Action? onClosed = null, bool? useEnglishFont = null)
    {
        GameObject obj;
        if (menu != null)
        {
            UI ui = Traverse.Create(menu).Field("ui").GetValue<UI>();
            obj = ui.AddUI(menu.dialogBoxPrefab.Clone());
        }
        else
        {
            obj = Singleton<GameServiceLocator>.instance.ui.CreateSimpleDialogue(message);
        }
        obj.RegisterOnDestroyCallback(() =>
        {
            currentDialog = null;
            onClosed?.Invoke();
        });
        if (useEnglishFont ?? !Util.IsUsingJapanese(message))
        {
            var translator = obj.GetComponentInChildren<TextTranslator>();
            translator.enabled = false;
            translator.DisableCustomizedFont();
        }
        UI.SetGenericText(obj, message);
        currentDialog = obj;
    }
    public static void UpdateDialogText(string message)
    {
        if (currentDialog == null) return;
        UI.SetGenericText(currentDialog, message);
    }
    public static void CloseDialog()
    {
        if (currentDialog != null)
        {
            currentDialog.SetActive(false);
            currentDialog = null;
        }
    }
    private static GameObject? pauseMenuCache = null;
    public static void ClosePauseMenu()
    {
        if (!Context.GameStarted) return;
        if (!Context.gameServiceLocator.levelUI.pauseMenuOpen) return;
        if (pauseMenuCache == null)
        {
            pauseMenuCache = GameObject.Find("/LevelSingletons/UICanvas/UIElements/Menu");
        }
        pauseMenuCache.GetComponent<PauseMenu>().Close();
    }
    public static void ShowSubMenu(OptionsMenu? menu, IEnumerable<SubmenuItemEntry> items, Action? onClosed = null, bool? useEnglishFont = null, int initialIndex = 0)
    {
        var l = items.ToList();
        AbstractMenu m = null!;
        Func<Action> getReflesher = () => null!;
        Action setup = () =>
        {
            List<MenuItem> menuItems = [.. items.Select(i => i.ToMenuItem(() => m, getReflesher))];
            if (menuItems.Count >= 10)
            {
                m = ScrollableMenu.CreateScrollableMenu(menuItems.Select((MenuItem i) => i.name).ToArray(), menuItems.Select((MenuItem i) => i.action).ToArray(), initialIndex, useEnglishFont);
                if (onClosed != null) m.onKill += onClosed;
                OptionsMenu.PositionMenu(m.transform);
                return;
            }
            LinearMenu lm;
            if (menu != null)
            {
                m = lm = OptionsBuildSimpleMenu.BuildSimpleMenu(menu, menuItems);
            }
            else
            {
                m = lm = Singleton<GameServiceLocator>.instance.ui.CreateSimpleMenu(menuItems.Select((MenuItem i) => i.name).ToArray(), menuItems.Select((MenuItem i) => i.action).ToArray());
                OptionsMenu.PositionMenu(lm.transform);
            }
            lm.selectedIndex = initialIndex;
            if (onClosed != null) m.onKill += onClosed;
            if (useEnglishFont == false) return;
            var objs = lm.GetMenuObjects();
            var cnt = Math.Min(l.Count, objs.Count);
            for (int i = 0; i < cnt; i++)
            {
                if (useEnglishFont ?? !Util.IsUsingJapanese(l[i].getName()))
                {
                    var translator = objs[i].GetComponentInChildren<TextTranslator>();
                    translator.enabled = false;
                    translator.DisableCustomizedFont();
                }
            }
        };
        Action reflesh = () =>
        {
            if (m is ScrollableMenu sm) sm.Refresh([.. l.Select(e => e.getName())]);
            else if (m is LinearMenu lm)
            {
                var gameObjects = lm.GetMenuObjects();
                var num = Math.Min(l.Count, gameObjects.Count);
                for (int i = 0; i < num; i++)
                {
                    UI.SetGenericText(gameObjects[i], l[i].getName());
                }
            }
        };
        getReflesher = () => reflesh;
        setup();
    }
    public static void ShowConfirm(string text, string confirmText, string rejectText, Action onConfirmed, Action? onRejected = null)
    {
        var ui = Context.gameServiceLocator.ui;
        LinearMenu menu = null!;
        string[] texts = [confirmText, rejectText];
        menu = ui.CreateSimpleMenu(
            texts,
            [() =>  {
                menu.Kill();
                onConfirmed();
            }, () => {
                menu.Kill();
                onRejected?.Invoke();
            }]);
        var objs = menu.GetMenuObjects();
        for (int i = 0; i < objs.Count; i++)
        {
            if (!Util.IsUsingJapanese(texts[i]))
            {
                var translator = objs[i].GetComponentInChildren<TextTranslator>();
                translator.enabled = false;
                translator.DisableCustomizedFont();
            }
        }
        var obj = ui.CreateTextMenuItem(text);
        obj.transform.SetParent(menu.transform, false);
        obj.transform.SetAsFirstSibling();
        if (!Util.IsUsingJapanese(text))
        {
            var translator = obj.GetComponentInChildren<TextTranslator>();
            translator.enabled = false;
            translator.DisableCustomizedFont();
        }
        var transform = menu.transform;
        if (transform is RectTransform r)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(r);
            r.CenterWithinParent();
        }
    }
    public static void UpdateText(IMenuItem menu, Func<string> getText)
    {
        UI.SetGenericText(menu.gameObject, getText());
    }
    public static void UpdateText(IMenuItem menu, string text) => UpdateText(menu, () => text);
}

[HarmonyPatch(typeof(OptionsMenu))]
internal static class OptionsBuildSimpleMenu
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(OptionsMenu), "BuildSimpleMenu")]
    internal static LinearMenu BuildSimpleMenu(object instance, List<MenuItem> menuItems) => throw new NotImplementedException();
}

[HarmonyPatch(typeof(OptionsMenu))]
file class Patch
{

    [HarmonyPrefix]
    [HarmonyPatch("BuildSimpleMenu")]
    static bool PreFix(List<MenuItem> menuItems, OptionsMenu __instance)
    {
        if (IsMainMenu(menuItems))
        {
            var prop = menuItems.GetType().GetProperty("Capacity");
            var currentCapacity = (int)prop.GetValue(menuItems);
            var newSize = currentCapacity + MainMenu.itemsToBeAdded.Select(i => i.Value.Count).Sum();
            prop.SetValue(menuItems, newSize);
            foreach (var items in MainMenu.itemsToBeAdded)
            {
                menuItems.AddRange(items.Value.Select(item => item.ToMenuItem(__instance, () => () => { })));
            }
        }
        return true;
    }

    private static bool IsMainMenu(List<MenuItem> menuItems)
    {
        return Environment.StackTrace.Contains("\n  at OptionsMenu.BuildMainMenu ");
    }
}

