
using System.Reflection;
using ModdingAPI;

namespace Sidequel.Dialogue;

internal class Setup
{
    private static bool done = false;
    private static readonly List<NodeEntryBase> entries = [];
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        DialogueController.Setup(helper);
        AddNodes();
        NodeSelector.OnSetupDone();
        helper.Events.Gameloop.GameStarted += (_, _) =>
        {
            foreach (var entry in entries) entry.OnGameStarted();
        };
#if DEBUG
        Debug.Setup(helper);
#endif
    }
    private void AddNodes()
    {
        var asm = Assembly.GetExecutingAssembly();
        Debug($"Assembly {asm.FullName} {asm.Location} {asm.GetName().Version}");
        var types = asm.DefinedTypes.Where(type => typeof(NodeEntryBase).IsAssignableFrom(type) && !type.IsAbstract);
        foreach (var type in types)
        {
            var constructor = type.GetConstructor([]);
            if (constructor == null) continue;
            Debug($"found node {type.Name}");
            var entry = constructor.Invoke([]) as NodeEntryBase;
            if (entry != null)
            {
                entries.Add(entry);
                entry.Setup();
            }
        }
    }
}

