
using IGenericModConfigMenu;

namespace BeachstickballPlus;

internal sealed class ModConfig()
{
    public bool DoubleBall { get; set; } = true;
    public bool EnemyWhackNoise { get; set; } = false;
    public bool SpecialDialogue { get; set; } = true;
    public ModConfig(ModConfig modConfig) : this()
    {
        DoubleBall = modConfig.DoubleBall;
        EnemyWhackNoise = modConfig.EnemyWhackNoise;
        SpecialDialogue = modConfig.SpecialDialogue;
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
            export: () => config,
            displayName: "Beachstickball+"
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "DoubleBall",
            getValue: () => config.DoubleBall,
            setValue: val =>
            {
                config.DoubleBall = val;
                if (!val) DoubleVolleyball.OnDisabled();
            }
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "EnemyWhackSound",
            getValue: () => config.EnemyWhackNoise,
            setValue: val => config.EnemyWhackNoise = val
        );
        configMenu.AddBoolOption(
            mod: this,
            name: () => "SpecialDialogue",
            getValue: () => config.SpecialDialogue,
            setValue: val => config.SpecialDialogue = val
        );
    }
}

