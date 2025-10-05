
using ModdingAPI;
namespace GenericModConfigMenu.Options;

internal class BoolOption(Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<bool, string>? formatValue) : IOption
{
    private readonly Func<bool> getValue = getValue;
    private readonly Action<bool> setValue = setValue;
    private readonly Func<string> name = name;
    private string getName() => $"{name()}: {formatValue(getValue())}";
    private readonly Func<bool, string> formatValue = formatValue ?? (f => f ? "Y" : "N");
    public bool Unsaved { get; set; } = false;
    public bool Enabled => true;
    public SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu) => MenuItem();
    private SubmenuItemEntry MenuItem() => new(() => getName(), (getSubmenu, getReflesher) => () =>
    {
        Unsaved = true;
        setValue(!getValue());
        getReflesher()();
    });
}

