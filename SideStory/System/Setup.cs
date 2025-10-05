
using ModdingAPI;

namespace SideStory.System;

internal class Setup
{
    private static bool done = false;
    public Setup(IModHelper helper)
    {
        if (done) return;
        done = true;
        SaveData.Setup(helper);
        Tags.Setup(helper);
        EndGameController.Setup(helper);
    }
}

