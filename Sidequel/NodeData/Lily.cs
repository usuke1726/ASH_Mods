
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Lily : NodeEntry
{
    protected override Characters? Character => (Characters)Const.Object.LilyObjectId;
    internal const string Start1 = "Lily.Start1";
    internal const string Start2 = "Lily.Start2";
    internal const string Start3 = "Lily.Start3";
    internal const string Start3Imm = "Lily.Start3Imm";
    internal const string BuyFirst = "Lily.BuyFirst";
    internal const string Buy = "Lily.Buy";
    internal const string Start3Nothing = "Lily.Start3Nothing";
    private float lastBoughtTime;
    private bool TalkedImmediately => Time.time - lastBoughtTime < 10f;
    private static int num;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 11, digit2, [1, 5], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(7, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
                new(10, emote(Emotes.EyesClosed, Original)),
                new(11, emote(Emotes.Normal, Original)),
            ]),
            state(NodeStates.Refused),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "agreed"),
            lines(1, 6, digit2("O2"), []),
            end(),
            anchor("agreed"),
            lines(1, 5, digit2("O1"), [3], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
            ]),
            option(["O1.O1", "O1.O2"]),
            @if(() => LastSelected == 0, "agreed2"),
            lines(1, 2, digit2("O1.O2"), []),
            end(),
            anchor("agreed2"),
            next(() => BuyFirst),
        ], condition: () => NodeYet(Start1)),

        new(BuyFirst, [
            emote(Emotes.Happy, Original),
            line(1, Original),
            emote(Emotes.Normal, Original),
            @if(() => Items.CoinsNum < 5, "ShortOnCash"),
            item(Items.Coin, -5),
            item(Items.RubberFlowerSapling),
            lines(2, 14, digit2, [5, 6, 8, 13], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(4, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
                new(14, emote(Emotes.Happy, Original)),
            ]),
            done(Start1),
            command(() => lastBoughtTime = Time.time),
            end(),
            anchor("ShortOnCash"),
            lines(1, 5, digit2("ShortOnCash"), [1, 2, 3]),
        ], condition: () => false),

        new(Start2, [
            line(1, Original),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "buy"),
            line("O2.01", Original),
            end(),
            next(() => BuyFirst, anchor: "buy"),
        ], condition: () => NodeRefused(Start1)),

        new(Start3Imm, [
            line(1, Original),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "buy"),
            next(() => Start3Nothing),
            end(),
            anchor("buy"),
            emote(Emotes.Happy, Original),
            line("O1.01", Original),
            emote(Emotes.Normal, Original),
            next(() => Buy),
        ], condition: () => NodeDone(Start1) && TalkedImmediately, priority: 10),

        new(Start3, [
            line(1, Original),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "buy"),
            next(() => Start3Nothing),
            end(),
            anchor("buy"),
            emote(Emotes.Happy, Original),
            line("O1.01", Original),
            emote(Emotes.Normal, Original),
            next(() => Buy),
        ], condition: () => NodeDone(Start1)),

        new(Buy, [
            line(1, Original),
            @switch(() => Items.CoinsNum switch {
                >= 15 => "3",
                >= 10 => "2",
                >= 5 => "1",
                _ => "ShortOnCash",
            }),
            option(["O3", "O2", "O1"], anchor: "3"),
            command(() => num = 3 - LastSelected),
            @goto("buy"),
            option(["O2", "O1"], anchor: "2"),
            command(() => num = 2 - LastSelected),
            @goto("buy"),
            option(["O1"], anchor: "1"),
            command(() => num = 1),
            anchor("buy"),
            @if(() => num == 1,
                line("num1", Original),
                @if(() => num == 2,
                    line("num2", Original),
                    line("num3", Original)
                )
            ),
            line(2, Original),
            item(() => [Items.Coin, Items.RubberFlowerSapling], () => [-5*num, num]),
            lines(3, 4, digit2, [3], [new(4, emote(Emotes.Happy, Original))]),
            command(() => lastBoughtTime = Time.time),
            end(),
            anchor("ShortOnCash"),
            lines(1, 3, digit2("ShortOnCash"), [1, 3]),
        ], condition: () => false),

        new(Start3Nothing, [
            lines(1, 2, digit2, [], [new(2, emote(Emotes.Happy, Original))]),
        ], condition: () => false),
    ];
}

