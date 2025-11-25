
using Sidequel.Dialogue;
using UnityEngine;

namespace Sidequel.NodeData;
internal class CampfireWil : StartNodeEntry
{
    internal const string NodeName = "FireOutWilStart";
    protected override string StartNode => NodeName;
    internal const string Start1 = "FireOutWil.Start1";
    internal const string Start2 = "FireOutWil.Start2";
    internal const string Start3 = "FireOutWil.Start3";
    internal const string Wil = "DadBoatDeer1";
    internal const string Kid = "KidBoatDeer2";
    private Transform camera = null!;
    private bool CameraActive { get => camera.gameObject.activeSelf; set => camera.gameObject.SetActive(value); }
    protected override Node[] Nodes => [
        new(Start1, [
            command(() => CameraActive = true),
            wait(0.5f),
            look(Kid, Player),
            wait(0.5f),
            lines(1, 7, digit2, i => i switch {
                2 => Player,
                6 => Wil,
                _ => Kid,
            }, [
                new(2, look(Player, Kid)),
                new(5, look(Kid, Wil)),
                new(5, look(Player, Wil)),
                new(5, look(Wil, Kid)),
            ]),
            done(),
            command(() => CameraActive = false),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            command(() => CameraActive = true),
            wait(0.5f),
            look(Kid, Player),
            wait(0.5f),
            lines(1, 7, digit2, i => i switch {
                7 => Player,
                3 => Wil,
                _ => Kid,
            }, [
                new(1, look(Player, Kid)),
                new(1, look(Kid, Wil)),
                new(2, look(Player, Wil)),
                new(2, look(Wil, Kid)),
                new(4, look(Player, Kid)),
                new(5, look(Kid, Player)),
            ]),
            done(),
            command(() => CameraActive = false),
        ], condition: () => NodeDone(Start1) && NodeYet(Start2)),

        new(Start3, [
            command(() => CameraActive = true),
            wait(0.5f),
            look(Kid, Player),
            wait(0.5f),
            lines(1, 6, digit2, i => i switch {
                6 => Player,
                3 => Wil,
                _ => Kid,
            }, [
                new(1, look(Player, Kid)),
                new(1, look(Kid, Wil)),
                new(2, look(Player, Wil)),
                new(2, look(Wil, Kid)),
                new(4, look(Player, Kid)),
                new(4, look(Kid, Player)),
            ]),
            command(() => CameraActive = false),
        ], condition: () => NodeDone(Start2)),
    ];
    internal override void OnGameStarted()
    {
        var fire = GameObject.FindObjectsOfType<Campfire>().FirstOrDefault(f => f.transform.position.z > 1300);
        Assert(fire != null, "campfire is null");
        fire!.dialogueNode = NodeName;
        var cutscenes = GameObject.Find("Cutscenes").transform;
        var cutscene = cutscenes.Find("FoxPhoto").gameObject.Clone().transform;
        cutscene.SetParent(cutscenes);
        cutscene.gameObject.name = "FireOutOrangeIslandCutscene";
        cutscene.position = new(143.7969f, 65.5245f, 1367.557f);
        cutscene.localRotation = Quaternion.Euler(37.6849f, 170.1141f, 0);
        camera = cutscene.Find("FoxZoomCam");
        camera.transform.localPosition = Vector3.zero;
        camera.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
internal class CampfireCharlie : StartNodeEntry
{
    protected override string StartNode => "FireOutBearStart";
    internal const string T1 = "FireOutCharlie.T1";
    internal const string CT1 = "FireOutCharlie.CT1";
    internal const string CT2 = "FireOutCharlie.CT2";
    internal const string AT1 = "FireOutCharlie.AT1";
    internal const string AT2 = "FireOutCharlie.AT2";
    internal const string Charlie = "Charlie2";
    internal const string Tim = "Tim3";
    internal const string Alex = "ClimbingRhino3";
    private bool IsNearby(ModdingAPI.Characters ch, Vector3 expectedPos) => (Ch(ch).transform.position - expectedPos).sqrMagnitude <= 100;
    private bool IsCharlieNearby => IsNearby(ModdingAPI.Characters.Charlie2, new(315.703f, 400.04f, 636.9765f));
    private bool IsAlexNearby => IsNearby(ModdingAPI.Characters.ClimbingRhino3, new(323.41f, 398.97f, 626.03f));
    private bool cachedCharlieNearBy;
    private bool cachedAlexNearBy;
    protected override Node[] Nodes => [
        new(T1, [
            wait(0.5f),
            look(Tim, Player),
            wait(0.5f),
            look(Player, Tim),
            lines(1, 4, digit2, i => i switch {
                4 => Player,
                _ => Tim,
            }, [
                new(4, emote(Emotes.Happy, Player)),
            ]),
        ], condition: () => !IsCharlieNearby && !IsAlexNearby),

        new(CT1, [
            wait(0.5f),
            look(Charlie, Player),
            look(Tim, Player),
            wait(0.5f),
            lines(1, 6, digit2, i => i switch {
                3 => Tim,
                2 or 4 => Player,
                _ => Charlie,
            }, [
                new(1, look(Player, Charlie)),
                new(3, look(Player, Tim)),
                new(3, look(Charlie, Tim)),
                new(5, look(Charlie, Player)),
                new(5, look(Player, Charlie)),
            ]),
            done(),
        ], condition: () => IsCharlieNearby && !IsAlexNearby && NodeYet(CT1)),

        new(CT2, [
            wait(0.5f),
            look(Charlie, Player),
            look(Tim, Player),
            wait(0.5f),
            lines(1, 4, digit2, i => i switch{
                3 => Player,
                4 => Tim,
                _ => Charlie,
            }, [
                new(1, look(Player, Charlie)),
                new(4, look(Player, Tim)),
            ]),
        ], condition: () => IsCharlieNearby && !IsAlexNearby && NodeDone(CT1)),

        new(AT1, [
            wait(0.5f),
            @if(() => cachedCharlieNearBy = IsCharlieNearby, look(Charlie, Player)),
            look(Alex, Player),
            look(Tim, Player),
            wait(0.5f),
            lines(1, 7, digit2, i => i switch {
                1 or 2 => Alex,
                6 => Tim,
                _ => Player,
            }, [
                new(1, look(Player, Alex)),
                new(6, look(Player, Tim)),
                new(6, @if(() => cachedCharlieNearBy, look(Charlie, Tim))),
            ]),
            done(),
            @if(() => cachedCharlieNearBy, done(CT1)),
        ], condition: () => IsAlexNearby && NodeYet(AT1)),

        new(AT2, [
            wait(0.5f),
            look(Alex, Player),
            look(Tim, Player),
            wait(0.5f),
            lines(1, 2, digit2, i => i switch {
                1 => Alex,
                _ => Player,
            }, [
                new(1, look(Player, Alex)),
            ]),
        ], condition: () => IsAlexNearby && NodeDone(AT1)),
    ];
}

internal class CampfireMay : StartNodeEntry
{
    protected override string StartNode => "FireOutAuntStart";

    internal const string Start1 = "FireOutMay.Start1";
    internal const string Start2 = "FireOutMay.Start2";
    internal const string May = "AuntMay";
    internal const string Claire = "Claire";
    protected override Node[] Nodes => [
        new(Start1, [
            wait(0.5f),
            look(Claire, Player),
            look(May, Player),
            wait(0.5f),
            lines(1, 8, digit2, i => i switch {
                3 or 4 or 8 => Player,
                2 or 5 => May,
                _ => Claire,
            }, [
                new(1, look(Player, May)),
                new(6, look(Player, Claire)),
                new(6, look(May, Claire)),
            ]),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            wait(0.5f),
            look(Claire, Player),
            look(May, Player),
            wait(0.5f),
            lines(1, 7, digit2, i => i switch {
                2 => Player,
                5 or 6 => May,
                _ => Claire,
            }, [
                new(1, look(Player, Claire)),
                new(5, look(May, Claire)),
                new(5, look(Player, May)),
                new(5, look(Claire, May)),
                new(5, emote(Emotes.Happy, May)),
                new(7, emote(Emotes.EyesClosed, Claire)),
            ]),
        ], condition: () => NodeDone(Start1)),
    ];
}

internal class CampfireJon : StartNodeEntry
{
    protected override string StartNode => "FireOutRangerStart";
    internal const string Start1 = "FireOutJon.Start1";
    internal const string Start2 = "FireOutJon.Start2";
    internal const string Jon = "RangerJon";
    protected override Node[] Nodes => [
        new(Start1, [
            wait(0.5f),
            look(Jon, Player),
            wait(0.5f),
            look(Player, Jon),
            lines(1, 3, digit2, i => i switch {
                3 => Player,
                _ => Jon,
            }),
            done(),
        ], condition: () => NodeYet(Start1)),

        new(Start2, [
            wait(0.5f),
            look(Jon, Player),
            wait(0.5f),
            look(Player, Jon),
            lines(1,5, digit2, i => i  switch {
                2 or 3 => Player,
                _ => Jon,
            }),
        ], condition: () => NodeDone(Start1)),
    ];
}

