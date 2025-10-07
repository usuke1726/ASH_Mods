
using SideStory.Dialogue;
using SideStory.System;

namespace SideStory.NodeData;
internal class RubberFlower : StartNodeEntry
{
    protected override string StartNode => "BucketSignStart";
    private static readonly string viewedOnceTag = "Node.RubberFlowerSignViewed";
    protected override Node[] Nodes => [new([
        line("node.sign.bucketsign1", Original),
        @if(() => STags.GetBool(viewedOnceTag), "1", null),
        line("node.sign.bucketsign2", Original),
        line("node.sign.bucketsign3", Original, anchor: "1"),
        line("node.sign.bucketsign4", Original),
        @if(() => STags.GetBool(viewedOnceTag), "2", null),
        line("node.sign.bucketsign5", Original),
        line("node.sign.bucketsign6", Original),
        lines(7, 11, i => $"node.sign.bucketsign{i}", Player),
        line("node.sign.bucketsign12", Player, anchor: "2"),
        tag(viewedOnceTag, true)
    ])];
}

