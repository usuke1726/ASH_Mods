
using System.Collections;

namespace SideStory.Dialogue.Actions;

internal class TagAction : BaseAction
{
    internal enum Type { Add, Toggle, }
    internal readonly string id;
    internal readonly object value;
    public TagAction(string id, object value, string? anchor = null) : base(ActionType.Tag, anchor)
    {
        this.id = id;
        this.value = value;
    }
    internal override IEnumerator Invoke(IConversation conversation)
    {
        if (value is Tuple<Type, object> valueWithType)
        {
            if (valueWithType.Item1 == Type.Add)
            {
                var v = valueWithType.Item2;
                if (v is int @int) Tags.AddInt(id, @int);
                else if (v is float @float) Tags.AddFloat(id, @float);
            }
        }
        else if (value is Type t && t == Type.Toggle) Tags.ToggleBool(id);
        Tags.Set(id, value);
        yield break;
    }
}

