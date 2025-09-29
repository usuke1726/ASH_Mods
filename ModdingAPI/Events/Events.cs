
namespace ModdingAPI.Events;

public interface IModEvents
{
    IGameloopEvents Gameloop { get; }
    ISystemEvents System { get; }
}

internal class ModEvents : IModEvents
{
    public IGameloopEvents Gameloop { get; } = GameloopEvents.instance;
    public ISystemEvents System { get; } = SystemEvents.instance;
}

