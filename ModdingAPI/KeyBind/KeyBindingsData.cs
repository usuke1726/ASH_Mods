
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ModdingAPI.KeyBind;

public interface IKeyBindingsData
{
    void SetDefault(IReadOnlyDictionary<string, string> keybinds);
    bool TryGetValue(string keyId, out string keybind, bool allowDefault = false);
    IEnumerable<string> GetKeyBinds(IEnumerable<string> ids, bool allowDefault = false);
}

internal class KeyBindingsData : IKeyBindingsData
{
    private static readonly string defaultFileName = "keybindings.jsonc";
    private static readonly List<string> allowedFileNames = [defaultFileName, "keybindings.json"];
    private static Dictionary<string, Dictionary<string, string>>? data = null;
    private static string filePath = null!;
    private static List<string> lines = [];
    private static bool usingCRLF;
    private static int firstParameterLine;
    private static readonly string indent = new(' ', 4);
    private static readonly List<string> comments = [.. """
        Syntax: repeat the following syntax with spaces in between

            <key>(holdingKey1+holdingKey2+...)[frame window]

            The frame window is optional. Default is 30 (about 0.5 second).

        Key types:
            :<str>    use key config in game
                :b1     Jump button
                :b2     Cancel/UseItem button
                :b3     Run button
                :b4     Menu button
                :menu   Menu button
                :lb/:rb     Left/Right Bumper
                :lt/:rt     Left/Right Trigger
            .<str>    use user-specified key string defined at button_aliases.json or "button_aliases" property in this file
                The file button_aliases.json must be on ModsPath or ModdingAPI folder.
                The property "button_aliases" in this file overloads the settings of button_aliases.json.
                The key string must contain only alphanumeric characters.
                This type of key is case-sensitive.
            @<str>    use member of enum InControl.InputControlType
            <str>     use member of enum UnityEngine.KeyCode

        Examples:
            ":b1(.RB)"  press jump button holding the key registered as "RB"
            "S(LeftControl+LeftAlt)"  press key S holding left control and left alt keys
            "F12 F12[10] Alpha0"  press F12 key 2 times quickly and then press "0" key on the top of the alphanumeric keyboard

        Schema:
            {
                "type": "object",
                "patternProperties": {
                    "^button_aliases$": {
                        "type": "object",
                        "additionalProperties": false,
                        "patternProperties": {"^\\.?[a-zA-Z0-9]+$": {"type": "string"}}
                    },
                    ".": {
                        "type": "object",
                        "patternProperties": {".": {"type": "string"}}
                    }
                }
            }
        """.Split("\n")];
    private static readonly string initialContents = """
        {
        }
        """;
    private static readonly JsonLoadSettings loadSettings = new()
    {
        CommentHandling = CommentHandling.Ignore
    };
    private static bool FindFile(out string path)
    {
        var modsPath = ModLoader.ModsPath;
        var files = allowedFileNames.Select(p => Path.Combine(modsPath, p)).ToList();
        path = files.Find(p => File.Exists(p));
        var found = path != null;
        path ??= Path.Combine(modsPath, defaultFileName);
        return found;
    }
    internal static void ReadData()
    {
        if (!FindFile(out filePath))
        {
            WriteInitialContents();
        }
        try
        {
            var contents = File.ReadAllText(filePath);
            usingCRLF = contents.Contains('\r');
            var obj = JObject.Parse(contents, loadSettings);
            lines = [.. contents.Replace("\r", "").Split('\n')];
            data = obj.ToObject<Dictionary<string, Dictionary<string, string>>>();
            string logMessage;
            if (data.Any())
            {
                firstParameterLine = (obj.First as IJsonLineInfo).LineNumber;
                logMessage = $"successfully loaded keybindings file {filePath}";
                if (data.TryGetValue("button_aliases", out var buttonAliases))
                {
                    ButtonMap.TrySetMap(buttonAliases);
                }
            }
            else
            {
                logMessage = $"successfully loaded keybindings file {filePath} but data is empty";
            }
            Monitor.SLogBepIn(logMessage, LogLevel.Debug);
        }
        catch (Exception e)
        {
            var mes = I18n_.Localize("KeyBindingsData.Error.FailedToLoad", Path.GetFileName(filePath), e.Message);
            Monitor.SLog(mes, LogLevel.Error);
        }
    }
    private static bool UseComment() => filePath.ToLower().EndsWith(".jsonc");
    private static void WriteInitialContents()
    {
        lines = [.. initialContents.Split("\n")];
        firstParameterLine = 2;
        if (UseComment())
        {
            lines.InsertRange(0, comments.Select(s => $"// {s}"));
            firstParameterLine += comments.Count;
        }
        usingCRLF = false;
        Write();
    }
    private static string Escape(string s)
    {
        return s
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\f", "\\f")
            .Replace("\b", "\\b")
            .Replace("\\", "\\\\");
    }
    private static void InsertNewObject(string id, IReadOnlyDictionary<string, string> keybindings)
    {
        if (data == null) return;
        var isEmpty = !data.Any();
        if (isEmpty) WriteInitialContents();
        data[id] = new(keybindings);
        var newValues = string.Join(",\n",
            keybindings.Select(pair => $"{indent}{indent}\"{Escape(pair.Key)}\": \"{Escape(pair.Value)}\"")
        ).Split("\n");
        List<string> newLines = [
            $"{indent}\"{Escape(id)}\": {{",
            .. newValues,
            $"{indent}}}{(isEmpty ? "" : ",")}"
        ];
        lines.InsertRange(firstParameterLine - 1, newLines);
        Write();
    }
    private static void Write()
    {
        var contents = string.Join(usingCRLF ? "\r\n" : "\n", lines);
        File.WriteAllText(filePath, contents);
    }

    private readonly string uniqueID;
    private Dictionary<string, string> defaultKeybinds = [];
    internal KeyBindingsData(string uniqueID)
    {
        this.uniqueID = uniqueID;
    }
    public void SetDefault(IReadOnlyDictionary<string, string> keybinds)
    {
        defaultKeybinds = new(keybinds);
        if (data == null) return;
        if (!data.ContainsKey(uniqueID))
        {
            InsertNewObject(uniqueID, defaultKeybinds);
        }
    }
    public bool TryGetValue(string keyId, out string keybind, bool allowDefault = false)
    {
        Monitor.SLog($"TryGetValue id {uniqueID}");
        keybind = null!;
        if (data == null) return false;
        if (data.TryGetValue(uniqueID, out var dict) && dict.TryGetValue(keyId, out keybind)) return true;
        if (allowDefault) return defaultKeybinds.TryGetValue(keyId, out keybind);
        else return false;
    }
    public IEnumerable<string> GetKeyBinds(IEnumerable<string> ids, bool allowDefault = false)
    {
        return ids
            .Select(id => TryGetValue(id, out var keybind, allowDefault) ? keybind : null!)
            .Where(keybind => keybind != null);
    }
}

