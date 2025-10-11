
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Dialogue;

[HarmonyPatch(typeof(DialogueInteractable))]
internal class DialogueInteractablePatch
{
    private static readonly FieldInfo eventField = typeof(DialogueInteractable).GetField("onConversationStart", BindingFlags.Instance | BindingFlags.NonPublic);

    [HarmonyPrefix()]
    [HarmonyPatch("Interact")]
    internal static bool Interact(DialogueInteractable __instance)
    {
        if (!State.IsActive) return true;
        if (NodeSelector.UseVanillaNode(__instance)) return true;
        if (Context.TryToGetPlayer(out var player))
        {
            player.TurnToFace(__instance.transform);
            var distance = (player.transform.position - __instance.transform.position).SetY(0);
            if (distance.sqrMagnitude < 5f.Sqr() && __instance.stepBack)
            {
                var magnitude = distance.magnitude;
                var num = Mathf.Lerp(25f, 0f, Mathf.InverseLerp(2f, 5f, magnitude));
                player.body.AddForce(distance / magnitude * num, ForceMode.Impulse);
            }
        }
        var conversation = DialogueController.instance.StartConversation(__instance);
        Traverse.Create(__instance).Field("conversation").SetValue(conversation);
        var eventHandler = eventField.GetValue(__instance) as MulticastDelegate;
        if (eventHandler != null)
        {
            foreach (var h in eventHandler.GetInvocationList())
            {
                h.Method.Invoke(h.Target, new object[] { conversation });
            }
        }
        return false;
    }
}

