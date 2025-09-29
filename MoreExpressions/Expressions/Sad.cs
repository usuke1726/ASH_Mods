
namespace MoreExpressions.Expressions;

internal class Sad : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        MaskTexture(ref tex.eyeL, (x, y) => y / 4.0f + 64 <= x);
        MaskTexture(ref tex.pupilL, (x, y) => y / 4.0f + 30 <= x);
        MaskTexture(ref tex.eyeR, (x, y) => y / -4.0f + 64 + 64 <= x);
        MaskTexture(ref tex.pupilR, (x, y) => y / -4.0f + 64 + 30 <= x);
        SetTextures(tex);
    }
}

