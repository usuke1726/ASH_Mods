
using CustomConversation.Actions;
using ICustomConversation;
using ModdingAPI.IO;

namespace CustomConversation;

internal class ConversationRegistry(ConversationDataSet data) : ConversationData
{
    private readonly ConversationDataSet data = data;
    protected override ConversationDataSet DataSet => data;
    private static readonly Dictionary<string, IConversationData> conversations = [];
    public static bool IsRegistered(string id) => conversations.ContainsKey(id);
    public static HashSet<string> IDs { get => [.. conversations.Keys]; }
    public static bool Register(string id, IConversationData conversationData)
    {
        if (conversations.ContainsKey(id))
        {
            Monitor.Log($"Conversation id {id} has been already registered", LL.Error);
            return false;
        }
        conversations[id] = conversationData;
        return true;
    }
    public static bool Register(string contents, bool silent = false) => Register(contents, out _, silent);
    public static bool Register(string contents, out string id, bool silent = false) => Register(contents.Split("\n"), out id, silent);
    private static bool Register(IEnumerable<string> lines, out string id, bool silent = false)
    {
        var success = Parser.TryParse(lines, out var data, out string error);
        if (!success)
        {
            Monitor.Log($"Failed loading conversation data!\n\t{error}", LL.Error);
            id = null!;
            return false;
        }
        id = data.ConversationId;
        if (!silent) Monitor.Log($"Successfully loaded test conversation data ({id})", LL.Info);
        return Register(id, new ConversationRegistry(data));
    }
    public static bool Register(TextFile file, bool silent = false) => Register(file, out _, silent);
    public static bool Register(TextFile file, out string id, bool silent = false)
    {
        try { return Register(file.ReadLines(), out id, silent); }
        catch (Exception e) { Monitor.Log($"ERROR: {e}"); id = null!; return false; }
    }
    public static bool TryGet(string id, out IConversationData val) => conversations.TryGetValue(id, out val);
    public static void TryStart(string id)
    {
        if (TryGet(id, out IConversationData val)) SpecialConversation.StartConversation(val);
    }
}

