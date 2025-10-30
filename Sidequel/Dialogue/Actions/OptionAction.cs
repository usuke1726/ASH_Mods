
using System.Collections;
using ModdingAPI;

namespace Sidequel.Dialogue.Actions;

internal class OptionAction : BaseAction
{
    internal readonly string[] options;
    internal int selected = -1;
    private readonly Func<string, string> formatter;
    public OptionAction(string[] options, Func<string, string>? replacer = null, string? anchor = null) : base(ActionType.Option, anchor)
    {
        this.options = options;
        formatter = replacer == null ? I18n : s => replacer(I18n(s));
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        conversation.currentSpeaker = Context.player.transform;
        yield return conversation.ShowOptions([.. options.Select(formatter)], idx => selected = idx);
    }
}

