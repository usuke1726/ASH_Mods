
using System.Collections;
using HarmonyLib;
using ModdingAPI;
using QuickUnityTools.Input;
using UnityEngine;

namespace Sidequel.System.Fishing;

internal class BillScene : MonoBehaviour
{
    private static bool isActive = false;
    internal static bool IsActive
    {
        get
        {
            if (!isActive) return false;
            if (LastTalkedTime < 0 || Time.time - LastTalkedTime > 20)
            {
                isActive = false;
                return false;
            }
            return true;
        }
    }
    internal static float LastTalkedTime { get; private set; } = -1;
    internal static void Activate()
    {
        LastTalkedTime = Time.time;
        isActive = true;
    }
    private class TargetPosition(Vector3 center, float maxDist, float zStart, float zEnd)
    {
        internal readonly Vector3 center = center;
        internal readonly float maxDist = maxDist;
        internal readonly float zStart = zStart;
        internal readonly float zEnd = zEnd;
    }
    private static readonly TargetPosition[] positions = [
        new(
            new(624.7294f, 0, 609.0444f),
            35.788f, 604.9974f, 610.7f
        ),
        new(
            new(626.3972f, 0, 597.3581f),
            48.965f, 590.9278f, 604.9974f
        ),
    ];
    internal static bool RunningCoroutine { get; private set; } = false;
    internal static bool IsSwimming { get; private set; } = false;
    internal static bool HasGotFish { get; private set; } = false;
    internal static bool TryToStart()
    {
        if (!IsActive) return true;
        var player = Context.player;
        if (!player.isGrounded) return true;
        var pos = player.transform.position.SetY(0);
        for (int i = 0; i < 2; i++)
        {
            var target = positions[i];
            if ((pos - target.center).sqrMagnitude > target.maxDist) continue;
            if (pos.z < target.zStart || pos.z > target.zEnd) continue;
            new GameObject("Sidequel_FishingScene").AddComponent<BillScene>();
            isActive = false;
            wasOnLeftSide = i == 0;
            return false;
        }
        return true;
    }
    private void Start()
    {
        fishingTutorial = GameObject.Find("Cutscenes").transform.Find("FishingTutorial").GetComponent<FishingTutorial>();
        RunningCoroutine = true;
        StartCoroutine(Coroutine());
    }
    private FishingTutorial fishingTutorial = null!;
    internal static void OnPromptClosed() => promptClosed = true;
    private static bool promptClosed = false;
    private static bool wasOnLeftSide = false;
    private const float BiteTimeout = 10.0f;
    private IEnumerator Coroutine()
    {
        var input = GameUserInput.CreateInput(gameObject);
        var species = FishSpecies.Load("CarpFish");
        Assert(species != null, "species is null!!");
        var player = Context.player;
        player.disableMenu = true;
        var npcAnimator = Traverse.Create(fishingTutorial).Field("npcAnimator").GetValue<NPCIKAnimator>();
        var originalRadius = npcAnimator.lookAtPlayerRadius;
        npcAnimator.lookAtPlayerRadius = 0f;
        if (wasOnLeftSide)
        {
            player.WalkTo(new(627.6956f, 102.9501f, 602.6671f), 6f);
            yield return new WaitUntil(() => !player.walkTo.HasValue);
        }
        Vector3 targetPos = new(623.7778f, 102.95f, 597.7756f);
        player.WalkTo(targetPos, 6f);
        yield return new WaitUntil(() => !player.walkTo.HasValue);
        if ((player.transform.position - targetPos).sqrMagnitude > 4f)
        {
            player.body.position = targetPos;
        }
        player.TurnToFace(fishingTutorial.castDirection);
        yield return new WaitForSeconds(0.5f);
        var heldItem = player.heldItem;
        if (heldItem == null || !heldItem.name.StartsWith("FishingRod"))
        {
            Holdable.EquipFromInventory(player, fishingTutorial.fishingRodItem);
            yield return new WaitForSeconds(0.5f);
        }
        var fishingRod = player.heldItem.GetComponent<FishingActions>();
        var originalCastSpeed = fishingRod.castSpeed;
        //Debug($"originalCastSpeed: {originalCastSpeed}"); // 60
        fishingRod.castSpeed = originalCastSpeed * 0.4f;
        player.UseItem();
        fishingRod.tutorialMode = true;
        fishingRod.allowSleeping = false;
        fishingTutorial.fishingCamera.SetActive(true);
        yield return new WaitForSeconds(6f);
        fishingRod.castSpeed = originalCastSpeed;
        NodeData.Fishing.StartConversation();
        yield return new WaitUntil(() => NodeData.Fishing.End);
        yield return new WaitForSeconds(2f);
        fishingRod.Nibble();
        yield return new WaitForSeconds(0.5f);
        fishingTutorial.fishingCamera.SetActive(false);
        NodeData.Fishing.StartConversation();
        yield return new WaitForSeconds(0.8f);
        fishingRod.Nibble();
        yield return new WaitForSeconds(1.2f);
        fishingRod.Nibble();
        yield return new WaitUntil(() => NodeData.Fishing.End);
        yield return new WaitForSeconds(1.5f);
        fishingRod.fishEncounter = species;
        var originalBiteTimeout = fishingRod.biteTimeout;
        //Debug($"originalBiteTimeout: {originalBiteTimeout}"); // 1.75
        fishingRod.biteTimeout = BiteTimeout;
        fishingRod.tutorialMode = false;
        fishingRod.Bite();
        fishingRod.biteTimeout = originalBiteTimeout;
        yield return new WaitForSeconds(0.5f);
        TemporalBox.Add(I18nLocalize("node.MidLowFishing.42"), 1.2f);
        yield return new WaitUntil(() => !fishingRod.isCast || input.GetUseItemButton().wasPressed);
        var lostFishBeforePressingButton = !fishingRod.isCast;
        if (!lostFishBeforePressingButton)
        {
            player.UseItem();
            GameObject.Destroy(input);
        }
        yield return new WaitUntil(() => !fishingRod.isCast);
        IsSwimming = player.isSwimming;
        if (HasGotFish)
        {
            //Debug($"hasGotFish waiting");
            yield return new WaitUntil(() => promptClosed);
            //Debug($"hasGotFish waiting done");
        }
        npcAnimator.lookAtPlayerRadius = originalRadius;
        NodeData.Fishing.StartConversation();
        if (lostFishBeforePressingButton)
        {
            GameObject.Destroy(input);
        }
        NodeData.Bill.afterFishing = true;
        player.disableMenu = false;
        fishingRod.allowSleeping = true;
        RunningCoroutine = false;
    }
    internal static void OnGotFish() => HasGotFish = true;
}

