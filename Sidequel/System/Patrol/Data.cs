
using UnityEngine;
using CP = Sidequel.Const.PatrolCheckpoints;

namespace Sidequel.System.Patrol;

internal class Data(Vector3 center, Vector3 size)
{
    private void Create(CP checkpoint)
    {
        new GameObject($"Sidequel_PatrolCheckpoint_{checkpoint}").AddComponent<Checkpoint>().Set(checkpoint, center + YOffset(size), size);
    }
    private static readonly Vector3 defSize = new(100, 70, 100);
    private static Vector3 YOffset(Vector3 size) => new(0, size.y * 0.5f, 0);
    private static readonly Dictionary<CP, Data> data = new()
    {
        [CP.VisitorCenter] = new(new(161.7521f, 32.1959f, 116.3725f), defSize),
        [CP.CharliesCabin] = new(new(310.2239f, 398.1545f, 628.6873f), new(50, 70, 100)),
        [CP.BoatShop] = new(new(144.8f, 10.5375f, 1281.924f), defSize),
        [CP.AbandonedBuilding] = new(new(106.7198f, 95.2447f, 714.1024f), new(100, 70, 80)),
        [CP.Cemetery] = new(new(183.1784f, 55.5487f, 1039.433f), defSize),
        [CP.PatsPointFrontSide] = new(new(782.9742f, 23.0218f, 95.0093f), defSize),
        [CP.PatsPointBackSide] = new(new(923.8657f, 18.5971f, 73.2695f), new(40, 100, 140)),
        [CP.MeteorLakeNorth] = new(new(575.9614f, 139.6488f, 678.9313f), defSize),
        [CP.MeteorLakeSouth] = new(new(573.1257f, 123.5162f, 445.1068f), defSize),
        [CP.MeteorLakeEast] = new(new(700.5755f, 139.6488f, 526.3012f), defSize),
        [CP.MeteorLakeWest] = new(new(455.3196f, 140.8858f, 575.3825f), defSize),
        [CP.MeteorLakeCenter] = new(new(568.1222f, 93.5599f, 566.8763f), defSize),
        [CP.SnowMountainBackSide] = new(new(486.8048f, 359.4586f, 775.8733f), defSize),
        [CP.StoneTower] = new(new(122.2644f, 101.1943f, 499.8791f), defSize),
        [CP.StoneTowerSouth] = new(new(73.1157f, 48.6944f, 365.7214f), defSize),
        [CP.OutlookPoint] = new(new(233.9966f, 243.0291f, 377.6696f), defSize),
        [CP.OrangeIslandWest] = new(new(424.5881f, 11.1741f, 1363.419f), defSize),
        [CP.SueSpot] = new(new(312.756f, 82.9193f, 260.5101f), defSize),
        [CP.CamperSpot] = new(new(460.0093f, 82.9216f, 476.5495f), defSize),
        [CP.WhiteCoastTrail] = new(new(464.8733f, 22.857f, 55.8472f), defSize),
        [CP.Ship] = new(new(736.1688f, 13.4794f, 744.4612f), defSize),
        [CP.CarterSpot] = new(new(263.8315f, 267.0797f, 541.3387f), defSize),
        [CP.Lighthouse] = new(new(541.2647f, 84.6239f, 355.5919f), defSize),
        [CP.SidBeach] = new(new(278.1137f, 11.0211f, 22.6944f), defSize),
        [CP.BeachstickCourt] = new(new(96.6818f, 10.4783f, 995.8109f), defSize),
        [CP.SamSpot] = new(new(897.8646f, 21.3554f, 518.8456f), defSize),
        [CP.SunhatDeerSpot] = new(new(962.7299f, 4.6985f, 954.7576f), defSize),
        [CP.CarterSpotEast] = new(new(445.754f, 240.9279f, 638.9698f), defSize),
        [CP.CarterSpotWest] = new(new(200.7706f, 288.9309f, 669.2079f), defSize),
        [CP.PeakPictureFoxSpot] = new(new(433.8105f, 498.581f, 824.551f), defSize),
        [CP.Peak] = new(new(396.7271f, 594.7486f, 819.3619f), defSize),
        [CP.ElectricityPylon] = new(new(589.0399f, 23.9825f, 835.9565f), defSize),
        [CP.OutlookPointFootSide] = new(new(199.5832f, 110.5332f, 283.6823f), defSize),
    };
    internal static void CreateCheckpoints()
    {
        foreach (var d in data) d.Value.Create(d.Key);
    }
}

