
using ModdingAPI;

namespace GenericModConfigMenu.Options;
internal class ActionOption(Action action, Func<string> name, bool closeMenu, Action<Action>? beforeClose, Func<bool>? condition) : IOption
{
    private readonly Action action = action;
    private readonly Func<string> name = name;
    private readonly bool closeMenu = closeMenu;
    private readonly Action<Action>? beforeClose = beforeClose;
    private readonly Func<bool>? condition = condition;
    public bool Unsaved { get; set; } = false;
    public bool Enabled
    {
        get
        {
            try { return condition?.Invoke() ?? true; }
            catch { return false; }
        }
    }
    public SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu) => new(() => name(), (getSubmenu) => () =>
    {
        if (closeMenu)
        {
            Action close = () =>
            {
                getSubmenu().Kill();
                modMenu?.Kill();
                topMenu?.Kill();
                action();
            };
            if (beforeClose != null) beforeClose(close);
            else close();
        }
        else
        {
            action();
        }
    });
}

