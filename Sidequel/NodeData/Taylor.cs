
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;
internal class Taylor : NodeEntry
{
    internal const string Start1 = "Taylor.Start1";
    internal const string Start2 = "Taylor.Start2";
    internal const string BeforeJA = "Taylor.BeforeJA";
    internal const string AfterJA1 = "Taylor.AfterJA1";
    internal const string AfterJA2 = "Taylor.AfterJA2";
    internal const string GoldMedal1 = "Taylor.GoldMedal1";
    internal const string GoldMedal2 = "Taylor.GoldMedal2";
    protected override Characters? Character => Characters.Taylor;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 12, digit2, [2, 4, 10], [
                new(6, emote(Emotes.Happy, Original)),
                new(9, emote(Emotes.Normal, Original)),
            ]),
            @if(() => _HM,
                lines(13, 13, digit2("HM", ""), [13]),
                lines(13, 14, digit2("L", ""), [13], [
                    new(14, emote(Emotes.Happy, Original)),
                ])
            ),
            done(),
        ], condition: () => NodeYet(Start1), priority: 10),

        new(Start2, [
            lines(1, 23, digit2, [1, 3, 4, 6, 7, 8, 16, 17, 18, 23]),
            done(),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2), priority: 10),

        new(BeforeJA, [
            lines(1, 3, digit2, [3]),
        ], condition: () => NodeDone(Start2) && _bJA),

        new(AfterJA1, [
            lines(1, 2, digit2, []),
            @if(() => _HM,
                lines(3, 4, digit2("HM", ""), [3, 4]),
                lines(3, 4, digit2("L", ""), [3, 4])
            ),
            lines(5, 19, digit2, [8, 12, 18, 19]),
            cont(-3),
            done(),
        ], condition: () => _aJA && NodeDone(Start2) && NodeYet(AfterJA1)),

        new(AfterJA2, [
            lines(1, 2, digit2, []),
            lineif(() => _HM, "HM03", "L03", Player),
        ], condition: () => _aJA && NodeDone(AfterJA1)),

        new(GoldMedal1, [
            lines(1, 19, digit2, [1, 2, 3, 6, 12, 16, 17, 18]),
            done(),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeYet(GoldMedal1), priority: 5),

        new(GoldMedal2, [
            lines(1, 3, digit2, [2]),
        ], condition: () => NodeActive(Const.Events.GoldMedal) && NodeDone(GoldMedal1), priority: 5),
    ];
}

