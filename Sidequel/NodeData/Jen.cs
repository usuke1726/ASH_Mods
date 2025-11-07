
using HarmonyLib;
using ModdingAPI;
using QuickUnityTools.Input;
using Sidequel.Dialogue;
using Sidequel.System;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Jen : NodeEntry
{
    internal const string BeforeJA1 = "Jen.BeforeJA1";
    internal const string BeforeJA2 = "Jen.BeforeJA2";
    internal const string CuteCan = "Jen.CuteCan";
    internal const string AfterJA1 = "Jen.AfterJA1";
    internal const string AfterJA1Accept = "Jen.AfterJA1.O1";
    internal const string AfterJA1Refuse = "Jen.AfterJA1.O2";
    internal const string AfterJA2 = "Jen.AfterJA2";
    internal const string ScaleEventNoScale = "Jen.ScaleEvent.NoScale";
    internal const string ScaleEvent1 = "Jen.ScaleEvent.Grade1";
    internal const string ScaleEvent2 = "Jen.ScaleEvent.Grade2";
    internal const string ScaleEvent3 = "Jen.ScaleEvent.Grade3";
    internal const string AfterScaleEvent = "Jen.AfterScaleEvent";
    protected override Characters? Character => Characters.Jen;
    private bool ScaleEventActive => NodeIP(Const.Events.ScaleEvent);
    internal static int LastGotBestScale { get => STags.GetInt(Const.STags.LastGotBestFishScale); set => STags.SetInt(Const.STags.LastGotBestFishScale, value); }
    private bool Has1 => Items.Has(Items.FishScale1);
    private bool Has2 => Items.Has(Items.FishScale2);
    private bool Has3 => Items.Has(Items.FishScale3);
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 6, digit2, [3, 6]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 3, digit2, [2]),
        ], condition: () => _bJA && NodeDone(BeforeJA1)),

        new(CuteCan, [
            lineif(() => _HM, "HM01", "L01", Player),
            lines(2, 10, digit2, [2, 3, 6, 7, 8, 9]),
            lineif(() => _HM, "HM11", "L11", Player),
            done(),
        ], condition: () => NodeYet(AfterJA1) && NodeYet(CuteCan) && Items.Has(Items.CuteEmptyCan), priority: 10),

        new(AfterJA1, [
            lines(1, 2, digit2, [], replacer: Const.formatJATrigger),
            @if(() => _HM, lines(3, 6, digit2("HM", ""), [3, 5]), lines(3, 7, digit2("L", ""), [3, 4, 7])),
            line(8, Original),
            option(["O1", "O2"]),
            command(() => SetNext(LastSelected == 0 ? AfterJA1Accept : AfterJA1Refuse)),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA1Accept, [
            lines(1, 7, digit2, [4, 5]),
            state(Const.Events.ScaleEvent, NodeStates.InProgress),
            done(AfterJA1),
        ], condition: () => false),

        new(AfterJA1Refuse,[
            lines(1, 2, digit2, []),
            lineif(() => _HM, "HM03", "L03", Player),
            state(AfterJA1, NodeStates.Refused),
        ], condition: () => false),

        new(AfterJA2, [
            line(1, Original),
            option(["O1", "O2"]),
            command(() => SetNext(LastSelected == 0 ? AfterJA1Accept : AfterJA1Refuse)),
        ], condition: () => NodeRefused(AfterJA1)),

        new(ScaleEventNoScale, [
            lines(1, 3, digit2, [2]),
            lineif(() => _HM, "HM04", "L04", Player),
        ], condition: () => ScaleEventActive && LastGotBestScale == 0),

        new(ScaleEvent1, [
            lines(1, 7, digit2, [2, 5, 6], [new(3, wait(1f))]),
            lineif(() => _HM, "HM08", "L08", Player),
            command(() => LastGotBestScale = 0),
        ], condition: () => ScaleEventActive && LastGotBestScale == 1),

        new(ScaleEvent2, [
            lines(1, 8, digit2, [2, 5, 6, 8], [new(3, wait(1f))]),
            tag(Const.STags.HasShownFishScale2ToJen, true),
            command(() => LastGotBestScale = 0),
        ], condition: () => ScaleEventActive && LastGotBestScale == 2),

        new(ScaleEvent3, [
            command(() => Assert(Items.Has(Items.FishScale3), "does not have FishScale3!!")),
            lines(1, 5, digit2, [2, 5], [
                new(3, wait(1f)),
                new(5, item(Items.FishScale3, -1)),
            ]),
            lineif(() => _HM, "HM06", "L06", Player),
            lines(7, 9, digit2, [9], [new(9, item(Items.Pencil, 1))]),
            lineif(() => _HM, "HM10", "L10", Player),
            line(11, Original),
            done(Const.Events.ScaleEvent),
        ], condition: () => ScaleEventActive && LastGotBestScale == 3),

        new(AfterScaleEvent, [
            line(1, Player),
            lineif(() => _HM, "HM02", "L02", Player),
            lines(3, 5, digit2, [4]),
            lineif(() => _HM, "HM06", "L06", Player),
        ], condition: () => NodeDone(Const.Events.ScaleEvent)),
    ];
}

internal class AfterFishing : NodeEntry
{
    protected override Characters? Character => null;

