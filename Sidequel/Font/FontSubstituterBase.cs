
using System.Reflection;
using HarmonyLib;
using ModdingAPI;
using TMPro;
using UnityEngine;

namespace Sidequel.Font;

internal abstract class FontSubstituterBase
{
    internal class CharacterData(char ch, string data)
    {
        internal readonly char ch = ch;
        internal readonly string data = data;
    }
    protected virtual bool DoNotAppend => false;
    protected virtual void DebugOnReady() { }
    protected abstract SystemLanguage Language { get; }
    protected abstract Dictionary<int, CharacterData> RaplacingCharacters { get; }
#if DEBUG
    private static List<FontSubstituterBase> debugOnReadyHandlers = [];
#endif
    protected FontSubstituterBase()
    {
#if DEBUG
        if (!DoNotAppend) handlers[Language] = this;
        debugOnReadyHandlers.Add(this);
#else
        handlers[Language] = this;
#endif
    }

    private static readonly Dictionary<SystemLanguage, FontSubstituterBase> handlers = [];
    private static readonly Dictionary<SystemLanguage, Texture2D> originalTextures = [];
    private static readonly Dictionary<SystemLanguage, Dictionary<int, bool[][]>> fontMapCache = [];
    private static readonly List<Tuple<char, char>> replaceMap = [];
    private static TMP_FontAsset fontAsset = null!;
    private static Texture2D texture = null!;
    private static SystemLanguage currentLanguage = SystemLanguage.English;
    private static bool wasSidequelActive = false;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.System.LocaleChanged += (_, e) =>
        {
            Reset(e.OldLanguage);
            if ((currentLanguage = e.NewLanguage) == SystemLanguage.English) return;
            if (handlers.TryGetValue(e.NewLanguage, out var handler))
            {
                SetFontAsset();
                if (!fontMapCache.TryGetValue(e.NewLanguage, out fontMaps))
                {
                    originalTextures.TryAdd(e.NewLanguage, fontAsset.atlasTexture);
                    SetFontMaps(e.NewLanguage, handler.RaplacingCharacters);
                }
                if (State.IsActive) Apply();
            }
#if DEBUG
            var h = debugOnReadyHandlers.Find(h => h.Language == e.NewLanguage);
            if (h != null)
            {
                SetFontAsset();
                h.DebugOnReady();
            }
#endif
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
        var asm = Assembly.GetExecutingAssembly();
        var types = asm.DefinedTypes.Where(type => typeof(FontSubstituterBase).IsAssignableFrom(type) && !type.IsAbstract);
        foreach (var type in types)
        {
            var constructor = type.GetConstructor([]);
            constructor?.Invoke([]);
        }
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
            var idx = map.Key;
            var tex = map.Value;
            if (idx < 0 || fontAsset.characterTable.Count <= idx) continue;
            var ch = fontAsset.characterTable[idx];
            Debug($"replacing {Convert.ToChar(ch.unicode)}");
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

    private static Dictionary<int, bool[][]> fontMaps = [];
    private static void SetFontMaps(SystemLanguage language, Dictionary<int, CharacterData> rowMaps)
    {
        fontMaps = [];
        var table = fontAsset.characterTable;
        replaceMap.Clear();
        foreach (var pair in rowMaps)
        {
            var idx = pair.Key;
            if (idx < 0 || table.Count <= idx) continue;
            fontMaps[idx] = Parse(pair.Value.data);
            replaceMap.Add(new(pair.Value.ch, Convert.ToChar(table[idx].unicode)));
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
    public sealed override bool Equals(object obj) => base.Equals(obj);
    public sealed override string ToString() => base.ToString();
    public sealed override int GetHashCode() => base.GetHashCode();
}

