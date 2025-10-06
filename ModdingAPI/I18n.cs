
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ModdingAPI;

internal class API_I18n : II18n
{
    internal API_I18n(IMod mod)
    {
        LoadI18nFile(mod);
    }
    private Dictionary<string, string> stringTables = [];
    public bool TryToLocalize(string tag, out string s, params IEnumerable<object> args) => TryParse(tag, out s, args);
    public string Localize(string tag, params IEnumerable<object> args) => TryParse(tag, out var s, args) ? s : "";
    private bool TryParse(string tag, out string s, IEnumerable<object> args)
    {
        if (!stringTables.TryGetValue(tag, out s)) return false;
        try
        {
            s = string.Format(s, [.. args]);
            return true;
        }
        catch { return false; }
    }
    private class Language(string name, string code)
    {
        public string Name = name;
        public string Code = code;
        public bool Equals(string tag) => tag == Name || tag == Code;
    }
    private static readonly Dictionary<SystemLanguage, Language> languages = new()
    {
        [SystemLanguage.Afrikaans] = new("afrikaans", "af"),
        [SystemLanguage.Arabic] = new("arabic", "ar"),
        [SystemLanguage.Basque] = new("basque", "eu"),
        [SystemLanguage.Belarusian] = new("belarusian", "be"),
        [SystemLanguage.Bulgarian] = new("bulgarian", "bg"),
        [SystemLanguage.Catalan] = new("catalan", "ca"),
        [SystemLanguage.Chinese] = new("chinese", "zh"),
        [SystemLanguage.Czech] = new("czech", "cs"),
        [SystemLanguage.Danish] = new("danish", "da"),
        [SystemLanguage.Dutch] = new("dutch", "nl"),
        [SystemLanguage.English] = new("english", "en"),
        [SystemLanguage.Estonian] = new("estonian", "et"),
        [SystemLanguage.Faroese] = new("faroese", "fo"),
        [SystemLanguage.Finnish] = new("finnish", "fi"),
        [SystemLanguage.French] = new("french", "fr"),
        [SystemLanguage.German] = new("german", "de"),
        [SystemLanguage.Greek] = new("greek", "el"),
        [SystemLanguage.Hebrew] = new("hebrew", "he"),
        [SystemLanguage.Icelandic] = new("icelandic", "is"),
        [SystemLanguage.Indonesian] = new("indonesian", "id"),
        [SystemLanguage.Italian] = new("italian", "it"),
        [SystemLanguage.Japanese] = new("japanese", "ja"),
        [SystemLanguage.Korean] = new("korean", "ko"),
        [SystemLanguage.Latvian] = new("latvian", "lv"),
        [SystemLanguage.Lithuanian] = new("lithuanian", "lt"),
        [SystemLanguage.Norwegian] = new("norwegian", "no"),
        [SystemLanguage.Polish] = new("polish", "pl"),
        [SystemLanguage.Portuguese] = new("portuguese", "pt"),
        [SystemLanguage.Romanian] = new("romanian", "ro"),
        [SystemLanguage.Russian] = new("russian", "ru"),
        [SystemLanguage.SerboCroatian] = new("serboCroatian", "sh"),
        [SystemLanguage.Slovak] = new("slovak", "sk"),
        [SystemLanguage.Slovenian] = new("slovenian", "sl"),
        [SystemLanguage.Spanish] = new("spanish", "es"),
        [SystemLanguage.Swedish] = new("swedish", "sv"),
        [SystemLanguage.Thai] = new("thai", "th"),
        [SystemLanguage.Turkish] = new("turkish", "tr"),
        [SystemLanguage.Ukrainian] = new("ukrainian", "uk"),
        [SystemLanguage.Vietnamese] = new("vietnamese", "vi"),
        [SystemLanguage.ChineseSimplified] = new("chineseSimplified", "zh-CN"),
        [SystemLanguage.ChineseTraditional] = new("chineseTraditional", "zh-TW"),
        [SystemLanguage.Hungarian] = new("hungarian", "hu"),
    };
    internal static SystemLanguage CurrentLanguage { get; private set; } = SystemLanguage.English;
    internal static void SetLanguage(string lang, ManualLogSource logger)
    {
        CurrentLanguage = StringToLanguage(lang);
        if (CurrentLanguage == SystemLanguage.Unknown)
        {
            logger.LogWarning($"unknown language \"{lang}\"");
        }
    }
    internal static void UpdateLanguage()
    {
        var lang = CurrentSystemLanguage();
        if (lang != SystemLanguage.Unknown) CurrentLanguage = lang;
    }
    private static SystemLanguage CurrentSystemLanguage()
    {
        var lang = PlayerPrefsAdapter.GetString("LANG_");
        if (lang == null) return SystemLanguage.Unknown;
        I18n.Language[] languages = [.. I18n.GetLanguages()];
        var idx = languages.IndexOf(l => l.saveName == lang);
        if (idx < 0) return SystemLanguage.Unknown;
        return languages[idx].systemLanguage;
    }
    private static SystemLanguage StringToLanguage(string lang)
    {
        var l = lang.ToLower();
        if (l == "system") return SystemLanguage.English;
        foreach (var language in languages)
        {
            if (language.Value.Equals(l)) return language.Key;
        }
        return SystemLanguage.Unknown;
    }
    private void LoadI18nFile(IMod mod)
    {
        var file = GetTargetFile(mod, out var usedDefaultFile, out var defaultFile);
        if (file == null) return;
        try
        {
            stringTables = ReadJsonFile(file);
            if (!usedDefaultFile)
            {
                CompleteTable(defaultFile);
            }
        }
        catch (Exception e)
        {
            Monitor.SLog($"failed to load i18n file {Path.GetFileName(file)} of mod {mod.UniqueID}:\n\t{e}");
            stringTables = [];
        }
    }
    private string? GetTargetFile(IMod mod, out bool usedDefaultFile, out string? defaultFile)
    {
        var path = Path.Combine(mod.HomePath, "i18n");
        usedDefaultFile = false;
        defaultFile = null;
        if (!Directory.Exists(path))
        {
            Monitor.SLogBepIn($"mod {mod.UniqueID} does not have folder \"i18n\"", LogLevel.Info);
            return null;
        }
        List<string> defaultFiles = [
            Path.Combine(path, "default.jsonc"),
            Path.Combine(path, "default.json"),
            Path.Combine(path, "en.jsonc"),
            Path.Combine(path, "en.json"),
        ];
        defaultFile = defaultFiles.Find(File.Exists);
        if (CurrentLanguage == SystemLanguage.Unknown) return null;
        var code = languages[CurrentLanguage].Code;
        List<string> files = [
            Path.Combine(path, $"{code}.jsonc"),
            Path.Combine(path, $"{code}.json")
        ];
        var file = files.Find(File.Exists);
        if (file != null) return file;
        usedDefaultFile = true;
        return defaultFile;
    }
    private void CompleteTable(string? defaultFile)
    {
        if (defaultFile == null) return;
        try
        {
            var tables = ReadJsonFile(defaultFile);
            foreach (var pair in tables)
            {
                stringTables.TryAdd(pair.Key, pair.Value);
            }
        }
        catch (Exception e)
        {
            Monitor.SLog($"failed to load default i18n file {Path.GetFileName(defaultFile)} :\n\t{e}");
        }
    }
    private Dictionary<string, string> ReadJsonFile(string file)
    {
        var contents = File.ReadAllText(file);
        return JObject
            .Parse(contents, new() { CommentHandling = CommentHandling.Ignore })
            .ToObject<Dictionary<string, string>>();
    }
}


