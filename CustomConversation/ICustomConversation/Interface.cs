
using ModdingAPI.IO;

namespace ICustomConversation;

public interface IConversationData
{
    bool TransitionStart { get; }
    bool TransitionEnd { get; }
    bool Finished { get; }
    void Update();
    void Setup() { }
    void Cleanup() { }
    void SetupWithinTransition() { }
    void CleanupWithinTransition() { }
}

public interface ISpecialConversation
{
    void StartConversation(IConversationData data);
    void StartConversation(string id);
    bool Register(string contents, out string id, bool silent = false);
    bool Register(string contents, bool silent = false);
    bool Register(TextFile file, out string id, bool silent = false);
    bool Register(TextFile file, bool silent = false);
    bool Register(string id, IConversationData conversationData);
    bool IsRegistered(string id);
    HashSet<string> IDs { get; }
}

