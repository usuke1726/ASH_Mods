
using ModdingAPI;
using Sidequel.Dialogue;

namespace Sidequel.NodeData;

internal class Jon : NodeEntry
{
    internal const string BeforeJA1 = "Jon.BeforeJA1";
    internal const string BeforeJA2 = "Jon.BeforeJA2";
    internal const string BeforeJA3 = "Jon.BeforeJA3";
    internal const string JA1 = "Jon.JA1";
    internal const string JA2 = "Jon.JA2";
    internal const string AfterJA1 = "Jon.AfterJA1";
    internal const string AfterJA2 = "Jon.AfterJA2";
    internal const string HighAfterJA2 = "Jon.HighAfterJA2";
    internal const string HighAfterJA3 = "Jon.HighAfterJA3";
    internal const string MidAfterJA2 = "Jon.MidAfterJA2";
    internal const string MidAfterJA3 = "Jon.MidAfterJA3";
    internal const string MidCartersConfessionToJon = "Jon.MidCartersConfessionToJon";
    internal const string MidAfterConfession = "Jon.MidAfterConfession";
    internal const string LowAfterJA2 = "Jon.LowAfterJA2";
    internal const string LowCartersConfessionToJon = "Jon.LowCartersConfessionToJon";
    internal const string LowAfterConfession = "Jon.LowAfterConfession";
    internal const string Sunscreen1 = "Jon.Sunscreen1";
    internal const string SunscreenBody = "Jon.SunscreenBody";
    internal const string Sunscreen2 = "Jon.Sunscreen2";
    internal const string Sunscreen3 = "Jon.Sunscreen3";
    internal const string OldPicture = "Jon.OldPicture";
    internal const string SouvenirMedal = "Jon.SouvenirMedal";
    internal const string GoldMedal = "Jon.GoldMedalEvent";
    internal const string AfterCoinCompletion1 = "Jon.AfterCoinCompletion1";
    internal const string AfterCoinCompletion2 = "Jon.AfterCoinCompletion2";
    internal const string TalkedJonOnce = "TalkedJonOnce";
    protected override Characters? Character => Characters.RangerJon;
    private bool HasClimbed => GetBool(Const.STags.HasClimbedPeakOnce);
    protected override Node[] Nodes => [
        new(BeforeJA1, [
            tag(TalkedJonOnce, true),
            lines(1, 11, digit2, [2, 4, 5], [
                new(1, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
                new(6, emote(Emotes.Happy, Original)),
                new(8, emote(Emotes.Normal, Original)),
            ]),
            @if(() => HasClimbed,
                lines(1, 2, digit2("Climbed"), [1, 2], replacer: ReplaceFeathersNum),
                lines(1, 10, digit2("NotClimbed"), [1, 2, 7, 8, 9, 10])
            ),
            lines(12, 13, digit2, []),
            lineif(() => HasClimbed, "Climbed.14", "NotClimbed.14", Original),
            line(15, Original),
            @if(() => HasClimbed,
                lines(16, 18, digit2("Climbed"), [18]),
                lines(16, 17, digit2("NotClimbed"), [17])
            ),
            done(),
        ], condition: () => _bJA && NodeYet(BeforeJA1), priority: int.MaxValue),

        new(BeforeJA2, [
            lines(1, 28, digit2, [1, 3, 4, 5, 6, 11, 15, 16, 17, 21, 22, 23, 27, 28], [
                new(16, emote(Emotes.EyesClosed, Player)),
                new(18, emote(Emotes.Normal, Player)),
                new(21, emote(Emotes.EyesClosed, Player)),
                new(22, emote(Emotes.Normal, Player)),
                new(26, emote(Emotes.Happy, Original)),
            ]),
            cont(-5),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA1) && NodeYet(BeforeJA2)),

        new(BeforeJA3, [
            lines(1, 8, digit2, [3, 4, 8], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA2) && NodeYet(BeforeJA3)),

        new(JA1, [
            lines(1, 3, digit2, [3], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "talk"),
            lines(1, 4, digit2("O1"), [4], [
                new(1, emote(Emotes.Happy, Original)),
            ]),
            end(),
            anchor("talk"),
            lines(1, 37, digit2("O2"), [1, 2, 6, 7, 15, 16, 18, 19, 26, 27, 34, 36, 37], [
                new(16, emote(Emotes.EyesClosed, Player)),
                new(23, emote(Emotes.Normal, Player)),
                new(33, emote(Emotes.Happy, Original)),
                new(36, transition(MoveForJA)),
                new(36, wait(1f)),
            ]),
            tag(Const.STags.JADone, true),
            tag(Const.STags.JATriggeredByJon, true),
            cont(-5),
            done(),
        ], condition: () => _bJA && NodeDone(BeforeJA3)),

        new(JA2, [
            lines(1, 3, digit2, [2], [
                new(1, emote(Emotes.Happy, Original)),
                new(3, emote(Emotes.Normal, Original)),
            ]),
            @switch(() => {
                if(Items.CoinsNum >= 350) return "ge350";
                return Items.CoinsNum - GetInt(Const.STags.CoinsAtJA) >= 150 ? "largeinc" : "smallinc";
            }),
            lines(1, 5, digit2("ge350"), [1, 3, 5], [new(4, emote(Emotes.Happy, Original))], anchor: "ge350"),
            end(),
            lines(1, 5, digit2("lt350largeinc"), [1, 2, 5], [new(3, emote(Emotes.Happy, Original))], anchor: "largeinc"),
            end(),
            lines(1, 4, digit2("lt350smallinc"), [1, 2, 3], anchor: "smallinc"),
            @switch(() => {
                if(!GetBool(JonTalkedAboutMay)) return "may";
                if(!GetBool(JonTalkedAboutWil)) return "wil";
                return "other";
            }),
            lines(1, 11, digit2("lt350smallinc.may"), [6, 7, 11], [new(10, emote(Emotes.Happy, Original))], anchor: "may"),
            tag(JonTalkedAboutMay, true),
            end(),
            lines(1, 5, digit2("lt350smallinc.wil"), [5], anchor: "wil"),
            tag(JonTalkedAboutWil, true),
            @if(() => GetBool(Const.STags.TalkedAboutWilOnce), "wilEnd", null),
            lines(6, 8, digit2("lt350smallinc.wil"), [6, 8]),
            lines(9, 14, digit2("lt350smallinc.wil"), [9, 10, 14], anchor: "wilEnd"),
            tag(Const.STags.TalkedAboutWilOnce, true),
            end(),
            lines(1, 2, digit2("lt350smallinc.other"), [], anchor: "other"),
            lineif(() => _HM, "lt350smallinc.other.HM03", "lt350smallinc.other.L03", Player),
            end(),
        ], condition: () => _aJA && _JAJon && !Items.CoinsSavedUp),

        new(AfterJA1, [
            tag(TalkedJonOnce, true),
            lines(1, 11, digit2, [3, 4, 8], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
            ]),
            lineif(() => _HM, "HM12", "L12", Player),
            done(),
        ], () => _aJA && !_JAJon && NodeYet(AfterJA1), priority: int.MaxValue),

        new(HighAfterJA2, [
            lines(1, 10, digit2(AfterJA2), [5, 6], useId: false),
            lines(1, 9, digit2, [1, 2, 6, 8, 9], [
                new(5, emote(Emotes.Happy, Original)),
            ]),
            done(),
        ], () => _aJA && !_JAJon && _H && NodeDone(AfterJA1) && NodeYet(HighAfterJA2)),

        new(HighAfterJA3, [
            lines(1, 3, digit2, [2], [new(3, emote(Emotes.Happy, Original))]),
        ], () => _aJA && !_JAJon && _H && NodeDone(HighAfterJA2)),

        new(MidAfterJA2, [
            lines(1, 10, digit2(AfterJA2), [5, 6], useId: false),
            lines(1, 17, digit2, [1, 2, 3, 4, 5, 6, 7, 8, 12, 13, 16, 17], [
                new(14, emote(Emotes.Happy, Original)),
                new(16, emote(Emotes.Normal, Original)),
            ]),
            done(),
            done(MidAfterJA3),
            next(() => MidCartersConfessionToJon),
        ], () => _aJA && !_JAJon && _M && NodeDone(AfterJA1) && NodeYet(HighAfterJA2) && NodeYet(MidAfterJA2)),

        new(MidAfterJA3, [
            lines(1, 21, digit2, [2, 3, 6, 9, 13, 14, 18, 19, 20, 21], [
                new(4, emote(Emotes.Happy, Original)),
                new(7, emote(Emotes.Normal, Original)),
            ]),
            done(),
            done(MidAfterJA2),
            next(() => MidCartersConfessionToJon),
        ], () => _aJA && !_JAJon && _M && NodeDone(HighAfterJA2) && NodeYet(MidAfterJA3)),

        new(MidCartersConfessionToJon, [
            line("01", Player),
            lines(digit2("MidCartersConfession"), Player, useId: false),
            lines(2, 6, digit2, []),
            cont(-20),
            done(),
        ], condition: () => false),

        new(LowAfterJA2, [
            lines(1, 20, digit2, [5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 17, 18, 20], [
                new(16, emote(Emotes.Happy, Original)),
                new(17, emote(Emotes.Normal, Original)),
            ]),
            done(),
            next(() => LowCartersConfessionToJon),
        ], condition: () => _aJA && !_JAJon && _L && NodeDone(AfterJA1) && NodeYet(HighAfterJA2) && NodeYet(MidAfterJA2)),

        new(LowCartersConfessionToJon, [
            lines(1, 22, digit2("LowCartersConfession"), Player, useId: false),
            wait(1f),
            lines(23, 32, digit2("LowCartersConfession"), Player, useId: false),
            wait(1.5f),
            lines(digit2, []),
            cont(-20),
            done(),
        ], condition: () => false),

        new(MidAfterConfession, [
            lines(1, 4, digit2, [3], [new(4, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeDone(MidCartersConfessionToJon) && _M),

        new(LowAfterConfession, [
            lines(1, 7, digit2, [3, 4, 7], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => (NodeDone(MidCartersConfessionToJon) || NodeDone(LowCartersConfessionToJon)) && _L),

        new(AfterCoinCompletion1, [
            lines(digit2, []),
            done(),
        ], condition: () => Items.CoinsSavedUp && NodeYet(AfterCoinCompletion1), priority: 10),

        new(AfterCoinCompletion2, [
            lines(digit2, []),
        ], condition: () => Items.CoinsSavedUp && NodeDone(AfterCoinCompletion1), priority: -1),

        new(Sunscreen1, [
            line(1, Player),
            emote(Emotes.Happy, Original),
            lineif(() => GetBool(TalkedJonOnce), "02", "MetFirst.02", Original),
            emote(Emotes.Normal, Original),
            lines(3, 13, digit2, [3, 4, 6]),
            next(() => SunscreenBody)
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeYet(Sunscreen1)),

        new(SunscreenBody, [
            option(["O1", "O2", "O3"]),
            @switch(() => LastSelected switch {
                0 => "strong",
                1 => "weak",
                _ => "refuse",
            }),

            anchor("strong"),
            lines(1, 4, digit2("O1"), []),
            option(["O1.O1", "O1.O2"]),
            @if(() => LastSelected == 1, "refuse"),
            emote(Emotes.Happy, Original),
            line("accept.01", Original),
            @if(() => Items.CoinsNum < 20, "shortOnCash"),
            item(Items.Coin, -20),
            item(Items.StrongSunscreen),
            done(Sunscreen1),
            line("accept.02", Original),
            state(Const.Events.Sunscreen, NodeStates.Stage1),
            end(),

            anchor("weak"),
            lines(1, 3, digit2("O2"), []),
            option(["O2.O1", "O2.O2"]),
            @if(() => LastSelected == 1, "refuse"),
            line("accept.01", Original),
            @if(() => Items.CoinsNum < 10, "shortOnCash"),
            item(Items.Coin, -10),
            item(Items.Sunscreen),
            line("accept.02", Original),
            done(Sunscreen1),
            end(),

            anchor("shortOnCash"),
            state(Sunscreen1, NodeStates.Refused),
            lines(1, 6, digit2("shortOnCash"), [1, 2, 6], [
                new(3, emote(Emotes.Happy, Original)),
                new(4, emote(Emotes.Normal, Original)),
                new(5, emote(Emotes.Happy, Original)),
            ]),
            end(),
            anchor("refuse"),
            state(Sunscreen1, NodeStates.Refused),
            lines(1, 4, digit2("refuse"), [4], [
                new(3, emote(Emotes.Happy, Original)),
            ]),
        ], condition: () => false),

        new(Sunscreen2, [
            lines(1, 10, digit2, [3, 7, 10], [
                new(8, emote(Emotes.Happy, Original)),
                new(9, emote(Emotes.Normal, Original)),
            ]),
            done(),
            end(condition: () => _H),
            lines(11, 12, digit2("ML", ""), [11], [new(12, emote(Emotes.Happy, Original))]),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeRefused(Sunscreen1) && NodeYet(Sunscreen2)),

        new(Sunscreen3, [
            lines(1, 2, digit2, [], [
                new(1, emote(Emotes.Happy, Original)),
                new(2, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2", "O3"]),
            @switch(() => LastSelected switch {
                0 => "body",
                1 => "sunscreen2again",
                _ => "refuse",
            }),

            anchor("body"),
            lines(1, 2, digit2("O1"), []),
            next(() => SunscreenBody),
            end(),

            anchor("refuse"),
            lines(1, 4, digit2($"{SunscreenBody}.refuse"), [4], [
                new(3, emote(Emotes.Happy, Original)),
            ], useId: false),
            end(),

            anchor("sunscreen2again"),
            lines(1, 6, digit2("O2"), [3, 4, 5], [
                new(6, emote(Emotes.Happy, Original)),
            ]),
            end(),
        ], condition: () => NodeIP(Const.Events.Sunscreen) && NodeDone(Sunscreen2)),

        new(OldPicture, [
            lines(1, 4, digit2, []),
            cont(-10),
            done(),
        ], condition: () => Items.Has(Items.OldPicture) && NodeYet(OldPicture), priority: 5),

        new(SouvenirMedal, [
            lines(1, 7, digit2, [1, 4], [
                new(3, emote(Emotes.Happy, Original)),
                new(6, emote(Emotes.Normal, Original)),
            ]),
            option(["O1", "O2"]),
            @if(() => LastSelected == 1, "refuse"),
            emote(Emotes.Happy, Original),
            line("O1.01", Original),
            item([Items.SouvenirMedal, Items.Coin], [-1, 10]),
            line("O1.02", Original),
            cont(-5),
            done(),
            end(),
            anchor("refuse"),
            lines(1, 2, digit2("O2"), [], [new(2, emote(Emotes.Happy, Original))]),
            state(NodeStates.Refused),
        ], condition: () => Items.Has(Items.SouvenirMedal) && NodeYet(SouvenirMedal), priority: 5),

        new(GoldMedal, [
            lines(1, 7, digit2, []),
            done(),
        ], condition: () => NodeIP(Const.Events.GoldMedal) && NodeYet(GoldMedal), priority: 5),
    ];
    private const string JonTalkedAboutWil = "Jon.HasTalkedAboutWil";
    private const string JonTalkedAboutMay = "Jon.HasTalkedAboutMay";
    private bool ConfessionDone => NodeDone(MidCartersConfessionToJon) || NodeDone(LowCartersConfessionToJon);

    internal override void OnGameStarted()
    {
        ModdingAPI.Character.OnSetupDone(() =>
        {
            if (_aJA && _JAJon) MoveForJA();
        });
    }
    private void MoveForJA()
    {
        var ch = Ch(Characters.RangerJon);
        ch.transform.position = new(301.9707f, 115.6644f, 291.2642f);
        Sidequel.Character.Pose.Set(ch.transform, Poses.Standing);
    }
    private static string ReplaceFeathersNum(string s) => s.Replace("{{Num}}", $"{GetInt(Const.STags.FeathersCountOnClimbedPeak)}");
}

