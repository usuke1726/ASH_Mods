
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
    protected override Node[] Nodes => [
        node("_debug_peak_cutscene", [
            wait(2f),
            new Mes("DUMMY PEAK (1)"),
            command(() => CameraActive = true),
            wait(3f),
            new Mes("DUMMY PEAK (2)"),
            new Mes("DUMMY PEAK (3)"),
            command(() => CameraActive = false),
        ]),
    ];
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
        System.STags.SetBool(Const.STags.HasClimbedPeakOnce, true);
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

