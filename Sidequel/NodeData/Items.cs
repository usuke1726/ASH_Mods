
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
    internal static bool cannnotWorkActivated = false;
    internal static bool shouldNotPlantActivated = false;
    internal static bool flowerNotFoundActivated = false;
    internal static bool shouldGetCloserActivated = false;
    internal static bool shouldNotRemoveActivated = false;
    internal const string CannotWork = "RubberFlowerSapling.CannotWork";
    internal const string ShouldNotPlant = "RubberFlowerSapling.ShouldNotPlant";
    internal const string FlowerNotFound = "RubberFlowerSapling.FlowerNotFound";
    internal const string ShouldGetCloser = "RubberFlowerSapling.ShouldGetCloser";
    internal const string ShouldNotRemove = "RubberFlowerSapling.ShouldNotRemove";
    protected override Characters? Character => null;
    protected override Node[] Nodes => [
        new(CannotWork, [
            command(() => cannnotWorkActivated = false),
            line("item.RubberFlowerSapling.cannotWork", Player, useId: false),
        ], condition: () => cannnotWorkActivated, priority: int.MaxValue),
        new(ShouldNotPlant, [
            command(() => shouldNotPlantActivated = false),
            line("item.RubberFlowerSapling.shouldNotPlant", Player, useId: false),
        ], condition: () => shouldNotPlantActivated, priority: int.MaxValue),
        new(FlowerNotFound, [
            command(() => flowerNotFoundActivated = false),
            line("item.RubberFlowerSapling.notFoundFlower", Player, useId: false),
        ], condition: () => flowerNotFoundActivated, priority: int.MaxValue),
        new(ShouldGetCloser, [
            command(() => shouldGetCloserActivated = false),
            line("item.RubberFlowerSapling.shouldGetCloser", Player, useId: false),
        ], condition: () => shouldGetCloserActivated, priority: int.MaxValue),
        new(ShouldNotRemove, [
            command(() => shouldNotRemoveActivated = false),
            line("item.RubberFlowerSapling.shouldNotRemove", Player, useId: false),
        ], condition: () => shouldNotRemoveActivated, priority: int.MaxValue),
    ];
}

