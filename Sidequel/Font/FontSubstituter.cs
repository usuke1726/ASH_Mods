
using HarmonyLib;
using ModdingAPI;
using TMPro;
using UnityEngine;

namespace Sidequel.Font;

internal static class FontSubstituter
{
    internal class CharacterData(char ch, string data)
    {
        internal readonly char ch = ch;
        internal readonly string data = data;
        internal static Dictionary<char, CharacterData> Load(II18n i18n)
        {
            Dictionary<char, CharacterData> result = [];
            int i = 0;
            while (true)
            {
                var data = i18n.Localize($"font{i}");
                if (string.IsNullOrEmpty(data)) return result;
                try
                {
                    Parse(data, out var target, out var chData);
                    result[target] = chData;
                }
                catch (Exception e)
                {
                    Debug($"FONT PARSING ERROR on font{i}: {e.Message}", LL.Error);
                }
                i++;
            }
        }
        private static void Parse(string data, out char target, out CharacterData value)
        {
            value = null!;
            target = default;
            var d1 = data.Split(":", 2);
            if (d1.Length != 2) throw new("missing colon");
            var meta = d1[0];
            var rowdata = d1[1];
            var d2 = meta.Split(",", 4);
            if (d2.Length != 4) throw new("meta data has not 4 values");
            var _ch = d2[0];
            if (_ch.Length != 1) throw new($"ch has not Length 1 (ch: \"{_ch}\")");
            char ch = _ch[0];
            var _oldCh = d2[1];
            if (_oldCh.Length != 1) throw new($"oldCh has not Length 1 (oldCh: \"{_ch}\")");
            target = _oldCh[0];
            if (!int.TryParse(d2[2], out var height)) throw new($"third value is not integer (value: {d2[2]})");
            if (!int.TryParse(d2[3], out var width)) throw new($"fourth value is not integer (value: {d2[3]})");
            if (rowdata.Length != height * width) throw new($"data size is inavlid (expected: {height} * {width} = {height * width}, actual: {rowdata.Length})");
            value = new(
                ch,
                string.Join("\n", Enumerable.Range(0, height).Select(i => rowdata[(i * width)..((i + 1) * width)]))
            );
        }
    }

