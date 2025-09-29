
using HarmonyLib;
using UnityEngine;

namespace ModdingAPI;

public static class Notification
{
    public static bool Enabled { get => FPSCounterPatch.Enabled; }
    public static int QueueSize { get => FPSCounterPatch.QueueSize; }
    public static int MaxQueueSize { get => FPSCounterPatch.MaxQueueSize; }

    // 数字だと1行15文字(ただしフォントは等幅ではないので i とかだともっと(31文字くらい)入る
    // 改行は普通に反映される．高さ制限はなく，画面を突っ切るほど高くできる
    // 日本語は使用不可．(フォントも英語フォントが使用される)
    public static void Push(string message, float time = 3.0f) => FPSCounterPatch.Push(message, time);
}

[HarmonyPatch(typeof(FPSCounter))]
internal class FPSCounterPatch
{
    public static bool Enabled { get; private set; } = false;
    public static int QueueSize { get => reservedMessages.Count; }
    public static readonly int MaxQueueSize = 50;
    private static string message = "";
    private static FPSCounter? instance = null;
    private static bool fpsHadBeenShowing = false;
    private static float lastTime = 0f;
    private static readonly Queue<Tuple<string, float>> reservedMessages = [];

    [HarmonyPrefix()]
    [HarmonyPatch("OnEnable")]
    public static bool Prefix(FPSCounter __instance)
    {
        instance = __instance;
        __instance.visible = GameSettings.showFPS;
        __instance.StartCoroutine(FPSPatch(__instance));
        return false;
    }
    internal static void Push(string text, float time)
    {
        if (instance == null) return;
        if (Enabled)
        {
            if (QueueSize < MaxQueueSize)
            {
                reservedMessages.Enqueue(new Tuple<string, float>(text, time));
            }
            return;
        }
        fpsHadBeenShowing = instance.visible;
        instance.visible = true;
        Enabled = true;
        lastTime = time;
        message = text;
        TryToSetText(instance, message);
    }
    private static System.Collections.IEnumerator FPSPatch(FPSCounter __instance)
    {
        var tr = Traverse.Create(__instance);
        while (true)
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return tr.Field("waitCommand").GetValue();
            float num = Time.realtimeSinceStartup - lastTime;
            int num2 = Time.frameCount - lastFrameCount;
            var fps = Mathf.RoundToInt((float)num2 / num);
            tr.Property("fps").SetValue(fps);
            if (__instance.visible)
            {
                var text = $"{fps} fps";
                if (Enabled)
                {
                    if (fpsHadBeenShowing) text += $"\n{message}";
                    else text = message;
                }
                Monitor.SLogBepIn($"Notified:\n{text}");
                TryToSetText(__instance, text);
            }
        }
    }
    private static void TryToSetText(FPSCounter i, string text)
    {
        try
        {
            i.text.text = text;
        }
        catch (Exception e)
        {
            Monitor.SLog($"Error on setting text to FPSCounter:\n{e}", LogLevel.Error);
        }
    }
    internal static void Update()
    {
        if (!Enabled) return;
        lastTime -= Time.deltaTime;
        if (lastTime < 0)
        {
            if (reservedMessages.TryDequeue(out var item))
            {
                message = item.Item1;
                lastTime = item.Item2;
                if (instance != null) TryToSetText(instance, message);
                return;
            }
            Enabled = false;
            if (instance != null)
            {
                instance.visible = fpsHadBeenShowing;
            }
            fpsHadBeenShowing = false;
        }
    }
}

