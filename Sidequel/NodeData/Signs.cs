
using Sidequel.Dialogue;
using Sidequel.System;

namespace Sidequel.NodeData;
internal class RubberFlower : StartNodeEntry
{
    protected override string StartNode => "BucketSignStart";
    internal const string Tag = "sign.RubberFlower";
    protected override Node[] Nodes => [new(Tag, [
        line(1, Original),
        @if(() => NodeDone(Tag), "1", null),
        line(2, Original),
        line(3, Original, anchor: "1"),
        line(4, Original),
        @if(() => _L, "low"),
        @if(() => NodeYet(Tag), lines(1, 6, digit2("HMFirst"), Player)),
        lines(7, 7, digit2("HM", ""), Player),
        done(),
        end(),
        anchor("low"),
        lines(7, 9, digit2("L", ""), Player),
    ])];
}

internal class OrangeIslandsSign : StartNodeEntry
{
    protected override string StartNode => "OrangeIslandsInfoStart";
    protected override Node[] Nodes => [new("sign.OrangeIsland", [
        lines(1, 11, digit2, [10, 11]),
    ])];
}

internal class Boat : StartNodeEntry
{
    protected override string StartNode => "NoBoatKeyStart";
    internal const string Tag = "sign.Boat";
    protected override Node[] Nodes => [new(Tag, [
        lines(1, 1, digit2, Player),
    ])];
}

