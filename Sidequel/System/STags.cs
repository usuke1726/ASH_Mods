
using System.Text.RegularExpressions;
using ModdingAPI;

namespace Sidequel.System;

internal static class STags
{
    private static readonly Dictionary<string, int> intValues = [];
    private static readonly Dictionary<string, float> floatValues = [];
    private static readonly Dictionary<string, string> stringValues = [];
    private static readonly Dictionary<string, bool> boolValues = [];
    internal static event Action? BeforeSaving = null;
    internal static void Setup(IModHelper helper)
    {
        helper.Events.Gameloop.GameStarted += (_, _) => EnsureDataLoaded();
        helper.Events.System.BeforeSaving += (_, _) => SaveHandler.WriteToSaveData();
        helper.Events.Gameloop.ReturnedToTitle += (_, _) =>
        {
            SaveHandler.hasLoaded = false;
        };
    }
    internal static void EnsureDataLoaded() => SaveHandler.EnsureDataLoaded();
    private static string FormatId(string id)
    {
#if DEBUG
        if (Regex.IsMatch(id, @"[^a-zA-Z0-9 ._-]"))
        {
            var s = id.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
            throw new Exception($"invalid tag id \"{s}\"");
        }
#endif
        SaveHandler.EnsureDataLoaded();
        return id;
    }
    public static void Set(string id, object value)
    {
        if (value is int @int) SetInt(id, @int);
        else if (value is float @float) SetFloat(id, @float);
        else if (value is string @string) SetString(id, @string);
        else if (value is bool @bool) SetBool(id, @bool);
    }
    public static void SetInt(string id, int value) => intValues[FormatId(id)] = value;
    public static void TrySetInt(string id, int value) => intValues.TryAdd(FormatId(id), value);
    public static void SetFloat(string id, float value) => floatValues[FormatId(id)] = value;
    public static void TrySetFloat(string id, float value) => floatValues.TryAdd(FormatId(id), value);
    public static void SetString(string id, string value) => stringValues[FormatId(id)] = value;
    public static void TrySetString(string id, string value) => stringValues.TryAdd(FormatId(id), value);
    public static void SetBool(string id, bool value = true) => boolValues[FormatId(id)] = value;
    public static void TrySetBool(string id, bool value) => boolValues.TryAdd(FormatId(id), value);
    public static void AddInt(string id, int value) => SetInt(id, GetInt(id, 0) + value);
    public static void AddFloat(string id, float value) => SetFloat(id, GetFloat(id, 0) + value);
    public static void ToggleBool(string id) => SetBool(id, !GetBool(id, false));
    public static bool TryGetInt(string id, out int value) => intValues.TryGetValue(FormatId(id), out value);
    public static bool TryGetFloat(string id, out float value) => floatValues.TryGetValue(FormatId(id), out value);
    public static bool TryGetString(string id, out string value) => stringValues.TryGetValue(FormatId(id), out value);
    public static bool TryGetBool(string id, out bool value) => boolValues.TryGetValue(FormatId(id), out value);
    public static int GetInt(string id, int defaultValue = 0) => TryGetInt(id, out var v) ? v : defaultValue;
    public static float GetFloat(string id, float defaultValue = 0) => TryGetFloat(id, out var v) ? v : defaultValue;
    public static string GetString(string id, string? defaultValue = null) => TryGetString(id, out var v) ? v : defaultValue!;
    public static bool GetBool(string id, bool defaultValue = false) => TryGetBool(id, out var v) ? v : defaultValue;


