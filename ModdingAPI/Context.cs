
using UnityEngine.SceneManagement;

namespace ModdingAPI;

using UnityScene = UnityEngine.SceneManagement.Scene;

#pragma warning disable IDE1006
public static class Context
{
    public static readonly string SceneUndefined = "<Undefined Scene>";
    public static class Scene
    {
        public static string CurrentScene { get => SceneName; }
        internal const string _TitleScene = "TitleScene";
        internal const string _GameScene = "GameScene";
        internal const string _CreditsScene = "CreditsScene";
        public static string TitleScene => _TitleScene;
        public static string GameScene => _GameScene;
        public static string CreditsScene => _CreditsScene;
    }
    public static string SceneName { get; private set; } = SceneUndefined;
    public static bool OnTitle { get; private set; } = false;
    public static bool OnCredits { get; private set; } = false;
    public static bool GameStarted { get; private set; } = false;
    public static bool IsQuitting { get; internal set; } = false;

    public static GameServiceLocator gameServiceLocator { get => Singleton<GameServiceLocator>.instance; }
    public static ServiceLocator serviceLocator { get => Singleton<ServiceLocator>.instance; }
    public static GlobalData globalData { get => Singleton<GlobalData>.instance; }
    public static LevelController levelController { get => gameServiceLocator.levelController; }
    public static UI ui { get => gameServiceLocator.ui; }
    public static LevelUI levelUI { get => gameServiceLocator.levelUI; }

    private static Player? playerCache = null;
    public static Player player { get => playerCache ??= levelController.player; }
    public static Player? Player { get { try { return player; } catch { return null; } } }
    public static bool TryToGetPlayer(out Player player) => (player = Player!) != null;
    public static bool CanPlayerMove { get; private set; } = false;
    internal static void UpdateScene(UnityScene scene)
    {
        Monitor.SLog($"New Scene \"{scene.name}\"", LogLevel.Debug);
        SceneName = scene.name;
        switch (SceneName)
        {
            case Scene._TitleScene:
                OnTitle = true;
                GameStarted = false;
                playerCache = null;
                CanPlayerMove = false;
                OnCredits = false;
                break;
            case Scene._GameScene:
                OnTitle = false;
                GameStarted = true;
                break;
            case Scene._CreditsScene:
                OnTitle = false;
                GameStarted = false;
                OnCredits = true;
                break;
        }
    }

    private static int immovableFrames = 0;
    public static readonly int PlayerImmovableDelay = 5;
    internal static void UpdateCanPlayerMove()
    {
        if (!GameStarted) return;
        var canMove = Player?.input?.hasFocus ?? false;
        if (canMove)
        {
            immovableFrames = 0;
            CanPlayerMove = true;
        }
        else if (CanPlayerMove)
        {
            immovableFrames++;
            if (immovableFrames >= PlayerImmovableDelay) CanPlayerMove = false;
        }
    }
}
#pragma warning restore IDE1006

