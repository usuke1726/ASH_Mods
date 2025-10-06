
using SideStory.Dialogue;

namespace SideStory.NodeData;

internal class Dummy : NodeEntry
{
    protected override bool IsGlobal => true;
    protected override Node[] Nodes => [
        new([
            line("node.dummy1", Player),
            line("node.dummy2", Original),
            emote(Emotes.EyesClosed, Original),
            line("node.dummy3", Original),
            emote(Emotes.Normal, Original),
            option(["node.dummy4.1", "node.dummy4.2"]),
            @if(() => LastSelected == 0, "yes", "no"),
            anchor("yes"),
            emote(Emotes.Happy, Original),
            line("node.dummy5.y", Original),
            line("node.dummy6.y", Original),
            item(Items.GoldenFeather),
            line("node.dummy7.y", Original),
            emote(Emotes.Normal, Original),
            end(),
            anchor("no"),
            emote(Emotes.Surprise, Original),
            line("node.dummy5.n", Original),
            emote(Emotes.Normal, Original),
            end(),
        ], priority: int.MinValue),
    ];
}

