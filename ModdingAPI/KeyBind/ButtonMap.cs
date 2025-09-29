
using System.Text.RegularExpressions;
using InControl;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ModdingAPI.KeyBind;
public static class ButtonMap
{
    public static IReadOnlyDictionary<string, Key> Map { get => map; set { map = new(value); } }
    private static Dictionary<string, Key> map = [];

    public static readonly IReadOnlyCollection<Key> DefaultViewButtons =
        [new(InputControlType.View)
    ];
    public static IReadOnlyCollection<Key> ViewButtons { get => viewButtons; set { viewButtons = [.. value]; } }
    internal static HashSet<Key> viewButtons = [.. DefaultViewButtons];
    private static readonly List<string> allowedFileNames = [
        "buttons.jsonc",
        "buttons.json",
        "button_aliases.jsonc",
        "button_aliases.json"
    ];

    private static bool FindFile(out string path)
    {
        List<string> directories = [ModLoader.ModsPath, ModdingApiMod.instance.HomePath];
        foreach (string dir in directories)
        {
            foreach (string file in allowedFileNames)
            {
                var p = Path.Combine(dir, file);
                if (File.Exists(p))
                {
                    path = p;
                    return true;
                }
            }
        }
        path = null!;
        return false;
    }
    internal static void Setup()
    {
        LoadButtonMapsData();
    }
    private static void LoadButtonMapsData()
    {
        if (!FindFile(out var file)) return;
        Dictionary<string, string> table;
        try
        {
            var contents = File.ReadAllText(file);
            table = JObject
                .Parse(contents, new() { CommentHandling = CommentHandling.Ignore })
                .ToObject<Dictionary<string, string>>();
        }
        catch { return; }
        TrySetMap(table);
    }
    internal static void TrySetMap(Dictionary<string, string> _map)
    {
        Dictionary<string, Key> newMap = [];
        foreach (var pair in _map)
        {
            var key = pair.Key;
            if (key.StartsWith(".")) key = key[1..];
            var value = pair.Value;
            if (!Regex.Match(key, @"^[a-zA-Z0-9]+$").Success) continue;
            if (TryParse(value, out var k))
            {
                Monitor.SLogBepIn($"successfully registered custom key: \"{key}\" -> \"{k}\"", LogLevel.Debug);
                newMap[key] = k;
            }
        }
        Map = newMap;
    }
    private static bool TryParse(string val, out Key key)
    {
        key = null!;
        val = val.Trim();
        if (val.StartsWith(".")) return false;
        if (val.StartsWith("KeyCode."))
        {
            if (Enum.TryParse<KeyCode>(val[8..], true, out var keyCode))
            {
                key = new(keyCode);
                return true;
            }
            else return false;
        }
        else if (val.StartsWith("Trigger."))
        {
            if (Enum.TryParse<Trigger>(val[8..], true, out var trigger))
            {
                key = new(trigger);
                return true;
            }
            else return false;
        }
        List<string> incontrolTypePrefixes = ["InputControlType", "InControl"];
        foreach (var prefix in incontrolTypePrefixes)
        {
            if (val.StartsWith(prefix))
            {
                val = val[prefix.Length..];
                if (Enum.TryParse<InputControlType>(val, true, out var inControlKey))
                {
                    key = new(inControlKey);
                    return true;
                }
                else return false;
            }
        }
        return Key.TryParse(val, out key);
    }

    internal static void StartDebugKeyWatcher()
    {
        if (!Context.OnTitle)
        {
            Monitor.SLog($"debug key watching is available only on title screen", LogLevel.Warning);
            return;
        }
        MainMenu.ShowDialog("key watching...\npress Escape to enter", useEnglishFont: true);
        var obj = new GameObject("DebugGamePadKeyWatching");
        var watcher = obj.AddComponent<IncontrolKeyWatcher>();
        InputInterceptor.EnableAll(new(
            onKeydown: info =>
            {
                watcher.Add($"  pressed KeyCode.{info.code}");
            }
        ), onDisabled: () =>
        {
            GameObject.Destroy(obj);
            MainMenu.CloseDialog();
        });
    }
    private class IncontrolKeyWatcher : MonoBehaviour
    {
        private readonly List<string?> pressedKeys = [];
        private int countdown = 2;
        public void Add(string mes)
        {
            pressedKeys.Add(mes);
            countdown = 2;
        }
        internal void Update()
        {
            foreach (InputControlType code in Enum.GetValues(typeof(InputControlType)))
            {
                var control = InputManager.ActiveDevice.GetControl(code);
                if (control.WasPressed)
                {
                    pressedKeys.Add($"  pressed InputControlType.{code}");
                    countdown = 2;
                }
            }
            if (countdown > 0) countdown--;
            if (countdown != 0) return;
            countdown = -1;
            if (pressedKeys.Any())
            {
                Monitor.SLog($"\n{string.Join($"\n", pressedKeys)}\n");
                pressedKeys.Clear();
            }
        }
    }
}

