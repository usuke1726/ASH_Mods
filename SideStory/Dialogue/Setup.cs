
using System.Reflection;
using ModdingAPI;

namespace SideStory.Dialogue;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        DialogueController.Setup(helper);
        AddNodes();
        NodeSelector.OnSetupDone();
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
            entry?.Setup();
        }
    }
}

