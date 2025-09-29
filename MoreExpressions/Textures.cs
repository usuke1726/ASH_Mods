
using UnityEngine;

namespace MoreExpressions;

#pragma warning disable IDE1006

public enum Parts
{
    eyeL, eyeR,
    pupilL, pupilR,
    happyL, happyR,
    blinkL, blinkR,
}

internal class Part(Texture2D tex, Color? color)
{
    public Texture2D texture = tex;
    public Color? color = color;
    public Part(Part part) : this(part.texture, part.color) { }
    public Part() : this(null!, null) { }
}

internal class Textures(
    Part? eyeL = null,
    Part? eyeR = null,
    Part? pupilL = null,
    Part? pupilR = null,
    Part? happyL = null,
    Part? happyR = null,
    Part? blinkL = null,
    Part? blinkR = null
    )
{
    public Part eyeL = eyeL ?? new();
    public Part eyeR = eyeR ?? new();
    public Part pupilL = pupilL ?? new();
    public Part pupilR = pupilR ?? new();
    public Part happyL = happyL ?? new();
    public Part happyR = happyR ?? new();
    public Part blinkL = blinkL ?? new();
    public Part blinkR = blinkR ?? new();
    public Textures(Textures tex) : this(tex.eyeL, tex.eyeR, tex.pupilL, tex.pupilR, tex.happyL, tex.happyR, tex.blinkL, tex.blinkR) { }
}

