
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Claire : NodeEntry
{
    protected override Characters? Character => (Characters)Const.Object.ClaireObjectId;
    internal const string BeforeJA1 = "Claire.BeforeJA1";
    internal const string BeforeJA2 = "Claire.BeforeJA2";
    internal const string BeforeJA3 = "Claire.BeforeJA3";
    internal const string BeforeJA4 = "Claire.BeforeJA4";
    internal const string AfterJA1 = "Claire.AfterJA1";
    internal const string AfterJA2 = "Claire.AfterJA2";
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(digit2, [1, 4, 5, 7, 8, 13, 14, 17, 18]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 15, digit2, [1, 4, 5, 7, 8, 11, 14, 15], [
                new(6, emote(Emotes.Happy, Original)),
                new(9, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 17, digit2, [2, 4, 5, 7, 8, 10, 13, 15, 17], [new(14, emote(Emotes.Happy, Original))]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA2) && NodeYet(BeforeJA3)),

        new(BeforeJA4, [
            lines(1, 6, digit2, [3, 5], [new(4, emote(Emotes.Happy, Original))]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA3)),

        new(AfterJA1, [
            lines(1, 2, digit2, [2]),
            line(3, Original, replacer: Const.formatJATrigger),
            lines(4, 14, digit2, [4, 6, 9, 11, 12]),
            item(Items.Coin, 50),
            lines(15, 22, digit2, [15, 16, 18, 19, 21, 22]),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            lines(1, 2, digit2, Original),
            @switch(() => Items.CoinsSavedUp ? "SavedUp" : Items.CoinsNum switch {
                >= 300 => "ge300",
                >= 200 => "ge200",
                >= 100 => "ge100",
                _ => "lt100"
            }),
            lines(1, 4, digit1("SavedUp"), [1, 2, 4], [new(3, emote(Emotes.Happy, Original))], anchor: "SavedUp"),
            end(),
            lines(1, 4, digit1("ge300"), [1, 2, 4], [new(3, emote(Emotes.Happy, Original))], anchor: "ge300"),
            end(),
            lines(1, 3, digit1("ge200"), [1, 3], [new(2, emote(Emotes.Happy, Original))], anchor: "ge200"),
            end(),
            lines(1, 2, digit1("ge100"), [1], [new(2, emote(Emotes.Happy, Original))], anchor: "ge100"),
            lineif(() => _HM, "ge100HM.3", "ge100L.3", Player),
            end(),
            lines(1, 2, digit1("lt100"), [1], anchor: "lt100"),
            lineif(() => _HM, "lt100HM.3", "lt100L.3", Player),
            end(),
        ], condition: () => _aJA && NodeDone(AfterJA1)),
    ];
}

