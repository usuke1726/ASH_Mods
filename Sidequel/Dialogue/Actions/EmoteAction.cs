
using System.Collections;
using ModdingAPI;

namespace Sidequel.Dialogue.Actions;

internal class EmoteAction : BaseAction, IInvokableInAction
{
    internal readonly Emotes emotion;
    internal readonly string speaker;
    public EmoteAction(Emotes emotion, string speaker, string? anchor = null) : base(ActionType.Emote, anchor)
    {
        this.emotion = emotion;
        this.speaker = speaker;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        Emote(conversation, speaker, emotion);
        yield break;
    }
    internal static void Emote(IConversation conversation, string speaker, Emotes emotion)
    {
        if (Character.TryGetCharacter(conversation, speaker, out var character))
        {
            switch (emotion)
            {
                case Emotes.Normal:
                    if (character.character == Characters.Claire) Sidequel.Character.Core.EmoteHalfEyes();
                    character.ClearEmotion();
                    break;
                case Emotes.PlayerWideEyes:
                    if (character.character == Characters.Claire) Sidequel.Character.Core.EmoteNormalEyes();
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
    }
}

