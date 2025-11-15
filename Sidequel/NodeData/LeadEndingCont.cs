

using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class LeadEndingCont : NodeEntry
{
    protected override Characters? Character => null;
    protected override Node[] Nodes => [];
    internal const string Id = "LeadEndingCont";
    internal static Node node = new(Id, [
        done(),
        wait(1f),
        lines(1, 6, digit2, Player, [
            new(3, wait(1f)),
            new(6, wait(1f)),
        ]),
        lineif(() => GetBool(Const.STags.HasClimbedPeakOnce), "HasClimbedOnce.07", "HasNotClimbed.07", Player),
    ]);
    internal static bool IsActive => Cont.IsEndingCont && NodeYet(Id);
}

