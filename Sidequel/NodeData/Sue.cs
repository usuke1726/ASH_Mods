
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Sue : NodeEntry
{
    protected override Characters? Character => Characters.Sue;
    internal const string BeforeJA = "Sue.BeforeJA";
    internal const string Talk = "Sue.Talk";
    internal const string End = "Sue.End";
    internal const string AfterJA = "Sue.AfterJA";
    private float lastBeforeJATime = 0;
    protected override Node[] Nodes => [
        new(BeforeJA, [
            command(OnStart),
            lines(1, 10, digit2, [2, 4, 5, 10], [
                new(3, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
            ]),
            done(),
            command(() => lastBeforeJATime = Time.time),
            command(OnEnd),
        ], condition: () => _bJA && NodeYet(BeforeJA)),

        new(Talk, [
            done(),
            command(OnStart),
            lineif(() => Time.time - lastBeforeJATime < 10, "imm01", "nonimm01", Original),
            lines(2, 7, digit2, [], [new(4, emote(Emotes.Happy, Original)), new(5, emote(Emotes.Normal, Original))]),
            @if(() => _HM, lines(8, 9, digit2("HM", ""), [8, 9]), lines(8, 9, digit2("L", ""), [8, 9])),
            lines(10, 20, digit2, [], [new(10, emote(Emotes.Happy, Original)), new(12, emote(Emotes.Normal, Original))]),
            @if(() => _HM, lines(21, 24, digit2("HM", ""), [21, 22, 23, 24]), lines(21, 23, digit2("L", ""), [21, 22, 23])),
            lines(25, 29, digit2, [], [new(25, emote(Emotes.Happy, Original)), new(26, emote(Emotes.Normal, Original))]),
            @switch(() => _H ? "H" : _M ? "M" : "L"),
            lines(30, 33, digit2("H", ""), [30, 33], [new(31, emote(Emotes.Happy, Original))], anchor: "H"),
            end(),
            lines(30, 34, digit2("M", ""), [30, 31, 34], [new(33, emote(Emotes.Happy, Original))], anchor: "M"),
            end(),
            lines(30, 32, digit2("L", ""), [30, 32], [new(31, emote(Emotes.Happy, Original))], anchor: "L"),
            end(),
        ], condition: () => NodeYet(Talk), priority: -1, onConversationFinish: OnEnd),

        new(End, [
            command(OnStart),
            lines(1, 3, digit2, [3], [new(2, emote(Emotes.Happy, Original))]),
            command(OnEnd),
        ], condition: () => NodeDone(Talk), priority: -1),

        new(AfterJA, [
            command(OnStart),
            lines(1, 4, digit2, [], replacer: Const.formatJATrigger),
            @if(() => _H,
                lines(5, 11, digit2("H", ""), [5, 6, 11]),
                lines(5, 9, digit2("ML", ""), [5, 6, 7], [
                    new(8, emote(Emotes.Happy, Original)),
                    new(9, emote(Emotes.Normal, Original)),
                ])
            ),
            lines(12, 28, digit2, [14, 17, 24, 27, 28]),
            item(Items.RunningShoes),
            lines(29, 30, digit2, [30], [new(29, emote(Emotes.Happy, Original))]),
            done(),
            command(OnEnd),
        ], condition: () => _aJA && NodeYet(AfterJA)),
    ];
    private void OnStart() => Pose(Characters.Sue, Poses.Standing);
    private void OnEnd()
    {
        Pose(Characters.Sue, Poses.ReallyAnxious);
        Ch(Characters.Sue).animator.SetBool("Happy", true);
    }
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            Ch(Characters.Sue).animator.SetBool("Happy", true);
        });
    }
}

