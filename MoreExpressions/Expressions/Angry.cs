
namespace MoreExpressions.Expressions;

internal class Angry : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        MaskTexture(ref tex.eyeL, (x, y) => y / -2.0f + 128 + 64 <= x);
        MaskTexture(ref tex.pupilL, (x, y) => y / -2.0f + 128 + 50 <= x);
        MaskTexture(ref tex.eyeR, (x, y) => y / 2.0f + 64 <= x);
        MaskTexture(ref tex.pupilR, (x, y) => y / 2.0f + 50 <= x);
        SetTextures(tex);
    }
}