    private static class SaveHandler
    {
        internal static bool hasLoaded = false;
        internal static void WriteToSaveData()
        {
            BeforeSaving?.Invoke();
            SaveIntValues();
            SaveFloatValues();
            SaveStringValues();
            SaveBoolValues();
        }
        internal static void EnsureDataLoaded()
        {
            if (hasLoaded) return;
            intValues.Clear();
            floatValues.Clear();
            stringValues.Clear();
            boolValues.Clear();
            if (!State.IsNewGame) LoadFromSaveData();
            hasLoaded = true;
        }
        private static void LoadFromSaveData()
        {
            LoadIntValues();
            LoadFloatValues();
            LoadStringValues();
            LoadBoolValues();
        }
        private static void LoadIntValues()
        {
            var data = Context.globalData.gameData.tags.GetString(Const.BuiltinGameData.STagsIntDataTag);
            if (data == null) return;
            Debug($"== load int:\n{data}");
            foreach (var item in Split(data))
            {
                if (int.TryParse(item.Item2, out var val)) intValues[item.Item1] = val;
            }
        }
        private static void LoadFloatValues()
        {
            var data = Context.globalData.gameData.tags.GetString(Const.BuiltinGameData.STagsFloatDataTag);
            if (data == null) return;
            Debug($"== load float:\n{data}");
            foreach (var item in Split(data))
            {
                if (float.TryParse(item.Item2, out var val)) floatValues[item.Item1] = val;
            }
        }
        private static void LoadStringValues()
        {
            var data = Context.globalData.gameData.tags.GetString(Const.BuiltinGameData.STagsStringDataTag);
            if (data == null) return;
            Debug($"== load string:\n{data}");
            foreach (var item in Split(data))
            {
                stringValues[item.Item1] = DeserializeStringValue(item.Item2);
            }
        }
        private static void LoadBoolValues()
        {
            var data = Context.globalData.gameData.tags.GetString(Const.BuiltinGameData.STagsBoolDataTag);
            if (data == null) return;
            Debug($"== load bool:\n{data}");
            foreach (var item in Split(data))
            {
                boolValues[item.Item1] = DeserializeBoolValue(item.Item2);
            }
        }

        private static void SaveIntValues()
        {
            var data = Join(intValues.Select(pair => new Tuple<string, string>(pair.Key, pair.Value.ToString())));
            Debug($"== save int:\n{data}");
            Context.globalData.gameData.tags.SetString(Const.BuiltinGameData.STagsIntDataTag, data);
        }
        private static void SaveFloatValues()
        {
            var data = Join(floatValues.Select(pair => new Tuple<string, string>(pair.Key, pair.Value.ToString())));
            Debug($"== save float:\n{data}");
            Context.globalData.gameData.tags.SetString(Const.BuiltinGameData.STagsFloatDataTag, data);
        }
        private static void SaveStringValues()
        {
            var data = Join(stringValues.Select(pair => new Tuple<string, string>(pair.Key, SerializeStringValue(pair.Value))));
            Debug($"== save string:\n{data}");
            Context.globalData.gameData.tags.SetString(Const.BuiltinGameData.STagsStringDataTag, data);
        }
        private static void SaveBoolValues()
        {
            var data = Join(boolValues.Select(pair => new Tuple<string, string>(pair.Key, SerializeBoolValue(pair.Value))));
            Debug($"== save bool:\n{data}");
            Context.globalData.gameData.tags.SetString(Const.BuiltinGameData.STagsBoolDataTag, data);
        }

        private static string SerializeStringValue(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\n", "\\n");
        }
        private static string DeserializeStringValue(string value)
        {
            return value.Replace("\\n", "\n").Replace("\\\\", "\\");
        }
        private static string SerializeBoolValue(bool value) => value ? "1" : "0";
        private static bool DeserializeBoolValue(string value) => value == "1";
        private static IEnumerable<Tuple<string, string>> Split(string data)
        {
            return data.Split("\n")
                .Select(line =>
                {
                    var a = line.Split(":", 2);
                    if (a.Length < 2) return null!;
                    return new Tuple<string, string>(a[0], a[1]);
                }).Where(a => a != null);
        }
        private static string Join(IEnumerable<Tuple<string, string>> data)
        {
            return string.Join("\n", data.Select(d => $"{d.Item1}:{d.Item2}"));
        }
    }
}

