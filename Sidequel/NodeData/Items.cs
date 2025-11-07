
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Binoculars : NodeEntry
{
    internal static bool activated = false;
    internal const string CannotUse = "Binoculars.CannotUse";
    protected override Characters? Character => null;
    protected override Node[] Nodes => [
        new(CannotUse, [
            command(() => activated = false),
            line("item.Binoculars.cannotUse", Player, useId: false),
        ], condition: () => activated, priority: int.MaxValue),
    ];
}

