
using ModdingAPI;
using UnityEngine;

namespace Sidequel.Item;

internal class ExtendedItem : ItemWrapperBase
{
    public CollectableItem.PickUpPrompt showPrompt = CollectableItem.PickUpPrompt.Always;
    public bool cannotDrop = false;
    public bool cannotStash = false;
    public int priority = 0;
    public Func<GameObject>? createWorldPrefab = null;

    private readonly bool[,] iconData;
    private bool ready = false;
    private static readonly int xOffset = 27;
    private static readonly int yOffset = 53;
    public ExtendedItem(string id, bool[,] iconData, Func<int?>? getState = null) : base(id, getState)
    {
        if (iconData.GetLength(0) != 12 || iconData.GetLength(1) != 12) throw new Exception("iconData must be 12x12 matrix");
        this.iconData = iconData;
    }
    public ExtendedItem(string id, int[,] iconData, Func<int?>? getState = null) : this(id, ToIconData(iconData), getState) { }
    public ExtendedItem(string id, string iconData, Func<int?>? getState = null) : this(id, ToIconData(iconData), getState) { }

    private static bool[,] ToIconData(int[,] iconData)
    {
        if (iconData.GetLength(0) != 12 || iconData.GetLength(1) != 12) throw new Exception("iconData must be 12x12 matrix");
        var ret = new bool[12, 12];
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                ret[i, j] = iconData[i, j] != 0;
            }
        }
        return ret;
    }
    private static bool[,] ToIconData(string iconData)
    {
        static bool IsTruthy(char c) => c != '0' && c != '.';
        var lines = iconData.Replace("\t", "").Replace(" ", "").Split("\n").Where(s => s.Trim().Length > 0).ToArray();
        var ret = new bool[12, 12];
        for (int i = 0; i < 12; i++)
        {
            var line = i < lines.Length ? lines[i] : "";
            for (int j = 0; j < 12; j++)
            {
                var c = j < line.Length ? line[j] : '0';
                ret[i, j] = IsTruthy(c);
            }
        }
        return ret;
    }
    internal override void OnGameStarted()
    {
        if (createWorldPrefab != null) item.worldPrefab = createWorldPrefab();
    }
    internal override void EnsureIconCreated(Sprite resource)
    {
        if (ready) return;
        item.showPrompt = showPrompt;
        item.cannotDrop = cannotDrop;
        item.cannotStash = cannotStash;
        item.priority = priority;
        var tex = Util.EditableTexture(resource.texture);
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                tex.SetPixel(x + xOffset, y + yOffset, iconData[11 - y, x] ? new(1, 1, 1, 1) : new(1, 1, 1, 0));
            }
        }
        tex.Apply();
        item.icon = Sprite.Create(tex, resource.rect, new(0.5f, 0.5f), resource.pixelsPerUnit);
        ready = true;
    }
}

