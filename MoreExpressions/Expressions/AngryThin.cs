
using UnityEngine;

namespace MoreExpressions.Expressions;

internal class AngryThin : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTexturesMain();
        Func<int, int, bool> pupilF = (x, y) => x == tex.pupilL.texture.width / 2 && y == tex.pupilL.texture.height / 2;
        MaskTexture(ref tex.eyeL, (x, y) => Mathf.Abs(x + 50 + 0.7f * y - tex.eyeL.texture.width) <= 20);
        MaskTexture(ref tex.pupilL, pupilF);
        MaskTexture(ref tex.eyeR, (x, y) => Mathf.Abs(x + 50 - 0.7f * y) <= 20);
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

