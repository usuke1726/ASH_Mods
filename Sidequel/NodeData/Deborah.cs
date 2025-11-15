
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Deborah : NodeEntry
{
    internal const string Start1 = "Deborah.Start1";
    internal const string Start2 = "Deborah.Start2";
    internal const string Low1 = "Deborah.Low1";
    internal const string Low2 = "Deborah.Low2";
    internal const string Low3 = "Deborah.Low3";
    internal const string Low4 = "Deborah.Low4";
    internal const string AntiqueFigure1 = "Deborah.AntiqueFigure1";
    internal const string BuyAntiqueFigure = "Deborah.AntiqueFigure1.O1";
    internal const string AntiqueFigure2 = "Deborah.AntiqueFigure2";
    internal const string GoldMedal = "Deborah.GoldMedal";
    protected override Characters? Character => Characters.WatchGoat;
    protected override Node[] Nodes => [
        new(Start1, [
            lines(1, 15, digit2, [2, 5, 6, 10, 11, 15], [
                new(12, emote(Emotes.Happy, Original)),
                new(13, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1), priority: 10),

        new(Start2, [
            lines(1, 4, digit2, [2]),
        ], condition: () => NodeDone(Start1), priority: -1),

        new(Low1, [
            lines(1, 11, digit2, [1, 2, 6, 11], [
                new(3, item(Items.WristWatch, -1)),
                new(3, emote(Emotes.Surprise, Original)),
                new(4, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
                new(10, emote(Emotes.Happy, Original)),
                new(11, item(Items.Coin, 50)),
            ]),
            cont(-10),
            done(),
        ], condition: () => NodeDone(Start1) && _L && NodeYet(Low1)),

        new(Low2, [
            lines(1, 9, digit2, [2, 6, 9], [
                new(8, emote(Emotes.EyesClosed, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Low1) && NodeYet(Low2), priority: 20),

        new(Low3, [
            lines(1, 19, digit2, [1, 2, 4, 5, 6, 8, 9, 10, 15, 16, 17], [
                new(4, emote(Emotes.EyesClosed, Player)),
                new(7, emote(Emotes.Surprise, Original)),
                new(8, emote(Emotes.Normal, Player)),
                new(11, emote(Emotes.Normal, Original)),
                new(14, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(Low2) && NodeYet(Low3), priority: 20),

        new(Low4, [
            lines(1, 5, digit2, [5], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(4, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => NodeDone(Low3), priority: -1),

        new(AntiqueFigure1, [
            lines(1, 26, digit2, [1, 2, 6, 7, 10, 11, 17, 18, 19, 23, 24], [
                new(16, emote(Emotes.Happy, Original)),
                new(17, emote(Emotes.Happy, Player)),
                new(18, emote(Emotes.Normal, Player)),
                new(18, emote(Emotes.Normal, Original)),
                new(21, emote(Emotes.Happy, Original)),
                new(23, emote(Emotes.Surprise, Player)),
                new(24, emote(Emotes.Normal, Player)),
                new(25, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "accept"),
            lines(1, 3, digit2("O2"), [3], [new(2, emote(Emotes.Happy, Original))]),
            state(NodeStates.Refused),
            end(),
            next(() => BuyAntiqueFigure, anchor: "accept"),
        ], condition: () => NodeYet(AntiqueFigure1) && Items.Has(Items.AntiqueFigure)),

        new(BuyAntiqueFigure, [
            lines(1, 10, digit2, [6, 7, 8, 10], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, item([Items.AntiqueFigure, Items.Coin], [-1, 60])),
                new(3, emote(Emotes.Normal, Original)),
                new(4, emote(Emotes.Happy, Original)),
            ]),
            cont(-5),
            done(AntiqueFigure1),
        ], condition: () => false),

        new(AntiqueFigure2, [
            line(1, Original),
            option(["O1", "O2"]),
            @if(() => LastSelected == 0, "accept"),
            lines(1, 2, digit2("O2"), [], [new(2, emote(Emotes.Happy, Original))]),
            state(NodeStates.Refused),
            end(),
            next(() => BuyAntiqueFigure, anchor: "accept"),
        ], condition: () => NodeRefused(AntiqueFigure1), priority: 30),

        new(GoldMedal, [
            lines(1, 8, digit2, [1, 2, 3, 5, 8]),
            @if(() => NodeDone(AntiqueFigure1), lines(1, 2, digit2("AntiqueFigureDone"), [2])),
            lines(9, 21, digit2, [12, 14, 15, 17, 18, 21]),
            done(),
        ], condition: () => NodeYet(GoldMedal) && NodeActive(Const.Events.GoldMedal)),
    ];
}

