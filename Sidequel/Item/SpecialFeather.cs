
#define THREE_FEATHERS_ONLY

using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Item;

internal class SpecialFeather : MonoBehaviour
{
    internal static bool HasGotFeather => Items.Has(Items.EternalFeather);
    private static SpecialFeather? instance = null;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            instance = null;
            SetupTowerViewerAtOutlookPoint();
            if (!State.IsActive || HasGotFeather) return;
            instance = new GameObject("Sidequel_SpecialFeatherController").AddComponent<SpecialFeather>();
        };
    }
    private static void SetupTowerViewerAtOutlookPoint()
    {
        var viewer = GameObject.Find("LevelObjects/Tools/TowerViewer (1)").GetComponent<TowerViewer>();
        viewer.maxOffsetAngle.y = 20;
    }
    private void Awake()
    {
        SetupFeatherObject();
        SetupCollider();
    }
    private BoxCollider collider = null!;
    private ParticleSystem.EmissionModule emission;
    private Collider featherCollider = null!;
    private float lastExitTime = -1;
    private const float Timeout = 15f;
    private Transform feather = null!;
    private bool isFadingOut = false;
    private float fadeStartTime = 0;
    private const float FadingTime = 0.5f;
    private const float FadingCoeff = 1.0f / FadingTime;
    private Vector3 defaultScale;
    private void SetupCollider()
    {
        collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.transform.position = new(859.91f, 137.8909f, 115.677f);
        collider.size = new(10f, 3f, 10f);
        gameObject.layer = 2;
    }
    private void SetupFeatherObject()
    {
        var obj = GameObject.Find("LevelObjects/PickUps").transform.Find("SilverFeather");
        var item = DataHandler.Find(Items.EternalFeather);
        Assert(item != null, "SpecialFeather item is null");
        featherCollider = obj.GetComponent<SphereCollider>();
        var collect = obj.GetComponent<CollectOnTouch>();
        collect.collectable = item!.item;
        collect.onCollect += () =>
        {
            SpecialFeatherPatch.OnGotFeather();
            GameObject.Destroy(gameObject);
            instance = null;
        };
        feather = obj.Find("Feather");
        defaultScale = feather.localScale;
        var material = feather.GetComponent<MeshRenderer>().material;
        if (material.mainTexture is Texture2D tex2d)
        {
            material.mainTexture = ChangeColor(tex2d);
        }
        else Assert(false, "feather texture is not 2D");
        var particle = obj.Find("Particle System").GetComponent<ParticleSystem>();
        emission = particle.emission;
        var mat = particle.GetComponent<ParticleSystemRenderer>().material;
        mat.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        var main = particle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.7925f, 0, 0.74f, 1));
        Hide(isImmediate: true);
    }
    private void Update()
    {
        var time = Time.time;
        if (lastExitTime > 0 && time - lastExitTime >= Timeout)
        {
            lastExitTime = -1;
            Hide();
        }
        if (isFadingOut)
        {
            if (time - fadeStartTime <= FadingTime)
            {
                var t = (time - fadeStartTime) * FadingCoeff;
                feather.localScale = Vector3.Lerp(defaultScale, Vector3.zero, Ease(t));
            }
            else
            {
                isFadingOut = false;
                feather.gameObject.SetActive(false);
            }
        }
    }
    internal static void ShowFeather() => instance?.Show();
    private void Show()
    {
        feather.parent.gameObject.SetActive(true);
        featherCollider.enabled = true;
        feather.gameObject.SetActive(true);
        feather.localScale = defaultScale;
        emission.enabled = true;
        lastExitTime = Time.time;
        Debug($"SpecialFeather Visible");
    }
    private void Hide(bool isImmediate = false)
    {
        featherCollider.enabled = false;
        emission.enabled = false;
        if (isImmediate)
        {
            isFadingOut = false;
            feather.gameObject.SetActive(false);
        }
        else
        {
            isFadingOut = true;
            fadeStartTime = Time.time;
        }
        Debug($"SpecialFeather Invisible");
    }
    private static float Ease(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1) * 0.5f;
    }
    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<Player>();
        var isPlayer = player != null && (bool)player;
        if (isPlayer)
        {
#if THREE_FEATHERS_ONLY
            if (Items.Num(Items.GoldenFeather) > 3) return;
#endif
            Show();
        }
    }
    private static Texture2D ChangeColor(Texture2D _tex)
    {
        var tex = Util.EditableTexture(_tex);
        Color darkPurple = new(0.4925f, 0, 0.4925f, 1);
        Color purple = new(0.7925f, 0, 0.74f, 1);
        Color lightColor = new(1, 1, 1, 1);
        // original colors: rgb(0.949, 0.937, 1.000), rgb(0.580, 0.682, 1.000), rgb(1, 1, 1)
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                var p = tex.GetPixel(x, y);
                Color newColor = p.g switch
                {
                    < 0.7f => lightColor, // feather shaft and rachis
                    < 0.95f => darkPurple, // outside
                    _ => darkPurple, // inside
                };
                tex.SetPixel(x, y, newColor);
            }
        }
        tex.Apply();
        return tex;
    }
}

[HarmonyPatch(typeof(Player))]
internal class SpecialFeatherPatch
{
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (State.IsActive && Items.Has(Items.EternalFeather)) OnGotFeather();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) => isActive = false;
    }
    internal static void OnGotFeather()
    {
        isActive = true;
        OnSilverFeathersUpdated(Context.player, 1);
    }
    private static bool isActive = false;
    [HarmonyPrefix()]
    [HarmonyPatch("UseFeather")]
    internal static bool UseFeather() => !isActive;

    [HarmonyPrefix()]
    [HarmonyPatch("DrainFeatherStamina")]
    internal static bool DrainFeatherStamina(float amount) => !isActive;

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Player), "OnSilverFeathersUpdated")]
    internal static void OnSilverFeathersUpdated(object instance, int number) => throw new NotImplementedException();
}

[HarmonyPatch(typeof(GameObjectID))]
internal class SilverFeatherCollectedPatch
{
    private const string Name = "SilverFeather";
    [HarmonyPrefix()]
    [HarmonyPatch("GetBoolForID")]
    internal static bool GetBoolForID(string prefix, GameObjectID __instance, ref bool __result)
    {
        if (!State.IsActive || __instance.name != Name) return true;
        __result = SpecialFeather.HasGotFeather;
        return false;
    }
    [HarmonyPrefix()]
    [HarmonyPatch("SaveBoolForID")]
    internal static bool SaveBoolForID(string prefix, bool value, GameObjectID __instance)
    {
        if (!State.IsActive || __instance.name != Name) return true;
        if (__instance.name != Name) return true;
        return false;
    }
}
[HarmonyPatch(typeof(TowerViewer))]
internal class TowerViewerPatch
{
    private const string Name = "TowerViewer (1)";
    private const float Prob = 0.4f;
    [HarmonyPrefix()]
    [HarmonyPatch("Interact")]
    internal static void OnInteracted(TowerViewer __instance)
    {
        if (!State.IsActive || __instance.name != Name) return;
        if (UnityEngine.Random.value > Prob) return;
        SpecialFeather.ShowFeather();
    }
}


