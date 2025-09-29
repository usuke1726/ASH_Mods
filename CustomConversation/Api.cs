
using ICustomConversation;
using ModdingAPI.IO;

namespace CustomConversation;

internal class SpecialConversationApi : ISpecialConversation
{
    internal static SpecialConversationApi instance = new();
    public void StartConversation(IConversationData data) => SpecialConversation.StartConversation(data);
    public void StartConversation(string id) => ConversationRegistry.TryStart(id);
    public bool Register(string contents, out string id, bool silent = false) => ConversationRegistry.Register(contents, out id, silent);
    public bool Register(string contents, bool silent = false) => ConversationRegistry.Register(contents, silent);
    public bool Register(TextFile file, out string id, bool silent = false) => ConversationRegistry.Register(file, out id, silent);
    public bool Register(TextFile file, bool silent = false) => ConversationRegistry.Register(file, silent);
    public bool Register(string id, IConversationData conversationData) => ConversationRegistry.Register(id, conversationData);
    public bool IsRegistered(string id) => ConversationRegistry.IsRegistered(id);
    public HashSet<string> IDs { get => ConversationRegistry.IDs; }
}

