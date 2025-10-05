
using ModdingAPI;

namespace SideStory.Character;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        Core.Setup(helper);
    }
}

