
namespace OceanHacks;

public partial class OceanHacks
{
#pragma warning disable IDE1006
    protected static OceanHacks? instance = new();
#pragma warning restore IDE1006
    protected bool HacksWaveHeightMyself = false;
    protected virtual float AdjustedWaveHeight(float velocity)
    {
        //return Mathf.Min(1087.5f / (velocity + 625), 1.5f); // v = 100で速度1.0, v = 3000で速度0.3 => 歩いているときでも頻繁にheightが変わって見栄えが悪い
        return velocity >= 1000 ? 750f / (velocity - 500) : 1.5f; // v = 1000まで速度1.5, v = 3000で速度0.3
    }
    protected virtual float HeightDiff(float targetHeight, float currentHeight)
    {
        return 0.02f;
    }
    protected virtual void Update() { }
    protected virtual void AdjustColors()
    {
        SetColor(ColorRegion.OutsideBorder, DefaultColors[ColorRegion.Coast]);
        SetColor(ColorRegion.InsideBorder, DefaultColors[ColorRegion.Coast]);
    }
}

