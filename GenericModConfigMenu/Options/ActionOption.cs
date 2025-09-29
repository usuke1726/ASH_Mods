
using ModdingAPI;

namespace GenericModConfigMenu.Options;
internal class ActionOption(Action action, Func<string> name, bool closeMenu, Action<Action>? beforeClose) : IOption
{
    private readonly Action action = action;
    private readonly Func<string> name = name;
    private readonly bool closeMenu = closeMenu;
    private readonly Action<Action>? beforeClose = beforeClose;
    public bool Unsaved { get; set; } = false;
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

