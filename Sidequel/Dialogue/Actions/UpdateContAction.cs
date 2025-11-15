
using System.Collections;

namespace Sidequel.Dialogue.Actions;

internal class UpdateContAction : BaseAction, IInvokableInAction
{
    private readonly int value;
    private readonly Func<bool>? condition;
#if DEBUG
    private static int totalCont = 0;
#endif
    public UpdateContAction(int value, Func<bool>? condition = null, string? anchor = null) : base(ActionType.Cont, anchor)
    {
        this.value = value;
        this.condition = condition;
#if DEBUG
        totalCont += value;
#endif
    }
    public override IEnumerator Invoke(IConversation conversation)
    {
        if (condition != null && !condition()) yield break;
        Cont.Add(value);
        yield break;
    }
#if DEBUG
    internal static void Debug_PrintTotalCont()
    {
        Debug($"== TOTAL CONT: {totalCont}", LL.Warning);
    }
#endif
}

