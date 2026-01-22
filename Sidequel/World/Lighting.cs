
//#define DEBUG_ENABLE_TO_TOGGLE
using ModdingAPI;
using UnityEngine;

namespace Sidequel.World;
internal class Lighting
{
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => SetupLighting();
#if DEBUG_ENABLE_TO_TOGGLE
        ModdingAPI.KeyBind.KeyBind.RegisterKeyBind("Alpha0", ToggleLighting);
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => isActive = false;
#endif
    }
    private const float ShadowNormalBias = 0;
    private const float DefaultShadowNormalBias = 1.571f;
    private static Light light = null!;
    private static void SetupLighting()
    {
        light = GameObject.Find("/LevelSingletons").transform.Find("Lighting/Directional Light").GetComponent<Light>();
#if DEBUG_ENABLE_TO_TOGGLE
        ToggleLighting();
#else
        light.shadowNormalBias = ShadowNormalBias;
#endif
    }
#if DEBUG_ENABLE_TO_TOGGLE
    private static bool isActive = false;
    private static void ToggleLighting()
    {
        if (!Context.GameStarted) return;
        isActive = !isActive;
        Debug($"ShadowHack {(isActive ? "ON" : "OFF")}", LL.Warning);
        light.shadowNormalBias = isActive ? ShadowNormalBias : DefaultShadowNormalBias;
    }
#endif
}

