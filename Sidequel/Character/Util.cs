
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Character;

internal abstract class ColorChanger()
{
    protected Material[] materials = [];
    private Texture2D[] originalTex = [];
    protected abstract Color Change(Color originalColor);
    internal ColorChanger WithMaterials(Material material) => WithMaterials([material]);
    internal ColorChanger WithMaterials(IEnumerable<Material> materials)
    {
        this.materials = [.. materials];
        originalTex = [.. materials.Select(r => (Texture2D)r.mainTexture)];
        return this;
    }
    private Transform head = null!;
    internal ColorChanger WithHead(Transform head)
    {
        this.head = head;
        return this;
    }
    internal struct EyeColors()
    {
        internal Color? eye = null;
        internal Color? eyeL = null;
        internal Color? eyeR = null;
        internal Color? pupil = null;
        internal Color? pupilL = null;
        internal Color? pupilR = null;
        internal Color? blink = null;
        internal Color? blinkL = null;
        internal Color? blinkR = null;
        internal Color? happyEye = null;
        internal Color? happyEyeL = null;
        internal Color? happyEyeR = null;
        private void TrySet(SpriteRenderer r, Color? c) { if (c != null) r.material.color = (Color)c; }
        internal void SetEye(SpriteRenderer? r, bool isL) { if (r != null) TrySet(r, (isL ? eyeL : eyeR) ?? eye); }
        internal void SetPupil(SpriteRenderer? r, bool isL) { if (r != null) TrySet(r, (isL ? pupilL : pupilR) ?? pupil); }
        internal void SetBlink(SpriteRenderer? r, bool isL)
        {
            if (r != null)
            {
                r.color = new(1, 1, 1, 1);
                TrySet(r, (isL ? blinkL : blinkR) ?? blink);
            }
        }
        internal void SetHappyEye(SpriteRenderer? r, bool isL)
        {
            if (r != null)
            {
                r.color = new(1, 1, 1, 1);
                TrySet(r, (isL ? happyEyeL : happyEyeR) ?? happyEye);
            }
        }
    }
    internal EyeColors eyeColors;
    internal void Apply()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            var tex = Util.EditableTexture(originalTex[i]);
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    var p = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, Change(p));
                }
            }
            tex.Apply();
            materials[i].mainTexture = tex;
        }
        if (head != null)
        {
            eyeColors.SetEye(head.Find("EyeL")?.GetComponent<SpriteRenderer>(), true);
            eyeColors.SetEye(head.Find("EyeR")?.GetComponent<SpriteRenderer>(), false);
            eyeColors.SetPupil(head.Find("EyeL/Pupil")?.GetComponent<SpriteRenderer>(), true);
            eyeColors.SetPupil(head.Find("EyeR/Pupil")?.GetComponent<SpriteRenderer>(), false);
            eyeColors.SetBlink(head.Find("EyeL/Blink")?.GetComponent<SpriteRenderer>(), true);
            eyeColors.SetBlink(head.Find("EyeR/Blink")?.GetComponent<SpriteRenderer>(), false);
            eyeColors.SetHappyEye(head.Find("EyeL/HappyEyes")?.GetComponent<SpriteRenderer>(), true);
            eyeColors.SetHappyEye(head.Find("EyeR/HappyEyes")?.GetComponent<SpriteRenderer>(), false);
        }
    }
    internal static float Distance(Color c1, Color c2) => Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b);
}

internal class ColorMapChanger(IEnumerable<Tuple<Color, Color>> colorMap) : ColorChanger()
{
    internal bool changeNearestColor = true;
    internal float distanceBound = 1e-2f;
    internal bool debugPrinting = true;
    private readonly List<Tuple<Color, Color>> colorMap = [.. colorMap];
    protected override Color Change(Color originalColor)
    {
        if (changeNearestColor)
        {
            float maxDist = float.MaxValue;
            Color ret = default;
            foreach (var c in colorMap)
            {
                var d = Distance(originalColor, c.Item1);
                if (d < distanceBound) return c.Item2;
                else if (d < maxDist)
                {
                    maxDist = d;
                    ret = c.Item2;
                }
            }
            return ret;
        }
        else
        {
            foreach (var c in colorMap)
            {
                if (Distance(originalColor, c.Item1) < distanceBound) return c.Item2;
            }
            return originalColor;
        }
    }
    internal void SetColor(int index, Color newColor)
    {
        if (index < 0 || index >= colorMap.Count) return;
        colorMap[index] = new(colorMap[index].Item1, newColor);
    }

#if DEBUG
    internal void ActivateDebugPatelle(string characterName) => DebugColorPatelle.Create(characterName, this);
    private class DebugColorPatelle : MonoBehaviour
    {
        internal static void Create(string name, ColorMapChanger changer)
        {
            new GameObject($"ccccc_{name}ColorPatelle").AddComponent<DebugColorPatelle>().changer = changer;
        }
        private ColorMapChanger changer = null!;
        private void Set(int index, Color color) { changer.SetColor(index, color); changer.Apply(); }
        private Color Get(int index) => index >= 0 && index < changer.colorMap.Count ? changer.colorMap[index].Item2 : new(0, 0, 0, 0);
        internal Color _c00 { get => Get(0); set => Set(0, value); }
        internal Color _c01 { get => Get(1); set => Set(1, value); }
        internal Color _c02 { get => Get(2); set => Set(2, value); }
        internal Color _c03 { get => Get(3); set => Set(3, value); }
        internal Color _c04 { get => Get(4); set => Set(4, value); }
        internal Color _c05 { get => Get(5); set => Set(5, value); }
        internal Color _c06 { get => Get(6); set => Set(6, value); }
        internal Color _c07 { get => Get(7); set => Set(7, value); }
        internal Color _c08 { get => Get(8); set => Set(8, value); }
        internal Color _c09 { get => Get(9); set => Set(9, value); }
        internal Color _c10 { get => Get(10); set => Set(10, value); }
        internal Color _c11 { get => Get(11); set => Set(11, value); }
        internal Color _c12 { get => Get(12); set => Set(12, value); }
        internal Color _c13 { get => Get(13); set => Set(13, value); }
        internal Color _c14 { get => Get(14); set => Set(14, value); }
        internal Color _c15 { get => Get(15); set => Set(15, value); }
        internal EyeColors _eyeColors { get => changer.eyeColors; set { changer.eyeColors = value; changer.Apply(); } }
    }
    private int debugColorIdx = -1;
    internal void DebugColorSet()
    {
        debugColorIdx = (debugColorIdx + 1) % colorMap.Count;
        for (int i = 0; i < colorMap.Count; i++) SetColor(i, i == debugColorIdx ? new(1, 0, 0, 1) : new(1, 1, 1, 1));
        Apply();
    }
#endif
}

