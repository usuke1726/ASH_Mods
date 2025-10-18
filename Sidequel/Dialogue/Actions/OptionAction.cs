
using System.Collections;
using ModdingAPI;

namespace Sidequel.Dialogue.Actions;

internal class OptionAction : BaseAction
{
    internal readonly string[] options;
    internal int selected = -1;
    public OptionAction(string[] options, string? anchor = null) : base(ActionType.Option, anchor)
    {
        this.options = options;
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        conversation.currentSpeaker = Context.player.transform;
        yield return conversation.ShowOptions([.. options.Select(I18n)], idx => selected = idx);
    }
}

