
using System.Collections;
using ModdingAPI;

namespace SideStory.Dialogue.Actions;

public enum Emotes
{
    Normal,
    PlayerWideEyes,
    Happy,
    Surprise,
    EyesClosed,
}

internal class EmoteAction : BaseAction
{
    internal readonly Emotes emotion;
    internal readonly string speaker;
    public EmoteAction(Emotes emotion, string speaker, string? anchor = null) : base(ActionType.Emote, anchor)
    {
        this.emotion = emotion;
        this.speaker = speaker;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
        if (Character.TryGetCharacter(conversation, speaker, out var character))
        {
            switch (emotion)
            {
                case Emotes.Normal:
                    if (character.character == Characters.Claire) SideStory.Character.Core.EmoteHalfEyes();
                    else character.ClearEmotion();
                    break;
                case Emotes.PlayerWideEyes:
                    if (character.character == Characters.Claire) SideStory.Character.Core.EmoteNormalEyes();
                    break;
                case Emotes.Happy: character.ShowEmotion(Emotion.Happy); break;
                case Emotes.Surprise: character.ShowEmotion(Emotion.Surprise); break;
                case Emotes.EyesClosed: character.ShowEmotion(Emotion.EyesClosed); break;
            }
        }
        else
        {
            Debug($"failed to get character!!");
        }
        yield break;
    }
}

