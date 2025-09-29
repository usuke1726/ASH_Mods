
using IGenericModConfigMenu;

namespace ClearView;

internal sealed class ModConfig()
{
    public bool Enabled { get; set; } = true;
    public bool NoCloud { get; set; } = true;
    public bool NoBorder { get; set; } = true;
    public int FogLevel { get; set; } = 0;
    public ModConfig(ModConfig modConfig) : this()
    {
        Enabled = modConfig.Enabled;
        NoCloud = modConfig.NoCloud;
        NoBorder = modConfig.NoBorder;
        FogLevel = modConfig.FogLevel;
    }
}

partial class ModEntry
{
    internal static ModConfig config = new();
    private void RegisterGenericModConfig()
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(Info.ModID);
        if (configMenu is null) return;
        configMenu.Register(
            mod: this,
            reset: () => config = new ModConfig(),
            import: c => config = new ModConfig(c),
            export: () => config
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "Enabled",
            getValue: () => config.Enabled,
            setValue: val =>
            {
                config.Enabled = val;
                UpdateEnabled(val);
            }
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "NoCloud",
            getValue: () => config.NoCloud,
            setValue: val =>
            {
                config.NoCloud = val;
                UpdateNoCloud(val);
            }
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "NoBorder",
            getValue: () => config.NoBorder,
            setValue: val =>
            {
                config.NoBorder = val;
            }
        );
        configMenu.AddNumberOption(
            mod: this,
            name: () => "FogLevel",
            getValue: () => config.FogLevel,
            setValue: val =>
            {
                config.FogLevel = val;
                UpdateFogLevel(val);
            },
            interval: 1,
            min: 0,
            max: 30
        );
    }
}

