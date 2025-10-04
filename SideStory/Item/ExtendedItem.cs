
using ModdingAPI;
using UnityEngine;

namespace SideStory.Item;

internal class ExtendedItem : CollectableItem
{
    public class I18nKeys(string readableName, string readableNamePlural, string description)
    {
        public readonly string ReadableName = readableName;
        public readonly string ReadableNamePlural = readableNamePlural;
        public readonly string Description = description;
    }
    private readonly I18nKeys i18nKeys;
    private readonly bool[,] iconData;
    private bool ready = false;
    private static readonly int xOffset = 27;
    private static readonly int yOffset = 53;
    internal readonly string id;
    public ExtendedItem(string id, I18nKeys i18nKeys, bool[,] iconData)
    {
        DataHandler.ValidateItemId(id);
        if (iconData.GetLength(0) != 12 || iconData.GetLength(1) != 12) throw new Exception("iconData must be 12x12 matrix");
        this.id = id;
        name = id;
        this.i18nKeys = i18nKeys;
        this.iconData = iconData;
        OnLocaleChanged();
        DataHandler.Register(this);
    }
    public ExtendedItem(string id, I18nKeys i18nKeys, int[,] iconData) : this(id, i18nKeys, ToIconData(iconData)) { }
    public ExtendedItem(string id, I18nKeys i18nKeys, string iconData) : this(id, i18nKeys, ToIconData(iconData)) { }

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
        var lines = iconData.Trim().Split("\n");
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
    internal void OnLocaleChanged()
    {
        readableName = I18n_.Localize(i18nKeys.ReadableName);
        readableNamePlural = I18n_.Localize(i18nKeys.ReadableNamePlural);
        description = I18n_.Localize(i18nKeys.Description);
    }
    internal void EnsureIconCreated(Sprite resource)
    {
        if (ready) return;
        var tex = Util.EditableTexture(resource.texture);
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                tex.SetPixel(x + xOffset, y + yOffset, iconData[11 - y, x] ? new(1, 1, 1, 1) : new(1, 1, 1, 0));
            }
        }
        tex.Apply();
        icon = Sprite.Create(tex, resource.rect, new(0.5f, 0.5f), resource.pixelsPerUnit);
        ready = true;
    }
}

