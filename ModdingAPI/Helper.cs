
namespace ModdingAPI;

using ModdingAPI.Events;
using ModdingAPI.KeyBind;

public interface IModHelper
{
    IModEvents Events { get; }
    IModRegistry ModRegistry { get; }
    IMod Mod { get; }
    IKeyBindingsData KeyBindingsData { get; }
}

internal class Helper(Mod mod) : IModHelper
{
    private static readonly ModEvents events = new();
    public IModEvents Events { get => events; }
    public IModRegistry ModRegistry { get => ModdingAPI.ModRegistry.instance; }
    public IMod Mod { get; private set; } = mod;
    public IKeyBindingsData KeyBindingsData { get => Mod.KeyBindingsData; }
}

