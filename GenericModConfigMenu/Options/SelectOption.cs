
using ModdingAPI;

namespace GenericModConfigMenu.Options;

internal abstract class SelectOption<T>(Func<T> getValue, Action<T> setValue, Func<string> name, T[] selection, Func<T, string>? formatValue) : IOption
{
    private readonly Func<T> getValue = getValue;
    private readonly Action<T> setValue = setValue;
    private readonly Func<string> name = name;
    private string getName()
    {
        if (ModConfig.config.ShowCurrentSelection)
        {
            return $"{name()}: {formatValue(getValue())}";
        }
        else
        {
            return name();
        }
    }
    private readonly Func<T, string> formatValue = formatValue ?? (v => $"{v}");
    private readonly List<T> selection = selection.ToList();

    protected abstract bool IsSameValue(T a, T b);
    protected abstract T val { get; set; }
    public bool Unsaved { get; set; } = false;

    public SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu) => MenuItem(topMenu);
    private SubmenuItemEntry MenuItem(OptionsMenu? menu) => new(() => getName(), (_, getReflesher) => () =>
    {
        val = getValue();
        var idx = Math.Max(selection.IndexOf(val), 0);
        MainMenu.ShowSubMenu(menu, selection.Select(s =>
            new SubmenuItemEntry(formatValue(s), getSubmenu => itemMenu =>
            {
                if (!IsSameValue(val, s)) Unsaved = true;
                val = s;
                getSubmenu().Kill();
            })),
            onClosed: () =>
            {
                setValue(val);
                getReflesher()();
            },
            initialIndex: idx
        );
    });
}

internal class SelectOptionInt : SelectOption<int>
{
    private int _val = 0;
    protected override int val { get => _val; set => _val = value; }
    protected override bool IsSameValue(int a, int b) => a == b;
    public SelectOptionInt(Func<int> getValue, Action<int> setValue, Func<string> name, int[] selection, Func<int, string>? formatValue) : base(getValue, setValue, name, selection, formatValue) { }
}

internal class SelectOptionFloat : SelectOption<float>
{
    private float _val = 0;
    protected override float val { get => _val; set => _val = value; }
    protected override bool IsSameValue(float a, float b) => MathF.Abs(a - b) < 1e-6f;
    public SelectOptionFloat(Func<float> getValue, Action<float> setValue, Func<string> name, float[] selection, Func<float, string>? formatValue) : base(getValue, setValue, name, selection, formatValue) { }
}
internal class SelectOptionString : SelectOption<string>
{
    private string _val = "";
    protected override string val { get => _val; set => _val = value; }
    protected override bool IsSameValue(string a, string b) => a == b;
    public SelectOptionString(Func<string> getValue, Action<string> setValue, Func<string> name, string[] selection, Func<string, string>? formatValue) : base(getValue, setValue, name, selection, formatValue) { }
}

