
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace CoinChallenge;

internal static class SpecialCoin
{
    private static GameObject dummyCoin = null!;
    private static int count = 0;
    public static readonly string SpecialCoinName = "SPECIAL COIN";
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            dummyCoin = GameObject.Find("BronzeCoin");
            Monitor.Log($"BronzeCoin is got {dummyCoin}");
            dummyCoin.SetActive(false);
        };
        helper.Events.Gameloop.GameQuitting += (_, _) => dummyCoin = null!;
    }
    internal static GameObject CloneCoin()
    {
        count++;
        dummyCoin.GetComponent<GameObjectID>().SaveBoolForID("COLLECTED_", value: false);
        var coin = dummyCoin.Clone();
        coin.name = $"{SpecialCoinName} ({count})";
        coin.SetActive(true);
        return coin;
    }
}

[HarmonyPatch(typeof(CollectOnTouch))]
internal class CollectOnTouchPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("Collect")]
    internal static void Collect(CollectOnTouch __instance)
    {
        if (!__instance.gameObject.name.StartsWith(SpecialCoin.SpecialCoinName)) return;
        var id = __instance.GetComponent<GameObjectID>();
        id.SaveBoolForID("COLLECTED_", value: false);
    }
}

