
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class PictureFox : NodeEntry
{
    internal const string BeforeJA1 = "PictureFox.BeforeJA1";
    internal const string BeforeJA2 = "PictureFox.BeforeJA2";
    internal const string AfterJA1 = "PictureFox.AfterJA1";
    internal const string AfterJA1Hold = "PictureFox.AfterJA1.O1";
    internal const string AfterJA1Accept = "PictureFox.AfterJA1.O2";
    internal const string AfterJA1Refuse = "PictureFox.AfterJA1.O3";
    internal const string AfterJA2 = "PictureFox.AfterJA2";
    internal const string AfterJA3 = "PictureFox.AfterJA3";
    internal const string AfterJA4 = "PictureFox.AfterJA4";
    internal const string AfterJA5 = "PictureFox.AfterJA5";
    protected override Characters? Character => Characters.PictureFox1;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 14, digit2, [1, 2, 4, 6, 7, 10, 11, 14], [
                new(3, emote(Emotes.Surprise, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(12, emote(Emotes.EyesClosed, Original)),
                new(13, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 6, digit2, [1]),
        ], condition: () => _bJA && NodeDone(BeforeJA1)),

        new(AfterJA1, [
            line(1, Original),
            lineif(() => _HM, "HM02", "L02", Player),
            line(3, Original, replacer: Const.formatJATrigger),
            lines(4, 8, digit2, [7]),
            @if(() => NodeYet(BeforeJA1), lines(1, 3, digit2("NotBeforeJA1Done"), [2])),
            line(9, Original),
            @if(() => NodeDone(BeforeJA1), line(10, Player)),
            option(["O1", "O2", "O3"]),
            next(() => LastSelected switch{
                1 => AfterJA1Accept,
                2 => AfterJA1Refuse,
                _ => AfterJA1Hold,
            }),
        ], condition: () => _aJA && NodeYet(AfterJA1)),

        new(AfterJA1Hold, [
            lines(1, 3, digit2, [3], [new(2, emote(Emotes.Happy, Original))]),
            state(AfterJA1, NodeStates.InProgress),
        ], condition: () => false),

        new(AfterJA1Refuse, [
            lines(1, 2, digit2, []),
            lineif(() => _HM, "HM03", "L03", Player),
            state(AfterJA1, NodeStates.Refused),
        ], condition: () => false),

        new(AfterJA1Accept, [
            lines(1, 5, digit2, [5], [
                new(1, emote(Emotes.Surprise, Original)),
                new(2, emote(Emotes.Happy, Original)),
                new(3, item([Items.GoldenFeather, Items.Coin], [-1, 40])),
                new(3, emote(Emotes.Normal, Original)),
                new(4, item(Items.Binoculars)),
                new(4, emote(Emotes.Happy, Original)),
            ]),
            command(() => acceptedTime = Time.time),
            cont(-5),
            done(AfterJA1),
        ], condition: () => false),

        new(AfterJA2, [
            line(1, Original),
            option(["O1", "O2", "O3"]),
            next(() => LastSelected switch{
                1 => AfterJA1Accept,
                2 => AfterJA1Refuse,
                _ => AfterJA1Hold,
            }),
        ], condition: () => NodeIP(AfterJA1)),

        new(AfterJA3, [
            lines(1, 5, digit2, [2, 4], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Surprise, Original)),
            ]),
            done(),
        ], condition: () => NodeDone(AfterJA1) && (NodeYet(AfterJA3) || TalkedToImmediately)),

        new(AfterJA4, [
            lines(1, 2, digit2, []),
        ], condition: () => NodeRefused(AfterJA1)),

        new(AfterJA5, [
            lines(1, 4, digit2, [4], [new(3, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeDone(AfterJA3) && !TalkedToImmediately),
    ];

    private float acceptedTime = -1;
    private bool TalkedToImmediately => acceptedTime > 0 && Time.time - acceptedTime < 30f;
}

