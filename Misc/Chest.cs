
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Misc;


[HarmonyPatch(typeof(Chest))]
internal class ChestPatch
{
    private static PropertyInfo setOpened = null!;

    [HarmonyPrefix()]
    [HarmonyPatch("Interact")]
    public static bool Interact(Chest __instance)
    {
        if (!ModConfig.config.EnableChestBoostReproduction) return true;
        var opened = Traverse.Create(__instance).Field("_opened").GetValue<bool>();
        if (opened) return false;
        __instance.openSound.Play();
        __instance.animator.enabled = true;
        __instance.animator.cullingMode = (AnimatorCullingMode)2;
        Timer disableSoon;
        disableSoon = __instance.RegisterTimer(3f, delegate { __instance.animator.enabled = false; });
        Traverse.Create(__instance).Field("disableSoon").SetValue(disableSoon);
        __instance.animator.SetTrigger("OpenAnimation");
        setOpened ??= typeof(Chest).GetProperty("opened", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance);
        setOpened.SetValue(__instance, true);
        var puff = __instance.puff;
        var prefabsInside = __instance.prefabsInside;
        var position = __instance.transform.position + Vector3.up * __instance.spawnOffset;
        var launchSpeed = __instance.launchSpeed;
        Action onAnyCollected = () =>
        {
            var component = __instance.GetComponent<GameObjectID>();
            if ((bool)component) component.SaveBoolForID("Opened_", value: true);
        };
        __instance.RegisterTimer(0.4f, () => puff.Play(), isLooped: false, useUnscaledTime: false);
        __instance.RegisterTimer(0.8f, delegate
        {
            SpawnRewardsPatch(
                prefabsInside,
                position,
                onAnyCollected,
                launchSpeed
            );
        });
        return false;
    }

    [HarmonyPostfix()]
    [HarmonyPatch("Interact")]
    public static void CloseChest(Chest __instance)
    {
        if (!ModConfig.config.EnableInfinityChest) return;
        setOpened ??= typeof(Chest).GetProperty("opened", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance);
        __instance.RegisterTimer(1.2f, delegate
        {
            setOpened.SetValue(__instance, false);
            __instance.animator.Play("ChestIdle", 0, 0);
        });
    }

    private static void SpawnRewardsPatch(GameObject[] rewardPrefabs, Vector3 spawnPosition, Action onAnyCollected, float launchSpeed = 20f, float autoCollectTime = 1f)
    {
        foreach (GameObject obj in rewardPrefabs)
        {
            GameObject val = obj.CloneAt(spawnPosition);
            Rigidbody component = val.GetComponent<Rigidbody>();
            ICollectable collectable = val.GetComponent<ICollectable>();
            if (component != null)
            {
                // main part of Chest Boost Reproduction 
                component.velocity = Vector3.up * launchSpeed * (0.8f + 15 * 0.4f);
                component.maxDepenetrationVelocity = Mathf.Infinity;
                component.AddTorque(UnityEngine.Random.insideUnitSphere * 720f);
                if (collectable != null)
                {
                    val.AddComponent<MagnetRigidbody>();
                }
            }
            if (collectable != null)
            {
                Action onCollect = null!;
                onCollect = delegate
                {
                    onAnyCollected?.Invoke();
                    collectable.onCollect -= onCollect;
                };
                collectable.onCollect += onCollect;
            }
            Context.gameServiceLocator.RegisterTimer(autoCollectTime, delegate
            {
                if (collectable is MonoBehaviour c && c != null) collectable.Collect();
            });
        }
    }
}

