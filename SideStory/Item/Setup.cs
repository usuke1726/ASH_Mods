
using ModdingAPI;

namespace SideStory.Item;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        DataHandler.Setup(helper);
    }
}

