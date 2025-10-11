
using ModdingAPI;

namespace Sidequel;

internal static class State
{
    public static bool IsActive { get; private set; } = false;
    public static bool IsNewGame { get; private set; } = false;
    public static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            IsActive = false;
            IsNewGame = false;
        };
    }
    public static void Activate() => IsActive = true;
    public static void SetNewGame() => IsNewGame = true;
}

