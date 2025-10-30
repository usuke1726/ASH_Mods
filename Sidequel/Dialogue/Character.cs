
using ModdingAPI;

namespace Sidequel.Dialogue;

internal static class Character
{
    public static readonly string Player = "Player";
    public static readonly string Original = "Original";
    public static bool TryGetCharacter(IConversation conversation, string name, out ModdingAPI.Character character)
    {
        if (name == Original)
        {
            return ModdingAPI.Character.TryGet(conversation.originalSpeaker, out character);
        }
        else if (name == Player)
        {
            character = ModdingAPI.Character.Get(Characters.Claire);
            return true;
        }
        else if (name == "Claire")
        {
            character = Sidequel.Character.Core.Claire;
            return true;
        }
        else return ModdingAPI.Character.TryGet(name, out character);
    }
}

