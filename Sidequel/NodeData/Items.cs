
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

internal class RubberFlowerSapling : NodeEntry
{
    internal static bool cannnotPlantActivated = false;
    internal static bool shouldNotPlantActivated = false;
    internal const string CannotPlant = "RubberFlowerSapling.CannotPlant";
    internal const string ShouldNotPlant = "RubberFlowerSapling.ShouldNotPlant";
    protected override Characters? Character => null;
    protected override Node[] Nodes => [
        new(CannotPlant, [
            command(() => cannnotPlantActivated = false),
            line("item.RubberFlowerSapling.cannotPlant", Player, useId: false),
        ], condition: () => cannnotPlantActivated, priority: int.MaxValue),
        new(ShouldNotPlant, [
            command(() => shouldNotPlantActivated = false),
            line("item.RubberFlowerSapling.shouldNotPlant", Player, useId: false),
        ], condition: () => shouldNotPlantActivated, priority: int.MaxValue),
    ];
}

