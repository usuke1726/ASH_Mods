
namespace MoreExpressions.Expressions;

internal class Thin : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        var eyeL = tex.eyeL.texture;
        var pupilL = tex.pupilL.texture;
        Func<int, int, bool> eyeF = (x, y) => Math.Abs(x - eyeL.width / 2) <= 20;
        Func<int, int, bool> pupilF = (x, y) => x == pupilL.width / 2 && y == pupilL.height / 2;
        MaskTexture(ref tex.eyeL, eyeF);
        MaskTexture(ref tex.pupilL, pupilF);
        MaskTexture(ref tex.eyeR, eyeF);
        MaskTexture(ref tex.pupilR, pupilF);
        tex.pupilL.color = tex.pupilR.color = new(0, 0, 0, 0);
        SetTextures(tex);
    }
    internal override void Activate(CharacterObject obj)
    {
        base.Activate(obj);
        obj.StopBlink();
    }
    internal override void Deactivate(CharacterObject obj)
    {
        base.Deactivate(obj);
        obj.RestartBlink();
    }
}

