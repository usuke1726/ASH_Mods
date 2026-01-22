
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class RumorGuy : NodeEntry
{
    internal const string BeforeJA1 = "RumorGuy.BeforeJA1";
    internal const string BeforeJA2 = "RumorGuy.BeforeJA2";
    internal const string BeforeJA3 = "RumorGuy.BeforeJA3";
    internal const string BeforeJA4 = "RumorGuy.BeforeJA4";
    internal const string AfterJA1 = "RumorGuy.AfterJA1";
    internal const string AfterJA2 = "RumorGuy.AfterJA2";
    internal const string AfterJA3 = "RumorGuy.AfterJA3";
    internal const string CuteEmptyCan = "RumorGuy.CuteEmptyCan";
    internal static bool TalkedAboutWatch => NodeDone(BeforeJA3) || NodeDone(AfterJA2);
    protected override Characters? Character => Characters.RumorGuy;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 3, digit2, [2]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "refuse"),
            lines(4, 18, digit2, [6, 9, 11, 15, 16, 18], [
                new(14, emote(Emotes.Happy, Original)),
                new(17, emote(Emotes.Normal, Original)),
            ]),
            done(),
            end(),
            anchor("refuse"),
            line("O2.01", Original),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 11, digit2, [1, 4, 8, 11]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 16, digit2, [2, 7, 8, 11, 14, 16], [
                new(15, emote(Emotes.Happy, Original)),
            ]),
            cont(-3),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA2) && NodeYet(BeforeJA3)),

        new(BeforeJA4, [
            lines(1, 4, digit2, [2, 4]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA3)),

        new(AfterJA1, [
            done(),
            cont(-3),
            lines(1, 4, digit2, [], [new(4, emote(Emotes.Happy, Original))], replacer: Const.formatJATrigger),
            emote(Emotes.Normal, Original),
            @if(() => NodeDone(BeforeJA4),
                lines(1, 4, digit2("BeforeJA4Done"), [1]),
                lines(1, 2, digit2("BeforeJA4Yet"), [1])
            ),
            lines(5, 12, digit2, [8, 10]),
            @switch(() => {
                if(!GetBool(Const.STags.HasCheckedChestOnce)) return "hasNotChecked";
                if(GetBool(Const.STags.HasGotItemFromChestOnce)) return "hasGotItem";
                return "hasNotGotItem";
            }),
            anchor("hasNotChecked"),
            lines(1, 1, digit2("HasNotChecked"), [1]),
            end(),
            anchor("hasGotItem"),
            lines(1, 2, digit2("HasGotItem"), [1]),
            end(),
            anchor("hasNotGotItem"),
            lines(1, 2, digit2("HasNotGotItem"), [1]),
            end(),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA2, [
            lines(1, 4, digit2, [1]),
            @if(() => GetBool(Const.STags.TalkedAboutWilOnce),
                lines(1, 2, digit2("WilKnown"), [1], [
                    new(2, emote(Emotes.Happy, Original)),
                ]),
                lines(1, 4, digit2("WilUnknown"), [1, 3], [
                    new(2, emote(Emotes.Happy, Original)),
                    new(4, emote(Emotes.Normal, Original)),
                ])
            ),
            emote(Emotes.Normal, Original),
            lines(5, 6, digit2, []),
            @if(() => GetBool(Const.STags.HasShownSomethingToWil),
                lines(1, 3, digit2("ShownToWil"), [1, 3]),
                lines(1, 1, digit2("NotShownToWil"), [1])
            ),
            lines(7, 8, digit2, []),
            @if(() => NodeDone(BeforeJA3),
                lines(1, 2, digit2("DeborahKnown"), [1], [
                    new(2, emote(Emotes.Happy, Original)),
                ]),
                lines(1, 6, digit2("DeborahUnknown"), [1, 2, 6], [
                    new(3, emote(Emotes.Happy, Original)),
                    new(4, emote(Emotes.Normal, Original)),
                ])
            ),
            lines(9, 12, digit2, [12], [
                new(9, emote(Emotes.Happy, Original)),
                new(10, emote(Emotes.Normal, Original)),
            ]),
            @if(() => _L, lines(13, 14, digit2("L", ""), [13], [
                new(14, emote(Emotes.Happy, Original)),
            ])),
            cont(-3),
            done(),
        ], condition: () => NodeDone(AfterJA1) && NodeYet(AfterJA2)),

        new(AfterJA3, [
            lines(1, 4, digit2, [1, 4]),
        ], condition: () => NodeDone(AfterJA2)),

        new(CuteEmptyCan, [
            line(1, Original),
            @if(() => GetInt(Const.STags.ItemCountFromChest) > 1,
                lines(1, 2, digit2("MoreThanOne"), Player),
                lines(1, 1, digit2("OnlyOne"), Player)
            ),
            lines(2, 7, digit2, [4], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "accept"),
            lines(1, 1, digit2("O2"), []),
            state(NodeStates.Refused),
            end(),
            anchor("accept"),
            lines(1, 4, digit2("O1"), [4], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(3, item([Items.CuteEmptyCan, Items.Coin], [-1, 5])),
                new(3, emote(Emotes.Happy, Original)),
            ]),
            cont(-3),
            done(),
        ], condition: () => NodeDone(AfterJA1) && Items.Has(Items.CuteEmptyCan) && NodeYet(CuteEmptyCan), priority: 5),
    ];
}

