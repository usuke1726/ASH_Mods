
using System.Text.RegularExpressions;
using IMoreExpressions;
using ModdingAPI;
using UnityEngine;

namespace CustomConversation.Actions;

/* 文法:

1行に1アクション

<time(float)> <character(string)> [talk|speak|sp|t] "<text(string)>" <showingTime(float,optional)> <xoffset(float,optional)>
<time(float)> <character(string)> [look|lk] <target(string or null)>
<time(float)> <character(string)> [draw-attention|attention|at]
<time(float)> <character(string)> [avoid-attention|avoid|aat|av]
<time(float)> <character(string)> [emote|em] <emote(string or null)>
<time(float)> <character(string)> [pose|ps] <pose(string or null)>
<time(float)> <character(string)> [end]

※ただし， talk は省略可能とする

・ 行頭あるいはスペースの後ろにくる "#", ";", "//" は，コメント文の始まりとみなされ，それ以降の行末までの内容は無視される

<time> に "_" を指定した場合，直前の行のイベントの終了時刻(talkなら開始時刻+表示時間，それ以外なら開始時刻)が用いられる
また， "__" を指定した場合は，直前のその前の時刻を用いられる
さらに "_?" を指定した場合は，最後に読み込まれた talk アクションの発生時刻(吹き出しが表示された瞬間)の時刻が用いられる
    (※talkアクション後の "_" は，吹き出しが消える瞬間の時刻となっている)
(詳しい挙動はParseLine関数を参照)

emote名などにnullを指定する場合はそのまま "null" とかくか，あるいは "_" でもよい

[conversationid|conversation|id] <id(string)> この構文は必須！(id文字列はcase-sensitive)
char [alias] <name> (position) [rotation]
camera (position) [rotation]

さらに，時刻(prevTime) をキャッシュして再利用する機能を追加！

@<name(string,nonspace)> <time(float)>
で キャッシュをそのtimeとして登録する．単に
@<name(string.nonspace)>
とすると，prevTimeがtimeとして使われる (つまり "@<name> _" と同じ)

キャッシュを登録したら，以降の行で

@<name> _ ...
とすると登録した時刻が，
@<name> _+0.5 ...
とすると登録した時刻の0.5秒後の時刻が用いられる

ある行の内容を "EOF" とだけにすると(空白は無視)，それ以降の行はすべてコメントとして無視されるようになる．
*/

