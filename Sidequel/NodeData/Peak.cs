
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using Sidequel.Dialogue;
using Sidequel.Dialogue.Actions;
using UnityEngine;

namespace Sidequel.NodeData;

internal class Peak : NodeEntry
{
    protected override Characters? Character => null;
    private static bool isActive = false;
    private Transform peakCutscene = null!;
    private Transform moon = null!;
    private Transform camera = null!;
    private bool CameraActive
    {
        get => camera.gameObject.activeSelf; set
        {
            camera.gameObject.SetActive(value);
            moon.gameObject.SetActive(value);
        }
    }
#pragma warning disable IDE1006
    private Node node(string id, List<BaseAction> actions) => new(id, actions, condition: () => isActive, priority: int.MaxValue, onConversationFinish: OnEnd);
#pragma warning restore IDE1006
    private class Mes(string line) : BaseAction(ActionType.Line)
    {
        public override IEnumerator Invoke(IConversation conversation)
        {
            conversation.currentSpeaker = Context.player.transform;
            yield return conversation.ShowLine(line);
        }
    }
    internal const string Entry = "Peak.Entry";
    internal const string HighMidFirst = "Peak.HighMidFirst";
    internal const string High = "Peak.High";
    internal const string Mid = "Peak.Mid";
    internal const string Low = "Peak.Low";
    private static bool wasFirstClimbing = false;
    protected override Node[] Nodes => [
        new(Entry, [
            command(() => isActive = false),
            wait(2f),
            next(() => {
                if(wasFirstClimbing){
                    return _H ? High : _M ? Mid : Low;
                }else{
                    return _HM ? HighMidFirst : Low;
                }
            }),
        ], condition: () => isActive, priority: int.MaxValue),
        new(HighMidFirst, [
            command(() => CameraActive = true),
            lines(1, 9, digit2, Player, [
                new(3, wait(2f)),
                new(5, wait(1f)),
                new(7, wait(1f)),
                new(9, command(() => CameraActive = false)),
                new(9, wait(2f)),
            ]),
            wait(1f),
        ], condition: () => false),
        new(High, [
            command(() => CameraActive = true),
            lines(1, 4, digit2, Player, [
                new(3, wait(2f)),
            ]),
            command(() => CameraActive = false),
        ], condition: () => false),
        new(Mid, [
            command(() => CameraActive = true),
            lines(1, 4, digit2, Player, [
                new(3, wait(2f)),
            ]),
            command(() => CameraActive = false),
        ], condition: () => false),

        new(Low, [
            command(() => CameraActive = true),
            lines(1, 3, digit2, Player, [
                new(3, wait(1f)),
            ]),
            @if(() => wasFirstClimbing,
                lines(1, 2, digit2("HasNotClibmed"), Player),
                lines(1, 2, digit2("HasClibmedOnce"), Player)
            ),
            wait(1f),
            line(4, Player),
            @if(() => wasFirstClimbing,
                lines(5, 6, digit2("HasNotClibmed"), Player),
                line(5, Player)
            ),
            @if(() => Cont.IsEndingCont && Items.CoinsSavedUp, "ending"),
            command(() => CameraActive = false),
            end(),

            anchor("ending"),
            lines(7, 15, digit2, Player),
            command(() => endingActive = true),
            transition(() => {
                System.Ending.CutsceneController.camera = camera;
                System.Ending.Controller.Prepare();
            }),
        ], condition: () => false, onConversationFinish: () => {
            if(endingActive){
                endingActive = false;
                System.Ending.Controller.StartScene();
            }
        }),
    ];
    private static bool endingActive = false;
    internal override void OnGameStarted()
    {
        peakCutscene = GameObject.Find("/Cutscenes/PeakCutscene").transform;
        moon = peakCutscene.Find("Moon");
        peakCutscene.Find("TopUpdraft").gameObject.SetActive(false);
        peakCutscene.Find("Bubbles").gameObject.SetActive(false);
        camera = peakCutscene.Find("SnowTipTopPos/SitCutsceneCam");
        camera.GetComponent<Animator>().speed = 2f;
    }
    internal static void OnReachedTop()
    {
        wasFirstClimbing = !System.STags.GetBool(Const.STags.HasClimbedPeakOnce);
        System.STags.SetBool(Const.STags.HasClimbedPeakOnce);
        System.STags.SetInt(Const.STags.FeathersCountOnClimbedPeak, Items.Num(Items.GoldenFeather));
        isActive = true;
        Context.gameServiceLocator.levelUI.HideUI(true);
        Dialogue.DialogueController.instance.StartConversation(null);
    }
    private static void OnEnd()
    {
        Context.gameServiceLocator.levelUI.HideUI(false);
    }
}

[HarmonyPatch(typeof(MountainTopCutscene))]
internal class MountainTopCutscenePatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("OnTriggerEnter")]
    internal static bool Prefix(Collider other)
    {
        if (!State.IsActive) return true;
        var player = other.GetComponent<Player>();
        if (player != null && (bool)player)
        {
            Peak.OnReachedTop();
        }
        return false;
    }
}

