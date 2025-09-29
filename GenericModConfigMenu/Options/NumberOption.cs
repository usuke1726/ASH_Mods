
using ModdingAPI;

namespace GenericModConfigMenu.Options;

internal abstract class NumberOptionBase<T, S>(Func<T> getValue, Action<T> setValue, Func<string> name, T interval, T? min, T? max, Func<S, string>? formatValue) : IOption
    where T : struct
{
    private readonly Func<T> getValue = getValue;
    private readonly Action<T> setValue = setValue;
    private readonly Func<string> name = name;
    private string getName()
    {
        if (ModConfig.config.ShowCurrentNumber)
        {
            return $"{name()}: {formatValue(ToS(getValue()))}";
        }
        else
        {
            return name();
        }
    }
    private readonly Func<S, string> formatValue = formatValue ?? (v => $"{v}");
    private readonly T interval = interval;
    private readonly T? min = min;
    private readonly T? max = max;
    private T val = default;
    private KeyWatcher? keyWatcher = null;

    public bool Unsaved { get; set; } = false;

    protected abstract S ToS(T v);
    protected abstract T Add(T a, T b);
    protected abstract T Minus(T v);
    protected abstract bool GreaterThan(T a, T b);
    protected abstract bool IsSameValue(T a, T b);
    public SubmenuItemEntry MenuItem(OptionsMenu? topMenu, AbstractMenu? modMenu) => MenuItem(topMenu);
    private SubmenuItemEntry MenuItem(OptionsMenu? menu) => new(() => getName(), (_, getReflesher) => () =>
    {
        val = getValue();
        keyWatcher = new();
        Func<T, Action> c = i => () =>
        {
            var newVal = Add(val, i);
            if (min != null && GreaterThan((T)min, newVal)) return;
            if (max != null && GreaterThan(newVal, (T)max)) return;
            val = newVal;
            MainMenu.UpdateDialogText(formatValue(ToS(val)));
        };
        var mint = Minus(interval);
        keyWatcher.Watch(ArrowKey.Up, onKeydown: c(interval), onKeyhold: c(interval));
        keyWatcher.Watch(ArrowKey.Down, onKeydown: c(mint), onKeyhold: c(mint));
        MainMenu.ShowDialog(menu, formatValue(ToS(val)), onClosed: () =>
        {
            keyWatcher.Dispose();
            keyWatcher = null;
            if (!IsSameValue(val, getValue())) Unsaved = true;
            setValue(val);
            getReflesher()();
        });
    });
}

internal class NumberOptionInt : NumberOptionBase<int, int>
{
    protected override int ToS(int v) => v;
    protected override int Add(int a, int b) => a + b;
    protected override int Minus(int v) => -v;
    protected override bool GreaterThan(int a, int b) => a > b;
    protected override bool IsSameValue(int a, int b) => a == b;

    public NumberOptionInt(Func<int> getValue, Action<int> setValue, Func<string> name, int interval, int? min, int? max, Func<int, string>? formatValue) : base(getValue, setValue, name, interval, min, max, formatValue) { }
}

internal class NumberOptionFloat : NumberOptionBase<float, string>
{
    private readonly int? digit;
    protected override string ToS(float v)
    {
        if (digit != null) v = MathF.Round(v, (int)digit, MidpointRounding.AwayFromZero);
        return $"{v}";
    }
    protected override float Add(float a, float b) => a + b;
    protected override float Minus(float v) => -v;
    protected override bool GreaterThan(float a, float b) => a > b;
    protected override bool IsSameValue(float a, float b) => MathF.Abs(a - b) < 1e-6f;

    public NumberOptionFloat(Func<float> getValue, Action<float> setValue, Func<string> name, float floaterval, float? min, float? max, int? digit, Func<string, string>? formatValue) : base(getValue, setValue, name, floaterval, min, max, formatValue)
    {
        this.digit = digit;
    }
}