internal static class Parser
{
    public enum Type
    {
        Invalid,
        Talk,
        Look,
        DrawAttention,
        AvoidAttention,
        Emote,
        Pose,
        End
    }
    private static readonly Dictionary<string, Characters> aliases = [];
    private static float pprevTime = 0;
    private static float prevTime = 0;
    private static float prevTalkStartTime = 0;
    private static readonly Dictionary<string, float> cachedTimes = [];
    private static bool reservedCacheTime = false;
    private static string cacheTimeString = "";
    private static readonly HashSet<string> conversationIds = [];
    private static void Reset()
    {
        aliases.Clear();
        cachedTimes.Clear();
        pprevTime = 0;
        prevTime = 0;
        prevTalkStartTime = 0;
        reservedCacheTime = false;
        cacheTimeString = "";
    }
    public static bool TryParse(IEnumerable<string> data, out ConversationDataSet result, out string error)
    {
        Reset();
        error = null!;
        result = new();
        foreach (var line in data)
        {
            var l = FormatSpace(line).Trim();
            if (l == "EOF") return true;
            var e = ParseLine(l, ref result);
            if (e != null) { error = $"error at the line \"{line}\":\n\t{e}"; return false; }
        }
        if (result.ConversationId == null)
        {
            error = $"ConversationId is not specified";
            return false;
        }
        return true;
    }
    private static string? ParseLine(string line, ref ConversationDataSet result)
    {
        if (line.Length == 0) return null;
        if (IsComment(line)) return null;
        if (IsConversationIdLine(line)) return ParseConversationId(line, ref result);
        if (IsCharacterInitLine(line)) return ParseCharacterInit(line, ref result);
        if (IsCameraInitLine(line)) return ParseCameraInit(line, ref result);

        string? error;

        error = ExtractTime(out var time, ref line);
        if (error != null) return error;
        if (reservedCacheTime && line.Length == 0)
        {
            cachedTimes[cacheTimeString] = prevTime;
            return null;
        }
        error = ExtractCharacter(out var character, ref line);
        if (error != null) return error;
        error = ExtractType(out var type, ref line);
        if (error != null) return error;

        ActionCore? action = type switch
        {
            Type.Talk => ParseTalk(line, time, character, ref error),
            Type.Look => ParseLook(line, time, character, ref error),
            Type.Emote => ParseEmote(line, time, character, ref error),
            Type.Pose => ParsePose(line, time, character, ref error),
            Type.DrawAttention => new DrawAttentionAction(character, time),
            Type.AvoidAttention => new AvoidAttentionAction(character, time),
            Type.End => new EndAction(character, time),
            _ => throw new Exception($"unexpected type {type}"),
        };
        if (action == null) return error;
        result.Actions.Add(action);
        if (reservedCacheTime) cachedTimes[cacheTimeString] = prevTime;
        return null;
    }
    private static string? ExtractTime(out float time, ref string line)
    {
        time = default;
        reservedCacheTime = false;
        string val;
        var error = ExtractUntilSpace(out val, ref line, "time not found");
        if (error != null) return error;

        if (val.StartsWith('@'))
        {
            val = val[1..];
            if (cachedTimes.TryGetValue(val, out var cachedTime))
            {
                prevTime = cachedTime;
            }
            else
            {
                reservedCacheTime = true;
                cacheTimeString = val;
            }
            if (reservedCacheTime && line.Length == 0) return null;
            ExtractUntilSpace(out val, ref line);
        }

        if (val == "_") time = prevTime;
        else if (val == "__") time = pprevTime;
        else if (val == "_?") time = prevTalkStartTime;
        else if (val.StartsWith("_+"))
        {
            if (!float.TryParse(val[2..], out var diffTime)) return $"invalid time \"{val}\"";
            time = prevTime + diffTime;
        }
        else if (val.StartsWith("__+"))
        {
            if (!float.TryParse(val[3..], out var diffTime)) return $"invalid time \"{val}\"";
            time = pprevTime + diffTime;
        }
        else if (val.StartsWith("_?+"))
        {
            if (!float.TryParse(val[3..], out var diffTime)) return $"invalid time \"{val}\"";
            time = prevTalkStartTime + diffTime;
        }
        else if (!float.TryParse(val, out time)) return $"not float time \"{val}\"";
        //if (Mathf.Abs(prevTime - time) > 1e-6f)
        if (!Mathf.Approximately(time, prevTime))
        {
            pprevTime = prevTime;
            prevTime = time;
        }
        if (time < 0) return $"negative time: {val}";
        return null;
    }
    private static string? ExtractCharacter(out Characters character, ref string line)
    {
        character = default;
        var error = ExtractUntilSpace(out var val, ref line, "character not found");
        if (error != null) return error;
        if (!TryGetCharacter(val, out character)) return $"invalid character: \"{val}\"";
        return null;
    }
    private static string? ExtractType(out Type type, ref string line)
    {
        type = default;
        if (line.StartsWith('"'))
        {
            type = Type.Talk;
            return null;
        }
        ExtractUntilSpace(out string val, ref line);
        type = val switch
        {
            "talk" or "speak" or "sp" or "t" => Type.Talk,
            "look" or "lk" => Type.Look,
            "draw-attention" or "attention" or "at" => Type.DrawAttention,
            "avoid-attention" or "avoid" or "aat" or "av" => Type.AvoidAttention,
            "emote" or "em" => Type.Emote,
            "pose" or "ps" => Type.Pose,
            "end" => Type.End,
            _ => Type.Invalid,
        };
        if (type == Type.Invalid) return $"invalid type: \"{val}\"";
        return null;
    }
    private static SpeakAction? ParseTalk(string data, float time, Characters character, ref string? error)
    {
        var firstQuotation = data.IndexOf('"');
        if (firstQuotation < 0)
        {
            error = "double quotation not found";
            return null;
        }
        int secondQuotation = firstQuotation;
        while (true)
        {
            secondQuotation = data.IndexOf('"', secondQuotation + 1);
            if (secondQuotation < 0)
            {
                error = "closing double quotation not found";
                return null;
            }
            else if (data[secondQuotation - 1] != '\\') break;
        }
        var text = data[(firstQuotation + 1)..secondQuotation].Replace("\\\"", "\"").Replace("\\n", "\n");
        data = TrimStart(data[(secondQuotation + 1)..]);

        string val;
        float v;
        ExtractUntilSpace(out val, ref data);
        float? showingTime = float.TryParse(val, out v) ? v : null;
        ExtractUntilSpace(out val, ref data);
        float? xOffset = float.TryParse(val, out v) ? v : null;
        SpeakAction ret = new(character, time, text, showingTime, xOffset);
        prevTalkStartTime = prevTime;
        prevTime += ret.showingTime;
        return ret;
    }
    private static LookAtAction? ParseLook(string data, float time, Characters character, ref string? error)
    {
        ExtractUntilSpace(out var val, ref data);
        if (IsNullString(val)) return new(character, time, null);
        if (!TryGetCharacter(val, out var target))
        {
            error = $"invalid character: \"{val}\"";
            return null;
        }
        return new(character, time, target);
    }
    private static EmoteAction? ParseEmote(string data, float time, Characters character, ref string? error)
    {
        ExtractUntilSpace(out var val, ref data);
        if (IsNullString(val)) return new(character, time, null);
        if (Enum.TryParse<Expression>(val, true, out var res)) return new(character, time, res);
        error = $"invalid emotion \"{val}\"";
        return null;
    }
    private static PoseAction? ParsePose(string data, float time, Characters character, ref string? error)
    {
        ExtractUntilSpace(out var val, ref data);
        if (IsNullString(val)) return new(character, time, null);
        if (Enum.TryParse<Pose>(val, true, out var res)) return new(character, time, res);
        error = $"invalid pose \"{val}\"";
        return null;
    }
    private static void ExtractUntilSpace(out string val, ref string line)
    {
        int idx = line.IndexOf(' ');
        if (idx >= 0)
        {
            val = line[0..idx];
            line = TrimStart(line[idx..]);
        }
        else
        {
            val = line;
            line = "";
        }
    }
    private static string? ExtractUntilSpace(out string val, ref string line, string errorMes)
    {
        int idx = line.IndexOf(' ');
        if (idx < 0) { val = ""; return errorMes; }
        val = line[0..idx];
        line = TrimStart(line[idx..]);
        return null;
    }
    private static bool IsNullString(string val)
    {
        string s = val.ToLower();
        return s == "null" | s == "_";
    }
    private static bool IsComment(string val)
    {
        val = val.TrimStart();
        return val.StartsWith("#") || val.StartsWith(";") || val.StartsWith("//");
    }
    private static string TrimStart(string val)
    {
        return IsComment(val) ? "" : val.TrimStart();
    }
    private static Regex spacePattern = new(@"[^\S\n]");
    private static string FormatSpace(string s) => spacePattern.Replace(s, " ");
    private static readonly string[] idKeywords = ["id ", "conversation ", "conversationid "];
    private static bool IsConversationIdLine(string val)
    {
        string s = val.ToLower();
        return idKeywords.Any(k => s.StartsWith(k));
    }
    private static bool IsCharacterInitLine(string val)
    {
        string s = val.ToLower();
        return s.StartsWith("character ") || s.StartsWith("char ");
    }
    private static bool IsCameraInitLine(string val)
    {
        string s = val.ToLower();
        return s.StartsWith("camera ");
    }
    private static bool TryGetCharacter(string name, out Characters character)
    {
        if (aliases.TryGetValue(name, out character)) return true;
        return CharactersParser.TryParse(name, out character);
    }
    private static string? ParseConversationId(string line, ref ConversationDataSet result)
    {
        ExtractUntilSpace(out _, ref line);
        ExtractUntilSpace(out var id, ref line);
        id = id.Trim();
        if (id.Length == 0) return "id not found";
        if (conversationIds.Contains(id)) return $"ConversationId {id} has already registered";
        result.ConversationId = id;
        conversationIds.Add(id);
        return null;
    }
    private static string? ParseCharacterInit(string line, ref ConversationDataSet result)
    {
        ExtractUntilSpace(out _, ref line);

        string? alias = null;
        if (line.StartsWith('['))
        {
            var aliasEnd = line.IndexOf(']');
            if (aliasEnd == -1) return $"closing ] not found";
            alias = line[1..aliasEnd].Replace(" ", "");
            if (alias.Length == 0) return $"alias is empty";
            if (aliases.ContainsKey(alias)) return $"alias {alias} is already registered";
            if (IsNullString(alias)) return $"null string \"{alias}\" cannot be used as alias";
            line = TrimStart(line[(aliasEnd + 1)..]);
        }

        string? error = ExtractUntilSpace(out var val, ref line, "character not found");
        if (error != null) return error;
        if (!CharactersParser.TryParse(val, out var character)) return $"invalid character: \"{val}\"";
        if (alias != null) aliases[alias] = character;

        int start;
        int end;
        start = line.IndexOf('(');
        end = line.IndexOf(')');
        List<float> coords = [];
        if (start >= 0 && start < end)
        {
            var posData = line[(start + 1)..end];
            line = TrimStart(line[(end + 1)..]);
            if (line.StartsWith(",")) line = TrimStart(line[1..]);
            var scoords = posData.Replace(",", " ").Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);
            if (scoords.Length < 2) return $"invalid position: \"{posData}\"";
            for (int i = 0; i < scoords.Length; i++)
            {
                if (!float.TryParse(scoords[i], out var value)) return $"cannot parse to float: \"{scoords[i]}\"";
                coords.Add(value);
            }
        }
        else return $"position not found";
        Vector3? rotation = null;
        start = line.IndexOf('[');
        end = line.IndexOf(']');
        if (start >= 0 && start < end)
        {
            var posData = line[(start + 1)..end];
            var scoords = posData.Replace(",", " ").Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);
            if (scoords.Length < 3) return $"invalid rotation: \"{posData}\"";
            List<float> rot = [];
            for (int i = 0; i < scoords.Length; i++)
            {
                if (!float.TryParse(scoords[i], out var value)) return $"cannot parse to float: \"{scoords[i]}\"";
                rot.Add(value);
            }
            rotation = new(rot[0], rot[1], rot[2]);
        }
        result.InitialPositions[character] = coords.Count switch
        {
            2 => new Position2(new(coords[0], coords[1]), rotation),
            3 => new Position3(new(coords[0], coords[1], coords[2]), rotation),
            _ => throw new Exception()
        };
        return null;
    }
    private static string? ParseCameraInit(string line, ref ConversationDataSet result)
    {
        ExtractUntilSpace(out _, ref line);

        int start;
        int end;

        Vector3? position = null;
        start = line.IndexOf('(');
        end = line.IndexOf(')');
        if (start >= 0 && start < end)
        {
            var posData = line[(start + 1)..end];
            var scoords = posData.Replace(",", " ").Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);
            if (scoords.Length < 3) return $"invalid position: \"{posData}\"";
            List<float> rot = [];
            for (int i = 0; i < scoords.Length; i++)
            {
                if (!float.TryParse(scoords[i], out var value)) return $"cannot parse to float: \"{scoords[i]}\"";
                rot.Add(value);
            }
            position = new(rot[0], rot[1], rot[2]);
        }

        Vector3? rotation = null;
        start = line.IndexOf('[');
        end = line.IndexOf(']');
        if (start >= 0 && start < end)
        {
            var posData = line[(start + 1)..end];
            var scoords = posData.Replace(",", " ").Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);
            if (scoords.Length < 3) return $"invalid rotation: \"{posData}\"";
            List<float> rot = [];
            for (int i = 0; i < scoords.Length; i++)
            {
                if (!float.TryParse(scoords[i], out var value)) return $"cannot parse to float: \"{scoords[i]}\"";
                rot.Add(value);
            }
            rotation = new(rot[0], rot[1], rot[2]);
        }

        result.CameraPosition = position;
        result.CameraRotation = rotation;
        return null;
    }
}

