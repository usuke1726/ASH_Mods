
using UnityEngine;

namespace MoreExpressions.Expressions;

internal class Sparkle : CustomExpression
{
    protected override void SetupTextures()
    {
        var tex = GetTextures([Parts.pupilL, Parts.pupilR]);
        var pupilL = tex.pupilL.texture;
        var pupilR = tex.pupilR.texture;
        MaskTexture(ref tex.pupilL, (x, y) => MaskL(x, y, pupilL.width, pupilL.height));
        MaskTexture(ref tex.pupilR, (x, y) => MaskR(x, y, pupilR.width, pupilR.height));
        SetTextures(tex);
    }
    private static bool MaskL(int x, int y, int width, int height)
    {
        bool isInDiamond;
        int a1, b1;
        int cx, cy;
        float gamma;
        cx = 110; cy = 160; a1 = 75; b1 = 60; gamma = 2.0f;

        float dx = Mathf.Abs(x - cx);
        float dy = Mathf.Abs(y - cy);
        if (dy <= b1 && dx <= a1)
        {
            isInDiamond = dy <= Mathf.Pow((a1 - dx) / a1, gamma) * b1; // modified gamma correction function to pass through points (a1, 0) and (0, b1)
        }
        else isInDiamond = false;
        return !isInDiamond;
    }
    private static bool MaskR(int x, int y, int width, int height)
    {
        return MaskL(x, y, width, height);
    }
}

