
using ModdingAPI;
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Jim : NodeEntry
{
    internal const string BeforeJA1 = "Jim.BeforeJA1";
    internal const string BeforeJA2 = "Jim.BeforeJA2";
    internal const string Nothing = "Jim.Nothing";
    internal const string HighMidPeak1 = "Jim.HighMidPeak1";
    internal const string HighMidPeak2 = "Jim.HighMidPeak2";
    internal const string LowPeak = "Jim.LowPeak";
    internal const string AfterJA1 = "Jim.AfterJA1";
    internal const string AfterJA2 = "Jim.AfterJA2";
    internal const string AfterJA3 = "Jim.AfterJA3";
    internal const string Peak1 = "Jim.Peak1";
    internal const string HowClimbed = "Jim.Peak1.HowClimbed";
    internal const string Peak2 = "Jim.Peak2";
    private PathNPCMovement path = null!;
    private bool IsAtPeak => path.nextNode is 17 or 18;
    private bool IsClimbing => path.nextNode is >= 1 and <= 16;
    private bool IsDescending => path.nextNode is 0 or >= 19;
    protected override Characters? Character => Characters.OutlookPointGuy;
    private static readonly float afterJA2border = Const.Cont.LowBorderValue + 30.1f;
    private static bool IsJA2Active => Cont.Value <= afterJA2border;
    private float afterJA1Time = -1;
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            lines(1, 2, digit2, [2], [new(1, emote(Emotes.Happy, Original))]),
            emote(Emotes.Normal, Original),
            lineif(() => IsAtPeak, "AtOutlook.03", "NotAtOutlook.03", Original),
            lines(4, 16, digit2, [4, 6, 10, 11, 15], [
            ]),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1)),

        new(BeforeJA2, [
            lines(1, 23, digit2, [6, 7, 14, 19, 20, 21], [
                new(2, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
                new(8, emote(Emotes.Happy, Original)),
                new(9, emote(Emotes.Normal, Original)),
                new(22, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(Nothing, [
            lines(1, 4, digit2, [3], [
                new(4, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => true, priority: int.MinValue),

        new(HighMidPeak1, [
            lines(1, 12, digit2, [3, 9, 10, 11], [
                new(1, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
            ]),
            done(),
        ], condition: () => IsAtPeak && _HM && NodeYet(HighMidPeak1)),

        new(HighMidPeak2, [
            lines(1, 4, digit2, [3], [
                new(1, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => IsAtPeak && _HM && NodeDone(HighMidPeak1)),

        new(LowPeak, [
            lines(1, 4, digit2, [3], [
                new(1, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => IsAtPeak && _L),

        new(AfterJA1, [
            lines(1, 2, digit2, [], replacer: Const.formatJATrigger),
            lines(3, 14, digit2, [4, 6, 9, 10, 11, 12], [
                new(5, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
            ]),
            command(() => afterJA1Time = Time.time),
            done(),
        ], condition: () => _aJA && NodeYet(AfterJA1), priority: -1),

        new(AfterJA2, [
            lineif(() => afterJA1Time > 0 && Time.time - afterJA1Time < 20, "Immediately.01", "01", Original),
            lines(2, 52, digit2, [4, 5, 15, 16, 19, 20, 21, 22, 23, 26, 27, 28, 29, 34, 35, 39, 43, 44, 50, 51], [
                new(3, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
                new(11, emote(Emotes.Happy, Original)),
                new(12, emote(Emotes.Normal, Original)),
                new(14, emote(Emotes.Happy, Original)),
                new(15, emote(Emotes.Normal, Original)),
                new(31, emote(Emotes.Happy, Original)),
                new(32, emote(Emotes.Normal, Original)),
                new(48, item(Items.JimsAddressNote)),
                new(52, emote(Emotes.Happy, Original)),
            ]),
            cont(-20),
            done(),
        ], condition: () => IsJA2Active && NodeDone(AfterJA1) && NodeYet(AfterJA2), priority: 10),

        new(AfterJA3, [
            lines(1, 8, digit2, [1, 3, 4, 8]),
            done(),
        ], condition: () => NodeDone(AfterJA2) && NodeYet(AfterJA3), priority: 10),

        new(Peak1, [
            lines(1, 4, digit2, [2]),
            @if(() => GetBool(Const.STags.HasClimbedPeakOnce), "climbed"),
            lines(1, 7, digit2("NotClimbedYet"), [2, 3], [
                new(4, emote(Emotes.Happy, Original)),
                new(5, emote(Emotes.Normal, Original)),
                new(7, emote(Emotes.Happy, Original)),
            ]),
            state(NodeStates.InProgress),
            end(),
            anchor("climbed"),
            lines(1, 10, digit2("HasClimbed"), [1, 4, 7], replacer: ReplaceNum),
            next(() => HowClimbed),
        ], condition: () => NodeYet(Peak1) && (
            (_bJA && NodeDone(BeforeJA2)) || (_aJA && NodeDone(AfterJA3))
        )),

        new(HowClimbed, [
            lines(1, 6, digit2, [2, 3, 6]),
            done(Peak1),
        ], condition: () => false),

        new(Peak2, [
            lines(1, 3, digit2, [1, 2], replacer: ReplaceNum),
            next(() => HowClimbed),
        ], condition: () => NodeIP(Peak1) && GetBool(Const.STags.HasClimbedPeakOnce), priority: 10),
    ];
    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            path = Ch(Characters.OutlookPointGuy).gameObject.GetComponent<PathNPCMovement>();
        });
    }
    private static string ReplaceNum(string s) => s.Replace("{{Num}}", $"{GetInt(Const.STags.FeathersCountOnClimbedPeak)}");
}

