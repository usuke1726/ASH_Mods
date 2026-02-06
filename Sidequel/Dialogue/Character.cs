
using ModdingAPI;

namespace Sidequel.Dialogue;

internal static class Character
{
    public static readonly string Player = "Player";
    public static readonly string Original = "Original";
    public static bool TryGetCharacter(IConversation conversation, string name, out ModdingAPI.Character character)
    {
        if (conversation != null && name == Original)
        {
            return ModdingAPI.Character.TryGet(conversation.originalSpeaker, out character);
        }
        else return TryGetCharacter(name, out character);
    }
    public static bool TryGetCharacter(string name, out ModdingAPI.Character character)
    {
        if (name == Player)
        {
            character = ModdingAPI.Character.Get(Characters.Claire);
            return true;
        }
        else if (Sidequel.Character.Core.TryGetNPC(name, false, out var npc))
        {
            character = npc;
            return true;
        }
        else return ModdingAPI.Character.TryGet(name, out character);
    }
    public static ModdingAPI.Character Get(Characters ch)
    {
        if (Sidequel.Character.Core.TryGetNPC((int)ch, out var npc)) return npc;
        else return ModdingAPI.Character.Get(ch);
    }
}

