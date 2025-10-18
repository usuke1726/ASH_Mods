
#if DEBUG
using System.Collections;
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.Dialogue.Actions;

namespace Sidequel.NodeData;

internal class Debug : GlobalNodeEntry
{
    private class Mes(string line) : BaseAction(ActionType.Line)
    {
        public override IEnumerator Invoke(IConversation conversation)
        {
            conversation.currentSpeaker = Context.player.transform;
            yield return conversation.ShowLine(line);
        }
    }
    protected override Node[] Nodes => [
        new("_debug", [new Mes("THIS MESSAGE\nSHOULD NOT APPEAR!")], priority: int.MinValue),
    ];
}
#endif

