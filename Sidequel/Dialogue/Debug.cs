
#if DEBUG
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Dialogue;

internal class Debug : MonoBehaviour
{
    private string textCache = null!;
    private float lastShownTime = 0;
    private const float TimeInterval = 1.0f;
    public string Text
    {
        get
        {
            if (Time.time - lastShownTime < TimeInterval) return textCache;
            lastShownTime = Time.time;
            return textCache = __CurrentText();
        }
        set => __UpdateText(value);
    }
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            new GameObject("DebugSidequel_Dialogue").AddComponent<Debug>();
        };
    }
    public string __CurrentText()
    {
        var content = GetContent();
        if (content == null) return null!;
        else return content.textMesh.text.Replace("\n", "\\n");
    }
    public void __UpdateText(string text)
    {
        text = text.Replace("\\n", "\n");
        var content = GetContent();
        if (content == null) return;
        content.Reset(text);
        textCache = text;
    }
    private TextBoxContent? GetContent()
    {
        try
        {
            var conv = Traverse.Create(DialogueController.instance).Field("currentConversation").GetValue<TextBoxConversation>();
            if (conv == null || !conv.isAlive) return null;
            var box = Traverse.Create(conv).Field("floatingBox").GetValue<FloatingBox>();
            if (box == null) return null;
            var content = Traverse.Create(box).Field("currentContent").GetValue<IFloatingBoxContent>() as TextBoxContent;
            if (content != null) return content;
            content = Traverse.Create(box).Field("upcomingContent").GetValue<IFloatingBoxContent>() as TextBoxContent;
            if (content != null) return content;
            throw new("content is not TextBoxContent");
        }
        catch (Exception e)
        {
            Monitor.Log($"error on get content (debug): {e}", LL.Warning, true);
            return null;
        }
    }
}
#endif

