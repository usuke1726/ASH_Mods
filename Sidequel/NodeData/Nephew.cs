
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Nephew : NodeEntry
{
    internal const string BeforeJA1 = "Nephew.BeforeJA1";
    internal const string BeforeJA2 = "Nephew.BeforeJA2";
    internal const string AfterJA1 = "Nephew.AfterJA1";
    internal const string AfterJA2 = "Nephew.AfterJA2";
    internal const string GoldMedalEvent1 = "Nephew.GoldMedalEvent1";
    internal const string GoldMedalEvent2 = "Nephew.GoldMedalEvent2";
    protected override Characters? Character => Characters.RunningNephew;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 5, digit2, [2]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 4, digit2, [1, 4]),
        ], condition: () => _bJA && NodeDone(BeforeJA1)),

        new(AfterJA1, [
            lines(1, 12, digit2, [3, 6, 8, 9, 12]),
            cont(-3),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            lines(1, 4, digit2, [1, 4]),
        ], condition: () => _aJA && NodeDone(AfterJA1)),

        new(GoldMedalEvent1, [
            lines(1, 17, digit2, [1, 2, 4, 5, 9, 11, 12, 15, 16], [
                new(9, emote(Emotes.Surprise, Player)),
                new(10, emote(Emotes.Normal, Player)),
                new(17, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeYet(GoldMedalEvent1)),

        new(GoldMedalEvent2, [
            lines(1, 4, digit2, [1, 4]),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeDone(GoldMedalEvent1)),
    ];
}

