
using System.Reflection;
using Cinemachine;
using HarmonyLib;
using ModdingAPI;
using UnityEngine;

namespace ClearView;

[HarmonyPatch(typeof(BorderLine))]
internal class BorderLinePatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("OutsideBorder")]
    public static bool Prefix(Vector3 point) => !ModEntry.config.NoBorder;
}

// Prevents from disappearing the ocean when the player is on the top of Outlook Point
[HarmonyPatch(typeof(Renderer))]
internal class MeshRendererPatch
{
    [HarmonyPrefix()]
    [HarmonyPatch("enabled", MethodType.Setter)]
    public static bool Prefix(bool value, Renderer __instance)
    {
        if (__instance.name == "CameraOceanWater") return false;
        return true;
    }
}


partial class ModEntry
{
    private GameObject cloud = null!;
    private GameObject cloud1 = null!;
    private GameObject cloud2 = null!;
    private GameObject ocean1 = null!;
    private GameObject ocean2 = null!;
    private float ocean1DefaultMaxSize;
    private float ocean2DefaultMaxSize;
    private float ocean2DefaultColliderY;
    private float playerDefaultMovementForce;
    private float playerDefaultMidairForce;
    private float defaultFogStart;
    private float defaultFogEnd;
    private Dictionary<string, float> VCamDefaultFarClip = [];
    private Dictionary<TerrainBaker, float> defaultDistance = [];
    private float defaultCinemaCameraFarClip;
    private void Setup()
    {
        cloud = GameObject.Find("Clouds");
        cloud1 = GameObject.Find("Clouds (1)");
        cloud2 = GameObject.Find("Clouds (2)");

        UpdateNoCloud(config.NoCloud);

        ocean1 = GameObject.Find("OceanWaterFloor");
        ocean2 = GameObject.Find("CameraOceanWater");
        ocean1DefaultMaxSize = ocean1.GetComponent<CameraFollowPlane>().maxSize;
        ocean2DefaultMaxSize = ocean2.GetComponent<CameraFollowPlane>().maxSize;
        ocean2DefaultColliderY = ocean2.GetComponent<BoxCollider>().size.y;

        defaultFogStart = RenderSettings.fogStartDistance;
        defaultFogEnd = RenderSettings.fogEndDistance;

        var camera = Context.levelController.cinemaCamera;
        var cont = camera.GetComponent<CinemaCameraControls>();
        var cam = cont.GetComponent<CinemachineVirtualCamera>();
        defaultCinemaCameraFarClip = cam.m_Lens.FarClipPlane;

        var player = Context.player;
        playerDefaultMovementForce = player.movementForce;
        playerDefaultMidairForce = player.midairForce;

        UpdateEnabled(config.Enabled);

        var floor = GameObject.Find("/World/Bounds/Floor");
        floor.GetComponent<BoxCollider>().size = new(20, 1, 20);
    }
    private void UpdateNoCulling(bool f)
    {
        var manager = Context.gameServiceLocator.cullingManager;
        var cullingGroup = Traverse.Create(manager).Field("cullingGroup").GetValue();
        if (cullingGroup == null) return;
        var method = cullingGroup.GetType().GetMethod("SetBoundingDistances", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
        if (method == null) return;
        var value = f ? new float[3] { 4250f, 4500f, 5000f } : new float[3] { 250f, 500f, 1000f };
        method.Invoke(cullingGroup, new object[1] { value });

        Monitor.Log($"UpdateNoCulling done successfullly", LL.Debug);
    }
    private void UpdateNoCloud(bool f)
    {
        var m = cloud.GetComponent<MeshRenderer>();
        var m1 = cloud1.GetComponent<MeshRenderer>();
        var m2 = cloud2.GetComponent<MeshRenderer>();
        if (m != null) m.forceRenderingOff = f;
        if (m1 != null) m1.forceRenderingOff = f;
        if (m2 != null) m2.forceRenderingOff = f;
        if (m != null && m1 != null && m2 != null) Monitor.Log($"all cloud ok", LL.Debug);
    }
    private void UpdateFogLevel(int level)
    {
        if (!config.Enabled) level = 25;
        var (start, end) = level switch
        {
            0 => (4500, 5000),
            1 => (3500, 5000),
            2 => (3000, 4000),
            3 => (2500, 3500),
            4 => (2000, 3000),
            5 => (1500, 2500),
            6 => (1000, 2300),
            7 => (800, 1900),
            8 => (680, 1500),
            9 => (600, 1200),
            10 => (550, 1200),
            11 => (490, 1000),
            12 => (450, 1000),
            13 => (410, 900),
            14 => (380, 900),
            15 => (350, 800),
            16 => (320, 800),
            17 => (290, 700),
            18 => (260, 700),
            19 => (230, 700),
            20 => (200, 600),
            21 => (180, 500),
            22 => (160, 450),
            23 => (140, 400),
            24 => (120, 350),
            25 => (100, 300), // default values
            26 => (90, 270),
            27 => (80, 240),
            28 => (70, 210),
            29 => (60, 180),
            30 => (50, 150),
            _ => (4500, 5000)
        };
        RenderSettings.fogStartDistance = start;
        RenderSettings.fogEndDistance = end;
    }
    private void Reset()
    {
        setupDone = false;
        vCamObjCache.Clear();
        VCamDefaultFarClip.Clear();
        defaultDistance.Clear();
    }
    private void UpdateEnabled(bool f)
    {
        ocean1.GetComponent<CameraFollowPlane>().maxSize = f ? 600 : ocean1DefaultMaxSize;
        ocean2.GetComponent<CameraFollowPlane>().maxSize = f ? 600 : ocean2DefaultMaxSize;
        var box = ocean2.GetComponent<BoxCollider>();
        box.size = box.size.SetY(f ? 1.965f : ocean2DefaultColliderY);
        UpdateFogLevel(config.FogLevel);
        UpdateNoCulling(f);

        foreach (var terrain in UnityEngine.Object.FindObjectsOfType<TerrainBaker>())
        {
            if (!defaultDistance.ContainsKey(terrain)) defaultDistance[terrain] = terrain.smallTreeFlatCullDistance;
            terrain.smallTreeFlatCullDistance = f ? 5000f : defaultDistance[terrain];
        }

        foreach (var towerViwer in UnityEngine.Object.FindObjectsOfType<TowerViewer>())
        {
            var cam = towerViwer.GetComponentInChildren<CinemachineVirtualCamera>();
            var key = $"towerviewer:{towerViwer.name}";
            Monitor.Log($"VCam name: {towerViwer.name}, cam active: {cam != null}", LL.Debug);
            if (cam == null) continue;
            if (!VCamDefaultFarClip.ContainsKey(key)) VCamDefaultFarClip[key] = cam.m_Lens.FarClipPlane;
            var to = f ? 4000f : VCamDefaultFarClip[key];
            if (Mathf.Abs(cam.m_Lens.FarClipPlane - to) > 5f)
            {

                cam.m_Lens.FarClipPlane = to;
            }
        }
    }

    private bool setupDone = false;
    private Dictionary<string, GameObject?> vCamObjCache = [];
    private void Update()
    {
        if (!setupDone) { setupDone = true; Setup(); }
        if (!config.Enabled) return;
        List<string> vCamNames = [
            "StandardGameVCam",
            "BackBeach",
            "BackBeachZoomOut",
            "Beach",
            "CraterLakeRidge",
            "FireRidge",
            "FireTower",
            "Forest",
            "SnowFlighht",
            "SnowPeak",
            "SnowPeakCrack",
            "SnowPeakZoom",
            "WestRidge",
            "SnowVeryPeak",
            "SnowTip",
            "SnowHikerCam",
            "RockCameraArea",
            "DebugIsland",
            "DebugIslandZoom",
            "DebugIslandHouse",
            "DebugIslandWaterCamera",
            "CaveCam",
            "Cliffs",
            "BoatIsland",
            "SitCutsceneCam",
            "CellCutsceneCam",
            "TipTopCutsceneLakeCam",
            "FoxZoomCam",
            "FoxZoomCamLight",
        ];
        foreach (var name in vCamNames)
        {
            if (!vCamObjCache.ContainsKey(name)) vCamObjCache[name] = null;
            vCamObjCache[name] ??= GameObject.Find(name);
            var obj = vCamObjCache[name];
            if (obj == null) continue;
            var vCam = obj.GetComponentInChildren<CinemachineVirtualCamera>();
            if (vCam == null) continue;
            if (!VCamDefaultFarClip.ContainsKey(name)) VCamDefaultFarClip[name] = vCam.m_Lens.FarClipPlane;
            var to = config.Enabled ? 4000f : VCamDefaultFarClip[name];
            if (Mathf.Abs(vCam.m_Lens.FarClipPlane - to) > 5f)
            {
                Monitor.Log($"VCam name: {name}, far (original): {vCam.m_Lens.FarClipPlane} -> {to}", LL.Debug);
                vCam.m_Lens.FarClipPlane = to;
            }
        }
        UpdatePeakCutscene();

        var cinemaFarClipTo = config.Enabled ? 40000f : defaultCinemaCameraFarClip;
        var camera = Context.levelController.cinemaCamera;
        var cont = camera.GetComponent<CinemaCameraControls>();
        var cam = cont.GetComponent<CinemachineVirtualCamera>();
        if (Mathf.Abs(cam.m_Lens.FarClipPlane - cinemaFarClipTo) > 5f)
        {
            Monitor.Log($"=== invalid farClip {cam.m_Lens.FarClipPlane}", LL.Debug);
            cam.m_Lens.FarClipPlane = cinemaFarClipTo;
        }
    }

    private float sitCutsceneDefaultFogStart = -1;
    private float sitCutsceneDefaultFogEnd = -1;
    private float sitCutsceneFogStart = -1;
    private float sitCutsceneFogEnd = -1;
    private float targetFogStart = -1;
    private float targetFogEnd = -1;
    private int peakCutsceneMode = 0;
    private void UpdatePeakCutscene()
    {
        var sitObj = vCamObjCache["SitCutsceneCam"] ??= GameObject.Find("SitCutsceneCam");
        var cellObj = vCamObjCache["CellCutsceneCam"] ??= GameObject.Find("CellCutsceneCam");
        if (peakCutsceneMode == 0)
        {
            if (sitObj != null)
            {
                peakCutsceneMode++;
                sitCutsceneFogStart = sitCutsceneDefaultFogStart = RenderSettings.fogStartDistance;
                sitCutsceneFogEnd = sitCutsceneDefaultFogEnd = RenderSettings.fogEndDistance;
                targetFogStart = Math.Min(sitCutsceneFogStart, 250f);
                targetFogEnd = Math.Min(sitCutsceneFogEnd, 1200f);
                Monitor.Log($"== Cutscene start!!", LL.Debug, true);
            }
            return;
        }
        else if (peakCutsceneMode == 1)
        {
            if (cellObj != null && !cellObj.activeSelf)
            {
                peakCutsceneMode++;
                Monitor.Log($"== Cutscene end!!", LL.Debug, true);
                return;
            }
            var fstart = Mathf.Abs(sitCutsceneFogStart - 100f) >= 0.1f;
            var fend = Mathf.Abs(sitCutsceneFogEnd - 300f) >= 0.1f;
            if (fstart)
            {
                sitCutsceneFogStart = Mathf.MoveTowards(sitCutsceneFogStart, targetFogStart, 3f);
                RenderSettings.fogStartDistance = sitCutsceneFogStart;
            }
            if (fend)
            {
                sitCutsceneFogEnd = Mathf.MoveTowards(sitCutsceneFogEnd, targetFogEnd, 3f);
                RenderSettings.fogEndDistance = sitCutsceneFogEnd;
            }
        }
        else if (peakCutsceneMode == 2)
        {
            var fstart = Mathf.Abs(sitCutsceneFogStart - sitCutsceneDefaultFogStart) >= 0.1f;
            var fend = Mathf.Abs(sitCutsceneFogEnd - sitCutsceneDefaultFogEnd) >= 0.1f;
            if (fstart)
            {
                sitCutsceneFogStart = Mathf.MoveTowards(sitCutsceneFogStart, sitCutsceneDefaultFogStart, 20f);
                RenderSettings.fogStartDistance = sitCutsceneFogStart;
            }
            if (fend)
            {
                sitCutsceneFogEnd = Mathf.MoveTowards(sitCutsceneFogEnd, sitCutsceneDefaultFogEnd, 20f);
                RenderSettings.fogEndDistance = sitCutsceneFogEnd;
            }
            if (!fstart && !fend) peakCutsceneMode++;
        }
    }
}

