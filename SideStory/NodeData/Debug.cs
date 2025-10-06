
#if DEBUG
using System.Collections;
using ModdingAPI;
using SideStory.Dialogue;
using SideStory.Dialogue.Actions;

namespace SideStory.NodeData;

internal class Debug : GlobalNodeEntry
{
    private class Mes(string line) : BaseAction(ActionType.Line)
    {
        internal override IEnumerator Invoke(IConversation conversation)
        {
            conversation.currentSpeaker = Context.player.transform;
            yield return conversation.ShowLine(line);
        }
    }
    protected override Node[] Nodes => [
        new([new Mes("THIS MESSAGE\nSHOULD NOT APPEAR!")], priority: int.MinValue),
    ];
}
#endif

