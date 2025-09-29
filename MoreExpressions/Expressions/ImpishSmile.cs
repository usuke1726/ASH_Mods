
using UnityEngine;

namespace MoreExpressions.Expressions;

internal class ImpishSmile : CustomExpression
{
    private GameObject _happyL = null!;
    private GameObject _happyR = null!;
    private Sprite defaultHappyLSprite = null!;
    private Sprite defaultHappyRSprite = null!;
    private SpriteRenderer rendererL = null!;
    private SpriteRenderer rendererR = null!;
    private Sprite newSpriteL = null!;
    private Sprite newSpriteR = null!;
    protected override void SetupTextures()
    {
        var tex = GetTextures([Parts.happyL, Parts.happyR]);
        var happyL = tex.happyL.texture;
        var happyR = tex.happyR.texture;
        MaskTexture(ref tex.happyL, (x, y) => MaskL(x, y, happyL.width, happyL.height));
        MaskTexture(ref tex.happyR, (x, y) => MaskR(x, y, happyR.width, happyR.height));
        SetTextures(tex);
    }
    private static bool MaskR(int x, int y, int width, int height)
    {
        float a, b, gamma;
        a = 90; b = 40; gamma = 2.0f;
        float dx = Mathf.Abs(x - width);
        float dy = Mathf.Abs(y - height);
        if (dy <= b && dx <= a)
        {
            dx = a - dx;
            dy = b - dy;
            return dy <= Mathf.Pow((a - dx) / a, gamma) * b;
        }
        else return true;
    }
    private static bool MaskL(int x, int y, int width, int height)
    {
        float a, b, gamma;
        a = 90; b = 40; gamma = 2.0f;
        float dx = Mathf.Abs(x);
        float dy = Mathf.Abs(y - height);
        if (dy <= b && dx <= a)
        {
            dx = a - dx;
            dy = b - dy;
            return dy <= Mathf.Pow((a - dx) / a, gamma) * b;
        }
        else return true;
    }
    protected override void Update(CharacterObject obj) { }
    internal override void Activate(CharacterObject obj)
    {
        base.Activate(obj);
        obj.ShowVanillaEmotion(Emotion.Happy);
    }
}

