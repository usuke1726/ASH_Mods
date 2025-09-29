
//#define DEBUG_OCEAN_COLOR
//#define DEBUG_WAVE_HEIGHT

using ModdingAPI;
using UnityEngine;

namespace OceanHacks;

partial class OceanHacks
{
    // 通常の海のレンダラー
    private static MeshRenderer? oceanRendererCache = null;
    // ボートに乗っているときだけ縞模様に表示されるレンダラー
    // 何のために用意されているレンダラーかは分からないが，こちらのレンダラーのマテリアルのプロパティも変更しないと，色などが一部反映されなくなる
    private static MeshRenderer? longCameraRendererCache = null;
    private static readonly Dictionary<ColorRegion, Color> defaultColors = [];
    protected static IReadOnlyDictionary<ColorRegion, Color> DefaultColors => defaultColors;
    private static bool setupDone = false;
    private static void Reset()
    {
        oceanRendererCache = null;
        longCameraRendererCache = null;
        setupColorsDone = false;
    }
    internal static void Setup(IModHelper helper)
    {
        if (!setupDone)
        {
            setupDone = true;
            helper.Events.Gameloop.ReturnedToTitle += (s, e) => Reset();
            helper.Events.Gameloop.PlayerUpdated += (s, e) => UpdateMain();
        }
    }
    private static void UpdateMain()
    {
        var adjuster = instance;
        if (adjuster == null) return;
        if (!TryToGetRenderers(out _, out _)) Monitor.Log($"failed to get renderers", LL.Warning);
        SetupColors(adjuster);
        adjuster.Update();
        if (!adjuster.HacksWaveHeightMyself) AdjustWaveHeight(adjuster);
    }

    protected static void Apply(Action<MeshRenderer> action)
    {
        if (TryToGetRenderers(out var renderer, out var longCameraRenderer))
        {
            action(renderer);
            action(longCameraRenderer);
        }
    }
    protected static bool TryToGetRenderers(out MeshRenderer renderer, out MeshRenderer longCameraRenderer)
    {
        if (oceanRendererCache != null && longCameraRendererCache != null)
        {
            renderer = oceanRendererCache;
            longCameraRenderer = longCameraRendererCache;
            return true;
        }
        var obj = GameObject.Find("CameraOceanWater");
        renderer = (oceanRendererCache = obj?.GetComponent<MeshRenderer>())!;
        var obj2 = GameObject.Find("Ocean")?.transform?.Find("LongCameraOceanWater")?.gameObject;
        longCameraRenderer = (longCameraRendererCache = obj2?.GetComponent<MeshRenderer>())!;
        return renderer != null && longCameraRenderer != null;
    }
    private static void AdjustWaveHeight(OceanHacks adjuster)
    {
        if (!TryToGetRenderers(out var renderer, out _)) return;
        var camera = Camera.main;
        if (camera == null) return;
        var velocity = camera.velocity.sqrMagnitude;
        var height = Mathf.Max(adjuster.AdjustedWaveHeight(velocity), 0);
        var currentHeight = renderer.material.GetFloat(Tags.WaveHeight);
        var diff = Mathf.Abs(adjuster.HeightDiff(height, currentHeight));
        DebugWaveHeight(adjuster);
        float? to = null;
        if (height - currentHeight >= diff) to = currentHeight + diff;
        else if (height - currentHeight <= -diff) to = currentHeight - diff;
        if (to != null) Apply(r => r.material.SetFloat(Tags.WaveHeight, (float)to));
    }

    [Conditional("DEBUG_WAVE_HEIGHT")]
    protected static void DebugWaveHeight(OceanHacks adjuster)
    {
        if (!TryToGetRenderers(out var renderer, out _)) return;
        var camera = Camera.main;
        if (camera == null) return;
        var velocity = camera.velocity.sqrMagnitude;
        var height = Mathf.Max(adjuster.AdjustedWaveHeight(velocity), 0);
        var currentHeight = renderer.material.GetFloat(Tags.WaveHeight);
        var color = velocity switch
        {
            > 20000 => "red",
            > 10000 => "yeloow",
            > 5000 => "green",
            _ => ""
        };
        Monitor.Log([
            [$"= V:", ""],
            [$" {velocity:00.00}", color],
            [$" (height -> {height}, current: {currentHeight})", ""]
        ], onlyMonitor: true);
    }

    private static bool setupColorsDone = false;
    private static void SetupColors(OceanHacks adjuster)
    {
        if (setupColorsDone) return;
        if (!TryToGetRenderers(out var renderer, out _)) return;
        setupColorsDone = true;
        foreach (ColorRegion region in Enum.GetValues(typeof(ColorRegion)))
        {
            defaultColors[region] = renderer.material.GetColor(TagFromRegion(region));
        }
        adjuster.AdjustColors();
        DebugOceanColor();
    }

    protected static void SetColor(ColorRegion region, Color color)
    {
        Apply(r => r.material.SetColor(TagFromRegion(region), color));
    }

    [Conditional("DEBUG_OCEAN_COLOR")]
    protected static void DebugOceanColor()
    {
        SetColor(ColorRegion.OutsideBorder, new(0, 0.5f, 1, 1)); // light blue
        SetColor(ColorRegion.InsideBorder, new(1, 1, 0, 1)); // yellow
        SetColor(ColorRegion.Normal, new(1, 0, 0, 1)); // red
        SetColor(ColorRegion.AroundIslands, new(0, 1, 0, 1)); // green
        SetColor(ColorRegion.Foam, new(1, 0.6f, 0, 1)); // orange
        SetColor(ColorRegion.Coast, new(1, 0, 1, 1)); // purple
        SetColor(ColorRegion.WhiteFoam, new(0, 1, 0.7f, 1)); // blue-green
    }
}

