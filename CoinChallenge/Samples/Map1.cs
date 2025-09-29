
using UnityEngine;

namespace CoinChallenge.Samples;

internal class Map1 : MapBase
{
    protected override IHeightMapCreator MapCreator => new Perlin()
    {
        scale = 1.0f,
        freq = 0.06f,

        flatWidth1 = 0.2f,
        flatWidth2 = 0.25f,
        roadFreq = 0.002f,
        roadWidth = 0.05f,

        r1HeightBase = 0.3f,
        r1HeightBound = 0.2f,
        r2HeightBase = 0.6f,
        r2HeightBound = 0.1f,
    };

    protected override float size => 64;
    protected override Vector3 position => new(-1400f, 0, -1400f);
    protected override int resolution => 257;
    public override int? AllowedFeathers => 7;
}


