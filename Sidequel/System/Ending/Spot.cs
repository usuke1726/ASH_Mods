
using System.Reflection;
using UnityEngine;

namespace Sidequel.System.Ending;

internal enum Atmospheres
{
    Default,
    Stormy,
    Ice,
    Pinkish,
}
internal class Spot(Spot.VectorTuple positions, Spot.VectorTuple rotations, Atmospheres atmosphere = Atmospheres.Default)
{
    internal class VectorTuple(Vector3 initial, Vector3? final = null)
    {
        internal Vector3 initial = initial;
        internal Vector3 final = final ?? initial;
    }
    private readonly Atmospheres atmosphere = atmosphere;
    private float startTime = -1;
    private float time;
    internal Vector3 InitialPosition { get; private set; } = positions.initial;
    internal Vector3 FinalPosition { get; private set; } = positions.final;
    internal Vector3 InitialRotation { get; private set; } = rotations.initial;
    internal Vector3 FinalRotation { get; private set; } = rotations.final;
    internal Vector3 Position { get; private set; } = positions.initial;
    internal Vector3 Rotation { get; private set; } = rotations.initial;
    internal void Update()
    {
        if (startTime < 0) startTime = Time.time;
        var t = EaseOut(Mathf.Clamp((Time.time - startTime) / time, 0, 1));
        t = Mathf.Clamp(t, 0, 1);
        Position = Vector3.Lerp(InitialPosition, FinalPosition, t);
        Rotation = Vector3.Lerp(InitialRotation, FinalRotation, t);
    }
    private static float Linear(float t) => t < 0.8f ? 1.15f * t : -3.75f * t * t + 7.15f * t - 2.4f;
    private static float EaseOut(float t) => 1 - (1 - t) * (1 - t);
    private static float EaseInOut(float t) => t < 0.5 ? 2 * t * t : -2 * t * t + 4 * t - 1;
    private static MethodInfo setAtmosphere = null!;
    private static Dictionary<Atmospheres, Atmosphere?> atmospheres = [];
    internal static void SetupAtmospheres()
    {
        atmospheres[Atmospheres.Default] = null;
        atmospheres[Atmospheres.Ice] = GameObject.Find("IceAtmoshphere").GetComponent<AtmosphereRegion>().atmosphere;
        atmospheres[Atmospheres.Stormy] = GameObject.Find("StormyAtmosphere").GetComponent<AtmosphereRegion>().atmosphere;
        atmospheres[Atmospheres.Pinkish] = GameObject.Find("PinkishAtmosphere").GetComponent<AtmosphereRegion>().atmosphere;
    }
    internal void Reset(float time)
    {
        this.time = Math.Max(time, 1f);
        setAtmosphere ??= typeof(AtmosphereController).GetMethod("SetAtmosphere", BindingFlags.NonPublic | BindingFlags.Instance);
        var atmosphereController = Singleton<GameServiceLocator>.instance.atmosphereController;
        setAtmosphere.Invoke(atmosphereController, [atmospheres[atmosphere]]);
        startTime = -1;
    }

    private static readonly Spot[] spots = [
        // mateor lake
        new(
            new(new(528.3746f, 298.0485f, 382.1736f), new(609.6685f, 298.0485f, 472.6471f)),
            new(new(51.9398f, 5.8725f, 0))
        ),

        // forest
        new(
            new(new(527.8304f, 212.9406f, 563.4382f), new(383.4098f, 186.1473f, 463.4812f)),
            new(new(54.0961f, 232.6185f, 359.5925f), new(41.5963f, 189.8337f, 359.5925f))
        ),

        // orange island
        new(
            new(new(137.1313f, 118.4114f, 1433.346f), new(41.9888f, 118.4114f, 1000.257f)),
            new(new(31.8065f, 181.5928f, 359.5925f), new(53.4853f, 106.3716f, 359.5925f))
        ),

        // may
        new(
            new(new(733.5867f, 82.703f, 334.1931f), new(733.5867f, 170.7363f, 334.1931f)),
            new(new(35.3729f, 288.2421f, 359.5925f), new(60.0504f, 288.2421f, 359.5925f))
        ),

        // charlie & climbers
        new(
            new(new(247.9232f, 507.6409f, 451.0605f), new(247.9232f, 507.6409f, 627.8513f)),
            //new(new(247.9232f, 507.6409f, 551.0605f), new(247.9232f, 507.6409f, 727.8513f)),
            new(new(48.9062f, 36.4507f, 0)),
            atmosphere: Atmospheres.Ice
        ),

        // peak
        //new(
        //    new(new(306.5406f, 688.2347f, 785.473f)
        //    // , new(368.1402f, 688.2347f, 887.2323f)
        //    ),
        //    new(new(48.2917f, 58.9963f, 0)
        //    // , new(48.2917f, 157.0107f, 0)
        //    ),
        //    atmosphere: Atmospheres.Pinkish
        //),

        // raining region
        new(
            new(new(319.2817f, 104.0695f, 1236.549f), new(588.5309f, 104.0695f, 921.6915f)),
            new(new(31.8067f, 149.4333f, 359.5925f)),
            atmosphere: Atmospheres.Stormy
        ),

        // jon
        new(
            new(new(204.1833f, 168.242f, -133.019f), new(204.1833f, 168.242f, 95.7339f)),
            new(new(41.5975f, 9.7834f, 359.5925f))
        ),

        // whitecoast trail
        new(
            new(new(261.2622f, 127.2522f, 18.5168f), new(459.3102f, 127.2522f, 90.6747f)),
            new(new(46.789f, 72.0363f, 0), new(46.789f, 36.4218f, 0))
        ),

        // avery
        //new(
        //    new(new(-32.4203f, 158.4523f, 397.9588f), new(161.8462f, 184.5761f, 249.3342f)),
        //    new(new(36.0966f, 128.5675f, 0), new(46.7883f, 104.8436f, 0))
        //),

        // sam
        //new(
        //    new(new(716.1357f, 143.5436f, 3.4305f), new(818.3155f, 143.5436f, 475.2779f)),
        //    new(new(46.7911f, 37.0394f, 0))
        //),

        // outlook point
        new(
            new(new(215.5456f, 361.1426f, 266.1502f), new(215.5456f, 361.1426f, 498.85f)),
            new(new(51.6851f, 33.0987f, 359.5925f), new(51.6851f, 17.0842f, 359.5925f))
        ),

        // ship
        new(
            new(new(882.9846f, 97.5435f, 834.8956f), new(1256.079f, 97.5435f, 972.4607f)),
            new(new(24.4005f, 240.3368f, 359.5924f))
        ),
    ];
    private static int idx = 0;
    internal static Spot Next()
    {
        var ret = spots[idx];
        idx = (idx + 1) % spots.Length;
        return ret;
    }
}

