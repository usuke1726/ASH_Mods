
using System.Collections;
using SideStory.System;

namespace SideStory.Dialogue.Actions;

internal class TagAction : BaseAction
{
    internal readonly string id;
    internal readonly object value;
    public TagAction(string id, object value, string? anchor = null) : base(ActionType.Tag, anchor)
    {
        this.id = id;
        this.value = value;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
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
        STags.Set(id, value);
        yield break;
    }
}