    private static readonly Dictionary<SystemLanguage, Texture2D> originalTextures = [];
    private static readonly Dictionary<SystemLanguage, Dictionary<char, bool[][]>> fontMapCache = [];
    private static readonly List<Tuple<char, char>> replaceMap = [];
    private static TMP_FontAsset fontAsset = null!;
    private static Texture2D texture = null!;
    private static SystemLanguage currentLanguage = SystemLanguage.English;
    private static bool wasSidequelActive = false;
    internal static void Setup(IMod mod)
    {
        var helper = mod.Helper;
        helper.Events.System.LocaleChanged += (_, e) =>
        {
            Reset(e.OldLanguage);
            if ((currentLanguage = e.NewLanguage) == SystemLanguage.English) return;
            SetFontAsset();
            if (!fontMapCache.TryGetValue(e.NewLanguage, out fontMaps))
            {
                originalTextures.TryAdd(e.NewLanguage, fontAsset.atlasTexture);
                SetFontMaps(e.NewLanguage, CharacterData.Load(mod.I18n_));
            }
            if (State.IsActive) Apply();
        };
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            if (!State.IsActive) return;
            wasSidequelActive = true;
            Apply();
        };
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            if (wasSidequelActive) Reset(currentLanguage);
            wasSidequelActive = false;
        };
    }

    private static void SetFontAsset()
    {
        var obj = Singleton<GameServiceLocator>.instance.ui.simpleDialoguePrefab.Clone();
        var translator = obj.GetComponentInChildren<TextTranslator>();
        if (translator == null) return;
        var text = Traverse.Create(translator).Field("tmpText").GetValue<TMP_Text>();
        if (text == null) return;
        fontAsset = text.font;
        texture = Util.EditableTexture(fontAsset.atlasTexture);
        GameObject.Destroy(obj);
    }
    internal static string Replace(string s)
    {
        foreach (var item in replaceMap)
        {
#if DEBUG
            Assert(!s.Contains(item.Item2), $"Using character which shouldn't be used!!!: {item.Item2}");
#endif
            s = s.Replace(item.Item1, item.Item2);
        }
        return s;
    }
    private static void Apply()
    {
        Assert(fontAsset != null, "fontAsset is null!!");
        if (fontAsset == null) return;
        Assert(texture != null, "texture is null!!");
        if (texture == null) return;
        foreach (var map in fontMaps)
        {
            var oldChar = map.Key;
            var tex = map.Value;
            if (!fontAsset.characterLookupTable.TryGetValue(oldChar, out var ch))
            {
                Debug($"The oldCh '{oldChar}' is not contained in characterTable!!", LL.Error);
                continue;
            }
            Debug($"replacing {oldChar} -> {replaceMap.Find(m => m.Item2 == oldChar)?.Item1}");
            var rect = ch.glyph.glyphRect;
            var width = Math.Min(rect.width, tex[0].Length);
            var height = Math.Min(rect.height, tex.Length);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color p = tex[y][x] ? new(0, 0, 0, 1) : new(0, 0, 0, 0);
                    texture.SetPixel(x + rect.x, y + rect.y, p);
                }
            }
        }
        texture.Apply();
        SetTexture(texture);
    }
    private static void SetTexture(Texture2D tex)
    {
        if (fontAsset == null) return;
        fontAsset.atlasTextures[0] = tex;
        Traverse.Create(fontAsset).Field("m_AtlasTexture").SetValue(tex);
        fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, tex);
    }
    private static void Reset(SystemLanguage language)
    {
        if (originalTextures.TryGetValue(language, out var tex)) SetTexture(tex);
    }

    private static Dictionary<char, bool[][]> fontMaps = [];
    private static void SetFontMaps(SystemLanguage language, Dictionary<char, CharacterData> rowMaps)
    {
        fontMaps = [];
        replaceMap.Clear();
        foreach (var pair in rowMaps)
        {
            var oldChar = pair.Key;
            fontMaps[oldChar] = Parse(pair.Value.data);
            replaceMap.Add(new(pair.Value.ch, oldChar));
        }
        fontMapCache[language] = fontMaps;
    }
    private static bool[][] Parse(string data)
    {
        HashSet<char> trueChars = ['1', '#'];
        var lines = data.Trim().Split("\n").Select(s => s.Trim()).Where(s => s.Length > 0).Reverse().ToList();
        var width = lines.Select(l => l.Length).Min();
        var height = lines.Count;
        return [.. lines.Select(line => Enumerable.Range(0, width).Select(i => trueChars.Contains(line[i])).ToArray())];
    }
    internal static void Debug_PrintInfo(char character, bool printData = true)
    {
        if (fontAsset == null)
        {
            Debug($"fontAsset is null", LL.Error);
            return;
        }
        var list = fontAsset.characterTable;
        for (int i = 0; i < list.Count; i++)
        {
            var ch = list[i];
            if (Convert.ToChar(ch.unicode) != character) continue;
            var rect = ch.glyph.glyphRect;
            var height = rect.height;
            var width = rect.width;
            Debug($"== FOUND character\n\tcharacter = {character}\n\tindex = {i}\n\t(height, width) = ({height}, {width})", LL.Warning);
            if (printData)
            {
                List<string> body = [];
                for (int y = 0; y < height; y++)
                {
                    string s = "";
                    for (int x = 0; x < width; x++)
                    {
                        var f = texture.GetPixel(x + rect.x, y + rect.y).a > 0.5f;
                        s += f ? "#" : ".";
                    }
                    body.Add(s);
                }
                body.Reverse();
                Debug($"texture:\n\n{string.Join($"\n", body)}\n\n{new string('=', 20)}\n\n", LL.Warning);
            }
            return;
        }
    }
    internal static void Debug_PrintInfo(uint unicode, bool printData = true) => Debug_PrintInfo(Convert.ToChar(unicode), printData);
}

