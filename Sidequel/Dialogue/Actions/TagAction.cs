
using System.Collections;
using Sidequel.System;

namespace Sidequel.Dialogue.Actions;

internal class TagAction : BaseAction, IInvokableInAction
{
    internal readonly string id;
    internal readonly Func<object> getValue;
    public TagAction(string id, Func<object> getValue, string? anchor = null) : base(ActionType.Tag, anchor)
    {
        this.id = id;
        this.getValue = getValue;
    }
    public TagAction(string id, object value, string? anchor = null) : this(id, () => value, anchor) { }
    public override IEnumerator Invoke(IConversation conversation)
    {
        var value = getValue();
        if (value is Tuple<TagActions, object> valueWithType)
        {
            if (valueWithType.Item1 == TagActions.Add)
            {
                var v = valueWithType.Item2;
                if (v is int @int) STags.AddInt(id, @int);
                else if (v is float @float) STags.AddFloat(id, @float);
            }
        }
        else if (value is TagActions t && t == TagActions.Toggle) STags.ToggleBool(id);
        else STags.Set(id, value);
        yield break;
    }
}

