
using IGenericModConfigMenu;
using ModdingAPI;

namespace GenericModConfigMenu;

internal class ModConfig()
{
    public bool ShowCurrentNumber { get; set; } = true;
    public bool ShowCurrentSelection { get; set; } = true;
    public ModConfig(ModConfig config) : this()
    {
        ShowCurrentNumber = config.ShowCurrentNumber;
        ShowCurrentSelection = config.ShowCurrentSelection;
    }
    internal static ModConfig config = new();
    internal static void Register(IMod mod, IGenericModConfigMenuApi api)
    {
        api.Register(
            mod: mod,
            displayName: "GMCM Options",
            reset: () => config = new(),
            import: c => config = new(c),
            export: () => config
        );
        api.AddBoolOption(
            mod: mod,
            name: () => "Show current number",
            getValue: () => config.ShowCurrentNumber,
            setValue: val => config.ShowCurrentNumber = val
        );
        api.AddBoolOption(
            mod: mod,
            name: () => "Show current selection",
            getValue: () => config.ShowCurrentSelection,
            setValue: val => config.ShowCurrentSelection = val
        );
    }
}

