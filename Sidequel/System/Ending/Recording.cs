
#if false
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.System.Ending;

internal class Recording
{
    private static PlayerRecorder recorder = null!;
    internal static bool IsRecording { get; private set; } = false;
    internal static void Toggle()
    {
        if (IsRecording) Stop(); else Start();
    }
    internal static void Start()
    {
        IsRecording = true;
        var obj = GameObject.Find("LevelSingletons/Services").transform.Find("Cheats/RecorderDummy");
        obj.gameObject.SetActive(true);
        recorder = obj.GetComponent<PlayerRecorder>();
        Traverse.Create(recorder).Field("player").SetValue(Context.player);
        recorder.StartRecording();
        Debug($"=== START RECORDING!!!", LL.Warning);
    }
    internal static void Stop()
    {
        IsRecording = false;
        recorder.StopRecording();
        var frames = recorder.replayData.frames;
        Monitor.Log($"====== RECORDING END ======\nframes:\n\n{string.Join("\n", frames.Select(Race.Deserializer.SerializeFrame))}\n\n=================================================\n\n", LL.Warning);
        recorder.NewRecording();
    }
}
#endif

