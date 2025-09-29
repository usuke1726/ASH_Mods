
using UnityEngine;

namespace MoreExpressions.Expressions;

internal class SadThin : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        var pupilL = tex.pupilL.texture;
        var eyeR = tex.eyeR.texture;
        Func<int, int, bool> pupilF = (x, y) => x == pupilL.width / 2 && y == pupilL.height / 2;
        MaskTexture(ref tex.eyeL, (x, y) => Mathf.Abs(x - 0.5f * y) <= 20);
        MaskTexture(ref tex.pupilL, pupilF);
        MaskTexture(ref tex.eyeR, (x, y) => Mathf.Abs(x + 0.5f * y - eyeR.width) <= 20);
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

