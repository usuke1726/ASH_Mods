
using ModdingAPI;
using ModdingAPI.KeyBind;
using QuickUnityTools.Input;
using UnityEngine;

namespace SideStory.System;

internal class EndGameController : MonoBehaviour
{
    public static readonly string WonGameTag = "_SYSTEM.WonGame";
    private static EndGameController instance = null!;
    private Transform closingCutscene = null!;
    private EndGameCutscene cutscene = null!;
    private GameUserInput inputLock = null!;
    private string versionString = "";
    private bool versionStringSet = false;
    public bool isRunning { get; private set; } = false;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            instance = new GameObject("SideStoryEndGameController").AddComponent<EndGameController>();
        };
        KeyBind.RegisterKeyBind("Alpha5(LeftControl)", EndGame, name: "(Debug)EndGame");
    }
    internal void Awake()
    {
        closingCutscene = GameObject.Find("Cutscenes").transform.Find("ClosingCutscene");
        cutscene = closingCutscene.Find("EndGameTrigger").GetComponent<EndGameCutscene>();
    }
    internal static void EndGame()
    {
        if (!Context.GameStarted || !State.IsActive) return;
        EndSpeedrun();
        instance.EndGameBody();
    }
    private static void EndSpeedrun()
    {
        if (instance.versionStringSet) return;
        instance.versionStringSet = true;
        if (LevelController.speedrunClockActive)
        {
            instance.versionString = VersionInfo.Load().version;
        }
    }
    private void EndGameBody()
    {
        inputLock = GameUserInput.CreateInput(gameObject);
        cutscene.versionString.text = versionString;
        closingCutscene.gameObject.SetActive(true);
        closingCutscene.Find("Canvas").gameObject.SetActive(true);
        STags.SetBool(WonGameTag);
        Context.gameServiceLocator.levelController.gameRunning = false;
        cutscene.timeline.Play();
        this.RegisterTimer((float)cutscene.timeline.duration, () =>
        {
            cutscene.defaultSnapshot.TransitionTo(0.05f);
            LevelController.SaveAndShowCredits();
        });
        cutscene.quietSnapshot.TransitionTo(2f);
        this.RegisterTimer(2f, () =>
        {
            Singleton<MusicManager>.instance.UnregisterAll();
            Singleton<MusicManager>.instance.TrimRetiredActiveMusicSets(0.01f);
        });
        isRunning = true;
    }
}

