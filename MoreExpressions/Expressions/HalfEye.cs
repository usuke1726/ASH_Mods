
namespace MoreExpressions.Expressions;

internal class HalfEye : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        var eyeL = tex.eyeL.texture;
        var pupilL = tex.pupilL.texture;
        Func<int, int, bool> eyeF = (x, y) => x * 2 >= eyeL.width;
        Func<int, int, bool> pupilF = (x, y) => x * 2 >= pupilL.width - 10;
        MaskTexture(ref tex.eyeL, eyeF);
        MaskTexture(ref tex.pupilL, pupilF);
        MaskTexture(ref tex.eyeR, eyeF);
        MaskTexture(ref tex.pupilR, pupilF);
        SetTextures(tex);
    }
}

