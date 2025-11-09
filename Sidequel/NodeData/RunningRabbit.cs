
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class RunningRabbit : NodeEntry
{
    internal const string Feather = "RunningRabbit.Feather";
    internal const string Start1 = "RunningRabbit.Start1";
    internal const string Start2 = "RunningRabbit.Start2";
    internal const string Start3 = "RunningRabbit.Start3";
    internal const string AfterJA = "RunningRabbit.AfterJA";
    internal const string HighMidGoldMedal1 = "RunningRabbit.HighMidGoldMedal1";
    internal const string LowGoldMedal1 = "RunningRabbit.LowGoldMedal1";
    internal const string GoldMedalReaction = "RunningRabbit.GoldMedalReaction";
    internal const string GoldMedal2 = "RunningRabbit.GoldMedal2";
    internal const string GoldMedal3 = "RunningRabbit.GoldMedal3";
    internal const string AfterGoldMedal = "RunningRabbit.AfterGoldMedal";
    protected override Characters? Character => Characters.RunningRabbit;
    protected override Node[] Nodes => [
        new(Feather, [
            lines(1, 5, digit2, [1, 2, 5]),
            done(),
        ], condition: () => NodeYet(RunningGoat.BeforeJA1) && NodeYet(Feather)),

        new(Start1, [
            lines(1, 3, digit2, []),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            lines(1, 5, digit2, [3, 5]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(Start3, [
            lines(1, 3, digit2, [3]),
        ], condition: () => NodeDone(Start2), priority: -1),

        new(AfterJA, [
            lines(1, 2, digit2, [], replacer: Const.formatJATrigger),
            lines(3, 8, digit2, [4, 8]),
            lineif(() => _HM, "HM09", "L09", Player),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA), priority: 1),

        new(HighMidGoldMedal1, [
            line(1, Player),
            @if(() => NodeDone(Feather),
                lines(1, 2, digit2("FeatherDone"), [1, 2]),
                lines(1, 1, digit2("FeatherYet"), [1])
            ),
            lines(2, 7, digit2, [3, 5, 6, 7]),
            next(() => GoldMedalReaction),
        ], condition: () => _aJA && Items.Has(Items.GoldMedal) && NodeYet(Const.Events.GoldMedal) && _HM),

        new(LowGoldMedal1, [
            lines(1, 4, digit2, [1, 2, 4]),
            next(() => GoldMedalReaction),
        ], condition: () => _aJA && Items.Has(Items.GoldMedal) && NodeYet(Const.Events.GoldMedal) && _L),

        new(GoldMedalReaction, [
            lines(1, 20, digit2, [3, 10, 14, 15, 16, 18, 19], [
                new(17, emote(Emotes.Happy, Original)),
                new(18, emote(Emotes.Normal, Original)),
                new(20, emote(Emotes.Happy, Original)),
            ]),
            state(Const.Events.GoldMedal, NodeStates.InProgress),
            tag(Const.STags.GoldMedalTriggeredByGoat, false),
        ], condition: () => false),

        new(GoldMedal2, [
            lines(1, 3, digit2, [2]),
        ], condition: () => NodeActive(Const.Events.GoldMedal)),

        new(GoldMedal3, [
            lines(1, 9, digit2, [1, 3, 4, 6, 7, 9], [
                new(8, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeS1(Const.Events.GoldMedal) && NodeYet(GoldMedal3), priority: 10),

        new(AfterGoldMedal, [
            lineif(() => GetBool(Const.STags.GoldMedalTriggeredByGoat), "TriggeredByGoat.01", "TriggeredByRabbit.01", Original),
            lines(2, 6, digit2, [3, 4, 6], [
                new(5, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => NodeDone(Const.Events.GoldMedal), priority: 10),
    ];
}

