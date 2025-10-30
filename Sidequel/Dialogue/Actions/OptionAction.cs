
using System.Collections;
using ModdingAPI;

namespace Sidequel.Dialogue.Actions;

internal class OptionAction : BaseAction
{
    internal readonly string[] options;
    internal int selected = -1;
    private readonly Func<string, string> formatter;
    private readonly bool useId;
    public OptionAction(string[] options, Func<string, string>? replacer = null, bool useId = true, string? anchor = null) : base(ActionType.Option, anchor)
    {
        this.options = options;
        this.useId = useId;
        formatter = replacer == null ? s => I18n(s, useId) : s => replacer(I18n(s, useId));
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        conversation.currentSpeaker = Context.player.transform;
        yield return conversation.ShowOptions([.. options.Select(formatter)], idx => selected = idx);
    }
}

