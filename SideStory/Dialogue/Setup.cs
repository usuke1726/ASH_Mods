
using ModdingAPI;
using SideStory.Dialogue.Data;

namespace SideStory.Dialogue;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        DialogueController.Setup(helper);
        Tags.Setup(helper);
        AddNodes();
        NodeSelector.OnSetupDone();
    }
    private void AddNodes()
    {
        Dummy.Setup();
    }
}

