
using SideStory.Dialogue.Actions;
using SideStory.Item;

namespace SideStory.Dialogue.Data;

internal class Dummy : DataEntry
{
    internal static void Setup()
    {
        NodeSelector.RegisterNode(new([
            new LineAction("node.dummy1", Player),
            new LineAction("node.dummy2", Original),
            new EmoteAction(Emotes.EyesClosed, Original),
            new LineAction("node.dummy3", Original),
            new EmoteAction(Emotes.Normal, Original),
            new OptionAction(["node.dummy4.1", "node.dummy4.2"]),
            new IfAction(() => LastSelected == 0, "yes", "no"),
            new Anchor("yes"),
            new EmoteAction(Emotes.Happy, Original),
            new LineAction("node.dummy5.y", Original),
            new LineAction("node.dummy6.y", Original),
            new GetItemAction(Items.GoldenFeather),
            new LineAction("node.dummy7.y", Original),
            new EmoteAction(Emotes.Normal, Original),
            new NodeCompleteAction(),
            new Anchor("no"),
            new EmoteAction(Emotes.Surprise, Original),
            new LineAction("node.dummy5.n", Original),
            new EmoteAction(Emotes.Normal, Original),
            new NodeCompleteAction(),
        ], priority: int.MinValue));
    }
}

