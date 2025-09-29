using ModdingAPI;
using UnityEngine;

namespace CoinChallenge;

internal interface IHeightMapCreator
{
    void SetSeed(int? seed);
    void Make(ref float[,] map, int dim);
    List<CollectOnTouch> PlaceCoins(Terrain terrain, int coinMaxCount);
}

internal class Perlin : IHeightMapCreator
{
    public float scale = 1.0f;
    public float freq = 0.06f;
    public float flatWidth1 = 0.2f;
    public float flatWidth2 = 0.2f;
    public float roadFreq = 0.002f;
    public float roadWidth = 0.05f;

    public float r1HeightBase = 0.3f;
    public float r1HeightBound = 0.2f;
    public float r2HeightBase = 0.6f;
    public float r2HeightBound = 0.1f;


    private float perlinHeight = 0;
    private float road1Start = 0;
    private float road1End = 0;
    private float road2Start = 0;
    private float road2End = 0;
    private List<Tuple<int, int>> placeableCoords = [];
    private bool place = false;

    private float xBase;
    private float yBase;
    private int coinSeed;
    public void SetSeed(int? seed)
    {
        using Util.RandomSeed s = new(seed);
        xBase = UnityEngine.Random.value * 1000f;
        yBase = UnityEngine.Random.value * 1000f;
        coinSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }
    public void Make(ref float[,] map, int dim)
    {
        int d = Math.Min(100, dim / 6);
        Func<float, float, float> noise = (x, y) => Mathf.PerlinNoise(x + xBase, y + yBase);
        for (int x = 0; x < dim; x++)
        {
            for (int y = 0; y < dim; y++)
            {
                place = false;
                float r1Height = noise(x * roadFreq, y * roadFreq) * r1HeightBound + r1HeightBase;
                float r2Height = noise(x * roadFreq + 500, y * roadFreq + 500) * r2HeightBound + r2HeightBase;
                road1Start = r1Height - flatWidth1 / 2;
                road1End = r1Height + flatWidth1 / 2;
                road2Start = r2Height - flatWidth2 / 2;
                road2End = r2Height + flatWidth2 / 2;

                perlinHeight = noise(x * freq, y * freq);
                map[x, y] = ConvSlope(perlinHeight);

                int _d = new List<int>([x, y, dim - x, dim - y]).Min();
                if (_d < d) map[x, y] *= _d / (float)d;
                else if (place && map[x, y] > 0.1f) placeableCoords.Add(new(x, y));
            }
        }
        FilterPlaceable(ref map);
    }
    private float ConvSlope(float slope)
    {
        if (slope > road2Start)
        {
            if (slope < road2End)
            {
                slope = road2Start;
                place = true;
            }
            else slope -= flatWidth2;
        }
        else if (slope > road1Start)
        {
            if (slope < road1End)
            {
                slope = road1Start;
                place = true;
            }
            else slope -= flatWidth1;
        }
        return slope;
    }
    private void FilterPlaceable(ref float[,] map)
    {
        List<int> removed = [];
        var num = placeableCoords.Count;
        void FilterOneCoord(ref float[,] map, int idx, int x, int y)
        {
            var h0 = map[x, y];
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    var h1 = map[x - 1 + j, y - 1 + k];
                    //if (!Mathf.Approximately(h0, h1))
                    if (Mathf.Abs(h0 - h1) > 0.001f)
                    {
                        removed.Add(idx);
                        return;
                    }
                }
            }
        }
        for (int i = num - 1; i >= 0; i--)
        {
            var coord = placeableCoords[i];
            FilterOneCoord(ref map, i, coord.Item1, coord.Item2);
        }
        foreach (var i in removed) placeableCoords.RemoveAt(i);
    }
    public List<CollectOnTouch> PlaceCoins(Terrain terrain, int coinMaxCount)
    {
        var dim = terrain.terrainData.heightmapResolution;
        var indexes = Enumerable.Range(0, placeableCoords.Count).ToList();
        List<CollectOnTouch> coins = [];

        int num = Math.Min(indexes.Count, coinMaxCount);
        using Util.RandomSeed s = new(coinSeed);
        for (int i = 0; i < num; i++)
        {
            var idx = UnityEngine.Random.Range(0, indexes.Count);
            var coord = placeableCoords[indexes[idx]];
            indexes.RemoveAt(idx);

            var X = coord.Item1;
            var Z = coord.Item2;
            var coin = SpecialCoin.CloneCoin();
            var height = terrain.terrainData.GetHeight(X, Z);
            var localX = terrain.terrainData.heightmapScale.x * X;
            var localZ = terrain.terrainData.heightmapScale.z * Z;
            coin.transform.position = terrain.transform.position.SetY(0) + new Vector3(localX, height, localZ);
            coin.GetComponent<Rigidbody>().isKinematic = true;
            coins.Add(coin.GetComponent<CollectOnTouch>());
        }
        return coins;
    }
}

internal abstract class MapBase : ICoinChallenge
{
    public bool ForceFinished { get => false; }
    public bool UseTransitionStart { get => true; }
    public bool UseTransitionEnd { get => false; }
    public virtual int? AllowedFeathers { get => null; }
    public virtual float? ArrowHintTime { get => 30; }
    public List<CollectOnTouch> Coins { get => coins; }

    private List<CollectOnTouch> coins = [];
    protected abstract IHeightMapCreator MapCreator { get; }
    protected abstract float size { get; }
    protected abstract Vector3 position { get; }
    protected abstract int resolution { get; }
    protected virtual int coinMaxCount { get => 100; }
    public void Setup()
    {
        Monitor.Log($"== Setup Start", LL.Warning, true);
        var prevObj = GameObject.Find("/World/Terrains/NEW TERRAIN");
        if (prevObj != null) GameObject.Destroy(prevObj);
        var mapCreator = MapCreator;
        mapCreator.SetSeed(null);
        var terrains = GameObject.Find("/World/Terrains");
        var terrain = terrains.transform.Find("Terrain").gameObject;
        var data = new TerrainData();
        var s = size;
        Vector3 pos = position;
        data.size = new(s, 200, s);
        data.heightmapResolution = resolution;
        var newHeights = new float[data.heightmapResolution, data.heightmapResolution];
        mapCreator.Make(ref newHeights, data.heightmapResolution);

        data.terrainLayers = terrain.GetComponent<Terrain>().terrainData.terrainLayers;

        data.SetHeights(0, 0, newHeights);

        var obj = Terrain.CreateTerrainGameObject(data);
        obj.name = "NEW TERRAIN";
        var newTerrain = obj.GetComponent<Terrain>();
        newTerrain.materialTemplate = terrain.GetComponent<Terrain>().materialTemplate;

        obj.transform.parent = terrains.transform;
        obj.transform.position = pos;
        var collider = obj.GetComponent<TerrainCollider>();
        collider.terrainData = data;
        obj.layer = 12; // layer: SoftGround (climbable layer)
        coins = mapCreator.PlaceCoins(newTerrain, coinMaxCount);
        Context.player.transform.position = pos + Vector3.up * 2;
        Context.gameServiceLocator.levelController.cinemaCamera.SetActive(true);
        Monitor.Log($"== Setup End", LL.Warning, true);
    }
    public void Cleanup()
    {
        Context.gameServiceLocator.levelController.cinemaCamera.SetActive(false);
        if (coins == null) return;
        foreach (var coin in coins)
        {
            if (coin == null) continue;
            var obj = coin.gameObject;
            if (obj != null) GameObject.Destroy(obj);
        }
    }
}

