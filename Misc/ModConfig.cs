
using IGenericModConfigMenu;
using ModdingAPI;

namespace Misc;

internal class ModConfig()
{
    public bool EnableInfinityStamina { get; set; } = true;
    public bool EnableSuperJump { get; set; } = true;
    public bool EnableInfinityChest { get; set; } = true;
    public bool EnableChestBoostReproduction { get; set; } = true;
    public bool EnableTurbo { get; set; } = true;
    public ModConfig(ModConfig modConfig) : this()
    {
        EnableInfinityStamina = modConfig.EnableInfinityStamina;
        EnableSuperJump = modConfig.EnableSuperJump;
        EnableInfinityChest = modConfig.EnableInfinityChest;
        EnableChestBoostReproduction = modConfig.EnableChestBoostReproduction;
        EnableTurbo = modConfig.EnableTurbo;
    }
    internal static ModConfig config = new();
    internal static void Setup(IMod mod)
    {
        mod.Helper.Events.Gameloop.GameLaunched += (_, _) => RegisterModConfig(mod);
    }
    private static void RegisterModConfig(IMod mod)
    {
        var configMenu = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(Info.ModID);
        if (configMenu is null) return;
        configMenu.Register(
            mod: mod,
            reset: () =>
            {
                config = new();
                TurboClaire.OnEnabledChanged();
            },
            import: c =>
            {
                config = new(c);
                TurboClaire.OnEnabledChanged();
            },
            export: () => config,
            displayName: "Misc"
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "InfinityStamina",
            getValue: () => config.EnableInfinityStamina,
            setValue: val => config.EnableInfinityStamina = val
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "SuperJump",
            getValue: () => config.EnableSuperJump,
            setValue: val => config.EnableSuperJump = val
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "InfinityChest",
            getValue: () => config.EnableInfinityChest,
            setValue: val => config.EnableInfinityChest = val
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "ChestBoost",
            getValue: () => config.EnableChestBoostReproduction,
            setValue: val => config.EnableChestBoostReproduction = val
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "TurboClaire",
            getValue: () => config.EnableTurbo,
            setValue: val =>
            {
                config.EnableTurbo = val;
                TurboClaire.OnEnabledChanged();
            }
        );
    }
}

