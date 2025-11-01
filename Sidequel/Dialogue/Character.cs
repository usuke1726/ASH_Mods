
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
        else return TryGetCharacter(name, out character);
    }
    public static bool TryGetCharacter(string name, out ModdingAPI.Character character)
    {
        if (name == Player)
        {
            character = ModdingAPI.Character.Get(Characters.Claire);
            return true;
        }
        else if (name == Const.Object.Claire || name == "Claire")
        {
            character = Sidequel.Character.Core.Claire;
            return true;
        }
        else return ModdingAPI.Character.TryGet(name, out character);
    }
    public static ModdingAPI.Character Get(Characters ch)
    {
        if ((int)ch == Const.Object.ClaireObjectId) return Sidequel.Character.Core.Claire;
        else return ModdingAPI.Character.Get(ch);
    }
}

