
using IGenericModConfigMenu;

namespace CustomBoatRace;
internal sealed class ModConfig()
{
    public bool Enabled { get; set; } = true;
    public string CourseId { get; set; } = "default";
    public ModConfig(ModConfig modConfig) : this()
    {
        Enabled = modConfig.Enabled;
        CourseId = modConfig.CourseId;
    }
}

partial class ModEntry
{
    internal static ModConfig config = new();
    public void RegisterGenericModConfig()
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
            setValue: val => config.Enabled = val
        );
        configMenu.AddSelectOption(
            mod: this,
            name: () => "CourseID",
            getValue: () => config.CourseId,
            setValue: val => config.CourseId = val,
            selection: [.. BoatRaceCourse.IDs]
        );
    }
}

