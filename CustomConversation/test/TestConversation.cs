
namespace CustomConversation.test;

using CustomConversation;
using ModdingAPI;
using ModdingAPI.IO;
using ModdingAPI.KeyBind;

internal static class TestConversation
{
    private static string id = null!;
    // private static readonly string demoFileName = "data_expressions.txt";
    private static readonly string demoFileName = "data_demo1.txt";
    private static TextFile file = null!;
    [Conditional("DEBUG")]
    internal static void Setup(IMod mod)
    {
        file = mod.TextFile(demoFileName);
        //KeyBind.RegisterKeyBind("Alpha0 Alpha0", () =>
        //{
        //    if (!Context.CanPlayerMove) return;
        //    TryStart();
        //}, name: "CONVERSATION TEST");
    }
    internal static void TryStart()
    {
        if (Load()) ConversationRegistry.TryStart(id);
    }
    internal static bool Load() => ConversationRegistry.Register(file, out id);
}

