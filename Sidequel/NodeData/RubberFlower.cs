
using Sidequel.Dialogue;
using Sidequel.System;

namespace Sidequel.NodeData;
internal class RubberFlower : StartNodeEntry
{
    protected override string StartNode => "BucketSignStart";
    private static readonly string viewedOnceTag = "Node.RubberFlowerSignViewed";
    protected override Node[] Nodes => [new("sign.bucketsign", [
        line("1", Original),
        @if(() => STags.GetBool(viewedOnceTag), "1", null),
        line("2", Original),
        line("3", Original, anchor: "1"),
        line("4", Original),
        @if(() => STags.GetBool(viewedOnceTag), "2", null),
        line("5", Original),
        line("6", Original),
        lines(7, 11, i => $"{i}", Player),
        line("12", Player, anchor: "2"),
        tag(viewedOnceTag, true)
    ])];
}