    internal const string FishScaleNodeEntry = "AfterFishing.FishScaleEntry";
    internal const string FishScale1 = "AfterFishing.FishScale1";
    internal const string FishScale2 = "AfterFishing.FishScale2";
    internal const string FishScale3 = "AfterFishing.FishScale3";
    internal const string FishScale4 = "AfterFishing.FishScale4";
    internal const string FishScale5 = "AfterFishing.FishScale5";
    internal const string FishScale6 = "AfterFishing.FishScale6";
    internal static bool hasCaughtFishJustNow = false;
    internal static Fish caughtFish = null!;
    internal static bool IsReady => NodeIP(Const.Events.ScaleEvent);
    private string GradeDownScaleNode => NodeDone(FishScale5) ? FishScale6 : FishScale5;
    private string? scale = null;
    protected override Node[] Nodes => [
        new(FishScaleNodeEntry, [
            command(() => {
                hasCaughtFishJustNow = false;
                SetGettingScale();
                if(!GetBool(Const.STags.HasGotFishScaleOnce)){
                    STags.SetBool(Const.STags.HasGotFishScaleOnce);
                    SetNext(FishScale1);
                    return;
                }
                if(scale == null){
                    SetNext(GradeDownScaleNode);
                    return;
                }
                if(scale == Items.FishScale3){
                    SetNext(FishScale4);
                    return;
                }
                if(scale == Items.FishScale2){
                    SetNext(Items.Has(Items.FishScale3) ? GradeDownScaleNode : FishScale3);
                }else{
                    SetNext(Items.Has(Items.FishScale3) || Items.Has(Items.FishScale2) ? GradeDownScaleNode : FishScale2);
                }
            }),
        ], condition: () => hasCaughtFishJustNow && IsReady, priority: int.MaxValue),

        new(FishScale1, [
            lines(1, 1, digit2, Player),
            command(() => {
                if(scale == null) Jen.LastGotBestScale = 1;
                else if(scale == Items.FishScale1) Jen.LastGotBestScale = 1;
                else if(scale == Items.FishScale2) Jen.LastGotBestScale = 2;
                else if(scale == Items.FishScale3) Jen.LastGotBestScale = 3;
            }),
            item(() => scale ?? Items.FishScale1),
            reset(),
        ], condition: () => false),
        new(FishScale2, [
            lines(1, 1, digit2, Player),
            command(() => Jen.LastGotBestScale = 1),
            item(() => Items.FishScale1),
            reset(),
        ], condition: () => false),
        new(FishScale3, [
            lines(1, 1, digit2, Player),
            command(() => Jen.LastGotBestScale = 2),
            item(() => Items.FishScale2),
            reset(),
        ], condition: () => false),
        new(FishScale4, [
            lines(1, 1, digit2, Player),
            command(() => Jen.LastGotBestScale = 3),
            item(() => Items.FishScale3),
            reset(),
        ], condition: () => false),
        new(FishScale5, [
            lines(1, 2, digit2, Player),
            done(),
        ], condition: () => false),
        new(FishScale6, [
            lines(1, 1, digit2, Player),
        ], condition: () => false),
    ];

#pragma warning disable IDE1006
    private Dialogue.Actions.UpdateNodeStateAction reset() => new(FishScale5, NodeStates.NotYet);
#pragma warning restore IDE1006
    private void SetGettingScale()
    {
        var fish = caughtFish;
        var rare = fish.rare;
        var bound = Mathf.Max(fish.species.size.max - fish.species.size.min, 0.01f);
        var sizeRatio = Mathf.Clamp((fish.size - fish.species.size.min) / bound, 0, 1);
        Tuple<float, float, float> borders = (rare, sizeRatio) switch
        {
            (true, < 0.4f) or (false, < 0.1f) => new(0.7f, 0.9f, 1f),
            (true, < 0.9f) or (false, < 0.4f) => new(0.4f, 0.7f, 1f),
            (true, _) or (false, < 0.7f) => new(0.1f, 0.4f, 0.9f),
            _ => new(0.05f, 0.2f, 0.7f),
        };
        var f = UnityEngine.Random.value;
        if (f < borders.Item1) scale = Items.FishScale3;
        else if (f < borders.Item2) scale = Items.FishScale2;
        else if (f < borders.Item3) scale = Items.FishScale1;
        else scale = null;
    }
}

[HarmonyPatch(typeof(FishCollectPrompt))]
internal class FishCollectPromptPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Update")]
    internal static bool Patch(FishCollectPrompt __instance)
    {
        if (!State.IsActive || !AfterFishing.IsReady) return true;
        var input = Traverse.Create(__instance).Field("input").GetValue<FocusableUserInput>();
        if (input.WasDismissPressed())
        {
            AfterFishing.hasCaughtFishJustNow = true;
            GameObject.Destroy(__instance.gameObject);
            Dialogue.DialogueController.instance.StartConversation(null);
            return false;
        }
        return true;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("Setup")]
    internal static void Setup(Fish fish)
    {
        if (!State.IsActive || !AfterFishing.IsReady) return;
        //Debug($"FishCollectPrompt.Setup called");
        AfterFishing.caughtFish = fish;
    }
}

