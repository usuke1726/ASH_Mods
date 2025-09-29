
namespace ModdingAPI.Events;

#pragma warning disable IDE1006
public class UpdatedEventArgs : EventArgs
{
    public readonly float time;
    public readonly float deltaTime;
    public UpdatedEventArgs() : base()
    {
        time = UnityEngine.Time.time;
        deltaTime = UnityEngine.Time.deltaTime;
    }
}
public class TitleScreenUpdatedEventArgs(TitleScreen titleScreen) : UpdatedEventArgs()
{
    public readonly TitleScreen titleScreen = titleScreen;
}
public class PlayerUpdatedEventArgs(Player player) : UpdatedEventArgs()
{
    public readonly Player player = player;
}
#pragma warning restore IDE1006

public interface IGameloopEvents
{
    event EventHandler<EventArgs>? ModsLoaded;
    event EventHandler<EventArgs>? GameLaunched;
    event EventHandler<TitleScreenUpdatedEventArgs>? TitleScreenUpdated;
    event EventHandler<EventArgs>? GameStarted;
    event EventHandler<EventArgs>? CreditsStarted;
    event EventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;
    event EventHandler<TitleScreenUpdatedEventArgs>? BeforeTitleScreenUpdated;
    event EventHandler<PlayerUpdatedEventArgs>? BeforePlayerUpdated;
    event EventHandler<EventArgs>? GameQuitting;
    event EventHandler<EventArgs>? ReturnedToTitle;
}

internal class GameloopEvents : IGameloopEvents
{
    internal static GameloopEvents instance = new();
    public event EventHandler<EventArgs>? ModsLoaded;
    public event EventHandler<EventArgs>? GameLaunched;
    public event EventHandler<TitleScreenUpdatedEventArgs>? TitleScreenUpdated;
    public event EventHandler<EventArgs>? GameStarted;
    public event EventHandler<EventArgs>? CreditsStarted;
    public event EventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;
    public event EventHandler<TitleScreenUpdatedEventArgs>? BeforeTitleScreenUpdated;
    public event EventHandler<PlayerUpdatedEventArgs>? BeforePlayerUpdated;
    public event EventHandler<EventArgs>? GameQuitting;
    public event EventHandler<EventArgs>? ReturnedToTitle;
    internal static void OnModsLoaded() => instance.ModsLoaded?.Invoke(null, null!);
    internal static void OnGameLaunched() => instance.GameLaunched?.Invoke(null, null!);
    internal static void OnTitleScreenUpdated(TitleScreen i) => instance.TitleScreenUpdated?.Invoke(null, new(i));
    internal static void OnGameStarted() => instance.GameStarted?.Invoke(null, null!);
    internal static void OnCreditsStarted() => instance.CreditsStarted?.Invoke(null, null!);
    internal static void OnPlayerUpdated(Player i) => instance.PlayerUpdated?.Invoke(null, new(i));
    internal static void OnBeforeTitleScreenUpdated(TitleScreen i) => instance.BeforeTitleScreenUpdated?.Invoke(null, new(i));
    internal static void OnBeforePlayerUpdated(Player i) => instance.BeforePlayerUpdated?.Invoke(null, new(i));
    internal static void OnGameQuitting() => instance.GameQuitting?.Invoke(null, null!);
    internal static void OnReturnedToTitle() => instance.ReturnedToTitle?.Invoke(null, null!);
}

